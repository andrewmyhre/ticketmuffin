var verificationAttempted = false;
$(document).ready(function () {
    $.ajaxSetup(
        { accept: 'application/json', dataType: 'json', contentType: 'application/json' }
    );

    $('#PayPalEmail').change(function () { VerifyPayPalAccount(null); });
    $('#PayPalFirstName').change(function () { VerifyPayPalAccount(null); });
    $('#PayPalLastName').change(function () { VerifyPayPalAccount(null); });
    $('.verify-paypal').click(function() { VerifyPayPalAccount(null); });
    $('#paypal-form').submit(function () {
        if (!verificationAttempted) {
            VerifyPayPalAccount(function () { $('#paypal-form').submit(); });
            return false;
        }
        return true;
    });

    if ($('#PayPalFirstName').val() + $('#PayPalLastName').val() + ($('#PayPalEmail').val()) != '') {
        //VerifyPayPalAccount(null);
    } else {
        $('#paypal-form input[type="submit"]').attr('disabled', 'disabled');
    }
});

function VerifyPayPalAccount(onVerified) {
    $('#paypal-form input[type="submit"]').attr('disabled', 'disabled');
    var firstname = $('#PayPalFirstName').val();
    var lastname = $('#PayPalLastName').val();
    var email = $('#PayPalEmail').val();

    if (firstname=='' || lastname=='' || email=='')
        return;

    var data = "?email=" + email + "&firstname=" + firstname + "&lastname=" + lastname;

    $('#pp-verify-result').html('We are checking your PayPal credentials');
    $('#pp-verify-result').removeClass('pp-notfound pp-unverified pp-verified');
    $('#pp-verify-result').addClass('pp-checking');

    $.post('/api/accounts/verify-paypal' + data)
            .success(function (response) {
                if (response.Success) {
                    if (response.Verified) {
                        $('#pp-verify-result').html('These settings match a valid PayPal account');
                        $('#pp-verify-result').removeClass('pp-notfound pp-unverified pp-checking');
                        $('#pp-verify-result').addClass('pp-verified');
                    } else {
                        $('#pp-verify-result').html('We could not verify your PayPal account.');
                        $('#pp-verify-result').removeClass('pp-notfound pp-verified pp-checking');
                        $('#pp-verify-result').removeClass('pp-unverified pp-verified pp-checking');
                        $('#pp-verify-result').addClass('pp-notfound');
                    }
                    verificationAttempted = true;
                    $('#paypal-form input[type="submit"]').attr('disabled', false);
                    if (onVerified) {
                        onVerified();
                    }
                }
            })
                .error(function () { });
}