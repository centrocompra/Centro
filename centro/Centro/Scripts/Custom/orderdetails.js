function UpdateOrder(sender) {
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { OrderID: $('#OrderID').val(), OrderStatus: $('#OrderStatusId').val(), ShippingStatus: $('#ShippingStatusId').val(), TrackingID: $('#TrackingID').val() },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Shops/UpdateOrder',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
            }
            else {
                Message('Something went wrong', ActionStatus.Error);
            }
        }
    });
}