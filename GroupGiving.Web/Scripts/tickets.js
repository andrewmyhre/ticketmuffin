$(function () {

    $.ajaxSetup(
        { accept: 'application/json', dataType: 'json', contentType: 'application/json' }
    );

    $("#salesEndDate").datepicker({ dateFormat: 'dd/mm/yy' });

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
    } else {
        $('#tickets-form input[type="submit"]').attr('disabled', 'disabled');
    }
    

});

function VerifyPayPalAccount(onVerified) {
    $('#tickets-form input[type="submit"]').attr('disabled', 'disabled');
    var firstname = $('#PayPalFirstName').val();
    var lastname = $('#PayPalLastName').val();
    var email = $('#PayPalEmail').val();

    var data = '{"email":"' + email + '", "firstname":"' + firstname + '", "lastname":"' + lastname + '"}';

    $.post('/api/accounts/verify-paypal', data)
            .success(function (response) {
                if (response.Success) {
                    if (response.AccountStatus == 'VERIFIED') {
                        $('#pp-verify-result').html('<span style="color:lime">Valid!</span>');
                    } else if (response.AccountStatus == 'UNVERIFIED') {
                        $('#pp-verify-result').html('<span style="color:lime">Your account exists but is unverified.</span>');
                    }
                    paypalVerified = true;
                    $('#tickets-form input[type="submit"]').attr('disabled', false);
                    if (onVerified) {
                        onVerified();
                    }
                } else {
                    $('#pp-verify-result').html('<span style="color:red">A PayPal account matching those credentials could not be found. Please ensure that the name and email address provided match the details for your PayPal account.</span>');
                    $('#paypal-name-container').slideDown();
                    paypalVerified = false;
                    $('#tickets-form input[type="submit"]').attr('disabled', 'disabled');
                }
            })
            .error(function () { });
}