//====== Enums & Constants =========

var ActionStatus = {
    None: 0,
    Successfull: 1,
    Error: 2,
    LoggedOut: 3,
    Unauthorized: 4
}

var HttpType = {
    Post: "POST",
    Get: "GET"
};

var MessageType = {
    Successfull: 1,
    Error: 2,
    Warning: 3,
    None: 4
};



var UpdationType = {
    Insert: 1,
    Update: 2,
    Delete: 3
};



var SpecialsSortType = {
    ExpirationDate: 1,
    Createddate: 2,
    ByReplies: 3,
    ByViews: 4
};

var Constants = {
    InvalidLoginAttempt: 5,
    InvalidLoginAttemptForCaptcha: 3
};
var ActionOnProduct = {
    Activate: 101,
    DeActivate: 102,
    Delete: 103,
    Featured: 104
};
//==================================

$(document).ready(function () {
    document.addEventListener("click", function (e) {
        if (e.button == 1) {
            e.preventDefault();
            //alert(e.button);
        }
    }, true);

    $('.tabs,.product-grid').live("contextmenu", function (e) {
        return false;
    });
    

//    $('.tabs').bind('click', function (e) {
//        if (e.which == 2)
//            e.preventDefault();
//    });

    CreateWaterMark();
    $.InitLogin();
    ApplyMaxLength();
    $('.Message_DIV').bind('click', function () {
        $(this).hide();
    });
});


/// <summary>
/// <para>Author: Sudeep Sehgal</para>
/// <para>Created: 17 may 2012</para>
/// <para>This function shows a throbber at a position specified by the throbberPosition variable</para>
/// <para>Usage: ShowThrobber({ my: "left center", at: "right center", of: sender, offset: "5 0" });</para>
/// </summary>
function ShowThrobber(throbberPosition) {

    $("#MainThrobberImage").show().position(throbberPosition);
}


/// <summary>
/// <para>Author: Sudeep Sehgal</para>
/// <para>Created: 17 may 2012</para>
/// <para>This function removes the throbber </para>
/// </summary>
function RemoveThrobber() {
    $("#MainThrobberImage").hide();
}


/// <summary>
/// <para>Author: Sudeep Sehgal</para>
/// <para>Created: 17 may 2012</para>
/// <para>This function displayes a formatted message </para>
/// </summary>

function ShowMessage(messageControl, message, messageType) {

    /*================= Sample Usage =========================
    ShowMessage($(selector), "This is a dummy message", MessageType.Success)
    ==========================================================*/

    if (messageType == MessageType.Success) {
        $(messageControl).html(message).removeClass("failure-msg").addClass("success-msg").fadeIn();
        $("html,body").animate({ scrollTop: "0" }, "slow");
        setTimeout(function () {
            $(messageControl).html('').hide();
        }, 5000);
    }
    else if (messageType == MessageType.Error) {
        $(messageControl).html(message).removeClass("success-msg").addClass("failure-msg").fadeIn();
        $("html,body").animate({ scrollTop: "0" }, "slow");
    }
    else if (messageType == MessageType.Warning) {
        $(messageControl).html(message).removeClass("failure-msg").addClass("success-msg").fadeIn();
        $("html,body").animate({ scrollTop: "0" }, "slow");
    }
    else {
        $(messageControl).html('').hide();
    }
    RemoveThrobber();
}

function RemoveMessage(messageControl) {
    $(messageControl).html('').hide();
}

jQuery.fn.reset = function () {
    $(this).each(function () { this.reset(); });
}

