var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
/**
 * 通用工具类
 */
export class CommonUtils {
    /**
     * 对象转JSON字符串
     */
    static toJson(obj) {
        return JSON.stringify(obj);
    }
    /**
     * 获取URL查询参数
     */
    static getQueryParam(name) {
        const queryString = window.location.search.substring(1);
        const params = new URLSearchParams(queryString);
        return params.get(name);
    }
    /**
     * 发送POST请求
     */
    static ajaxPost(url, model, success, complete, error) {
        return __awaiter(this, void 0, void 0, function* () {
            let response;
            try {
                const response = yield fetch(url, {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json'
                    },
                    body: JSON.stringify(model)
                });
                const data = yield response.json();
                if (success) {
                    return yield success(data);
                }
                return {
                    isSuccess: true,
                    data: data,
                    message: ''
                };
            }
            catch (err) {
                if (error) {
                    return yield error(err);
                }
                return {
                    isSuccess: false,
                    data: null,
                    message: '请求失败'
                };
            }
            finally {
                if (complete) {
                    return yield complete(response);
                }
            }
        });
    }
    /**
     * 生成随机数字字符串
     */
    static createRandCode(len) {
        return Array.from({ length: len }, () => Math.floor(Math.random() * 10)).join('');
    }
    /**
     * 邮箱验证
     */
    static emailValid(email) {
        const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        return !this.isEmptyOrNull(email) && emailRegex.test(email);
    }
    /**
     * 判断字符串是否为空
     */
    static isEmptyOrNull(str) {
        return str === null || str === undefined || str.toString().trim() === '';
    }
    /**
     * 显示错误消息
     */
    static showErrorMsg(msg) {
        this.showMsg('error', msg);
    }
    /**
     * 显示信息消息
     */
    static showInfoMsg(msg) {
        this.showMsg('info', msg);
    }
    /**
     * 显示警告消息
     */
    static showWarningMsg(msg) {
        this.showMsg('warning', msg);
    }
    /**
     * 显示成功消息
     */
    static showSuccessMsg(msg) {
        this.showMsg('success', msg);
    }
    /**
     * 清除消息
     */
    static clearMsg() {
        if (this.timerId > 0) {
            clearTimeout(this.timerId);
            this.timerId = -1;
        }
        const messageElement = document.querySelector('.error-message');
        if (messageElement) {
            messageElement.className = 'error-message d-none';
        }
    }
    /**
     * 显示消息
     */
    static showMsg(type, msg) {
        const messageElement = document.querySelector('.error-message');
        if (!messageElement)
            return;
        // 清除之前的定时器
        if (this.timerId > 0) {
            clearTimeout(this.timerId);
            this.timerId = -1;
        }
        // 设置消息样式和内容
        messageElement.className = `error-message alert-${type}`;
        messageElement.textContent = msg;
        // 设置新的定时器
        this.timerId = window.setTimeout(() => {
            messageElement.className = 'error-message d-none';
            this.timerId = -1;
        }, 3000);
    }
}
CommonUtils.timerId = -1;
//# sourceMappingURL=utils.js.map