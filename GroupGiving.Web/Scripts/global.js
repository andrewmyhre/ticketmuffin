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
            function (data) {
                if (data.Success)
                    document.location.href = data.RedirectUrl;
                else {
                    $('#invalidCredentials').show();
                    $('form#signin input[type=submit]').attr('disabled', '');
                }
            },
            'json');
        $('form#signin input[type=submit]').attr('disabled', 'true');
        $('#invalidCredentials').hide();
        return false;

    });
});