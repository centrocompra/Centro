var BillingAddress = {
    OnBillingAddressComplete: function (obj) {
        var json = $.parseJSON(obj.responseText);
        if (json.Status == ActionStatus.Successfull) {
            Message(json.Message, ActionStatus.Successfull);
            $("form[name=billingAddress]").reset()
            setTimeout(function () {
                window.location.href = '/Home/ManageBillingAddress';
            }, 3000);
        }
        else {
            Message(json.Message, ActionStatus.Error);
        }
    }
};