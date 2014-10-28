function setHubTemplate(sender, hubTemplateId) {
    $(sender).parent().find('img').removeClass('selected-hub-template');
    $(sender).find('img').addClass('selected-hub-template');
    $('#HubTemplateID').val(hubTemplateId);

    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: null,
        data: { HubTemplateID: hubTemplateId },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Hub/_LoadTemplate',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $('#Result').html(data.Results[0]);
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
        $('#hub_result').html(json.Results[0]);
        PageNumbering(json.Results[1]);
        UserID = null;
        TopicID = $('.searchType option:selected').val();
        search = $('input[name=Job-Keyword]').val();
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

function UploadHubPicture(sender, form) {
    var newForm = $("<FORM>");
    newForm.attr({ method: "POST", enctype: "multipart/form-data" }).hide();
    newForm.appendTo($("body"));

    var $this = $(sender), $clone = $this.clone();
    $this.after($clone).appendTo($(newForm));

    var progress = '<div class="progress"><div class="bar"><div class="percent">0%</div ></div ></div>';
    $('#picture-div-hub').html(progress);

    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(newForm),
        form: $(newForm),
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Hub/UploadHubPicture',
        success: function (data) {
            $('#picture-div-hub').find("img[name=img_loading]").remove();
            if (data.Status == ActionStatus.Successfull) {
                if (data.Results == null || data.Results == 'null') {
                    Message(data.Message, ActionStatus.Error);
                }
                else {
                    var div = $('#picture-div-hub');
                    var image = '';
                    image = '<div class="image-box image-box2"><img src="/Temp/' + data.Results[0] + '/' + data.Results[2] + '" /><label onclick="DeletePic(this,null,-1)" class="close" >&nbsp;</label></div>';
                    $(div).html(image);
                    HideMessage();
                }
                $(newForm).remove();
                $('#picture-div-hub .progress').remove();
            }
            else {
                Message(data.Message, ActionStatus.Error);
                $(newForm).remove();
                $('#picture-div-hub .progress').remove();
            }
        },
        bar: $('.bar'),
        percent: $('.percent'),
        status: $('#status')
    });
}

function UploadPicture(sender, form, counter) {
    var newForm = $("<FORM>");
    newForm.attr({ method: "POST", enctype: "multipart/form-data" }).hide();
    newForm.appendTo($("body"));

    var hidden = $('<input>');
    hidden.attr({ type: 'hidden', name: 'Counter', value: counter });
    hidden.appendTo(newForm);

    var $this = $(sender), $clone = $this.clone();
    $this.after($clone).appendTo($(newForm));

    $('.file-div-' + counter).hide();
    var progress = '<div class="progress"><div class="bar"><div class="percent">0%</div ></div ></div>';
    $('#picture-div-' + counter).append(progress);

    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(newForm),
        form: $(newForm),
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Hub/UploadPicture',
        success: function (data) {
            $('#picture-div-' + counter).find("img[name=img_loading]").remove();
            if (data.Status == ActionStatus.Successfull) {
                if (data.Results == null || data.Results == 'null') {
                    Message(data.Message, ActionStatus.Error);
                }
                else {
                    var div = $('#picture-div-' + counter);
                    var image = '';
                    image = '<div class="image-box image-box1"><img src="/Temp/' + data.Results[0] + '/' + data.Results[2] + '" /><label onclick="DeletePic(this,null,' + counter + ')" class="close" >x</label></div>';
                    $(div).append(image);
                    HideMessage();
                }
                $(newForm).remove();
                $('#picture-div-' + counter + ' .progress').remove();
            }
            else {
                $('.file-div-' + counter).show();
                Message(data.Message, ActionStatus.Error);
                $(newForm).remove();
                $('#picture-div-' + counter + ' .progress').remove();
            }
        },
        bar: $('.bar'),
        percent: $('.percent'),
        status: $('#status')
    });
}


