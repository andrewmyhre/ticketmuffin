$(document).ready(function () {
    $('#showSignIn').click(function () {
        $('#popup-signIn').show();
        $("#popup-signIn input:first").focus();
        $('#popup-signIn .hide').click(function () { $('#popup-signIn').hide(); });
    });

    $('form#signin')
    .submit(function () {
        $.post('/account/signin',
            $('form#signin').serialize(),
            onLoginRequestComplete,
            'json');
        $('form#signin input[type=submit]').attr('disabled', 'true');
        $('#login-progress').show();
        $('#invalidCredentials').hide();
        return false;

    });
    
    function onLoginRequestComplete(data) {
        $('#login-progress').hide();
        $('form#signin input[type=submit]').removeAttr('disabled');
        if (data.Success)
            document.location.href = data.RedirectUrl;
        else {
            $('#invalidCredentials').show();
        }
    }
});