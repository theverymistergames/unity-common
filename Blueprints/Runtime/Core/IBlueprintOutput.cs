namespace MisterGames.Blueprints {

    public interface IBlueprintOutput {
        R GetOutputPortValue<R>(int port);
    }

    public interface IBlueprintOutput<out R> {
        R GetOutputPortValue(int port);
    }
    
    public interface IBlueprintOutput<in T, out R> {
        R GetOutputPortValue(int port, T arg);
    }
    
    public interface IBlueprintOutput<in T1, in T2, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2);
    }

    public interface IBlueprintOutput<in T1, in T2, in T3, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2, T3 arg3);
    }

    public interface IBlueprintOutput<in T1, in T2, in T3, in T4, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2, T3 arg3, T4 arg4);
    }

    public interface IBlueprintOutput<in T1, in T2, in T3, in T4, in T5, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);
    }

    public interface IBlueprintOutput<in T1, in T2, in T3, in T4, in T5, in T6, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6);
    }

    public interface IBlueprintOutput<in T1, in T2, in T3, in T4, in T5, in T6, in T7, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7);
    }

    public interface IBlueprintOutput<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8);
    }
    
    public interface IBlueprintOutput<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9);
    }
    
    public interface IBlueprintOutput<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10);
    }
    
    public interface IBlueprintOutput<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11);
    }
    
    public interface IBlueprintOutput<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12);
    }
    
    public interface IBlueprintOutput<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13);
    }
    
    public interface IBlueprintOutput<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14);
    }
    
    public interface IBlueprintOutput<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15);
    }
    
    public interface IBlueprintOutput<in T1, in T2, in T3, in T4, in T5, in T6, in T7, in T8, in T9, in T10, in T11, in T12, in T13, in T14, in T15, in T16, out R> {
        R GetOutputPortValue(int port, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8, T9 arg9, T10 arg10, T11 arg11, T12 arg12, T13 arg13, T14 arg14, T15 arg15, T16 arg16);
    }
    
}
