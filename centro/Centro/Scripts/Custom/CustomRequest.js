function ChangeTitle(sender, cls, id) {
    $('.txt' + cls).val($('.lbl' + cls).text()).show();
    $('.cancel, .update').show();
    $('.edit').hide();
    $('.lbl' + cls).hide();
}

function CancelChange(sender, cls) {
    $('.txt' + cls).val('').hide();
    $('.cancel, .update').hide();
    $('.edit').show();
    $('.lbl' + cls).show();
}

function UpdateTitle(sender, cls, id) {
    var title = $('.txt' + cls).val();
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { RequestID: id, Title: title },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Message/UpdateCustomRequestTitle',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                $('.txt' + cls).val('').hide();
                $('.cancel, .update').hide();
                $('.edit').show();
                $('.lbl' + cls).text(title).show();
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

var Request = {
    OnRequestComplete: function (obj) {
        var json = $.parseJSON(obj.responseText);
        if (json.Status == ActionStatus.Successfull) {
            Message(json.Message, ActionStatus.Successfull);
            $("form[name=customRequest]").reset();
            setTimeout(function () {
                window.location.href = window.location.href;
                //                if ($('#From').val() == "MyContracts")
                //                    window.location.href = '/User/MyContracts';
                //                else
                //                    window.location.href = '/Message/Index#Request';
            }, 1000);
        }
        else {
            Message(json.Message, ActionStatus.Error);
        }
    }
};

function SendCustonRequest(sender, status) {
    $("#RequestStatus").val(status);
    $(sender).parents("form:first").submit();
}
var file_count = 0;
var files = [];
function UploadRequestAttachment(sender, form) {
    var newForm = $("<FORM>");
    newForm.attr({ method: "POST", enctype: "multipart/form-data" }).hide();
    newForm.appendTo($("body"));

    file_count++;
    var $this = $(sender), $clone = $this.clone();
    $this.after($clone).appendTo($(newForm));
    var progress = '<div class="progress"><div class="bar"><div class="percent">0%</div ></div ></div>';
    $('.uploaded-files').append('<div class="' + file_count + '">' + progress + '</div>');
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(newForm),
        form: $(newForm),
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Shops/UploadRequestAttachment',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $('#request_attachment_msg').text('');
                if (data.Results == null || data.Results == 'null') {
                    Message(data.Message, ActionStatus.Error);
                }
                else {
                    //var str = '<div class="image-box">' + data.Results[1] + '<label class="close" onclick=DeleteFile(this,"' + data.Results[0] + '")>x</label></div>';
                    var str = '<div class="image-box"><input type="text" onblur=saveUploadedFileNames(this,"' + data.Results[0] + '") value="' + data.Results[1] + '" /><label class="close" onclick=DeleteFile(this,"' + data.Results[0] + '")>x</label></div>';
                    $('.uploaded-files').find('.' + file_count).html(str);
                    files.push(data.Results[1] + ":" + data.Results[0]);
                    HideMessage();
                    $(newForm).remove();
                }
                $('.progress').remove();
            }
            else {
                $('.uploaded-files').find('.' + file_count).remove();
                Message(data.Message, ActionStatus.Error);
                $(newForm).remove();
                $('.progress').remove();
            }
        },
        bar: $('.bar'),
        percent: $('.percent'),
        status: $('#status')
    });

}

function saveUploadedFileNames(obj, fileactualName) {
    var fileName = $(obj).val();
    var fileitem = fileName + ":" + fileactualName;
    var fileExist = false;
    for (var i = 0; i < files.length; i++) {
        if (files[i].split(':')[1] == fileactualName) {
            files[i] = fileitem;
            break;
        }
    }
    $('#hdnFiles').val(files);
}


