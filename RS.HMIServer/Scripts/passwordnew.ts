$(function () {
    $(".btn-passnew").click(() => {
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
})


