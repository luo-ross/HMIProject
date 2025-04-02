"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.validatePassword = void 0;
function validatePassword() {
    var _this = this;
    $('#btn-password-openclose').click(function (e) {
        var inputpassword = $(_this).parent('.form-input-border').find('.form-input-password');
        var type = inputpassword.attr('type') == 'password' ? 'text' : 'password';
        inputpassword.attr('type', type);
        if (type === 'password') {
            $('#svg-eyeopen').addClass('d-none');
            $('#svg-eyeclose').removeClass('d-none');
        }
        else {
            $('#svg-eyeopen').removeClass('d-none');
            $('#svg-eyeclose').addClass('d-none');
        }
    });
}
exports.validatePassword = validatePassword;
//# sourceMappingURL=inputpassword.js.map