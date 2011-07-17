$(document).ready(function () {

    $('.emailTest')
        .click(function (event) {
            var templateName = $(event.target).attr('templateName');
            var href = $(event.target).attr('href');
            
            $.post(href,
                'toAddress=' + $('#toAddress').val() + '&emailTemplate=' + templateName,
                function (data) {
                    alert('email sent');
                });
            
            return false;
        });

});