function DeletePic(sender, picId, counter) {
    if (picId == undefined || picId == null)
        picId = null;
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { filename: $(sender).parent().find('img').attr('src'), pic_id: picId },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Hub/DeleteTempPicture',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $(sender).parent().remove();
                $('.file-div-' + counter).show();
                $('#hdnImageName_' + counter).val('');
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

var count = 1;
var rows = [];
var except = [];
rows.push(0);
//except.push(0);

function AddRow(sender) {
    var str = '<div class="mb20 row autosuggest-wrapper1  autosuggest-wrapper-' + count + '">' + //
		        '<div class="floatRight width50">' +
		        '<blockquote class="pdl12p gray-border-left ml20 ">' +
                    '<div class="file-div-' + count + '">' +
                        '<label class="add-photo-btn"><strong>upload</strong><span>' +
                        '<input type="file" id="picture" name="file" onchange="UploadPicture(this, \'picture-form\',' + count + ');"/></span></label>' +
                    '</div>' +
                    '<div id="picture-div-' + count + '"></div>' +
                 '</blockquote>' +
		        '<div class="submit ml10">' +
			        '<input type="button" class="button-minus" value="Remove" onclick="RemoveRow(this, \'autosuggest-wrapper-' + count + '\',' + count + ');" />' +
		        '</div>' +
                '</div>' +
                '<textarea id="Count_' + count + '" class="width45" name="Count_' + count + '" data-val-required="*Required." data-val="true"></textarea>' +
                '<span data-valmsg-replace="true" data-valmsg-for="Count_' + count + '" class="field-validation-valid ml15"></span>' +
	        '</div>';
    $('.hub-create-list').append(str);
    rows.push(count);
    $('#Rows').val(rows.join(','));
    //$("#Count_" + count).jqte();
    count = count + 1;
    ResetUnobtrusiveValidation($('form'));
}

function RemoveRow(sender, cls, counter) {
    $('.' + cls).remove();
    except.push(counter);
    $('#Except').val(except.join(','));

}

function AddTemplate2(sender) {
    var str = '<div class="autosuggest-wrapper1 template2_' + count + '">' +
		            '<textarea id="Count_' + count + '" name="Count[' + count + ']" rows="15" cols="70" style="width: 100%"><div id="picture-div-' + count + '"></div>&nbsp;' +
	                '</textarea>' + '<label class="add-photo-btn"><strong>upload</strong><span>' +
                            '<input type="file" id="picture" name="file" onchange="UploadPicture2(this, \'picture-form\',' + count + ');"/></span></label>' +
		            '<div class="submit mt10">' +
			            '<input type="button" onclick="RemoveTemplate2(this,' + count + ')" class="button-minus" value="Remove">' +
		            '</div>' +
            '</div>';
    $('.hub-create-list').append(str);
    $("#Count_" + count).jqte();
    rows.push(count);
    $('#Rows').val(rows.join(','));
    count++;
}

function RemoveTemplate2(sender, counter) {
    $('.template2_' + counter).hide();
    except.push(counter);
    $('#Except').val(except.join(','));
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { count: counter },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Hub/DeleteTemp2Picture',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {

            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function OnHubComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        setTimeout(function () {
            window.location.href = '/Hub/Index';
        }, 3000);
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

function UploadPicture2(sender, form, counter) {
    $('.jqte_editor').focus();
    var newForm = $("<FORM>");
    newForm.attr({ method: "POST", enctype: "multipart/form-data" }).hide();
    newForm.appendTo($("body"));

    var hidden = $('<input>');
    hidden.attr({ type: 'hidden', name: 'Counter', value: counter });
    hidden.appendTo(newForm);

    var $this = $(sender), $clone = $this.clone();
    $this.after($clone).appendTo($(newForm));

    $('.file-div-' + counter).hide();
    var progress = '<div class="progress"><div class="bar"><div class="percent">0%</div ></div ></div>';
    $('#picture-div-' + counter).append(progress);

    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(newForm),
        form: $(newForm),
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Hub/UploadPicture',
        success: function (data) {
            $('#picture-div-' + counter).remove();
            if (data.Status == ActionStatus.Successfull) {
                if (data.Results == null || data.Results == 'null') {
                    Message(data.Message, ActionStatus.Error);
                }
                else {
                    var div = $('#picture-div-' + counter).parent('.jqte_editor');
                    var image = '';
                    $(div).focus();
                    HideMessage();

                    // get the selection range (or cursor     position)
                    var range = window.getSelection().getRangeAt(0);

                    // create image
                    var newImg = document.createElement('img');
                    newImg.src = '/Temp/' + data.Results[0] + '/' + data.Results[1];
                    newImg.className = "inner-image max-w100pc";

                    var rangeClass = $(range.startContainer.parentNode).attr('class');
                    if (rangeClass == undefined)
                        rangeClass = $(range.startContainer.parentNode).parent().attr('class');
                    console.log(rangeClass);
                    if (rangeClass == 'percent')
                        console.log($(rangeClass).parent().html());
                    if (rangeClass == 'jqte_editor' || rangeClass == 'jqte jqte_focused') {
                        range.insertNode(newImg);
                    }

                }
                $(newForm).remove();
                $('#picture-div-' + counter + ' .progress').remove();
            }
            else {
                $('.file-div-' + counter).show();
                Message(data.Message, ActionStatus.Error);
                $(newForm).remove();
                $('#picture-div-' + counter + ' .progress').remove();
            }
        },
        bar: $('.bar'),
        percent: $('.percent'),
        status: $('#status')
    });
}

