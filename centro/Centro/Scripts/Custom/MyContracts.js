

function GotoInvoices(url) {
    window.location.href = url;
}

function SetTab(sender) {
    var li = $(sender).parent().parent().find('li').removeClass('active');
    $(sender).parent().addClass('active');
}

function MoveTo(sender, type) {
    var Ids = [];
    $('.contract-checkbox').each(function () {
        if ($(this).is(':checked')) {
            Ids.push($(this).attr('id'));
        }
    });
    if (Ids.length <= 0)
        return false;
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { Ids: Ids.join(','), MoveTo: type },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/MoveTo',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                setTimeout(function () {
                    window.location.href = window.location.href;
                }, 3000);
            }
            else {
                Message('Something went wrong', ActionStatus.Error);
            }
        }
    });
}

$('.checkall').live('click', function () {
    
    if ($(this).is(':checked')) {
        $(this).parent().parent().find('.MyContrators table td input[type=checkbox]').each(function () {
            $(this).attr('checked', 'checked');
        });
    }
    else {
        $(this).parent().parent().find('.MyContrators table td input[type=checkbox]').each(function () {
            $(this).removeAttr('checked');
        });
    }
});

function stopEvents(event) {
    event.cancelBubble = true;
    if (event.stopPropagation)
        event.stopPropagation();
}

function UpdateNextStep(sender, event, cls, ID) {
    event.cancelBubble = true;
    if (event.stopPropagation)
        event.stopPropagation();

    var nextstep = $(sender).parent().find('.' + cls + ' option:selected').val();
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { ID: ID, NextStep: nextstep },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/UpdateNextStep',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}