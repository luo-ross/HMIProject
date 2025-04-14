/**
 * 表示一个不带参数但有返回值的方法
 * @template TResult 返回值类型
 */
export type Func<TResult> = () => TResult;

/**
 * 表示一个带一个参数和返回值的方法
 * @template T 参数类型
 * @template TResult 返回值类型
 */
export type Func1<T, TResult> = (arg: T) => TResult;

/**
 * 表示一个带两个参数和返回值的方法
 * @template T1 第一个参数类型
 * @template T2 第二个参数类型
 * @template TResult 返回值类型
 */
export type Func2<T1, T2, TResult> = (arg1: T1, arg2: T2) => TResult;

/**
 * 表示一个带三个参数和返回值的方法
 * @template T1 第一个参数类型
 * @template T2 第二个参数类型
 * @template T3 第三个参数类型
 * @template TResult 返回值类型
 */
export type Func3<T1, T2, T3, TResult> = (arg1: T1, arg2: T2, arg3: T3) => TResult;

/**
 * 表示一个带四个参数和返回值的方法
 * @template T1 第一个参数类型
 * @template T2 第二个参数类型
 * @template T3 第三个参数类型
 * @template T4 第四个参数类型
 * @template TResult 返回值类型
 */
export type Func4<T1, T2, T3, T4, TResult> = (arg1: T1, arg2: T2, arg3: T3, arg4: T4) => TResult;

/**
 * 表示一个带五个参数和返回值的方法
 * @template T1 第一个参数类型
 * @template T2 第二个参数类型
 * @template T3 第三个参数类型
 * @template T4 第四个参数类型
 * @template T5 第五个参数类型
 * @template TResult 返回值类型
 */
export type Func5<T1, T2, T3, T4, T5, TResult> = (arg1: T1, arg2: T2, arg3: T3, arg4: T4, arg5: T5) => TResult;

/**
 * 表示一个带六个参数和返回值的方法
 * @template T1 第一个参数类型
 * @template T2 第二个参数类型
 * @template T3 第三个参数类型
 * @template T4 第四个参数类型
 * @template T5 第五个参数类型
 * @template T6 第六个参数类型
 * @template TResult 返回值类型
 */
export type Func6<T1, T2, T3, T4, T5, T6, TResult> = (arg1: T1, arg2: T2, arg3: T3, arg4: T4, arg5: T5, arg6: T6) => TResult;

/**
 * 表示一个带七个参数和返回值的方法
 * @template T1 第一个参数类型
 * @template T2 第二个参数类型
 * @template T3 第三个参数类型
 * @template T4 第四个参数类型
 * @template T5 第五个参数类型
 * @template T6 第六个参数类型
 * @template T7 第七个参数类型
 * @template TResult 返回值类型
 */
export type Func7<T1, T2, T3, T4, T5, T6, T7, TResult> = (arg1: T1, arg2: T2, arg3: T3, arg4: T4, arg5: T5, arg6: T6, arg7: T7) => TResult;

/**
 * 表示一个带八个参数和返回值的方法
 * @template T1 第一个参数类型
 * @template T2 第二个参数类型
 * @template T3 第三个参数类型
 * @template T4 第四个参数类型
 * @template T5 第五个参数类型
 * @template T6 第六个参数类型
 * @template T7 第七个参数类型
 * @template T8 第八个参数类型
 * @template TResult 返回值类型
 */
export type Func8<T1, T2, T3, T4, T5, T6, T7, T8, TResult> = (arg1: T1, arg2: T2, arg3: T3, arg4: T4, arg5: T5, arg6: T6, arg7: T7, arg8: T8) => TResult;

/**
 * 表示一个带九个参数和返回值的方法
 * @template T1 第一个参数类型
 * @template T2 第二个参数类型
 * @template T3 第三个参数类型
 * @template T4 第四个参数类型
 * @template T5 第五个参数类型
 * @template T6 第六个参数类型
 * @template T7 第七个参数类型
 * @template T8 第八个参数类型
 * @template T9 第九个参数类型
 * @template TResult 返回值类型
 */
export type Func9<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> = (arg1: T1, arg2: T2, arg3: T3, arg4: T4, arg5: T5, arg6: T6, arg7: T7, arg8: T8, arg9: T9) => TResult;

/**
 * 表示一个带十个参数和返回值的方法
 * @template T1 第一个参数类型
 * @template T2 第二个参数类型
 * @template T3 第三个参数类型
 * @template T4 第四个参数类型
 * @template T5 第五个参数类型
 * @template T6 第六个参数类型
 * @template T7 第七个参数类型
 * @template T8 第八个参数类型
 * @template T9 第九个参数类型
 * @template T10 第十个参数类型
 * @template TResult 返回值类型
 */
export type Func10<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> = (arg1: T1, arg2: T2, arg3: T3, arg4: T4, arg5: T5, arg6: T6, arg7: T7, arg8: T8, arg9: T9, arg10: T10) => TResult; 
