Date.prototype.today = function () {
    var dd = ((this.getDate() < 10) ? "0" : "") + this.getDate();
    var mm = (((this.getMonth() + 1) < 10) ? "0" : "") + (this.getMonth() + 1);
    var yyyy = this.getFullYear();
    return mm + "/" + dd + "/" + yyyy;
    //return ((this.getDate() < 10) ? "0" : "") + this.getDate() + "/" + (((this.getMonth() + 1) < 10) ? "0" : "") + (this.getMonth() + 1) + "/" + this.getFullYear()
};
//For the time now
Date.prototype.timeNow = function () {
    return ((this.getHours() < 10) ? "0" : "") + this.getHours() + ":" + ((this.getMinutes() < 10) ? "0" : "") + this.getMinutes() + ":" + ((this.getSeconds() < 10) ? "0" : "") + this.getSeconds();
};
var newDate = new Date();
var lastRequestSentAt = newDate.today() + ' ' + newDate.timeNow()
setInterval(function () {
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $('#unreadMessages').parents("form:first"),
        data: {},
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: $('#unreadMessages'), offset: "5 0" },
        url: '/Message/TotalUnreadMessage',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                var count = parseInt(data.Object);
                if (count > 0)
                    $('#unreadMessages').text(count);
                else
                    $('#unreadMessages').hide();
            }
            else {
                $('#unreadMessages').hide();
            }
        }
    });
    //console.log(lastRequestSentAt);
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $('#unreadFeeds').parents("form:first"),
        data: {  },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: $('#unreadFeeds'), offset: "5 0" },
        url: '/User/NewAlerts',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                var count = parseInt(data.Results[1]);
                if (count > 0) {
                    $('.activity-log-pop ul').html(data.Results[0]);                    
                    //$('#unreadFeeds').text(count).show();
                }
                else {
                    $('#unreadFeeds').hide();
                }
            }
            else {
                $('#unreadFeeds').hide();
                $('#FeedsPopup').hide();
            }
        }
    });
}, 30000);