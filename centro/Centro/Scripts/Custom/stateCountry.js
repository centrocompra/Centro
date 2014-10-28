function OnCountryChange(sender, state, city) {
    var $this = $(sender);
    $("select#" + state).find('option').remove().end().append('<option value="">Select a state...</option>').val('');
    $("select#" + city).find('option').remove().end().append('<option value="">Select a city...</option>').val('');
    if ($this.val() != "" && $this.val() != undefined) {

        AjaxFormSubmit({
            type: "Post",
            validate: false,
            messageControl: null,
            throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
            url: '/User/GetStateList',
            data: { country_id: $(sender).val() },
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    if (data.Results == null || data.Results == 'null') {
                        Message(data.Message, ActionStatus.Error);
                    }
                    else {
                        FillSelectList($("select#" + state), data.Results[0], false);
                        HideMessage();
                    }
                }
                else {
                    Message(data.Message, ActionStatus.Error);
                }
            }
        });
    }
}

function OnStateChange(sender, city) {
    var $this = $(sender);
    $("select#" + city).find('option').remove().end().append('<option value="">Select a city...</option>').val('');
    if ($this.val() != "" && $this.val() != undefined) {

        AjaxFormSubmit({
            type: "Post",
            validate: false,
            messageControl: null,
            throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
            url: '/User/GetCityList',
            data: { state_id: $(sender).val() },
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    if (data.Results == null || data.Results == 'null') {
                        Message(data.Message, ActionStatus.Error);
                    }
                    else {
                        FillSelectList($("select#" + city), data.Results[0], false);
                        HideMessage();
                    }
                }
                else {
                    Message(data.Message, ActionStatus.Error);
                }
            }
        });
    }
}

