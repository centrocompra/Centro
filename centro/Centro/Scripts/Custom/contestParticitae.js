function ShowProductPicture(sender, img) {
    img = $(sender).find('img').attr('src');
    $('#primary-image').attr({ src: img });
}

function OnComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        window.location.href = '/Contest/ViewContest';
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

function SelectProduct(sender) {    
    var desc = $('option:selected', sender).attr('desc');
    var img = $('option:selected', sender).attr('img');
    $('#img').attr('src', img);
    $('#Desc').html(desc);
}

function VoteUpParticipant(sender, contestparticipantID, contestID) {
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: null,
        data: { ContestparticipantID: contestparticipantID, ContestID: contestID },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: $(sender), offset: "5 0" },
        url: '/Contest/ParticipantVoteUp',
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

function VoteDownParticipant(sender, contestparticipantID, contestID) {
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: null,
        data: { ContestparticipantID: contestparticipantID, ContestID: contestID },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: $(sender), offset: "5 0" },
        url: '/Contest/ParticipantVoteDown',
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

function OnCommentComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
//        var comment = '<div class="white-area pos-rel contest-comments">' +
//                            '<div class="posted">@item.Username on <strong class="orng">@item.CreatedOn.ToLongDateString()</strong> said:</div>' +
//                            '<div class="comment-body">@Html.Raw(item.Comment.Replace("\r\n", "<br/>"))</div>' +
//			            '</div>';
        //        $('.comments-list').append('<div class="white-area pos-rel comments-contest">' + json.Results[0] + '</div>');
        window.location.href = window.location.href;
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

function Donate(sender, div) {
    $('.' + div).toggle();
}

function MakeDonation(sender, ContestID, Amount) {
    var amt = $('#Amount').val();
    if (amt == '' || amt == undefined || amt == null) {
        $('#Amount').addClass('input-validation-error');
        $('.field-validation-error').html('*Required.');
        return false;
    }
    var regExp = /^([0-9]{1,99999})$/;
    if (!regExp.test(amt)) {
        $('#Amount').addClass('input-validation-error');
        $('.field-validation-error').html('Invalid Amount.');
        return false;
    }
    else {
        $('#Amount').removeClass('input-validation-error');
        $('.field-validation-error').html('');
    }
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: null,
        data: { ContestID: ContestID, Amount: amt },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: $(sender), offset: "5 0" },
        url: '/Contest/MakeDonation',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);                
                setTimeout(function () {
                    window.location.href = data.Results[0];
                }, 1000);
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}