function DeleteFile(sender, file) {
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { filename: file },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Shops/DeleteTempFileAttachment',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $(sender).parent().remove();
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function DeleteContractDoc(sender, fileID) {
    var confirm = window.confirm("Are you sure that you want to delete this document?");
    if (confirm) {
        AjaxFormSubmit({
            type: "POST",
            validate: true,
            parentControl: $(sender).parents("form:first"),
            data: { FileID: fileID },
            messageControl: null,
            throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
            url: '/Message/DeleteContractDoc',
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    var cls = '.' + fileID;
                    console.log(cls);
                    $('.' + fileID).remove();
                    Message(data.Message, ActionStatus.Successfull);
                }
                else {
                    Message(data.Message, ActionStatus.Error);
                }
            }
        });
    }
}

function UpdateRequestStatus(request_id, status) {
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: null,
        data: { request_id: request_id, request_status: status },
        messageControl: null,
        url: '/Shops/UpdateRequestStatus',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                //window.location.href = '/Shops/ManageRequests';
                $('ul.custom-requests li:first').find('a').trigger('click');
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}
function DeleteRequest(request_id) {
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: null,
        data: { request_id: request_id },
        messageControl: null,
        url: '/Shops/DeleteRequest',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                window.location.href = '/Message/Index#Request';
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}



/**************** Payment section ************************/
function EscrowTermaAndConditions(sender, InvoiceID, RequestID) {
    OpenPopupWindow({
        url: '/Message/_TermsAndConditions/' + InvoiceID + '/' + RequestID,
        width: 400,
        title: "Terms and Conditions"
    });
}

function AcceptTermsAndConditions(sender, InvoiceID, RequestID) {
    if ($('input[type=checkbox]').is(':checked')) {        
        ClosePopupWindow();
        EscrowPayment(sender, InvoiceID, RequestID);
    } else {
        $('.terms span.field-validation-error').html('*Required.');
        return false;
    }
}

function EscrowPayment(sender, InvoiceID, RequestID) {
    Message('Processing...');
    $(sender).parent().html('<img src="/Content/images/loading.gif" />');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: null,
        data: { InvoiceID: InvoiceID, RequestID: RequestID },
        messageControl: null,
        url: '/Payment/EscrowPayment',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                window.location.href = data.Results[0];
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function ReleasePayment(sender, InvoiceID, RequestID) {
    $(sender).parent().html('<img id="temp_img_' + InvoiceID + '" src="/Content/images/loading.gif" />');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: null,
        data: { InvoiceID: InvoiceID, RequestID: RequestID },
        messageControl: null,
        url: '/Payment/ReleasePayment',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                $('#' + InvoiceID).html('Payment Released');
                $('#temp_img_' + InvoiceID).parent().html('Completed');
                //window.location.href = data.Results[0];
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}


function setDocStatus(sender, id, type, old) {
    var go = confirm('After approving the document, you won\'t be able to change it. Are you sure you want to approve it?');
    if (go) {
        AjaxFormSubmit({
            type: "POST",
            validate: false,
            parentControl: null,
            data: { ID: id, Type: type },
            messageControl: null,
            url: '/Message/setDocStatus',
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    //Message(data.Message, ActionStatus.Successfull);
                    //if (old == "available") $(sender).attr('title', 'pending').addClass('pending').removeClass(old).attr('onclick', "setDocStatus(this, " + id + ", " + type + ", 'pending')");
                    //else 
                    $(sender).attr('title', 'happy').addClass('happy')
                                                            .removeClass(old)
                                                            .removeAttr('onclick');
                }
                else {
                    Message(data.Message, ActionStatus.Error);
                }
            }
        });
    }
}


function DeleteInvoice(invoice_id) {
    if (confirm("Are you sure to delete this invoice")) {
        Message('Processing...');
        $.ajax({
            url: "/Message/DeleteInvoice",
            type: "Post",
            data: { InvoiceId: invoice_id },
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    $('#tr_' + invoice_id).remove();
                    Message(data.Message, ActionStatus.Successfull);
                }
                else {
                    Message(data.Message, ActionStatus.Successfull);
                }
            }
        });
    }
}