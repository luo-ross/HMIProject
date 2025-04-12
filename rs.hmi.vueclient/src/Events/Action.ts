/**
 * 表示不带参数且不返回值的方法
 */
export type Action = () => void;

/**
 * 表示带一个参数且不返回值的方法
 */
export type Action1<T> = (arg: T) => void;

/**
 * 表示带两个参数且不返回值的方法
 */
export type Action2<T1, T2> = (arg1: T1, arg2: T2) => void;

/**
 * 表示带三个参数且不返回值的方法
 */
export type Action3<T1, T2, T3> = (arg1: T1, arg2: T2, arg3: T3) => void;

/**
 * 表示带四个参数且不返回值的方法
 */
export type Action4<T1, T2, T3, T4> = (arg1: T1, arg2: T2, arg3: T3, arg4: T4) => void;

/**
 * 表示带五个参数且不返回值的方法
 */
export type Action5<T1, T2, T3, T4, T5> = (arg1: T1, arg2: T2, arg3: T3, arg4: T4, arg5: T5) => void;

/**
 * 表示带六个参数且不返回值的方法
 */
export type Action6<T1, T2, T3, T4, T5, T6> = (arg1: T1, arg2: T2, arg3: T3, arg4: T4, arg5: T5, arg6: T6) => void;

/**
 * 表示带七个参数且不返回值的方法
 */
export type Action7<T1, T2, T3, T4, T5, T6, T7> = (arg1: T1, arg2: T2, arg3: T3, arg4: T4, arg5: T5, arg6: T6, arg7: T7) => void;

/**
 * 表示带八个参数且不返回值的方法
 */
export type Action8<T1, T2, T3, T4, T5, T6, T7, T8> = (arg1: T1, arg2: T2, arg3: T3, arg4: T4, arg5: T5, arg6: T6, arg7: T7, arg8: T8) => void;

/**
 * 表示带九个参数且不返回值的方法
 */
export type Action9<T1, T2, T3, T4, T5, T6, T7, T8, T9> = (arg1: T1, arg2: T2, arg3: T3, arg4: T4, arg5: T5, arg6: T6, arg7: T7, arg8: T8, arg9: T9) => void;

/**
 * 表示带十个参数且不返回值的方法
 */
export type Action10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10> = (arg1: T1, arg2: T2, arg3: T3, arg4: T4, arg5: T5, arg6: T6, arg7: T7, arg8: T8, arg9: T9, arg10: T10) => void; 
