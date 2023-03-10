using System.Collections.Generic;

namespace MisterGames.Blueprints.Compile {

    public readonly struct RuntimePort {

        public readonly List<RuntimeLink> links;

        public RuntimePort(List<RuntimeLink> links) {
            this.links = links;
        }
        
        public void Call() {
            for (int i = 0; i < links.Count; i++) links[i].Call();
        }
        public void Call<T>(T arg) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg);
        }
        public void Call<T1, T2>(T1 arg1, T2 arg2) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2);
        }
        public void Call<T1, T2, T3>(T1 arg1, T2 arg2, T3 arg3) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2, arg3);
        }
        public void Call<T1, T2, T3, T4>(T1 arg1, T2 arg2, T3 arg3, T4 arg4) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2, arg3, arg4);
        }
        public void Call<T1, T2, T3, T4, T5>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2, arg3, arg4, arg5);
        }
        public void Call<T1, T2, T3, T4, T5, T6>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2, arg3, arg4, arg5, arg6);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15);
        }
        public void Call<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16) {
            for (int i = 0; i < links.Count; i++) links[i].Call(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16);
        }
        
        public R Get<R>(R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(defaultValue) : defaultValue;
        }
        public R Get<T, R>(T arg, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, R>(T1 arg1, T2 arg2, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, T3, R>(T1 arg1, T2 arg2, T3 arg3, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, arg3, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, arg3, arg4, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, arg3, arg4, arg5, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, arg3, arg4, arg5, arg6, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, arg3, arg4, arg5, arg6, arg7, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, defaultValue) : defaultValue;
        }
        public R Get<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, R>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16, R defaultValue = default) {
            return links.Count > 0 ? links[0].Get(arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8, arg9, arg10, arg11, arg12, arg13, arg14, arg15, arg16, defaultValue) : defaultValue;
        }
    }

}
