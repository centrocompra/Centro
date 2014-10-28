var ResetPassword = {
    OnResetComplete: function (obj) {
        var json = $.parseJSON(obj.responseText);
        if (json.Status == ActionStatus.Successfull) {
            Message(json.Message, ActionStatus.Successfull);
            $("form[name=resetpassword]").reset();
            Message('Password has been changed successfuly. Redirecting...', ActionStatus.Successfull);
            setTimeout(function () {                
                window.location.href = json.Results[0];
            }, 3000);
        }
        else {
            Message(json.Message, ActionStatus.Error);
        }
    }
};
