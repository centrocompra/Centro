function OnReplyBegin() {
    var body = $('#Body');
    if ($(body).val().trim().length == 0) {
        $('.field-validation-error').text('*Required.').show();
        return false;
    }
    else {
        $('.field-validation-error').text('').hide();
    }
}

function OnReplyComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        setTimeout(function () {
            var url = window.location;
            console.log(window.location.hash);
            console.log(window.location);
            //if (window.location.hash) url = url.substring(0, url.length - 1);
            window.location = window.location.protocol + '//' + window.location.host + window.location.pathname;
        }, 100);
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

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

    //$('.attachments').append('<span class="' + file_count + '">&nbsp;<img src="/Content/Images/loading.gif" /></span>').show();

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
                    var str = data.Results[1] + '<a href="#" class="close2" onclick=DeleteFile(this,"' + data.Results[0] + '")>X</a>';
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
                if ($('.attachments').find('span').length == 0) { $('.attachments').hide(); }
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}
