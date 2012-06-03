$(document).ready(function () {
    $('.content-definition-heading').click(function () {
        $('.content-definition').hide();
        $('.selected').removeClass('selected');
        $(this).parent().children('.content-definition').show();
        $(this).parent().addClass('selected');
    });

    /*$('.content-culture-definition-heading').click(function () {
    $('.content-culture-definition').hide();
    $(this).parent().siblings('.content-culture-definition').show();
    });*/

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
    
    var content = $(element).children('div').children('form').children('div').children('textarea').val();

    $(element).html(content);
    $(element).removeClass('editing');
}

function edit(element) {
    var content = $(element).html();
    var container = $('<div></div>');
    var form = $('<form method="post" action="/contentmanager/' + $(element).attr('id') + '"></form');
    var editorLabel = $('<label for="content">Content:</label>');
    var editor = $('<textarea name="content">' + content + '</textarea>');
    var saveButton = $('<input type="submit" value="Save" />');
    var cancelButton = $('<a href="#">Cancel</a>');

    $(cancelButton).click(function() { cancelEdit($(element)); });

    $(container).append(form);
    $(form).append($('<div class="formline wholeline"></div>').append(editorLabel).append(editor));
    $(form).append(saveButton);
    $(form).append(cancelButton);

    $(element).html(container);
    $(element).unbind('click');

    $(element).addClass('editing');
}

function add(element) {
    var container = $('<div></div>');
    var form = $('<form method="post" action="/contentmanager/' + $(element).attr('id') + '"></form');
    var cultureLabel = $('<label for="culture">Culture code:</label>');
    var cultureSelection = $('<select name="culture"></select>');
    cultureSelection.append('<option value="en">English</option>');
    cultureSelection.append('<option value="pl">Polski</option>');
    var editorLabel = $('<label for="content">Content:</label>');
    var editor = $('<textarea name="content"></textarea>');
    var saveButton = $('<input type="submit" value="Save" />');
    var cancelButton = $('<a href="#">Cancel</a>');

    $(cancelButton).click(function () { cancelEdit($(element)); });

    $(container).append(form);
    $(form).append($('<div class="formline wholeline"></div>').append(cultureLabel).append(cultureSelection));
    $(form).append(cultureLabel);
    $(form).append(cultureSelection);
    $(form).append($('<div class="formline wholeline"></div>').append(editorLabel).append(editor));
    $(form).append(saveButton);
    $(form).append(cancelButton);

    $(element).html(container);
    $(element).unbind('click');

    $(element).addClass('editing');
}