/* Old Concept of adding Single image */
//function UploadPicture2(sender, form, counter) {
//    var newForm = $("<FORM>");
//    newForm.attr({ method: "POST", enctype: "multipart/form-data" }).hide();
//    newForm.appendTo($("body"));

//    var hidden = $('<input>');
//    hidden.attr({ type: 'hidden', name: 'Counter', value: counter });
//    hidden.appendTo(newForm);

//    var $this = $(sender), $clone = $this.clone();
//    $this.after($clone).appendTo($(newForm));

//    $('.file-div-' + counter).hide();
//    var progress = '<div class="progress"><div class="bar"><div class="percent">0%</div ></div ></div>';
//    $('#picture-div-' + counter).append(progress);

//    AjaxFormSubmit({
//        type: "POST",
//        validate: true,
//        parentControl: $(newForm),
//        form: $(newForm),
//        messageControl: null,
//        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
//        url: '/Hub/UploadPicture',
//        success: function (data) {
//            $('#picture-div-' + counter).find("img[name=img_loading]").remove();
//            if (data.Status == ActionStatus.Successfull) {
//                if (data.Results == null || data.Results == 'null') {
//                    Message(data.Message, ActionStatus.Error);
//                }
//                else {
//                    var div = $('#picture-div-' + counter).parent('.jqte_editor');
//                    $('#picture-div-' + counter).remove();
//                    var image = '';
//                    image = '<div id="picture-div-' + counter + '" aria-disabled="true"><div class="image-box"><img src="/Temp/' + data.Results[0] + '/' + data.Results[1] + '" /></div></div>&nbsp;';
//                    $(div).prepend(image);
//                    $(div).focus();
//                    HideMessage();
//                }
//                $(newForm).remove();
//                $('#picture-div-' + counter + ' .progress').remove();
//            }
//            else {
//                $('.file-div-' + counter).show();
//                Message(data.Message, ActionStatus.Error);
//                $(newForm).remove();
//                $('#picture-div-' + counter + ' .progress').remove();
//            }
//        },
//        bar: $('.bar'),
//        percent: $('.percent'),
//        status: $('#status')
//    });
//}
/* Old Concept of adding Single image */

function ActivateDeactivateHub(sender, HubID, HubStatus) {
    //Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: null,
        data: { HubID: HubID, HubStatus: HubStatus },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Hub/ActivateDeactivateHub',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                //Message(data.Message, ActionStatus.Successfull);
                //setTimeout(function () {
                window.location.href = '/Hub/Index';
                //}, 1000);
            }
            else {
                Message(data.Message, ActionStatus.Error);

            }
        }
    });
}