/// <summary>
/// <para>Author: Sudeep Sehgal</para>
/// <para>Created: 17 may 2012</para>
/// <para>This function validated the controls with specific datatypes and displayes the appropriate messages</para>
/// </summary>
function ValidateControls(messageControl, parentControl) {

    /*==========Sample Usage & Datatype Examples =============
    
    <input name="" type="text" required-field="true" required-message="xxxxxxxxx" datatype="number" message="xxxxxxx">
    <input name="" type="text" required-field="true" required-message="xxxxxxxxx" datatype="email" message="xxxxxxx">
    <input name="" type="file" required-field="true" required-message="xxxxxxxxx" datatype="image" message="xxxxxxx" allowed-formats="jpg.bmp.jpeg.gif.png">
    <select name="" required-field="true" required-message="xxxxxxxxx">

    if(ValidateControls($(selecter), $(selecter)) == false) return false;

    ==========================================================*/

    var errorMessage = "";
    parentControl = parentControl == undefined ? $("body") : parentControl;

    $(parentControl).find("input[required-field=true]").each(function () {

        if ($(this).attr("watermark")) {
            if ($(this).val() == $(this).attr("watermark")) {
                errorMessage += ($(this).attr("required-message") + "<br>");
            }
        }
        else if (!$.trim($(this).val())) {
            errorMessage += ($(this).attr("required-message") + "<br>");
        }
    });

    $(parentControl).find("input[confirm-field=true]").each(function () {
        if ($.trim($(this).val()) != $("input[name=" + $(this).attr("confirm-name") + "]").val()) {
            errorMessage += ($(this).attr("confirm-message") + "<br>");
        }
    });

    $(parentControl).find("textarea[required-field=true]").each(function () {
        if ($(this).attr("watermark")) {
            if ($(this).val() == $(this).attr("watermark")) {
                errorMessage += ($(this).attr("required-message") + "<br>");
            }
        }
        else if (!$(this).val()) {
            errorMessage += ($(this).attr("required-message") + "<br>");
        }
    });

    $(parentControl).find("select[required-field=true]").each(function () {
        if ($(this).val() == "0") {
            errorMessage += ($(this).attr("required-message") + "<br>");
        }
    });

    $(parentControl).find("input[datatype=number]").each(function () {
        if ($(this).attr("watermark")) {
            if ($(this).val() != $(this).attr("watermark")) {
                if (!IsNumeric($(this).val())) {
                    errorMessage += ($(this).attr("message") + "<br>");
                }
            }
        }
        else if ($(this).val()) {
            if (!IsNumeric($(this).val())) {
                errorMessage += ($(this).attr("message") + "<br>");
            }
        }
    });

    $(parentControl).find("input[datatype=email]").each(function () {
        if ($(this).attr("watermark")) {
            if ($(this).val() != $(this).attr("watermark")) {
                if (!IsEmail($(this).val())) {
                    errorMessage += ($(this).attr("message") + "<br>");
                }
            }
        }
        else if ($.trim($(this).val())) {
            if (!IsEmail($(this).val())) {
                errorMessage += ($(this).attr("message") + "<br>");
            }
        }
    });

    $(parentControl).find("input[datatype=image]").each(function () {
        if ($(this).val()) {
            var filetype = $(this).val().split(".");
            filetype = filetype[filetype.length - 1].toLowerCase();

            if ($(this).attr("allowed-formats").indexOf(filetype) == -1) {
                $(this).val("");
                errorMessage += ($(this).attr("message") + "<br>");
            }
        }
    });
    $(parentControl).find("input[type=hidden][required-field=true]").each(function () {
        if ($(this).val() == 0) {
            errorMessage += ($(this).attr("required-message") + "<br>");
        }
    });

    if (errorMessage) {
        ShowMessage(messageControl, errorMessage, MessageType.Error);
        return false;
    }
    return true;
}

/// <summary>
/// <para>Author: Sudeep Sehgal</para>
/// <para>Created: 17 may 2012</para>
/// <para>Validates a string and returns true if it is numeric else returns false</para>
/// </summary>
function IsNumeric(input) {
    return (input - 0) == input && input.length > 0;
}


/// <summary>
/// <para>Author: Sudeep Sehgal</para>
/// <para>Created: 17 may 2012</para>
/// <para>Validates a string and returns true if it is a valid email else returns false</para>
/// </summary>
function IsEmail(email) {
    var regex = /^([a-zA-Z0-9_\.\-\+])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;
    return regex.test(email);
}

$.IsNumericCustom = function (input) {
    return (input - 0) == input && input.length > 0;
}

$.IsEmailCustom = function (email) {
    var regex = /^([a-zA-Z0-9_\.\-\+])+\@(([a-zA-Z0-9\-])+\.)+([a-zA-Z0-9]{2,4})+$/;
    return regex.test(email);
}

$.IsCharNum = function (str) {
    if (/[^a-zA-Z0-9]/.test(str)) return false;
    return true;
}

$.IsAlphaNumeric = function (val) {
    if (/[^a-zA-Z0-9 ]/.test(val)) return false;
    return true;
}


