var SignIn = {
    OnSignInComplete: function (obj) {
        var json = $.parseJSON(obj.responseText);
        if (json.Status == ActionStatus.Successfull) {
            Message(json.Message, ActionStatus.Successfull);
            setTimeout(function () {
                Message('Redirecting...', ActionStatus.Successfull);
                window.location.href = json.Results[0];
            }, 100);
        }
        else if (json.ID == Constants.InvalidLoginAttemptForCaptcha) {
            $("div[name=captchaValidation]").show();
            Message(json.Message, ActionStatus.Error);
        }
        else {
            Message(json.Message, ActionStatus.Error);
        }
    }
};

