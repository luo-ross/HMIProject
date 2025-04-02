$(function () {
    $('.btn-password-openclose').on("click", function () {
        var $parent = $(this).parent();
        var inputpassword = $parent.find('.form-input-password');
        var $svgeyeopen = $parent.find('#svg-eyeopen');
        var $svgeyeclose = $parent.find('#svg-eyeclose');
        var type = inputpassword.attr('type') == 'password' ? 'text' : 'password';
        inputpassword.attr('type', type);
        if (type === 'password') {
            $svgeyeopen.addClass('d-none');
            $svgeyeclose.removeClass('d-none');
        }
        else {
            $svgeyeopen.removeClass('d-none');
            $svgeyeclose.addClass('d-none');
        }
    });
    $('.label-tab').on('click', function () {
        var url = $(this).data('url');
        if (url) {
            window.location.href = url;
        }
    });
});
//# sourceMappingURL=login.js.map