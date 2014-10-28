
function OnBegin(obj) {
    Message('Processing...');
}

function OnComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        setTimeout(function () {
            Message('Redirecting...', ActionStatus.Successfull);
            window.location.href = json.Results[0];
        }, 100);
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}


function OnFeeComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);        
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}