function SetHubUrl(obj) {
    var username = $('#username').val();
    var port = window.location.port;
    var hubTopic = $('#HubTopicID option:selected').html();
    hubTopic = hubTopic.replace(/ /g, "-");
    var base_url = 'http://' + window.location.hostname + ":" + port + "/Hubs/" + username + "/{id}/" + hubTopic + "/";
    var hubTitle = $('#Title').val().trim();
    if (port != "80") {
        hubTitle = hubTitle.replace(/ /g, "-");
        hubTitle = hubTitle.length > 25 ? hubTitle.substring(0, 24) : hubTitle;
        $('#spnHubUrl').html(hubTopic + "/" + hubTitle);
        hubTitle = base_url + hubTitle;
        $('#HubURL').val(hubTitle);
    }

}

//For Edit Hub
var countEdit;
var rowsEdit = [];
var exceptEdit = [];
$(document).ready(function () {
    countEdit = $('#ContentCount').val();
    if (parseInt(countEdit) > 0) {
        for (var i = 0; i < countEdit; i++) {
            rowsEdit.push(i);
        }
        $('#RowsEdit').val(rowsEdit.join(','));
    }

});

function AddRowEdit(sender) {

    var str = '<div class="mb20  row  autosuggest-wrapper-' + countEdit + '">' +
		        '<div class="floatRight width50">' +
		        '<blockquote class="pdl12p gray-border-left ml20 ">' +
                    '<div class="file-div-' + countEdit + '">' +
                        '<label class="add-photo-btn"><strong>upload</strong><span>' +
                        '<input type="file" id="picture" name="file" onchange="UploadPicture(this, \'picture-form\',' + countEdit + ');"/></span></label>' +
                    '</div>' +
                    '<div id="picture-div-' + countEdit + '"></div>' +
                 '</blockquote>' +
		        '<div class="submit ml10">' +
			        '<input type="button" class="button-minus" value="Remove" onclick="RemoveRowEdit(this, \'autosuggest-wrapper-' + countEdit + '\',' + countEdit + ');" />' +
		        '</div>' +
                '</div>' +
                '<textarea id="Count_' + countEdit + '" class="width45" name="Count_' + countEdit + '" data-val-required="*Required." data-val="true"></textarea>' +
                '<span data-valmsg-replace="true" data-valmsg-for="Count_' + countEdit + '" class="field-validation-valid ml15"></span>' +
	        '</div>';
    $('.hub-create-list').append(str);
    rowsEdit.push(countEdit);
    $('#RowsEdit').val(rowsEdit.join(','));
    //$("#Count_" + count).jqte();
    countEdit++;
    ResetUnobtrusiveValidation($('form'));
}

function RemoveRowEdit(sender, cls, counter) {
    $('.' + cls).hide();
    exceptEdit.push(counter);
    $('#ExceptEdit').val(exceptEdit.join(','));
    console.log($('#ExceptEdit').val());
}

function AddTemplateEdit2(sender) {
    var str = '<div class="autosuggest-wrapper template2_' + countEdit + '">' +
		            '<textarea id="Count_' + countEdit + '" name="Count[' + countEdit + ']" rows="15" cols="70" style="width: 100%"><div id="picture-div-' + countEdit + '"></div>&nbsp;' +
	                '</textarea>' + '<label class="add-photo-btn"><strong>upload</strong><span>' +
                            '<input type="file" id="picture" name="file" onchange="UploadPicture2(this, \'picture-form\',' + countEdit + ');"/></span></label>' +
		            '<div class="submit mt10">' +
			            '<input type="button" onclick="RemoveTemplate2(this,' + countEdit + ')" class="button-minus" value="Remove">' +
		            '</div>' +
            '</div>';
    $('.hub-create-list').append(str);
    $("#Count_" + countEdit).jqte();
    rowsEdit.push(countEdit);
    $('#RowsEdit').val(rowsEdit.join(','));
    countEdit++;
}

