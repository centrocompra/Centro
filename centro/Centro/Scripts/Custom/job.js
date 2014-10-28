function SetTab(sender) {
    var li = $(sender).parent().parent().find('li').removeClass('active');
    $(sender).parent().addClass('active');
//    if ($(sender).hasClass('job-applied')) {
//        $('.invited').hide();
//    } else {
//        $('.invited').show();
//    }
}

function IsPrivateJob(sender) {
    if ($(sender).is(':checked')) {
        $('.seller').show();
        $("#Seller").autocomplete({
            source: function (request, response) {
                AjaxFormSubmit({
                    type: "POST",
                    validate: false,
                    parentControl: $("#Seller").parents("form:first"),
                    data: { startswith: $("#Seller").val() },
                    messageControl: null,
                    throbberPosition: { my: "left center", at: "right center", of: $("#Seller"), offset: "5 0" },
                    url: '/Message/_Usernames',
                    success: function (data) {
                        var dataaa = $.parseJSON(data.Results[0]);
                        response($.map(dataaa, function (item) {
                            return {
                                //                        label: item.name + (item.adminName1 ? ", " + item.adminName1 : "") + ", " + item.countryName,
                                label: item,
                                value: item
                            }
                        }));                        
                    }
                });
            },
            minLength: 1,
            select: function (event, ui) {
                var value = $("#Seller").val().split(';');
                if ($("#Seller").val().indexOf(';') == -1)
                    $("#Seller").val(ui.item.label + ';');
                else {
                    value.pop();
                    value.push(ui.item.label);
                    $("#Seller").val(value.join(';') + ';');
                }
                $('#Seller').autocomplete("disable");
            },
            open: function () {
                $(this).removeClass("ui-corner-all").addClass("ui-corner-top");
            },
            close: function () {
                $(this).removeClass("ui-corner-top").addClass("ui-corner-all");
            }
        });
    }
    else {
        $('.seller').hide();
    }
}

function SellerChange(sender) {
    var value = $(sender).val().trim();
    if (value.indexOf(';') == -1) {
        $('#Seller').autocomplete("enable");
    }
}

function DeleteJob(sender, jobid, from) {
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { JobID: jobid, From: from },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/DeleteJob',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                setTimeout(function () {
                    window.location.href = '/User/' + from;
                }, 1000);
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function ActivateDeactivateJob(sender, jobid, IsActive) {
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { JobID: jobid, IsActive: IsActive },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/ActivateDeactivateJob',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                if (IsActive) {
                    $(sender).replaceWith('<a href="javascript:void(0)" onclick="ActivateDeactivateJob(this, ' + jobid + ', false)" class="">Deactivate</a>');
                    $('.' + jobid).html('Active');
                }
                else {
                    var str = '<a href="/User/CreateJob/' + jobid + '/Repost" class="">Repost</a>&nbsp;' +
                              '<a href="javascript:void(0)" onclick="DeleteJob(this, ' + jobid + ', \'Recruit\')" class="">Delete</a>';
                    $(sender).parent().html(str);
                    $('.' + jobid).html('Expired');
                }
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function OnJobComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        //$("form[name=job]").reset();
        setTimeout(function () {
            //window.location.href = '/User/' + $('#From').val();
            window.location.href = '/User/Recruit';
        }, 1000);
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

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
                    var str = '<div class="image-box"><span onblur=saveUploadedFileNames(this,"' + data.Results[0] + '") value="' + data.Results[1] + '">' + data.Results[1] + '</span><label class="close" onclick=DeleteFile(this,"' + data.Results[0] + '")>x</label></div>';
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

function ApplyForJobPopup(sender, JobID) {    
    OpenPopupWindow({
        url: '/User/ApplyForJobPopup/' + JobID,
        width: 300,
        title: ""
    });
}

function ApplyForJobPopupNoShop(sender, JobID) {
    OpenPopupWindow({
        url: '/User/ApplyForJobPopupNoShop',
        width: 300,
        title: ""
    });
}


function OnApplyJobComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        ClosePopupWindow();
        Message(json.Message, ActionStatus.Successfull);        
        setTimeout(function () {            
            window.location.href = '/User/Jobs';
        }, 1000);
    }
    else {
        Message(json.Message, ActionStatus.Error);
        ClosePopupWindow();
    }
}

function WithdrawApplication(sender, JobID) {
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: null,
        data: { JobID: JobID },
        messageControl: null,
        url: '/User/WithdrawApplication',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                setTimeout(function () {
                    window.location.href = window.location.href;
                }, 2000);
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function DeleteJobEntry(sender, JobApplicationID) {
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { JobApplicationID: JobApplicationID },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/DeleteJobEntry',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                setTimeout(function () {
                    window.location.href = window.location.href;
                }, 3000);
            }
            else {
                Message('Something went wrong', ActionStatus.Error);
            }
        }
    });
}

function SetSentTo(sender, JobID, UserID) {
    Message('Processing...');
    $('.action').replaceWith('&nbsp;<img src="/Content/images/loading.gif" />');
    $(sender).replaceWith('<span class="done">Processing...</span>');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { JobID: JobID, UserID: UserID},
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/SetSentTo',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                setTimeout(function () {
                    window.location.href = window.location.href;
                }, 3000);
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function AwardJobTo(sender, JobID, UserID, ShopID) {
    Message('Processing...');
    $('.action').replaceWith('&nbsp;<img src="/Content/images/loading.gif" />');
    $(sender).replaceWith('<span class="done">Processing...</span>');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { JobID: JobID, UserID: UserID, ShopID: ShopID },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/AwardJobTo',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                setTimeout(function () {
                    window.location.href = '/User/MyContracts';
                }, 3000);
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function OnFilterComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        HideMessage();
        $('#Result').html(json.Results[0]);
        PageNumbering(json.Results[1]);
        UserID = null;
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

function ViewJob(sender, url) {
    window.location.href = url;
}

var filter = false, sortOrder = 'Desc', sortColumn = 'CreatedOn', search = '';
paging.pageSize = 6;

function RecruitePaging(sender) {        
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { page_no: paging.startIndex, per_page_result: paging.pageSize, sortOrder: sortOrder, sortColumn: sortColumn, IsAwarded: isAwared, isPaging:true },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/_MyJobs',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $('#Result').html(data.Results[0]);
                PageNumbering(data.Results[1]);
                //totalRecords = data.Results[1]
                HideMessage();
            }
            else {
                Message('Something went wrong', ActionStatus.Error);
            }
        }
    });
}

function Paging(sender) {
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { page_no: paging.startIndex, per_page_result: paging.pageSize, sortOrder: sortOrder, sortColumn: sortColumn, UserID: UserID, IsFindJobs: true },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/_Jobs',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $('#Result').html(data.Results[0]);
                PageNumbering(data.Results[1]);
                //totalRecords = data.Results[1]
                HideMessage();
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

$(document).ready(function () {
    PageNumbering(totalCount);
});