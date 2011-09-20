$(document).ready(function () {
    var facebookHooks = new FacebookHooks();
    facebookHooks.bindToUi();

    $.ajaxSetup(
        { accept: 'application/json', dataType: 'json', contentType: 'application/json' }
    );

    $('#share-via-email-link').click(function () {
        if ($('#share-via-email-form').is(':visible'))
        {
            $('#share-via-email-form').slideUp();
        } else {
            $('#share-via-email-form').slideDown();
        }
    });

    $('#share-via-email-form').submit(function () {
        var data = '{"subject":"' + $('#Subject').val() + '","body":"' + $('#Body').val() + '","recipients":"' + $('#Recipients').val() + '"}';
        $.post("/api/events/" + $('#ShortUrl').val() + "/shareviaemail", data)
            .success(function (response) {
                $('#Recipients').val('');
                $('div.infobox div span').html('<p>The email was sent successfully</p>');
                $('.infobox').slideDown();
            })
            .error(function (response) {
                var data = $.parseJSON(response.responseText);
                $('#validation-messages').html('');
                for (var i = 0; i < data.Errors.length; i++)
                    $('#validation-messages').append('<p>' + data.Errors[i].ErrorMessage + '</p>');
            });
        return false;
    });
});

function FacebookHooks() {
    var self = this;

    var $publishLinks = $(".facebook-publish");

    this.bindToUi = function () {
        $publishLinks.click(self.share);
    };

    this.share = function () {
        var nameValue = $("#event-title").text() + " - JustGiving";
        var linkValue = $("#facebook-publish-link").text();
        var pictureValue = $("#facebook-publish-picture").text();
        var captionValue = $("#facebook-publish-caption").text();
        var descriptionValue = $("#facebook-publish-description").text();

        FB.ui({
            method: 'feed',
            name: nameValue,
            link: linkValue,
            picture: pictureValue,
            caption: captionValue,
            description: descriptionValue,
            message: ''
        });

        return false;

    };

}