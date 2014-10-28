var ForgotPassword = {
    OnForgotComplete: function (obj) {
        var json = $.parseJSON(obj.responseText);
        if (json.Status == ActionStatus.Successfull) {
            Message('Password reset link has been sent.', ActionStatus.Successfull);
            $("form[name=forgotpassword]").reset();
        }
        else {
            Message(json.Message, ActionStatus.Error);
        }
    }
};
