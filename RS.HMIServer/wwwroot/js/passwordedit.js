$(function () {
    $(".btn-passconfirm").click(() => {
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