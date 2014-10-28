function OnBegin() {
    Message('Processing...');
}

function OnComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        setTimeout(function () {
            window.location.href = '/Admin/Contest/ManageContest';
        }, 3000);
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

function OnRequestComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        ClosePopupWindow();
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

function UploadPicture(sender, form) {
    var newForm = $("<FORM>");
    newForm.attr({ method: "POST", enctype: "multipart/form-data" }).hide();
    newForm.appendTo($("body"));

    var $this = $(sender), $clone = $this.clone();
    $this.after($clone).appendTo($(newForm));

    var progress = '<div class="progress"><div class="bar"><div class="percent">0%</div ></div ></div>';
    $('#div-photos').append(progress);

    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(newForm),
        form: $(newForm),
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Admin/Contest/UploadPicture',
        success: function (data) {
            $("div.loading_ind").html('');
            if (data.Status == ActionStatus.Successfull) {
                if (data.Results == null || data.Results == 'null') {
                    Message(data.Message, ActionStatus.Error);
                }
                else {
                    $("#contest_img").attr("src", "/Temp/" + data.Results[0] + "/" + data.Results[2]);
                    HideMessage();
                }
                $('.progress').remove();
            }
            else {
                Message(data.Message, ActionStatus.Error);
                $('.progress').remove();
            }
        },
        bar: $('.bar'),
        percent: $('.percent'),
        status: $('#status')
    });

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

    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(newForm),
        form: $(newForm),
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Admin/Contest/UploadAttachments',
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
        url: '/Contest/DeleteTempAttachment',
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


function OpenRequestChallenge(sender) {
    OpenPopupWindow({
        url: '/Contest/OpenRequestChallenge/',
        width: 500,
        title: "Send Challenge Request"
    });
}



$(document).ready(function () {
    if ($('#StartDate') != null && $('#StartDate') != undefined && $('#StartDate').val() != undefined && $('#StartDate').val().split(' ').length > 1) {
        $('#StartDate').val($('#StartDate').val().substring(0, $('#StartDate').val().indexOf(' ')));
    }
    if ($('#EndDate') != null && $('#EndDate') != undefined && $('#EndDate').val() != undefined && $('#EndDate').val().split(' ').length > 1) {
        $('#EndDate').val($('#EndDate').val().substring(0, $('#EndDate').val().indexOf(' ')));
    }
    $('.date-control').datepicker({ minDate: new Date() });

//    $("#btnReqChallenge").bind("click", function () {
//        console.log("// Request a challenge");

//        var str = "<form id='frmContestRequest'><div class='request-form'>"
//            + "<h3>Send Challenge Request</h3>"
//            + "<div class='input-col'><label>Email Address: <div class='floatRight'><label><input type='checkbox' name='IsAccepted' /> I would like to be notified if my Challenge Request is accepted</label></div></label>"
//            + "<div class='input-text'><input type='text' class='text email' autofocus='autofocus' name='Email' /></div>"
//            + "<div class='input-col'><label>Title:</label><div class='input-text'><input type='text' class='text' name='Title' /></div>"
//            + "<div class='input-col'><label>Synosis:</label><div class='input-text'><textarea class='text' name='Synosis'></textarea></div>"
//            + "<div class='input-col'><label>Challenge Creteria:</label><div class='input-text'><textarea class='text' name='Criteria'></textarea></div>"
//            + "<div class='floatRight'><button class='button1' id='btnSubmitRequest'>Submit</button>"
//            + "</div><form>";

//        $.fancybox({
//            content: $(str).show()
//        });


//        $('.email', '#frmContestRequest').focus();
//        return false;
//    });

    // Click event on sumbit of Challenge Request
//    $(document).delegate('#btnSubmitRequest', 'click', function () {


//        $.ajax({
//            url: '/contest/RequestChallenge',
//            data: $('#frmContestRequest').serialize(),
//            type: 'POST',
//            success: function (data) {
//                if (data.Success) {
//                    $.fancybox('close');
//                }
//            }
//        });

//        //alert("Comming Soon...");
//        return false;
//    });
});

function AddRow(sender) {
    var row = '<div><input type="text" style="width:500px" class="input-box criteria">' +
                '<input type="button" onclick="RemoveRow(this)" value="-" /><div>';
    $('.criteria-row').append(row);
}

function RemoveRow(sender) {
    $(sender).parent().remove();
}

function submitContest() {
    var criteria = '';
    $('.criteria').each(function () {
        criteria += $(this).val() + '$$$';
    });
    $('#Criteria').val(criteria);
    $('form.section-inner').submit();
    //console.log($('#Criteria').val());
}

