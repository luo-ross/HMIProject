$(function () {
    $('.btn-register').on('click', function () {
        //尝试主动清除消息
        ClearMsg();
        var $email = $("#input-email");
        var $passwordinput = $("#input-password1");
        var $passwordconfirm = $("#input-password2");
        var $verify = $("#input-verify");
        //获取邮箱
        var email = $email.val();
        //获取密码输入
        var passwordinput = $passwordinput.val();
        //获取密码确认
        var passwordconfirm = $passwordconfirm.val();
        //获取验证码
        var verify = $verify.val();
        // 验证邮箱
        var emailRegex = /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/;
        if (isEmptyOrNull(email) || !emailRegex.test(email.toString())) {
            ShowWarningMsg("邮箱输入不正确?");
            $email.focus();
            return;
        }
        ;
        //验证密码输入
        if (isEmptyOrNull(passwordinput)) {
            ShowWarningMsg("密码输入不能为空?");
            $passwordinput.focus();
            return;
        }
        ;
        //验证密码确认输入
        if (isEmptyOrNull(passwordconfirm)) {
            ShowWarningMsg("密码确认输入不能为空?");
            $passwordconfirm.focus();
            return;
        }
        ;
        //验证密码是否相等
        if (!(passwordinput === passwordconfirm)) {
            ShowWarningMsg("2次输入密码不相同?");
            $passwordinput.focus();
            return;
        }
        ;
        //验证验证码输入
        if (isEmptyOrNull(verify)) {
            ShowWarningMsg("验证码输入不能为空?");
            $verify.focus();
            return;
        }
        ;
        //获取客户端Id
        //发起Ajax请求WebAPI请求
        $.ajax({
            url: "/SystemManage/Security/Register",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: toJson({
                email: $("#email").val()
            }),
            complete: function (e) {
            },
            success: function (operateResult) {
                if (!operateResult.IsSuccess) {
                    alert(operateResult.Message);
                }
            },
            error: function (e) {
            },
        });
    });
});
function isEmptyOrNull(str) {
    return str === null || str === undefined || $.trim(str) === '';
}
function ShowErrorMsg(msg) {
    ShowMsg('error', msg);
}
function ShowInfoMsg(msg) {
    ShowMsg('info', msg);
}
function ShowWarningMsg(msg) {
    ShowMsg('warning', msg);
}
function ShowDangerMsg(msg) {
    ShowMsg('danger', msg);
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
//# sourceMappingURL=register.js.map