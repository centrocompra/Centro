var totalRecords = parseInt(totalCount);
var Isbuyer = buyerValue;
var isDraft = draftValue;
var filter = false, sortOrder = 'Desc', sortColumn = 'CreatedOn', type='BuyerID';
paging.pageSize = 12;
$(document).ready(function () {
    PageNumbering(totalRecords);
});

function Paging(sender) {
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { page_no: paging.startIndex, per_page_result: paging.pageSize, sortColumn: sortColumn, sortOrder: sortOrder, type: type },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Message/GetServiceContracts',
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