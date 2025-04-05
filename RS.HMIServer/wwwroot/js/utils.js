var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
function toJson(obj) {
    return JSON.stringify(obj);
}
function getQueryParam(name) {
    // 获取URL的查询字符串部分，并去除开头的?  
    let queryString = window.location.search.substring(1);
    // 将查询字符串分割成键值对数组  
    let params = queryString.split('&').map(pair => pair.split('='));
    // 遍历键值对数组，找到指定的参数名并返回其值  
    for (let [key, value] of params) {
        if (decodeURIComponent(key) === name) {
            return decodeURIComponent(value) || null; // 返回解码后的值，如果没有值则返回null  
        }
    }
    return null; // 如果没有找到指定的参数，则返回null  
}
function AjaxPostAsync(url, model, success, complete, error) {
    return __awaiter(this, void 0, void 0, function* () {
        return yield $.ajax({
            url: url,
            type: "POST",
            contentType: "application/json",
            data: JSON.stringify(model),
            complete: function (e) {
                return __awaiter(this, void 0, void 0, function* () {
                    return yield (complete === null || complete === void 0 ? void 0 : complete(e));
                });
            },
            success: function (operateResult) {
                return __awaiter(this, void 0, void 0, function* () {
                    return yield (success === null || success === void 0 ? void 0 : success(operateResult));
                });
            },
            error: function (e) {
                return __awaiter(this, void 0, void 0, function* () {
                    return yield (error === null || error === void 0 ? void 0 : error(e));
                });
            },
        });
    });
}
// 使用jQuery的Ajax与async/await结合
function makeAjaxRequestAsync(url, data) {
    return __awaiter(this, void 0, void 0, function* () {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: url,
                type: 'POST',
                data: JSON.stringify(data),
                contentType: 'application/json',
                success: resolve,
                error: reject
            });
        });
    });
}
function createRandCode(len) {
    var randomString = '';
    // 生成指定长度的随机数字字符串
    for (var i = 0; i < len; i++) {
        // 生成0到9之间的随机数
        var randomDigit = Math.floor(Math.random() * 10);
        randomString += randomDigit;
    }
    return randomString;
}
function emailValid(email) {
    const emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
    if (isEmptyOrNull(email) || !emailRegex.test(email.toString())) {
        return false;
    }
    ;
    return true;
}
function isEmptyOrNull(str) {
    return str === null || str === undefined || $.trim(str) === '';
}
function ShowErrorMsg(msg) {
    ShowMsg('danger', msg);
}
function ShowInfoMsg(msg) {
    ShowMsg('info', msg);
}
function ShowWarningMsg(msg) {
    ShowMsg('warning', msg);
}
function ShowSuccessMsg(msg) {
    ShowMsg('success', msg);
}
function ClearMsg() {
    if (timerId > 0) {
        clearTimeout(timerId);
        timerId = -1;
    }
    var $errormessage = $('body').find(".error-message");
    $errormessage.removeAttr('class');
    $errormessage.attr('class', 'error-message d-none');
}
let timerId;
function ShowMsg(msgtype, msg) {
    var $errormessage = $('body').find(".error-message");
    if ($errormessage.length == 0) {
        return;
    }
    $errormessage.removeAttr('class');
    $errormessage.attr('class', 'error-message alert-' + msgtype);
    $errormessage.text(msg);
    if (timerId > 0) {
        clearTimeout(timerId);
        timerId = -1;
    }
    timerId = setTimeout(function () {
        $errormessage.removeAttr('class');
        $errormessage.attr('class', 'error-message d-none');
        timerId = -1;
    }, 3000);
}
//# sourceMappingURL=utils.js.map