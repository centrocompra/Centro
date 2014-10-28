function SetTab(sender) {
    var li = $(sender).parent().parent().find('li').removeClass('active');
    $(sender).parent().addClass('active');
}

var hash = window.location.hash.substring(1);
$(document).ready(function () {
    if (hash =="Shops") {
        $('.tabs li:nth-child(2) a').trigger('click');
    }
});