/// <summary>
/// <para>Author: Sudeep Sehgal</para>
/// <para>Created: 17 may 2012</para>
/// <para>This function is called on the document load and links a watermark functionality with the controls that have a watermark functionality</para>
/// </summary>
function CreateWaterMark() {

    /*==========Sample Usage & Datatype Examples =============    
    <input name="" type="text" watermark="XXXXXXXX">    
    ==========================================================*/

    $("input[type=text][watermark], textarea[watermark]").each(function () {
        $(this).bind("focus", function () {
            if ($(this).val() == $(this).attr("watermark")) $(this).val("");
        });

        $(this).bind("blur", function () {
            if ($(this).val() == "") $(this).val($(this).attr("watermark"));
        });
    });
}

function ApplyMaxLength() {
    $('textarea[maxlength]').keyup(function () {
        //get the limit from maxlength attribute  
        var limit = parseInt($(this).attr('maxlength'));
        //get the current text inside the textarea  
        var text = $(this).val();
        //count the number of characters in the text  
        var chars = text.length;

        //check if there are more characters then allowed  
        if (chars > limit) {
            //and if there are use substr to get the text before the limit  
            var new_text = text.substr(0, limit);

            //and change the current text with the new text  
            $(this).val(new_text);
        }
    });
}


/// <summary>
/// <para>Author: Sudeep Sehgal</para>
/// <para>Created: 23 May 2012</para>
/// <para>This function returs the JSON data for the selectlists. This also brings along the data for the associated child seleclists</para>
/// </summary>
function GetSelectListDataJSON(parameters) {

    /*=====================================Sample Usage======================================================
    GetSelectListDataJSON({ 
    listType: SelectListType.Country,                           //The seleclist whose data is to be fetched
    addDefaultValue: true,                                      //If a default valus needs to be inserted
    defaultTextFirst: "xxxxxx",                                 //The default text to be user for the current select list
    defaultTextSecond: "xxxxxx",                                //The default text to be user for the first child select list
    defaultTextThird: "xxxxxx",                                 //The default text to be user for the second child select list    
    parentID: 87                                                //The parent id to be used for fetvhing the data for the current selectlist                       
    error: function (err) { },                                  //called when an unexpected error occurs
    success: function (data) {}                                 //called after the request has been executed without any exception
    });
    ===============================================================================================*/

    $.ajax({
        url: MasterAbsoluteURLs.GetSelectListDataJSON,
        type: "POST",
        data: {
            listType: parameters.listType,
            addDefaultValue: parameters.addDefaultValue,
            defaultTextFirst: parameters.defaultTextFirst,
            defaultTextSecond: parameters.defaultTextSecond,
            defaultTextThird: parameters.defaultTextThird,
            parentID: parameters.parentID
        },
        error: function () { alert("Unexpected Error"); },
        success: function (data) {
            if (data.Status == true) parameters.success(data.Results);
            else parameters.error(data.Message);
        }
    });
}


/// <summary>
/// <para>Author: Sudeep Sehgal</para>
/// <para>Created: 23 May 2012</para>
/// <para>It appends the data of a json list to the specified dropdown</para>
/// </summary>
function FillSelectList(selectControl, data, clearData) {

    data = $.parseJSON(data);
    if (clearData != false) $(selectControl).html("");

    for (var i = 0; i < data.length; i++) {
        $(selectControl).append('<option value="' + data[i].Value + '">' + data[i].Text + '</option>');
    }
}

function NeglectWaterMark(arr, form) {
    for (var i = 0; i < arr.length; i++) {
        if (true) {
            if (arr[i].value == $(form).find("input[name=" + arr[i].name + "]").attr("watermark")) {
                arr[i].value = "";
            }
        }
    }

    return arr;
}

