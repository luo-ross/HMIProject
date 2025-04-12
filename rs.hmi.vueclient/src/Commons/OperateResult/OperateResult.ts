/**
 * 操作结果基类
 */
export class BaseOperateResult {
    /**
     * 操作是否成功
     */
    public IsSuccess: boolean = false;

    /**
     * 消息
     */
    public Message: string = '';

    /**
     * 错误代码
     * 0-100_0000 代表warning 可以返回给用户
     * 100_0000以后 代表异常 不可以返回给用户
     */
    public ErrorCode: number = 0;

    /**
     * 构造函数
     */
    constructor();
    /**
     * 构造函数
     * @param errorCode 错误码
     * @param message 错误消息
     */
    constructor(errorCode: number, message: string);
    /**
     * 构造函数
     * @param message 错误消息
     */
    constructor(message: string);
    constructor(errorCodeOrMessage?: number | string, message?: string) {
        if (typeof errorCodeOrMessage === 'number' && message) {
            this.ErrorCode = errorCodeOrMessage;
            this.Message = message;
        } else if (typeof errorCodeOrMessage === 'string') {
            this.Message = errorCodeOrMessage;
        }
    }
}

/**
 * 非泛型操作结果类
 */
export class SimpleOperateResult extends BaseOperateResult {
    /**
     * 创建成功结果
     * @returns 操作结果
     */
    public static CreateSuccessResult(): SimpleOperateResult {
        const result = new SimpleOperateResult();
        result.IsSuccess = true;
        result.Message = 'OK';
        return result;
    }

    /**
     * 创建失败结果
     * @param message 可选，失败消息
     * @returns 操作结果
     */
    public static CreateFailResult(message?: string): SimpleOperateResult {
        return message ? new SimpleOperateResult(message) : new SimpleOperateResult();
    }
}

/**
 * 泛型操作结果
 * @typeparam T 数据类型
 */
export class GenericOperateResult<T> extends BaseOperateResult {
    /**
     * 返回的泛型数据
     */
    public Data: T | null = null;

    /**
     * 构造函数
     */
    constructor();
    /**
     * 构造函数
     * @param errorCode 错误代码
     * @param message 错误消息
     */
    constructor(errorCode: number, message: string);
    /**
     * 构造函数
     * @param message 错误消息
     */
    constructor(message: string);
    constructor(errorCodeOrMessage?: number | string, message?: string) {
        if (typeof errorCodeOrMessage === 'number' && message) {
            super(errorCodeOrMessage, message);
        } else if (typeof errorCodeOrMessage === 'string') {
            super(errorCodeOrMessage);
        } else {
            super();
        }
    }

    /**
     * 创建成功结果
     * @param data 数据
     * @returns 操作结果
     */
    public static CreateSuccessResult<T>(data: T): GenericOperateResult<T> {
        const result = new GenericOperateResult<T>();
        result.IsSuccess = true;
        result.Message = 'OK';
        result.Data = data;
        return result;
    }

    /**
     * 创建失败结果
     * @param errorInfo 错误信息，可以是消息字符串或错误操作结果
     * @returns 操作结果
     */
    public static CreateFailResult<T>(errorInfo?: string | BaseOperateResult): GenericOperateResult<T> {
        const result = new GenericOperateResult<T>();
        
        if (typeof errorInfo === 'string') {
            result.Message = errorInfo;
        } else if (errorInfo instanceof BaseOperateResult) {
            result.IsSuccess = errorInfo.IsSuccess;
            result.ErrorCode = errorInfo.ErrorCode;
            result.Message = errorInfo.Message;
        }
        
        return result;
    }
} 