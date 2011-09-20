$(document).ready(function () {
    var facebookHooks = new FacebookHooks();
    facebookHooks.bindToUi();

    $('#share-via-email-link').click(function() { $('#share-via-email-form').slideDown(); });
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