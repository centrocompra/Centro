function LeaveFeedBack(id,type,requestType) {
   OpenPopupWindow({
       url: '/FeedBack/LeaveFeedBackPartial?id=' + id + '&type=' + type +'&requestType='+requestType,
        width: 400,
        title: "Leave Feedback"
   });
}

function SaveRating(obj)
{
    var rat = $(obj).children('input[type=hidden]').val();
    $('#Rating').val(rat);
}

function feedbackSaved(obj)
{
    ClosePopupWindow();
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        $('ul.tabs li:nth-child(1)').find('a').trigger('click');
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
    
    
}