function RemoveTemplateEdit2(sender, counter) {
    $('.template2_' + counter).hide();
    exceptEdit.push(counter);
    $('#ExceptEdit').val(exceptEdit.join(','));
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { count: counter },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Hub/DeleteTemp2Picture',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {

            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function OnHubEditComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        setTimeout(function () {
            window.location.href = '/Hub/Index';
        }, 3000);
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

//function UploadPictureEdit2(sender, form, counter) {
//    var newForm = $("<FORM>");
//    newForm.attr({ method: "POST", enctype: "multipart/form-data" }).hide();
//    newForm.appendTo($("body"));

//    var hidden = $('<input>');
//    hidden.attr({ type: 'hidden', name: 'Counter', value: counter });
//    hidden.appendTo(newForm);

//    var $this = $(sender), $clone = $this.clone();
//    $this.after($clone).appendTo($(newForm));

//    $('.file-div-' + counter).hide();
//    var progress = '<div class="progress"><div class="bar"><div class="percent">0%</div ></div ></div>';
//    $('#picture-div-' + counter).append(progress);

//    AjaxFormSubmit({
//        type: "POST",
//        validate: true,
//        parentControl: $(newForm),
//        form: $(newForm),
//        messageControl: null,
//        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
//        url: '/Hub/UploadPicture',
//        success: function (data) {
//            $('#picture-div-' + counter).find("img[name=img_loading]").remove();
//            if (data.Status == ActionStatus.Successfull) {
//                if (data.Results == null || data.Results == 'null') {
//                    Message(data.Message, ActionStatus.Error);
//                }
//                else {
//                    var div = $('#picture-div-' + counter);
//                    var image = '';
//                    image = '<div class="image-box"><img src="/Temp/' + data.Results[0] + '/' + data.Results[1] + '" /></div>&nbsp;';
//                    $(div).html(image);
//                    HideMessage();
//                }
//                $(newForm).remove();
//                $('#picture-div-' + counter + ' .progress').remove();
//            }
//            else {
//                $('.file-div-' + counter).show();
//                Message(data.Message, ActionStatus.Error);
//                $(newForm).remove();
//                $('#picture-div-' + counter + ' .progress').remove();
//            }
//        },
//        bar: $('.bar'),
//        percent: $('.percent'),
//        status: $('#status')
//    });
//}
//

function HubSaved(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        var commentId = json.ID;
        $('#Comment').val('');
        $.ajax({
            url: "/Hub/HubCommentByCommentID/",
            type: "Post",
            data: { CommentID: commentId },
            success: function (data) {
                $('#HubCommentRes').prepend(data);
                $('#noComment').hide();
            }
        });
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

function DeleteComment(commentID) {
    $.ajax({
        url: "/Hub/DeleteHubComment",
        type: "Post",
        data: { CommentID: commentID },
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                $('#Div_' + commentID).remove();
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        },
        error: function (error) {
            Message("Comment not deleted.");
        }
    });
}





function SortHub(sender) {
    sortColumn = $(sender).val();
    Paging($(sender));
    //$.ajax({
    //    url: "/Hub/_Hubs/",
    //    type: "Post",
    //    data: { SortBy: sort_by },
    //    success: function (data)
    //    {
    //        $('#hub_result').html(data);
    //    }
    //})
}


paging.pageSize = 10;
var userID = null, sort_by = "", search = "", TopicID = null, sortOrder = 'Desc', sortColumn = 'CreatedOn', IsManageHubs = false;
function Paging(sender) {
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { page_no: paging.startIndex, per_page_result: paging.pageSize, sortOrder: sortOrder, sortColumn: sortColumn, search: search, TopicID: TopicID, UserID: userID, IsManageHubs: IsManageHubs },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Hub/_Hubs',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $('#hub_result').html(data.Results[0]);
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

function setUserAndTopicID(userid, topicID)
{
    userID = userid;
    TopicID = topicID;
}
