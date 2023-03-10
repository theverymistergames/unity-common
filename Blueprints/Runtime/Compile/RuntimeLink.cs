namespace MisterGames.Blueprints.Compile {

    public readonly struct RuntimeLink {

        public readonly BlueprintNode node;
        public readonly int port;

        public RuntimeLink(BlueprintNode node, int port) {
            this.node = node;
            this.port = port;
        }

        public void Call() {
            if (node is IBlueprintEnter enter) enter.OnEnterPort(port);
        }
        public void Call<T>(T arg) {
            if (node is IBlueprintEnter<T> enter) enter.OnEnterPort(port, arg);
        }
        public void Call<T1, T2>(T1 arg1, T2 arg2) {
            if (node is IBlueprintEnter<T1, T2> enter) enter.OnEnterPort(port, arg1, arg2);
        }
        public void Call<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3) {
            if (node is IBlueprintEnter<T1, T2, T3> enter) enter.OnEnterPort(port, arg1, arg2, arg3);
        }
        public void Call<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            if (node is IBlueprintEnter<T1, T2, T3, T4> enter) enter.OnEnterPort(port, arg1, arg2, arg3, arg4);
        }
        public void Call<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            if (node is IBlueprintEnter<T1, T2, T3, T4, T5> enter) enter.OnEnterPort(port, arg1, arg2, arg3, arg4, arg5);
        }
        public void Call<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
            if (node is IBlueprintEnter<T1, T2, T3, T4, T5, T6> enter) enter.OnEnterPort(port, arg1, arg2, arg3, arg4, arg5, arg6);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
            if (node is IBlueprintEnter<T1, T2, T3, T4, T5, T6, T7> enter) enter.OnEnterPort(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
            if (node is IBlueprintEnter<T1, T2, T3, T4, T5, T6, T7, T8> enter) enter.OnEnterPort(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
            if (node is IBlueprintEnter<T1, T2, T3, T4, T5, T6, T7, T8, T9> enter) enter.OnEnterPort(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) {
            if (node is IBlueprintEnter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> enter) enter.OnEnterPort(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) {
            if (node is IBlueprintEnter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11> enter) enter.OnEnterPort(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) {
            if (node is IBlueprintEnter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12> enter) enter.OnEnterPort(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) {
            if (node is IBlueprintEnter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13> enter) enter.OnEnterPort(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) {
            if (node is IBlueprintEnter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14> enter) enter.OnEnterPort(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15) {
            if (node is IBlueprintEnter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15> enter) enter.OnEnterPort(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16) {
            if (node is IBlueprintEnter<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16> enter) enter.OnEnterPort(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        }

        public R Get<R>(R defaultValue = default) => node switch {
            IBlueprintOutput<R> outputR => outputR.GetOutputPortValue(port),
            IBlueprintOutput output => output.GetOutputPortValue<R>(port),
            _ => defaultValue
        };
        public R Get<T, R>(T arg, R defaultValue = default) {
            return node is IBlueprintOutput<T, R> output ? output.GetOutputPortValue(port, arg) : defaultValue;
        }
        public R Get<T1, T2, R>(T1 arg1, T2 arg2, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, R> output ? output.GetOutputPortValue(port, arg1, arg2) : defaultValue;
        }
        public R Get<T1, T2, T3, R>(T1 arg1, T2 arg2, T3 arg3, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, T3, R> output ? output.GetOutputPortValue(port, arg1, arg2, arg3) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, T3, T4, R> output ? output.GetOutputPortValue(port, arg1, arg2, arg3, arg4) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, T3, T4, T5, R> output ? output.GetOutputPortValue(port, arg1, arg2, arg3, arg4, arg5) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, T3, T4, T5, T6, R> output ? output.GetOutputPortValue(port, arg1, arg2, arg3, arg4, arg5, arg6) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, T3, T4, T5, T6, T7, R> output ? output.GetOutputPortValue(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, T3, T4, T5, T6, T7, T8, R> output ? output.GetOutputPortValue(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, T3, T4, T5, T6, T7, T8, T9, R> output ? output.GetOutputPortValue(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R> output ? output.GetOutputPortValue(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R> output ? output.GetOutputPortValue(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R> output ? output.GetOutputPortValue(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R> output ? output.GetOutputPortValue(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R> output ? output.GetOutputPortValue(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R> output ? output.GetOutputPortValue(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, R defaultValue = default) {
            return node is IBlueprintOutput<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, R> output ? output.GetOutputPortValue(port, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16) : defaultValue;
        }
    }

}
