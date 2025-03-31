$(function () {
    $(".btn-passnew").click(function () {
        $.ajax({
            url: "/password/reset",
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
//# sourceMappingURL=passwordnew.js.map