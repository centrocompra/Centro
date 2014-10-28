$(function () {
    $('#NewsLetterEnabled').attr('checked', 'checked');
});

var SignUp = {
    OnSignUpComplete: function (obj) {
        var json = $.parseJSON(obj.responseText);
        if (json.Status == ActionStatus.Successfull) {
            Message('Signup successful, redirecting...', ActionStatus.Successfull); // json.Message
            //$("form[name=signUp]").reset();
            window.location.href = '/Home/SignupSuccess?id=' + json.ID; ;
        }
        else {
            Message(json.Message, ActionStatus.Error);
        }
    }
};


function Resend(id, sender) {
    setTimeout(function () {
        Message('Processing...');
    }, 1000);
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { id: id },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Account/ResendVerificationEmail',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                setTimeout(function () {
                    HideMessage();
                }, 3000);
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}