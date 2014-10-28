function SetTab(sender) {
    var li = $(sender).parent().parent().find('li').removeClass('active');
    $(sender).parent().addClass('active');
}

/*********************** Payments Tab *****************************/
function OnBegin() {
    Message('Processing...');
}

function OnPaymentComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        if (next) {
            var a = $(".tabs li:nth-child(2)").find('a');
            $(a).trigger('click');
            SetTab(a);
            next = false;
        }
        setTimeout(function () { HideMessage(); }, 3000);
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
    IsCompleted();
}
var next = false;
function SaveNNextSalesTax(sender) {
    next = true;
    $('#Save').trigger('click');
}

function GotoPayments(sender) {
    if (salesTaxCount > 0) {
        next = true;
        var a = $(".tabs li:nth-child(2)").find('a');
        $(a).trigger('click');
        SetTab(a);
    } else {
        Message('Please enter at-least one sales tax', ActionStatus.Error);
        return false;
    }
}

/*********************** Sales Tax Tab *****************************/
var stateArray = [];
var countryArray = [];

function AddState(sender) {
    stateArray.length = 0;
    var states = $('#SelectedStates').val().split(',');
    $.each(states, function (i) {
        stateArray.push(states[i]);
    });

    var state = $('#ToStateID').val();
    var statetext = $('#ToStateID :selected').text();
    if (state != '' && $.inArray(state, stateArray) == -1) {
        stateArray.push(state);
        var ul = '<ul>' +
                    '<li class="col1">' + statetext + ' <input type="hidden" name="State_' + state + '" value="' + state + '" /> </li>' +
                    '<li class="col2"> <input data-val-required="Required." data-val-regex-pattern="^\d{0,2}(\.\d{1,2})?$" data-val-regex="Invalid Tax value." data-val="true"  type="text" name="USTax_' + state + '" style="width: 60px;" class="input-box required number ml5" min="0" max="100" style="width:50px;" step="any" /> % ' +
                    '<span class="field-validation-error" data-valmsg-for="USTax_' + state + '" data-valmsg-replace="true"><span class="" for="USTax_' + state + '" generated="true"></span></span>' +
                    '</li>' +
                    '<li class="col3"> <img onclick="deleteState(this,' + state + ');" alt="Remove" src="/images/close-btn.png"></li>' +
                '</ul>';
        $(ul).insertAfter('.stateHead');
        $('.stateHead').show();
        $('.USTaxRow').show();
        $('#SelectedStates').val(stateArray.join(","));
    }
}

function AddCountry(sender) {
   // debugger;
    var statetext = '';
    countryArray.length = 0;
    stateArray.length = 0;
    var countries = $('#SelectedCountries').val().split(',');
    var states = $('#SelectedStates').val().split(',');
    $.each(countries, function (i) {
        countryArray.push(countries[i]);
    });
    $.each(states, function (i) {
        if (states[i] != '')
            stateArray.push(states[i]);
    });
    var country = $('#ToCountryID').val();
    var state = ''
    if (country == '1') {
        var $state = $('#ToStateID option:selected');
        state = $state.val();
        if (state == '') {

            return false;
        }        
        statetext = $state.text();
    }
    var countrytext = $('#ToCountryID :selected').text();
    if ((country != '' && $.inArray(country, countryArray) == -1) || (country == '1' && $.inArray(state, stateArray) == -1)) {
        countryArray.push(country);
        if (state != '')
            stateArray.push(state);
        var countryUL = '<ul>' +
                            '<li class="col1">' + countrytext + ' <input type="hidden" name="Country_' + country + '" value="' + country + '" /> </li>' +
                            '<li class="col2 w100px">' + statetext + ' <input type="hidden" name="State_' + state + '" value="' + state + '" /> </li>' +
                            '<li class="col2 w140px"> <input data-val-required="Required." data-val-regex-pattern="^\d{0,2}(\.\d{1,2})?$" data-val-regex="Invalid Tax value." data-val="true"  type="text" name="Tax_' + (country == 1 || country == '1' ? state : country) + '" style="width: 60px;" min="0" max="100" class="input-box required number ml5" style="width:50px;" step="any" /> % ' +
                            '<span class="field-validation-error" data-valmsg-for="Tax_' + country + '" data-valmsg-replace="true"><span class="" for="Tax_' + country + '" generated="true"></span></span>' +
                            '</li>' +
                            '<li class="col3 w25px"> <img onclick="deleteCountry(this,' + country + ');" alt="Remove" src="/images/close-btn.png"></li>' +
                        '</ul>';
        $(countryUL).insertAfter('.countryHead');
        $('.countryHead').show();
        $('.TaxRow').show();
        $('#SelectedCountries').val(countryArray.join(","));
        $('#SelectedStates').val(stateArray.join(","));
    }
}

function deleteState(sender, state, sales_tax_id) {
    if (sales_tax_id != '' && sales_tax_id != undefined && sales_tax_id != null) {
        Message('Processing...', MessageType.None);
        AjaxFormSubmit({
            type: "POST",
            validate: true,
            parentControl: $(sender).parents("form:first"),
            data: { sales_tax_id: sales_tax_id },
            messageControl: null,
            throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
            url: '/User/DeleteSalesTax',
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    IsCompleted();
                    Message(data.Message, ActionStatus.Successfull);
                    setTimeout(function () { HideMessage() }, 3000);
                }
                else {
                    Message(data.Message, ActionStatus.Error);
                }
            }
        });
    }
    $(sender).parent().parent().remove();
    stateArray = jQuery.grep(stateArray, function (value) {
        return value != state;
    });
    $('#SelectedStates').val(stateArray.join(","));
    if ($('.stateTaxAdd ul').length == 1) {
        $('.USTaxRow').hide();
    }
    checkNext();
}

function checkNext() {
    if ($('.stateTaxAdd ul').length == 1 && $('.countryTaxAdd ul').length == 1) {
        $('#taxBtn').hide();
    }
    else {
        $('#taxBtn').show();
    }
}

function deleteCountry(sender, country, sales_tax_id) {
    if (sales_tax_id != '' && sales_tax_id != undefined && sales_tax_id != null) {
        //Message('Processing...', MessageType.None);
        AjaxFormSubmit({
            type: "POST",
            validate: true,
            parentControl: $(sender).parents("form:first"),
            data: { sales_tax_id: sales_tax_id },
            messageControl: null,
            throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
            url: '/User/DeleteSalesTax',
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    IsCompleted();
                    salesTaxCount--;
                    Message(data.Message, ActionStatus.Successfull);
                    setTimeout(function () { HideMessage() }, 3000);
                }
                else {
                    Message(data.Message, ActionStatus.Error);
                }
            }
        });
    }
    $(sender).parent().parent().remove();
    countryArray = jQuery.grep(countryArray, function (value) {
        return value != country;
    });
    $('#SelectedCountries').val(countryArray.join(","));
    if ($('.countryTaxAdd ul').length == 1) {
        //$('.TaxRow').hide();
    }
    checkNext();
}
var salesTaxCount = 0;
function OnUSComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        IsCompleted();
        var a = $(".tabs li:nth-child(1)").find('a');
        $(a).trigger('click');
        SetTab(a);
        setTimeout(function () { HideMessage(); }, 3000);
        checkNext();
        salesTaxCount++;
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

function SaveNNextPreview(sender) {
    window.location.href = '/User/PreviewShop';
}

function ShowCountries(sender) {
    //var value = $(sender).val();
    if ($(sender).is(':checked') == false)
    //if (value=='True')
        $('#ToCountryID').attr('disabled', 'disabled');
    else
        $('#ToCountryID').removeAttr('disabled');
}