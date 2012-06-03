var state = new Object();

$(document).ready(function () {
    state.editing = false;
    state.pageId = 0;
    state.contentLabel = '';
    state.culture = '';
    state.form = null;

    var editables = $('span[page-id]');
    editables
        .parents('a').unbind('click').attr('href', 'javascript: void(0)');
    editables.unbind('click');
    editables
        .click(function () {
            var editing = $(this);
            var pageId = $(this).attr('page-id');
            var contentLabel = $(this).attr('label');
            var culture = $(this).attr('culture');

            state.pageId = pageId;
            state.contentLabel = contentLabel;
            state.culture = culture;
            state.originalContent = $(this).html();
            state.editing = true;

            var form = $('<form action="/api/content/' + pageId + '/' + contentLabel + '/' + culture + '"></form>');
            form.append($('<input type="hidden" name="pageId" value="' + pageId + '" />'));
            form.append($('<input type="hidden" name="label" value="' + contentLabel + '" />'));
            form.append($('<input type="hidden" name="culture" value="' + culture + '" />'));
            form.append($('<textarea name="contentValue" style="width:100%;" rows="5">' + $(this).html() + '</textarea>'));
            var submitButton = $('<button>Update Translation</button>')
                .click(function () {
                    updateTranslation(state);
                    return false;
                });
            form.append(submitButton);
            var cancelButton = $('<a href="#">Cancel</a>')
                .click(function () {
                    stopEditing(state, state.originalContent);
                });
            form.append(cancelButton);
            state.form = form;
            $(this).unbind('click');
            $(this).html(form);
        });

});

function updateTranslation(editingState) {
    var content = $(editingState.form).children('textarea').val();
    var url = '/api/content/' + editingState.pageId + '/' + editingState.contentLabel + '/' + editingState.culture;
    $.ajax({
        url: url,
        type: 'PUT',
        data: 'contentValue=' + content,
        complete: function (jqXHR, textStatus) {
            if (textStatus) {
                stopEditing(editingState, content);
            } else {
                alert(textStatus);
            }
        }
    });
}

function stopEditing(editingState, content) {
    if (!editingState.editing)
        return;

    editingState.form.html(content);
    editingState.pageId = 0;
    editingState.contentLabel = '';
    editingState.culture = '';
    editingState.editing = false;
}