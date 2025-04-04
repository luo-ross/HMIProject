$(function () {
    var $email = $("#input-email");
    var $email = $("#input-email");
    var $passwordinput = $("#input-password1");
    var $passwordconfirm = $("#input-password2");
    var $verify = $("#input-verify");
    $('.btn-register-next').on('click', function () {
        //获取邮箱
        var email = $email.val();
        //获取密码输入
        var passwordinput = $passwordinput.val();
        //获取密码确认
        var passwordconfirm = $passwordconfirm.val();
        //获取验证码
        var verify = $verify.val();
        // 验证邮箱
        if (!emailValid(email)) {
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
            data: JSON.stringify({}),
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
    $('.btn-verifyrequest').click(function () {
        //获取邮箱
        var email = $email.val();
        // 验证邮箱
        if (!emailValid(email)) {
            ShowWarningMsg("邮箱输入不正确或者未输入?");
            $email.focus();
            return;
        }
        ;
        //发起Ajax请求WebAPI请求
        $.ajax({
            url: "/SystemManage/Security/GetEmailVerify",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: JSON.stringify({}),
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
//# sourceMappingURL=register.js.map