function AjaxFormSubmit(parameters) {
    /*=====================================Sample Usage======================================================
    AjaxFormSubmit({
    type: "POST",                                                                                       //default is "POST"
    validate: true,                                                                                     //if the form/data needs to be validated
    validationError: function () { },                                                                   //called only if validate is true
    parentControl: $(selector),                                                                         //required if validate is true
    error: function () { },                                                                             //called when an unexpected error occurs
    form:  $(selector),                                                                                 //neglected if data is not null
    data: {name: "value"}                                                                               //overwrites the form parameter
    messageControl:  $(selector),                                                                       //the control where the status message needs to be displayed
    beforeSubmit: function (arr, $form, options) { },                                                   //called before the form is actually submitted 
    throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },             //the position at which the throbber needs to be displayed 
    url: url,                                                                                           //the url that needs to be hit
    success: function (data) {},                                                                        //called after the request has been executed without any unhandeled exception
    bar: $('.bar'),                                                                                     //var bar = $('.bar');
    percent: $('.percent'),                                                                             //var percent = $('.percent');
    status: $('#status')                                                                                //var status = $('#status');
    });
    ===============================================================================================*/
    
    if (parameters.validate == true) {
        if (ValidateControls($(parameters.messageControl), $(parameters.parentControl)) == false) {
            if (parameters.validationError) parameters.validationError();
            return false;
        }
    }

    //Message($(parameters.messageControl), "", ActionStatus.None);
    ShowThrobber(parameters.throbberPosition);
    if (parameters.data == undefined) {
        $(parameters.form).ajaxForm({
            url: parameters.url,
            dataType: $.browser.msie ? "json" : undefined,
            type: parameters.type == undefined ? "POST" : parameters.type,
            error: function () {
                RemoveThrobber();
                Message("Unexpected Error", ActionStatus.Error);
                if (parameters.error != undefined) parameters.error();
            },
            beforeSubmit: function (arr, form, options) {
                arr = NeglectWaterMark(arr, form);
                if (parameters.beforeSubmit) parameters.beforeSubmit(arr, form, options);
            },
            beforeSend: function () {
                if (parameters.bar != null && parameters.status != null && parameters.status != undefined) {
                    parameters.status.empty();
                    var percentVal = '0%';
                    parameters.bar.width(percentVal)
                    parameters.percent.html(percentVal);
                }
            },
            uploadProgress: function (event, position, total, percentComplete) {
                if (parameters.bar != null) {
                    var percentVal = percentComplete + '%';
                    parameters.bar.width(percentVal)
                    parameters.percent.html(percentVal);
                }
            },
            success: function (data) {
                //RemoveThrobber();
                if (data.Status == undefined) {
                    Message('Invalid data returned in the response', ActionStatus.Error);
                    return false;
                }
                else if (parameters.bar != null) {
                    var percentVal = '100%';
                    parameters.bar.width(percentVal)
                    parameters.percent.html(percentVal);
                    parameters.success(data);
                }
                else {
                    parameters.success(data);
                }
                //                if (data.Status != true) {
                //                    Message(data.Message, ActionStatus.Error);
                //                    if (parameters.error != undefined) parameters.error();
                //                }
                //                else if (parameters.success) parameters.success(data);

            },
            complete: function (xhr) {
                // status.html(xhr.responseText);
            }
        }).submit();
    }
    else {

        $.ajax({
            url: parameters.url,
            type: parameters.type == undefined ? "POST" : parameters.type,
            error: function (a, b, c) {
                RemoveThrobber();
                Message($(parameters.messageControl), "Unexpected Error", ActionStatus.Error);
                if (parameters.error != undefined) parameters.error();
            },
            async: true,
            data: parameters.data,
            success: function (data) {
                RemoveThrobber();
                if (data.Status == undefined) {
                    Message($(parameters.messageControl), "Invalid data returned in the response", ActionStatus.Error);
                    return false;
                }
                else {
                    parameters.success(data);
                }
//                if (data.Status != true) {
//                    //ShowMessage($(parameters.messageControl), data.Message, MessageType.Error);
//                    Message(data.Message, ActionStatus.Error);
//                    if (parameters.error != undefined) parameters.error();
//                }
//                if (data.Status == ActionStatus.Error) {
//                    Message(data.Message, ActionStatus.Error);
//                    return false;
//                }
//                else if (parameters.success) parameters.success(data);
            },
            error: function (obj) {
                Message(data.Message, ActionStatus.Error);
            }
        });
    }
}


