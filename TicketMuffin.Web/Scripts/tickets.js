$(function () {

    $.ajaxSetup(
        { accept: 'application/json', dataType: 'json', contentType: 'application/json' }
    );

    $("#salesEndDate").datepicker({ dateFormat: dateFormat });

    $('#PayPalEmail').change(function () { VerifyPayPalAccount(null); });
    $('#PayPalFirstName').change(function () { VerifyPayPalAccount(null); });
    $('#PayPalLastName').change(function () { VerifyPayPalAccount(null); });
    $('#paypal-form').submit(function () {
        if (!verificationAttempted) {
            VerifyPayPalAccount(
                function () { $('#paypal-form').submit(); });
            return false;
        }
        return true;
    });

    if ($('#PayPalFirstName').val() + $('#PayPalLastName').val() + ($('#PayPalEmail').val()) != '') {
        VerifyPayPalAccount();
    } else {
        //$('#tickets-form input[type="submit"]').attr('disabled', 'disabled');
    }

    function VerifyPayPalAccount(onVerifiedCallback, onUnverifiedCallback, onNotFoundCallback) {
        $('#tickets-form input[type="submit"]').attr('disabled', 'disabled');
        var firstname = $('#PayPalFirstName').val();
        var lastname = $('#PayPalLastName').val();
        var email = $('#PayPalEmail').val();

        var data = '{"email":"' + email + '", "firstname":"' + firstname + '", "lastname":"' + lastname + '"}';

        $.post('/api/accounts/verify-paypal', data)
            .success(function (response) {
                if (response.Success) {
                    if (response.AccountStatus == "VERIFIED")
                        Verified( onVerifiedCallback);
                    else if (response.AccountStatus == "UNVERIFIED")
                        Unverified(onUnverifiedCallback);
                } else {
                    NotFound();
                }
            })
            .error(function() {
                NotFound(onNotFoundCallback);
            });
    }

    function Verified(onVerifiedCallback) {
        $('#pp-verify-result').html('<span style="color:lime">Valid!</span>');
        verificationAttempted = true;
        $('#tickets-form input[type="submit"]').attr('disabled', false);
        $('#cannot-verify-message').hide();
        if (onVerifiedCallback) {
            onVerifiedCallback();
        }
    }

    function Unverified(onUnverifiedCallback) {
        $('#pp-verify-result').html('<span style="color:lime">Your account exists but is unverified.</span>');
        $('#tickets-form input[type="submit"]').attr('disabled', false);
        verificationAttempted = true;
        if (onUnverifiedCallback)
            onUnverifiedCallback();
    }

    function NotFound(onNotFoundCallback) {
        $('#pp-verify-result').html('<span style="color:red">A PayPal account matching those credentials could not be found. Please ensure that the name and email address provided match the details for your PayPal account.</span>');
        $('#paypal-name-container').slideDown();
        verificationAttempted = true;
        $('#tickets-form input[type="submit"]').attr('disabled', false);
        $('#cannot-verify-message').show();
        if (onNotFoundCallback) {
            onNotFoundCallback();
        }
    }
});
