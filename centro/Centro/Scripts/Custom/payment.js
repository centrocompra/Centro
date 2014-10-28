function TermsAndConditions(sender) {
    OpenPopupWindow({
        url: '/Payment/_TermsAndConditions',
        width: 400,
        title: "Terms and Conditions"
    });
    return false;
}

function AcceptTermsAndConditions(sender) {
    if ($('input[type=checkbox]').is(':checked')) {
        ClosePopupWindow();
        //EscrowPayment(sender, InvoiceID, RequestID);
        //$(sender)
        $('#payment-form').submit();
        return true;
    } else {
        $('.terms span.field-validation-error').html('*Required.');
        return false;
    }
}


function OnBegin() {
    $('#Submit').attr('disabled', 'disabled')
                .removeClass('button1')
                .addClass('button1-disabled')
                .text('Please wait...');
    Message('Processing...');
}

function OnComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        window.location.href = json.Results[0];
        //window.location.href = '/Payment/Thankyou?id=' + json.Results[0] + '&shop_id=' + json.Results[1];
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

function OnStateChange(sender, city_id, selectedID) {
    var $this = $(sender);
    $('#'+city_id).find('option').remove().end().append('<option value="">Select a city...</option>').val('');
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
                        FillSelectList($('#' + city_id), data.Results[0], false);
                        if (selectedID != null && selectedID != undefined) {
                            $('#' + city_id).val(selectedID);
                            $('#BillingAddress_CityId').val($('#ShippingAddress_CityID').val());
                        }
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
var IsSameAsBillingClicked = false;
function OnCountryChange(sender, state_id, selectedID) {
    var $this = $(sender);
    $("#"+state_id).find('option').remove().end().append('<option value="">Select a state...</option>').val('');
    //$("#" + state_id).find('option').remove().end().append('<option value="">Select a city...</option>').val('');
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
                        FillSelectList($("#" + state_id), data.Results[0], false);
                        if (selectedID != null && selectedID != undefined) {
                            $('#' + state_id).val(selectedID);
                            if (!IsSameAsBillingClicked)
                                OnStateChange($('#BillingAddress_StateId'), 'BillingAddress_CityId', $('#BillingAddress_CityId').val());
                            else {
                                if ($('#BillingAddress_StateId').val() == $('#ShippingAddress_StateID').val()) {
                                    OnStateChange($('#ShippingAddress_StateID'), 'ShippingAddress_CityID', $('#BillingAddress_CityId').val());
                                    IsSameAsBillingClicked = false;
                                }
                            }
                        }
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


function SameAsBilling(sender) {
    if ($(sender).is(':Checked')) {
        IsSameAsBillingClicked = true;
        $('#ShippingAddress_FirstName').val($('#BillingAddress_FirstName').val());
        $('#ShippingAddress_LastName').val($('#BillingAddress_LastName').val());
        $('#ShippingAddress_Email').val($('#BillingAddress_Email').val());
        //$('#ShippingAddress_CountryId').val($('#BillingAddress_CountryId').val());
        if ($('#ShippingAddress_CountryID').val() == $('#BillingAddress_CountryId').val()) {
            console.log('a');
            $('#ShippingAddress_StateID').val($('#BillingAddress_StateId').val());
            OnCountryChange($('#ShippingAddress_CountryID'), 'ShippingAddress_StateID', $('#BillingAddress_StateId').val());
        }
        $('#ShippingAddress_Address').text($('#BillingAddress_Address').val());
        $('#ShippingAddress_PostCode').val($('#BillingAddress_PostCode').val());        
    }
    else {
    }
   // 
}


function SaveBillingAddress(sender) {
    var hidden = $('<input>');
    hidden.attr({ type: 'hidden', name: 'SaveBilling', value: true });
    hidden.appendTo($('#form'));
    $('#form').submit();
}