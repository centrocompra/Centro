var totalRecords = parseInt(totalCount);
var orderStatus = status;
var Id = parseInt(id);
var filter = false, sortOrder = 'Desc', sortColumn = 'CreatedOn';
paging.pageSize = 20;
$(document).ready(function () {
    PageNumbering(totalRecords);
});

function Paging(sender) {
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { page_no: paging.startIndex, per_page_result: paging.pageSize, sortColumn: sortColumn, sortOrder: sortOrder, orderStatus: status, id: Id, type: type },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Shops/OrderListingPaging',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $('#Result').html(data.Results[0]);
                PageNumbering(data.Results[1]);
                HideMessage();
            }
            else {
                Message('Something went wrong', ActionStatus.Error);
            }
        }
    });
}
function SetTab(sender) {
    var li = $(sender).parent().parent().find('li').removeClass('active');
    $(sender).parent().addClass('active');    
}

function CancelOrder(sender, OrderID) {
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { OrderID: OrderID },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Shops/CancelOrder',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                $('ul.tabs li:nth-child(1)').find('a').trigger('click');
            }
            else {
                Message('Something went wrong', ActionStatus.Error);
            }
        }
    });
}