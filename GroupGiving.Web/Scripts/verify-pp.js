var paypalVerified = false;
$(document).ready(function () {
    $.ajaxSetup(
        { accept: 'application/json', dataType: 'json', contentType: 'application/json' }
    );

    $('#PayPalEmail').change(function () { VerifyPayPalAccount(null); });
    $('#PayPalFirstName').change(function () { VerifyPayPalAccount(null); });
    $('#PayPalLastName').change(function () { VerifyPayPalAccount(null); });
    $('#paypal-form').submit(function () {
        if (!paypalVerified) {
            VerifyPayPalAccount(function () { $('#paypal-form').submit(); });
            return false;
        }
        return true;
    });

    if ($('#PayPalFirstName').val() + $('#PayPalLastName').val() + ($('#PayPalEmail').val()) != '') {
        VerifyPayPalAccount(null);
    }
    $('#paypal-form input[type="submit"]').attr('disabled', 'disabled');
});

function VerifyPayPalAccount(onVerified) {
    $('#paypal-form input[type="submit"]').attr('disabled', 'disabled');
    var firstname = $('#PayPalFirstName').val();
    var lastname = $('#PayPalLastName').val();
    var email = $('#PayPalEmail').val();

    var data = '{"email":"' + email + '", "firstname":"' + firstname + '", "lastname":"' + lastname + '"}';

    $('#pp-verify-result').html('We are checking your PayPal credentials');
    $('#pp-verify-result').removeClass('pp-notfound pp-unverified pp-verified');
    $('#pp-verify-result').addClass('pp-checking');

    $.post('/api/accounts/verify-paypal', data)
            .success(function (response) {
                if (response.Success) {
                    if (response.AccountStatus == 'VERIFIED') {
                        $('#pp-verify-result').html('These settings match a valid PayPal account');
                        $('#pp-verify-result').removeClass('pp-notfound pp-unverified pp-checking');
                        $('#pp-verify-result').addClass('pp-verified');
                    } else if (response.AccountStatus == 'UNVERIFIED') {
                        $('#pp-verify-result').html('The PayPal account matching these settings is unverified.');
                        $('#pp-verify-result').removeClass('pp-notfound pp-verified pp-checking');
                        $('#pp-verify-result').addClass('pp-unverified');
                    }
                    paypalVerified = true;
                    $('#paypal-form input[type="submit"]').attr('disabled', false);
                    if (onVerified) {
                        onVerified();
                    }
                } else {
                    $('#pp-verify-result').html('A PayPal account matching these credentials does not exist. You can create an account online at <a href="https://www.paypal.com/uk/cgi-bin/webscr?cmd=_registration-run" target="_blank">PayPal.com</a>');
                    $('#paypal-name-container').slideDown();
                    paypalVerified = false;
                    $('#paypal-form input[type="submit"]').attr('disabled', 'disabled');
                    $('#pp-verify-result').removeClass('pp-unverified pp-verified pp-checking');
                    $('#pp-verify-result').addClass('pp-notfound');
                }
            })
            .error(function () { });
}