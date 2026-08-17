using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using MisterGames.Common.Async;
using MisterGames.Common.Maths;
using MisterGames.Common.Tick;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MisterGames.UI.Components {
    
    public sealed class UiTextPrinter : MonoBehaviour {

        [Header("Text")]
        [SerializeField] private TMP_Text _defaultTextField;
        
        [Header("Print")]
        [SerializeField] private bool _useTimeScale = false;
        [SerializeField] [Min(0f)] private float _symbolDelayMin = 0.1f;
        [SerializeField] [Min(0f)] private float _symbolDelayMax = 0.1f;
        [SerializeField] [Min(0f)] private float _forceFinishSymbolDelay = 0.01f;
        [SerializeField] private SpecialSymbolData[] _specialSymbols;

        [Serializable]
        private struct SpecialSymbolData {
            public SymbolMask symbolMask;
            [Min(0f)] public float delayMin;
            [Min(0f)] public float delayMax;
        }

        [Flags]
        private enum SymbolMask {
            None = 0,
            Space = 1,
            NewLine = 2,
            Comma = 4,
            Period = 8,
            QuestionMark = 16,
            ExclamationMark = 32,
            Semicolon = 64,
            Colon = 128,
            Ellipsis = 256,
        }
        
        private const string TransparentTagOpen = "<color=#00000000>";
        private const string TransparentTagClose = "</color>";
        private static readonly string TransparentNewLine = $"{TransparentTagOpen}\n.{TransparentTagClose}";

        /// <summary>
        /// Tags that override color or alpha of the not printed part of the content
        /// and therefore must not be applied before caret reaches them.
        /// </summary>
        private static readonly string[] ColorTags = { "color", "alpha", "gradient", "mark" };

        /// <summary>
        /// Custom tag &lt;speed=x&gt;...&lt;/speed&gt; multiplies printing speed of the symbols inside,
        /// where x is 0 or greater: 0.5 is two times slower, 2 is two times faster, 0 is instant printing.
        /// TMP has no such tag, so it is removed from the text passed into the text field.
        /// </summary>
        private const string SpeedTag = "speed";
        private const float DefaultSpeed = 1f;

        /// <summary>
        /// Custom tag &lt;pause=x&gt; delays printing of the next symbol by x seconds. It has no closing form.
        /// TMP has no such tag, so it is removed from the text passed into the text field.
        /// </summary>
        private const string PauseTag = "pause";

        public TMP_Text DefaultTextField => _defaultTextField;
        
        private CancellationTokenSource _destroyCts;
        private CancellationTokenSource _enableCts;

        private readonly Dictionary<int, byte> _operationIdMap = new();
        private readonly Dictionary<int, float> _immediateFinishRequestsMap = new();
        private readonly Stack<TextBuffer> _textBufferPool = new();

        /// <summary>
        /// Reusable buffers of a single print operation: allocated once and returned into pool at the end of printing.
        /// </summary>
        private sealed class TextBuffer {
            public readonly StringBuilder stringBuilder = new();
            public readonly List<int> hiddenTailRanges = new();
            public readonly List<int> customTagRanges = new();
            public readonly List<SpeedChange> speedChanges = new();
            public readonly List<float> speedStack = new();
        }

        private readonly struct SpeedChange {

            public readonly int index;
            public readonly float speed;

            public SpeedChange(int index, float speed) {
                this.index = index;
                this.speed = speed;
            }
        }

        private void Awake() {
            AsyncExt.RecreateCts(ref _destroyCts);
        }

        private void OnDestroy() {
            AsyncExt.DisposeCts(ref _destroyCts);
        }

        private void OnEnable() {
            AsyncExt.RecreateCts(ref _enableCts);
        }

        private void OnDisable() {
            AsyncExt.DisposeCts(ref _enableCts);
        }

        public void SetText(TMP_Text textField, string content) {
            CancelPrinting(textField, clear: true);

            if (string.IsNullOrEmpty(content)) return;

            var buffer = GetTextBuffer();
            var sb = buffer.stringBuilder;

            sb.Clear();
            sb.EnsureCapacity(content.Length);

            CollectTags(content, buffer.hiddenTailRanges, buffer.customTagRanges, buffer.speedChanges, buffer.speedStack);
            AppendSkippingRanges(sb, content, 0, content.Length, buffer.customTagRanges);

            textField.SetText(sb);

            ReleaseTextBuffer(buffer);
        }
        
        public async UniTask PrintTextAsync(
            TMP_Text textField,
            string content,
            CancellationToken cancellationToken) 
        {
            int hash = textField.GetHashCode();
            byte id = _operationIdMap.GetValueOrDefault(hash);
            byte currentId;

            _operationIdMap[hash] = id.IncrementUncheckedRef();

            var buffer = GetTextBuffer();
            var sb = buffer.stringBuilder;
            var hiddenTailRanges = buffer.hiddenTailRanges;
            var customTagRanges = buffer.customTagRanges;
            var speedChanges = buffer.speedChanges;

            sb.Clear();

            // Keeping the whole text in a single string builder chunk:
            // otherwise truncating length over a chunk border reallocates chunk arrays on each printed symbol.
            sb.EnsureCapacity(content.Length + TransparentNewLine.Length + TransparentTagOpen.Length + TransparentTagClose.Length);

            CollectTags(content, hiddenTailRanges, customTagRanges, speedChanges, buffer.speedStack);

            int length = content.Length;
            int pointer = 0;
            int printedLength = 0;
            int speedChangeIndex = 0;

            bool useTimeScale = _useTimeScale;
            float delayAccum = 0f;
            float speed = DefaultSpeed;
            char prev = '\0';

            try {
                while (pointer < length &&
                       !cancellationToken.IsCancellationRequested &&
                       !_destroyCts.IsCancellationRequested &&
                       _operationIdMap.TryGetValue(hash, out currentId) && currentId == id)
                {
                    float symbolDelay = -1f;
                    bool isForceFinish = _immediateFinishRequestsMap.TryGetValue(hash, out float finishDelay);

                    if (isForceFinish) {
                        if (finishDelay <= 0f) {
                            sb.Length = 0;
                            AppendSkippingRanges(sb, content, 0, length, customTagRanges);
                            textField.SetText(sb);

                            _operationIdMap.Remove(hash);
                            _immediateFinishRequestsMap.Remove(hash);
                            break;
                        }

                        symbolDelay = finishDelay;
                    }

                    int start = pointer;
                    int newLineStart = -1;

                    // Pause tag is processed as a step that prints no symbol and only delays the next one.
                    bool isPauseStep = TryConsumePauseTag(content, ref pointer, length, out float pause);

                    if (isPauseStep) {
                        // Force finish is an explicit request to print faster and therefore ignores pauses.
                        if (symbolDelay < 0f) symbolDelay = pause;
                    }
                    else {
                        char c = GetNextCharSkippingTags(content, ref pointer, length);

                        int nextPointer = pointer;
                        char next = GetNextCharSkippingTags(content, ref nextPointer, length);

                        // Speed tags passed by caret are applied to the current symbol.
                        while (speedChangeIndex < speedChanges.Count && speedChanges[speedChangeIndex].index < pointer) {
                            speed = speedChanges[speedChangeIndex++].speed;
                        }

                        // Force finish delay is an explicit request to print faster and therefore ignores speed tags.
                        if (symbolDelay < 0f) {
                            symbolDelay = speed > 0f ? GetSymbolDelay(prev, c, next) / speed : 0f;
                        }

                        if (c == '\n' && IsEndOfContent(content, pointer, length)) {
                            // Trailing new line is trimmed during TMP layout, so it is replaced
                            // with a transparent new line followed by a symbol to keep the last line.
                            newLineStart = pointer >= start + 2 && content[pointer - 1] == 'n' && content[pointer - 2] == '\\'
                                ? pointer - 2
                                : pointer - 1;
                        }

                        prev = c;
                    }

                    delayAccum += symbolDelay;

                    // Printed content is only appended: already printed part of the string builder is never rebuilt.
                    sb.Length = printedLength;

                    if (newLineStart >= 0) {
                        AppendSkippingRanges(sb, content, start, newLineStart, customTagRanges);
                        sb.Append(TransparentNewLine);
                    }
                    else {
                        AppendSkippingRanges(sb, content, start, pointer, customTagRanges);
                    }

                    printedLength = sb.Length;

                    sb.Append(TransparentTagOpen);
                    AppendSkippingRanges(sb, content, pointer, length, hiddenTailRanges);
                    sb.Append(TransparentTagClose);

                    // Symbols printed without delay are shown at once with the next symbol that has a delay.
                    if (delayAccum > 0f || pointer >= length) {
                        textField.SetText(sb);
                    }

                    // Delay of the last symbol would only postpone the end of printing, as nothing is printed after it.
                    // Printing must not stay in progress while the whole text is already shown:
                    // otherwise force finish requests are consumed by a printing that has nothing left to print.
                    // Trailing pause tag is an explicit request to wait and therefore is not skipped.
                    if (pointer >= length && !isPauseStep) break;

                    float startTime = useTimeScale ? TimeSources.scaledTime : TimeSources.unscaledTime;
                    finishDelay = isForceFinish ? finishDelay : -1f;

                    while ((useTimeScale ? TimeSources.scaledTime : TimeSources.unscaledTime) - startTime < delayAccum &&
                           !cancellationToken.IsCancellationRequested &&
                           !_destroyCts.IsCancellationRequested &&
                           _operationIdMap.TryGetValue(hash, out currentId) && currentId == id &&
                           finishDelay.IsNearlyEqual(_immediateFinishRequestsMap.GetValueOrDefault(hash, -1f)))
                    {
                        await UniTask.Yield();
                    }

                    float newFinishDelay = _immediateFinishRequestsMap.GetValueOrDefault(hash, -1f);

                    if (finishDelay.IsNearlyEqual(newFinishDelay)) {
                        delayAccum -= (useTimeScale ? TimeSources.scaledTime : TimeSources.unscaledTime) - startTime;
                        continue;
                    }

                    delayAccum = 0f;
                }
            }
            finally {
                ReleaseTextBuffer(buffer);
            }

            // Printing is superseded by a newer printing of the same text field, which owns the maps now.
            if (!_operationIdMap.TryGetValue(hash, out currentId) || currentId != id) return;

            // Cleaned up even if printing was cancelled: otherwise a force finish request left in the map
            // is applied to the next printing of the same text field.
            _operationIdMap.Remove(hash);
            _immediateFinishRequestsMap.Remove(hash);
        }

        public void CancelPrinting(TMP_Text textField, bool clear = false) {
            int hash = textField?.GetHashCode() ?? 0;
            
            _operationIdMap.Remove(hash);
            _immediateFinishRequestsMap.Remove(hash);
            
            if (clear && textField != null) textField.SetText((string) null);
        }

        /// <summary>
        /// Returns true if there was a printing in progress to finish, so that a caller can tell
        /// whether the request was consumed or there was nothing left to print.
        /// </summary>
        public bool ForceFinishPrinting(TMP_Text textField, float symbolDelay = -1f) {
            int hash = textField?.GetHashCode() ?? 0;

            if (_operationIdMap.ContainsKey(hash)) {
                if (symbolDelay < 0f) symbolDelay = _forceFinishSymbolDelay;
                _immediateFinishRequestsMap[hash] = symbolDelay;
                return true;
            }

            _operationIdMap.Remove(hash);
            _immediateFinishRequestsMap.Remove(hash);

            return false;
        }

        private TextBuffer GetTextBuffer() {
            return _textBufferPool.Count > 0 ? _textBufferPool.Pop() : new TextBuffer();
        }

        private void ReleaseTextBuffer(TextBuffer buffer) {
            buffer.stringBuilder.Clear();
            buffer.hiddenTailRanges.Clear();
            buffer.customTagRanges.Clear();
            buffer.speedChanges.Clear();
            buffer.speedStack.Clear();

            _textBufferPool.Push(buffer);
        }

        /// <summary>
        /// Searches for tags once per content, so that appending text
        /// does not require scanning the content char by char on each printed symbol.
        /// Ranges are written as [start, end) index pairs.
        /// </summary>
        /// <param name="hiddenTailRanges">Tags removed from the not printed part of the text: color and speed tags.</param>
        /// <param name="customTagRanges">Speed and pause tags, removed from the whole text as TMP does not support them.</param>
        /// <param name="speedChanges">Speed value applied starting from the content index.</param>
        /// <param name="speedStack">Reusable buffer to support nested speed tags.</param>
        private static void CollectTags(
            string content,
            List<int> hiddenTailRanges,
            List<int> customTagRanges,
            List<SpeedChange> speedChanges,
            List<float> speedStack)
        {
            hiddenTailRanges.Clear();
            customTagRanges.Clear();
            speedChanges.Clear();
            speedStack.Clear();

            int length = content.Length;
            int pointer = 0;
            float speed = DefaultSpeed;

            while (pointer < length) {
                if (content[pointer] != '<') {
                    pointer++;
                    continue;
                }

                int closeIndex = GetTagCloseIndex(content, pointer, length);

                if (closeIndex < 0) {
                    pointer++;
                    continue;
                }

                if (IsPauseTag(content, pointer, closeIndex, out _)) {
                    customTagRanges.Add(pointer);
                    customTagRanges.Add(closeIndex + 1);

                    hiddenTailRanges.Add(pointer);
                    hiddenTailRanges.Add(closeIndex + 1);
                }
                else if (IsSpeedTag(content, pointer, closeIndex, out bool isClosingTag, out float tagSpeed)) {
                    if (isClosingTag) {
                        int last = speedStack.Count - 1;

                        speed = last >= 0 ? speedStack[last] : DefaultSpeed;
                        if (last >= 0) speedStack.RemoveAt(last);
                    }
                    else {
                        speedStack.Add(speed);
                        speed = tagSpeed;
                    }

                    speedChanges.Add(new SpeedChange(closeIndex + 1, speed));

                    customTagRanges.Add(pointer);
                    customTagRanges.Add(closeIndex + 1);

                    hiddenTailRanges.Add(pointer);
                    hiddenTailRanges.Add(closeIndex + 1);
                }
                else if (IsColorTag(content, pointer, closeIndex)) {
                    hiddenTailRanges.Add(pointer);
                    hiddenTailRanges.Add(closeIndex + 1);
                }

                pointer = closeIndex + 1;
            }
        }

        /// <summary>
        /// Moves pointer over the closest pause tag, if it is placed before the next printed symbol.
        /// Tags between pointer and the pause tag are left for the next step, so that they are applied
        /// together with the symbol they belong to.
        /// </summary>
        private static bool TryConsumePauseTag(string content, ref int pointer, int length, out float pause) {
            pause = 0f;
            int scan = pointer;

            while (scan < length && content[scan] == '<') {
                int closeIndex = GetTagCloseIndex(content, scan, length);
                if (closeIndex < 0) return false;

                if (IsPauseTag(content, scan, closeIndex, out pause)) {
                    pointer = closeIndex + 1;
                    return true;
                }

                scan = closeIndex + 1;
            }

            return false;
        }

        private static bool IsPauseTag(string content, int openIndex, int closeIndex, out float pause) {
            pause = 0f;

            int pointer = openIndex + 1;
            int end = pointer + PauseTag.Length;

            if (end >= closeIndex ||
                string.Compare(content, pointer, PauseTag, 0, PauseTag.Length, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

            return content[end] == '=' && TryParseFloat(content, end + 1, closeIndex, out pause);
        }

        private static bool IsSpeedTag(string content, int openIndex, int closeIndex, out bool isClosingTag, out float speed) {
            speed = DefaultSpeed;
            isClosingTag = false;

            int pointer = openIndex + 1;

            if (pointer < closeIndex && content[pointer] == '/') {
                isClosingTag = true;
                pointer++;
            }

            int end = pointer + SpeedTag.Length;

            if (end > closeIndex ||
                string.Compare(content, pointer, SpeedTag, 0, SpeedTag.Length, StringComparison.OrdinalIgnoreCase) != 0)
            {
                return false;
            }

            if (isClosingTag) return end == closeIndex;

            return end < closeIndex && content[end] == '=' && TryParseFloat(content, end + 1, closeIndex, out speed);
        }

        /// <summary>
        /// Parses a not negative float without allocations and independently of the current culture,
        /// as tag values always use '.' as a decimal separator.
        /// </summary>
        private static bool TryParseFloat(string content, int start, int end, out float result) {
            result = 0f;

            float value = 0f;
            int pointer = start;
            bool hasDigits = false;

            for (; pointer < end; pointer++) {
                char c = content[pointer];
                if (c < '0' || c > '9') break;

                value = value * 10f + (c - '0');
                hasDigits = true;
            }

            if (pointer < end && (content[pointer] == '.' || content[pointer] == ',')) {
                pointer++;
                float fraction = 0.1f;

                for (; pointer < end; pointer++) {
                    char c = content[pointer];
                    if (c < '0' || c > '9') break;

                    value += (c - '0') * fraction;
                    fraction *= 0.1f;
                    hasDigits = true;
                }
            }

            if (!hasDigits || pointer != end) return false;

            result = value;
            return true;
        }

        /// <summary>
        /// Returns index of the tag closing bracket, or -1 if content at <paramref name="openIndex"/> is not a tag.
        /// TMP stops tag validation at a nested '&lt;', so that text like "&lt;10 &lt;color=#980000&gt;"
        /// is printed as a literal "&lt;10 " followed by an actual color tag.
        /// </summary>
        private static int GetTagCloseIndex(string content, int openIndex, int length) {
            for (int i = openIndex + 1; i < length; i++) {
                char c = content[i];

                if (c == '<') break;
                if (c == '>') return IsTag(content, openIndex, i) ? i : -1;
            }

            return -1;
        }

        /// <summary>
        /// Checks that text between brackets has a shape of a rich text tag:
        /// "&lt;name&gt;", "&lt;/name&gt;", "&lt;name=value&gt;", "&lt;name attribute=value&gt;"
        /// or a "&lt;#RRGGBB&gt;" color short form.
        /// Prevents printing a text like "a &lt; b and b &gt; c" as if it had tags in it.
        /// </summary>
        private static bool IsTag(string content, int openIndex, int closeIndex) {
            int pointer = openIndex + 1;

            if (pointer < closeIndex && content[pointer] == '/') pointer++;

            if (pointer >= closeIndex) return false;

            // Color short form contains hex digits only.
            if (content[pointer] == '#') {
                for (int i = pointer + 1; i < closeIndex; i++) {
                    if (!IsHexDigit(content[i])) return false;
                }

                return closeIndex > pointer + 1;
            }

            if (!IsTagNameChar(content[pointer], isFirstChar: true)) return false;

            while (pointer < closeIndex && IsTagNameChar(content[pointer], isFirstChar: false)) {
                pointer++;
            }

            if (pointer == closeIndex || content[pointer] == '=') return true;

            // Tag name can only be followed by attributes, each of them requires a value.
            if (content[pointer] != ' ') return false;

            for (int i = pointer + 1; i < closeIndex; i++) {
                if (content[i] == '=') return true;
            }

            return false;
        }

        private static bool IsTagNameChar(char c, bool isFirstChar) {
            return c is >= 'a' and <= 'z' or >= 'A' and <= 'Z' ||
                   !isFirstChar && (c is >= '0' and <= '9' or '-' or '_');
        }

        private static bool IsHexDigit(char c) {
            return c is >= '0' and <= '9' or >= 'a' and <= 'f' or >= 'A' and <= 'F';
        }

        /// <summary>
        /// Appends content in range [<paramref name="start"/>, <paramref name="end"/>),
        /// skipping the passed tag ranges. Removing tags does not affect text layout, as tags have no size.
        /// </summary>
        private static void AppendSkippingRanges(StringBuilder sb, string content, int start, int end, List<int> ranges) {
            int pointer = start;

            for (int i = 0; i < ranges.Count && pointer < end; i += 2) {
                int rangeEnd = ranges[i + 1];
                if (rangeEnd <= pointer) continue;

                int rangeStart = ranges[i];
                if (rangeStart >= end) break;

                if (rangeStart > pointer) sb.Append(content, pointer, rangeStart - pointer);

                pointer = rangeEnd;
            }

            if (pointer < end) sb.Append(content, pointer, end - pointer);
        }

        private static bool IsColorTag(string content, int openIndex, int closeIndex) {
            int pointer = openIndex + 1;

            if (pointer < closeIndex && content[pointer] == '/') pointer++;

            // <#RRGGBB> is a short form of the color tag.
            if (pointer < closeIndex && content[pointer] == '#') return true;

            for (int i = 0; i < ColorTags.Length; i++) {
                string tag = ColorTags[i];
                int end = pointer + tag.Length;

                if (end > closeIndex ||
                    string.Compare(content, pointer, tag, 0, tag.Length, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    continue;
                }

                // Tag name must not be a prefix of some other tag name.
                if (end == closeIndex || content[end] == '=' || content[end] == ' ') return true;
            }

            return false;
        }

        private static char GetNextCharSkippingTags(string content, ref int pointer, int length) {
            char c = pointer < length ? content[pointer++] : '\0';

            while (c == '<') {
                int closeIndex = GetTagCloseIndex(content, pointer - 1, length);
                if (closeIndex < 0) break;

                pointer = closeIndex + 1;
                c = pointer < length ? content[pointer++] : '\0';
            }

            if (c == '\\' && pointer < length && content[pointer] == 'n') {
                pointer++;
                c = '\n';
            }

            return c;
        }
        
        private static bool IsEndOfContent(string content, int pointer, int length) {
            while (pointer < length) {
                char c = GetNextCharSkippingTags(content, ref pointer, length);

                if (c == '\0') break;
                if (c == '\n' || !char.IsWhiteSpace(c)) return false;
            }

            return true;
        }

        private float GetSymbolDelay(char prev, char curr, char next) {
            for (int i = 0; i < _specialSymbols.Length; i++) {
                ref var data = ref _specialSymbols[i];

                if ((data.symbolMask & SymbolMask.Space) != 0 && curr == ' ' && next != ' ' || 
                    (data.symbolMask & SymbolMask.NewLine) != 0 && curr == '\n' && next != '\n' || 
                    (data.symbolMask & SymbolMask.Comma) != 0 && curr == ',' && next != ',' ||
                    (data.symbolMask & SymbolMask.Period) != 0 && prev != '.' && curr == '.' && next != '.' ||
                    (data.symbolMask & SymbolMask.QuestionMark) != 0 && curr == '?' && next != '?' ||
                    (data.symbolMask & SymbolMask.ExclamationMark) != 0 && curr == '!' && next != '!' ||
                    (data.symbolMask & SymbolMask.Semicolon) != 0 && curr == ';' && next != ';' ||
                    (data.symbolMask & SymbolMask.Colon) != 0 && curr == ':' && next != ':' ||
                    (data.symbolMask & SymbolMask.Ellipsis) != 0 && (curr == '…' && next != '…' || prev == '.' && curr == '.' && next != '.')) 
                {
                    return Random.Range(data.delayMin, data.delayMax);
                }
            }

            return Random.Range(_symbolDelayMin, _symbolDelayMax);
        }
    }
    
}