/// <summary>
/// <para>Author: Sudeep Sehgal</para>
/// <para>Created: 23 May 2012</para>
/// <para>It opens a popup window as per the specified parameters</para>
/// </summary>
function OpenPopupWindow(parameters) {
    /*=====================================Sample Usage======================================================
    OpenPopupWindow({
    url: url,           //the url that needs to be hit
    width: xxx,         //The width of the popup window 
    offsetX: xxx,       //No of pixels to be added horizontally from the center of the screen 
    offsetY: xxx,       //No of pixels to be added vertically from the center of the screen 
    title: "xxxxxx"     //The text to be displayed as the title of the popup windiw 
    });
    ===============================================================================================*/

    var offsetX = parameters.offsetX == undefined ? "0" : parameters.offsetX;
    var offsetY = parameters.offsetY == undefined ? "0" : parameters.offsetY;

    $("#lightbox_overlay").show();
    $("#lightBox div.popUpContent h2[name=Title]").html(parameters.title);
    if (parameters.title == '' || parameters.title == null || parameters.title == undefined) {
        $("#lightBox div.popUpContent h2[name=Title]").hide();
    }

    $("#lightBox div.popUpContent div[name=ActualContent]").css("min-width", parameters.width == undefined ? 400 : parameters.width).css('max-width','600px')
    .html('<div id="StatusMessagePopup" class="failure-msg" style="display:none;margin-top:0px;margin-left:0px"></div>');
    //$("#lightBox").show().position({ my: "center center", at: "center center", of: $(window), offset: offsetX + " " + offsetY });
    

    if (parameters.form != undefined) parameters.data = "null";
    else if (parameters.data == undefined) parameters.data = "null";

    AjaxFormSubmit({
        type: "POST",
        validate: false,
        throbberPosition: { my: "center center", at: "center center", of: $("#lightBox div.popUpContent div[name=ActualContent]"), offset: "0 0" },
        messageControl: $("div#StatusMessagePopup"),
        url: parameters.url,
        data: parameters.data,
        form: parameters.form,
        success: function (data) {
            var str = '';
            if (data.Status == ActionStatus.Successfull) {
                str = data.Results[0];
            }
            else {
                str = '<a href="' + data.Results[0] + '">Login</a> is required to perform this action.';
            }
            $("#lightBox div.popUpContent div[name=ActualContent]").html(str);
            var screen_center_x = $(window).width() / 2;
            var screen_center_y = $(window).height() / 2;
            var popup_width = $("#lightBox").width();
            var popup_height = $("#lightBox").height();

            var popup_x = screen_center_x - (popup_width / 2);
            var popup_y = screen_center_y - (popup_height / 2);
            $('#lightBox').show().css('left', popup_x).css('top', popup_y);
        }
    });
}

function CenterPopup(parameters) {
    var offsetX = parameters.offsetX == undefined ? "0" : parameters.offsetX;
    var offsetY = parameters.offsetY == undefined ? "0" : parameters.offsetY;

    $("#lightBox").show().position({ my: "center center", at: "center center", of: $(window), offset: offsetX + " " + offsetY });
}


/// <summary>
/// <para>Author: Sudeep Sehgal</para>
/// <para>Created: 23 May 2012</para>
/// <para>It closes the popup window</para>
/// </summary>
function ClosePopupWindow() {

    $("#lightBox").hide();
    $("#lightbox_overlay").hide();
    $("#lightBox div.popUpContent div[name=ActualContent]").html("");
    RemoveThrobber();
}


function ShowLogin(sender) {
    OpenPopupWindow({
        url: MasterAbsoluteURLs._Login,
        width: 380,
        title: "Sign in to Addbuild"
    });
}

function SubmitDataLogin(returnUrl, sender) {
    AjaxFormSubmit({
        url: MasterAbsoluteURLs.Home,
        type: "POST",
        validate: true,
        parentControl: $("div[name=Login]"),
        messageControl: $("div[name=Login]  div#StatusMessagelog"),
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "-85 0" },
        complete: function () { RemoveThrobber(); },
        data: {
            username: $("div[name=Login] input[name=userid]").val(),
            password: $("div[name=Login] input[name=pass]").val(),
            rememberMe: false,
            returnUrl: returnUrl
        },
        success: function (data) {
            location.reload();
        }
    });
}

function PressSubmitDataLogin(e, returnurl) {
    var code = (e.keyCode ? e.keyCode : e.which);
    if (code == 13) {
        SubmitDataLogin(returnurl, $("div[name=Login] #btnlogin"));
    }
}

function S4() {
    return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
}
function GUID() {
    return (S4() + S4() + "-" + S4() + "-" + S4() + "-" + S4() + "-" + S4() + S4() + S4());
}

