/**
 * 缓存主键
 */
export class MemoryCacheKey {
    private static _initialized = false;

    /**
     * RSA全局公钥
     */
    public static readonly GlobalRSASignPublicKey: string;

    /**
     * RSA全局私钥
     */
    public static readonly GlobalRSASignPrivateKey: string;

    /**
     * RSA全局公钥
     */
    public static readonly GlobalRSAEncryptPublicKey: string;

    /**
     * RSA全局私钥
     */
    public static readonly GlobalRSAEncryptPrivateKey: string;

    /**
     * 会话
     */
    public static readonly SessionModelKey: string;

    /**
     * 服务端RSA全局公钥
     */
    public static readonly ServerGlobalRSASignPublicKey: string;

    /**
     * 服务端RSA全局私钥
     */
    public static readonly ServerGlobalRSAEncryptPublicKey: string;

    /**
     * 初始化静态字段
     */
    private static initialize(): void {
        if (MemoryCacheKey._initialized) {
            return;
        }

        // 使用 Object.defineProperty 来模拟 readonly 行为
        Object.defineProperty(MemoryCacheKey, 'GlobalRSASignPublicKey', {
            value: crypto.randomUUID(),
            writable: false,
            configurable: false
        });

        Object.defineProperty(MemoryCacheKey, 'GlobalRSASignPrivateKey', {
            value: crypto.randomUUID(),
            writable: false,
            configurable: false
        });

        Object.defineProperty(MemoryCacheKey, 'GlobalRSAEncryptPublicKey', {
            value: crypto.randomUUID(),
            writable: false,
            configurable: false
        });

        Object.defineProperty(MemoryCacheKey, 'GlobalRSAEncryptPrivateKey', {
            value: crypto.randomUUID(),
            writable: false,
            configurable: false
        });

        Object.defineProperty(MemoryCacheKey, 'SessionModelKey', {
            value: crypto.randomUUID(),
            writable: false,
            configurable: false
        });

        Object.defineProperty(MemoryCacheKey, 'ServerGlobalRSASignPublicKey', {
            value: crypto.randomUUID(),
            writable: false,
            configurable: false
        });

        Object.defineProperty(MemoryCacheKey, 'ServerGlobalRSAEncryptPublicKey', {
            value: crypto.randomUUID(),
            writable: false,
            configurable: false
        });

        MemoryCacheKey._initialized = true;
    }

    /**
     * 静态构造函数
     */
    static {
        MemoryCacheKey.initialize();
    }
} 