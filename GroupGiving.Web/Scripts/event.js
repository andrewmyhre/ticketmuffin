﻿$(document).ready(function () {
    $('div.contact-box').hide();
    $('#contactForm').validate();
    $('a.contact-organiser').click(function () {
        $('a.contact-organiser').toggle();
        $('div.contact-box').slideToggle();
        $('div.contact-box textarea').focus();
    });

    $('#contactForm').submit(function () {
        var message = $('div.contact-box textarea').val();
        var senderName = $('div.contact-box input[name=senderName]').val();
        var senderEmail = $('div.contact-box input[name=senderEmail]').val();

        $.post($('#contactForm').attr('action'),
            'message=' + message + '&senderName=' + senderName + '&senderEmail=' + senderEmail,
        contactSent,
        'json');

        return false;
    });

    function contactSent() {
        $('a.contact-organiser').toggle();
        $('div.contact-box').slideToggle();
        $('div.contact-box textarea').val('');
        $('div.contact-box input[name=senderName]').val('');
        $('div.contact-box input[name=senderEmail]').val('');
        $('#message-sent').show();
    }
});
