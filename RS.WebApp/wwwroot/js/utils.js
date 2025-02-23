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
//# sourceMappingURL=utils.js.map