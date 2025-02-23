$(function () {
    //$(".form").validate({
    //    rules: {
    //        password1: {
    //            required: true,
    //            minlength: 8
    //        },
    //        password2: {
    //            required: true,
    //            minlength: 8,
    //            equalTo: "#password1"
    //        },
    //    },
    //    messages: {
    //        password2: {
    //            equalTo: "2次输入密码不一致"
    //        },
    //    }
    //});
    $(".btn-passconfirm").click(function () {
        debugger;
        var form = $(".form");
        if (!form.valid()) {
            return false;
        }
        var password = $("#password1").val();
        password = CryptoJS.MD5(password.toString()).toString();
        var email = getQueryParam("email");
        var token = getQueryParam("token");
        $.ajax({
            url: "/password/confirm",
            type: "POST",
            contentType: "application/json",
            dataType: "json",
            data: toJson({
                password: password,
                email: email,
                token: token
            }),
            complete: function (e) {
            },
            success: function (operateResult) {
                if (operateResult.IsSuccess) {
                    alert(operateResult.Message);
                }
            },
            error: function (e) {
            },
        });
    });
});
//# sourceMappingURL=passwordedit.js.map