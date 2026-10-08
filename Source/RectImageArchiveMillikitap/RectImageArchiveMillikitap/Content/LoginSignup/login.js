$(document).ready(function () {    
    initLangSelect();
    const loginText = document.querySelector(".title-text .login");
    const loginForm = document.querySelector("form.login");
    const loginBtn = document.querySelector("label.login");
    const signupBtn = document.querySelector("label.signup");
    const signupLink = document.querySelector("form .signup-link a");
    signupBtn.onclick = (() => {
        loginForm.style.marginLeft = "-50%";
        loginText.style.marginLeft = "-50%";
    });
    loginBtn.onclick = (() => {
        loginForm.style.marginLeft = "0%";
        loginText.style.marginLeft = "0%";
    });
    signupLink.onclick = (() => {
        signupBtn.click();
        return false;
    });
    $('form.signup').on('submit', function (e) {
        var email = $.trim($(this).find('[name=Username]').val() || '');
        if (!isAllowedRuEmail(email)) {
            e.preventDefault();
            alert('Допускаются только адреса на доменах .ru и .рф');
            return false;
        }
        if (!$(this).find('[name=PrivacyAccepted]').is(':checked')) {
            e.preventDefault();
            alert('Нужно согласие с политикой конфиденциальности');
            return false;
        }
    });
    mask(".tel_input");
});

function isAllowedRuEmail(email) {
    var match = email.match(/^[^@\s]+@([^@\s]+)$/);
    if (!match) {
        return false;
    }
    var host = match[1].toLowerCase();
    return host.endsWith('.ru') || host.endsWith('.рф') || host.endsWith('.xn--p1ai');
}

function initLangSelect() {
    var currentLang = getCookieParam('lng');
    var langSelect = $('.langSelect');
    if (currentLang != null && currentLang != undefined) {
        $(langSelect).val(currentLang);
        console.log(currentLang);
        $(langSelect).attr('data-lang', currentLang);
    }
    else {
        $(langSelect).attr('data-lang', 1);
    }
    $(langSelect).on('change', function () {
        var langId = $(this).val();
        $(langSelect).attr('data-lang', langId);
        setCookieParam('lng', parseInt(langId));
        location.reload();
    });    
}

function setCookieParam(p, v) {
    $.cookie(p, v, {
        expires: 360,
        path: '/'
    });
    location.reload();
};

function getCookieParam(p) {
    return $.cookie(p);
};