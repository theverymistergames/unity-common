﻿using System;
using System.Runtime.CompilerServices;

namespace MisterGames.Scenario.Events {

    public static class EventReferenceExtensions {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetCount(this EventReference e) {
            return EventSystem.Main.GetCount(e);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsRaised(this EventReference e) {
            return EventSystem.Main.IsRaised(e);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Raise(this EventReference e, int add = 1) {
            EventSystem.Main.Raise(e, add);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCount(this EventReference e, int count) {
            EventSystem.Main.SetCount(e, count);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Raise<T>(this EventReference e, T data, int add = 1) {
            EventSystem.Main.Raise(e, data, add);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetCount<T>(this EventReference e, T data, int count) {
            EventSystem.Main.SetCount(e, data, count);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subscribe(this EventReference e, IEventListener listener) {
            EventSystem.Main.Subscribe(e, listener);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unsubscribe(this EventReference e, IEventListener listener) {
            EventSystem.Main.Unsubscribe(e, listener);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<T>(this EventReference e, IEventListener<T> listener) {
            EventSystem.Main.Subscribe(e, listener);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unsubscribe<T>(this EventReference e, IEventListener<T> listener) {
            EventSystem.Main.Unsubscribe(e, listener);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subscribe(this EventReference e, Action listener) {
            EventSystem.Main.Subscribe(e, listener);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unsubscribe(this EventReference e, Action listener) {
            EventSystem.Main.Unsubscribe(e, listener);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Subscribe<T>(this EventReference e, Action<T> listener) {
            EventSystem.Main.Subscribe(e, listener);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Unsubscribe<T>(this EventReference e, Action<T> listener) {
            EventSystem.Main.Unsubscribe(e, listener);
        }
    }

}
