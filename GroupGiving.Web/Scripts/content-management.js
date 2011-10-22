$(document).ready(function () {
    $('.content-definition-heading').click(function () {
        $('.content-definition').hide();
        $('.content-culture-definition').hide();
        $(this).parent().children('.content-definition').show();
    });

    $('.content-culture-definition-heading').click(function () {
        $('.content-culture-definition').hide();
        $(this).parent().siblings('.content-culture-definition').show();
    });

    $('.content-culture-definition').click(function () {
        edit($(this));
    });

    $('.content-culture-definition-edit-link').click(function () {
        cancelEdit($('.editing'));
        $(this).parent().siblings('.content-culture-definition').show();
        edit($(this).parent().siblings('.content-culture-definition'));
    });

    $('.add-culture-link').click(function () {
        add($(this).parent());
    });

});

function cancelEdit(element) {
    if ($(element).length == 0)
        return;
    
    var content = $(element).children('form').children('textarea').val();

    $(element).html(content);
    $(element).removeClass('editing');
}

function edit(element) {
    var content = $(element).html();
    var container = $('<div></div>');
    var form = $('<form method="post" action="/contentmanager/' + $(element).attr('id') + '"></form');
    var editor = $('<textarea name="content">' + content + '</textarea>');
    var saveButton = $('<input type="submit" value="Save" />');

    $(container).append(form);
    $(form).append(editor);
    $(form).append(saveButton);

    $(element).html(form);
    $(element).unbind('click');

    $(element).addClass('editing');
}

function add(element) {
    var container = $('<div></div>');
    var form = $('<form method="post" action="/contentmanager/' + $(element).attr('id') + '"></form');
    var cultureLabel = $('<label for="culture">Culture code (e.g: pl):</label>');
    var cultureInput = $('<input type="text" name="culture" />');
    var editor = $('<textarea name="content"></textarea>');
    var saveButton = $('<input type="submit" value="Save" />');

    $(container).append(form);
    $(form).append(cultureLabel);
    $(form).append(cultureInput);
    $(form).append(editor);
    $(form).append(saveButton);

    $(element).html(form);
    $(element).unbind('click');
}