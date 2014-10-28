var url = '';
function CategoryComplete(obj) {
    var json = obj;
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        setTimeout(function () {
            window.location.href = json.Results[0];
        }, 1000);
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}


var AddCategory = {
    OnAddSubCategoryComplete: function (obj) {
        var json = $.parseJSON(obj.responseText);
        if (json.Status == ActionStatus.Successfull) {
            Message(json.Message, ActionStatus.Successfull);
            setTimeout(function () {
                Message('Redirecting...', ActionStatus.Successfull);
                window.location.href = url;
            }, 100);
        }
        else {
            Message(json.Message, ActionStatus.Error);
        }
    }
};


