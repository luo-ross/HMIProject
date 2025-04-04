function toJson(obj) {
    return JSON.stringify(obj);
}
function getQueryParam(name) {
    // 获取URL的查询字符串部分，并去除开头的?  
    var queryString = window.location.search.substring(1);
    // 将查询字符串分割成键值对数组  
    var params = queryString.split('&').map(function (pair) { return pair.split('='); });
    // 遍历键值对数组，找到指定的参数名并返回其值  
    for (var _i = 0, params_1 = params; _i < params_1.length; _i++) {
        var _a = params_1[_i], key = _a[0], value = _a[1];
        if (decodeURIComponent(key) === name) {
            return decodeURIComponent(value) || null; // 返回解码后的值，如果没有值则返回null  
        }
    }
    return null; // 如果没有找到指定的参数，则返回null  
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
    var emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
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
var timerId;
function ShowMsg(msgtype, msg) {
    var $errormessage = $('body').find(".error-message");
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