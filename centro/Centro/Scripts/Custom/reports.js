function SaveReportMessage(event, id, type, requestType) {
    event.cancelBubble = true;
    if (event.stopPropagation)
        event.stopPropagation();

    OpenPopupWindow({
        url: '/Hub/Reports?PageURL='+id,
        width: 400,
        title: "Send Report"
    });    
}


function ReportSent(obj) {
    ClosePopupWindow();
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }


}

function VoteUp(sender, id) {
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { ID: id },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Contest/VoteUp',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                setTimeout(function () {
                    window.location.href = window.location.href;
                }, 3000);
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}