function HideMessage() {
    $('.Message_DIV').hide();
    $('.Message_DIV').find('span').text('');
}
function Message(message, MessageType) {
    
    $('.Message_DIV').show();
    var messageControl = $('.Message_DIV');//.find('span');

    if (MessageType == ActionStatus.Successfull) {
        $("html,body").animate({ scrollTop: "0" }, "slow");
        $(messageControl).html(message).removeClass("failure-msg loading-msg").addClass("success-msg").fadeIn();
        setTimeout(function () {
            $(messageControl).html('').hide();
        }, 5000);
    }
    else if (MessageType == ActionStatus.Error) {
        $("html,body").animate({ scrollTop: "0" }, "slow");
        $(messageControl).html(message).removeClass("success-msg loading-msg").addClass("failure-msg").fadeIn();
    }
    else {
        $("html,body").animate({ scrollTop: "0" }, "slow");
        $(messageControl).html(message).removeClass("success-msg failure-msg").addClass("loading-msg");
    }
}

$.InitLogin = function () {
    $('.loginBox').hide(); // login box will hide initially

    $('ul#navMenu .login').click(function () {
        $('.loginBox').slideToggle('slow', function () {
        });
    });
    $('.loginBox .logincenter a.close').click(function () {
        $('.loginBox').slideToggle('hide', function () {
        });
    });

    $("div.topheadcenter ul#navMenu a#signUp").bind("click", function () {
        LoginRegister.Registration($(this));
    });

}

/*reset unobtrusive validation*/

function ResetUnobtrusiveValidation(form) {
    form.removeData('validator');
    form.removeData('unobtrusiveValidation');
    $.validator.unobtrusive.parse(form);
}
function postifyData(value) {
    var result = {};
    var buildResult = function (object, prefix) {

        for (var key in object) {

            var postKey = isFinite(key)
                ? (prefix != "" ? prefix : "") + "[" + key + "]"
                : (prefix != "" ? prefix + "." : "") + key;

            switch (typeof (object[key])) {

                case "number": case "string": case "boolean":
                    result[postKey] = object[key];
                    break;

                case "object":
                    if (object[key] != null) {
                        if (object[key].toUTCString) result[postKey] = object[key].toUTCString().replace("UTC", "GMT");
                        else buildResult(object[key], postKey != "" ? postKey : key);
                    }
            }
        }
    };

    buildResult(value, "");
    return result;
};

Array.prototype.remove = function (elem) {
    var match = -1;

    while ((match = this.indexOf(elem)) > -1) {
        this.splice(match, 1);
    }
};

jQuery.validator.addMethod('numericgreaterthan', function (value, element, params) {
    var otherValue = $(params.element).val();

    return isNaN(value) && isNaN(otherValue) || (params.allowequality === 'True' ? parseFloat(value) >= parseFloat(otherValue) : parseFloat(value) > parseFloat(otherValue));
}, '');

jQuery.validator.unobtrusive.adapters.add('numericgreaterthan', ['other', 'allowequality'], function (options) {
    var prefix = options.element.name.substr(0, options.element.name.lastIndexOf('.') + 1),
    other = options.params.other,
    fullOtherName = appendModelPrefix(other, prefix),
    element = $(options.form).find(':input[name=' + fullOtherName + ']')[0];

    options.rules['numericgreaterthan'] = { allowequality: options.params.allowequality, element: element };
    if (options.message) {
        options.messages['numericgreaterthan'] = options.message;
    }
});

function appendModelPrefix(value, prefix) {
    if (value.indexOf('*.') === 0) {
        value = value.replace('*.', prefix);
    }
    return value;
}

function removejscssfile(filename, filetype) {    
    var targetelement = (filetype == "js") ? "script" : (filetype == "css") ? "link" : "none" //determine element type to create nodelist from
    var targetattr = (filetype == "js") ? "src" : (filetype == "css") ? "href" : "none" //determine corresponding attribute to test for
    var allsuspects = document.getElementsByTagName(targetelement)
    for (var i = allsuspects.length; i >= 0; i--) { //search backwards within nodelist for matching elements to remove
        if (allsuspects[i] && allsuspects[i].getAttribute(targetattr) != null && allsuspects[i].getAttribute(targetattr).indexOf(filename) != -1)
            allsuspects[i].parentNode.removeChild(allsuspects[i]) //remove element by calling parentNode.removeChild()
    }
}


    