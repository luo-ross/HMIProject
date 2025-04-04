
var rSAPublicKey;
var rSAPrivateKey;



function generateRSAKeys() {
    var keyPair;
    var publicKey;
    var privateKey;
    // 生成RSA密钥对
  return    window.crypto.subtle.generateKey({
        name: "RSA-OAEP",
        modulusLength: 2048,
        publicExponent: new Uint8Array([1, 0, 1]), // 65537
        hash: "SHA-256",
    }, true, // 可导出
        ["encrypt", "decrypt"]).then(function (result) {
            keyPair = result;
            // 导出公钥 - 使用SPKI格式，与C#的ExportRSAPublicKey()对应
            return window.crypto.subtle.exportKey("spki", keyPair.publicKey);
        }).then(function (result) {
            publicKey = result;
            // 导出私钥 - 使用PKCS8格式，与C#的ExportRSAPrivateKey()对应
            return window.crypto.subtle.exportKey("pkcs8", keyPair.privateKey);
        }).then(function (result) {
            privateKey = result;
            // 转换为Base64格式，与C#的输出格式保持一致
            rSAPublicKey = btoa(String.fromCharCode.apply(null, new Uint8Array(publicKey)));
            rSAPrivateKey = btoa(String.fromCharCode.apply(null, new Uint8Array(privateKey)));


            // 将Date对象转换为Unix时间戳（毫秒）
            var timestamp = new Date().getTime();
            //创建会话请求
            var sessionRequestModel =
            {
                RsaPublicKey: rSAPublicKey,
                Nonce: createRandCode(10).toString(),
                TimeStamp: timestamp.toString(),
                AudienceType: "WebAudience",
                MsgSignature:"123"
            };


            //发起Ajax请求WebAPI请求
            $.ajax({
                url: "/api/v1/General/GetSessionModel",
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(sessionRequestModel),
                complete: function (e) {
                  
                },
                success: function (operateResult) {
                    if (!operateResult.IsSuccess) {
                        ShowErrorMsg(operateResult.Message)
                    }
                },
                error: function (e) {

                },
            });



        }).catch(function (error) {
            ShowErrorMsg('生成密钥对时出错：' + error.message);
        });
}


$(function () {

    generateRSAKeys();

    $('.btn-password-openclose').on("click", function () {
        var $parent = $(this).parent();
        var inputpassword = $parent.find('.form-input-password');
        var $svgeyeopen = $parent.find('#svg-eyeopen');
        var $svgeyeclose = $parent.find('#svg-eyeclose');
        var type = inputpassword.attr('type') == 'password' ? 'text' : 'password';
        inputpassword.attr('type', type);
        if (type === 'password') {
            $svgeyeopen.addClass('d-none')
            $svgeyeclose.removeClass('d-none');
        } else {
            $svgeyeopen.removeClass('d-none')
            $svgeyeclose.addClass('d-none');
        }
    })

    $('.label-tab').on('click', function () {
        var url = $(this).data('url');
        if (url) {
            window.location.href = url;
        }
    });
})

