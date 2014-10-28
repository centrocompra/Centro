var hash = window.location.hash.substring(1)
var message_type = hash, IsRead = null, IsArchived = false;


function listMessage(sender, type, isRead, isArchived) {
    message_type = type;
    IsRead = isRead;
    IsArchived = isArchived;
    search = '';
    $('#Search').val('');
    window.location.href = '/Message#' + message_type;
    MessagePaging(sender);
}

$("#Receiver").autocomplete({
    source: function (request, response) {
        AjaxFormSubmit({
            type: "POST",
            validate: false,
            parentControl: $("#Receiver").parents("form:first"),
            data: { startswith: $("#Receiver").val() },
            messageControl: null,
            throbberPosition: { my: "left center", at: "right center", of: $("#Receiver"), offset: "5 0" },
            url: '/Message/_Usernames',
            success: function (data) {
                var dataaa = $.parseJSON(data.Results[0]);
                response($.map(dataaa, function (item) {
                    return {
                        //                        label: item.name + (item.adminName1 ? ", " + item.adminName1 : "") + ", " + item.countryName,
                        label: item,
                        value: item
                    }
                }));
            }
        });
    },
    minLength: 1,
    select: function (event, ui) {
        //                log(ui.item ?
        //        					"Selected: " + ui.item.label :
        //        					"Nothing selected, input was " + this.value);
        var value = $("#Receiver").val().split(';');
        if ($("#Receiver").val().indexOf(';') == -1)
            $("#Receiver").val(ui.item.label + ';');
        else {
            value.pop();
            value.push(ui.item.label);
            $("#Receiver").val(value.join(';') + ';');
        }

    },
    open: function () {
        $(this).removeClass("ui-corner-all").addClass("ui-corner-top");
    },
    close: function () {
        $(this).removeClass("ui-corner-top").addClass("ui-corner-all");
    }
});

var file_count = 0;
function UploadAttachments(sender, form) {
    var newForm = $("<FORM>");
    newForm.attr({ method: "POST", enctype: "multipart/form-data" }).hide();
    newForm.appendTo($("body"));

    var el = document.createElement("input");
    el.type = "hidden";
    el.name = 'Count';
    el.value = ++file_count;

    var $this = $(sender), $clone = $this.clone();
    $this.after($clone).appendTo($(newForm));
    $this.after(el).appendTo($(newForm));

    var progress = '<div class="progress"><div class="bar"><div class="percent">0%</div ></div ></div>';
    $('.attachments').append('<div class="' + file_count + '">' + progress + '</div>').show();

    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(newForm),
        form: $(newForm),
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Message/UploadAttachments',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                if (data.Results == null || data.Results == 'null') {
                    Message(data.Message, ActionStatus.Error);
                }
                else {
                    var str = data.Results[1] + '<a href="javascript:;" class="close2" onclick=DeleteFile(this,"' + data.Results[0] + '")>X</a>';
                    $('.attachments').find('.' + data.Results[2]).html(str);
                    HideMessage();
                    $(newForm).remove();
                }
                $('.progress').remove();
            }
            else {
                $('.attachments').find('.' + data.Results[2]).remove();
                if ($('.attachments').find('div').length == 0) { $('.attachments').hide(); }
                Message(data.Message, ActionStatus.Error);
                $(newForm).remove();
                $('.progress').remove();
            }
        },
        bar: $('.bar'),
        percent: $('.percent'),
        status: $('#status')
    });
}

function DeleteFile(sender, file) {
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { filename: file },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Message/DeleteTempAttachment',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $(sender).parent().remove();
                if ($('.attachments').find('div').length == 0) { $('.attachments').hide(); }
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function OnComposeComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        window.location.href = '/Message/Index';    
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

function checkall(sender) {    
    if ($(sender).is(':checked')) {
        $('.msg-list input[type=checkbox]').attr('checked', 'checked');        
    }
    else {
        $('.msg-list input[type=checkbox]').removeAttr('checked');
    }
}

function Delete(sender) {
    var Ids = [];
    $('.msg-list input[type=checkbox]:checked').each(function () {
        Ids.push($(this).attr('id'));
    });
    if (Ids.length > 0) {
        AjaxFormSubmit({
            type: "POST",
            validate: true,
            parentControl: $(sender).parents("form:first"),
            data: { Ids: Ids.join(',') },
            messageControl: null,
            throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
            url: '/Message/DeleteMessages',
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    listMessage(sender, message_type, IsRead, IsArchived);
                }
                else {
                    Message(data.Message, ActionStatus.Error);
                }
            }
        });
    }
    else {
        Message('Select any record to delete/mark as archive.', ActionStatus.Error);
    }
}

function markAsArchive(sender) {
    var Ids = [];
    $('.msg-list input[type=checkbox]:checked').each(function () {
        Ids.push($(this).attr('id'));
    });
    if (Ids.length > 0) {
        AjaxFormSubmit({
            type: "POST",
            validate: true,
            parentControl: $(sender).parents("form:first"),
            data: { Ids: Ids.join(',') },
            messageControl: null,
            throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
            url: '/Message/MarkAsArchive',
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    listMessage(sender, message_type, IsRead, IsArchived)
                }
                else {
                    Message(data.Message, ActionStatus.Error);
                }
            }
        });
    }
    else {
        Message('Select any record to delete/mark as archive.', ActionStatus.Error);
    }
}

function loadAll(sender) {
    $('.hide').show();
    $(sender).parent().parent().remove();
}