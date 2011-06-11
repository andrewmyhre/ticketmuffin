$(document).ready(function () {
    $('#showSignIn').click(function () {
        $('#signInForm').show();
        $("#signInForm input:first").focus();
        $('#signInForm .hide').click(function () { $('#signInForm').hide(); });
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