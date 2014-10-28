function selectslot(sender) {
    if ($(sender).hasClass('available')) {
        $(sender).removeClass('available');
    }
    else if ($(sender).hasClass('selected')) {
        $(sender).removeClass('selected');
    }
    else {
        $(sender).addClass('selected');
    }
}

function SaveAvailablity(sender, shop_id) {
    var slots = [];
    $('.main-chart tr td div').each(function () {
        if ($(this).hasClass('selected') || $(this).hasClass('available')) {
            slots.push($(this).attr('row') + ',' + $(this).attr('col'));
        }
    });
    var list = slots.join(':');
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender),
        data:{slotlist: list, shop_id: shop_id, TimeZone: $('#TimeZone').val()},
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/SaveShopAvailablity',
        success: function (data) {            
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
            }
            else {
                Message(data.Message, ActionStatus.Error);                
            }
        }
    });
}