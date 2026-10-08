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
    mask(".tel_input");
});

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