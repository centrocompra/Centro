var filter = false, sortOrder = 'Desc', sortColumn = 'CreatedOn', search = '', CategoryID = CID, UserID = UID;
paging.pageSize = 9;

function SortContest(sender) {
    sortColumn = $(sender).val();
    sortOrder = $('option:selected', sender).attr('order');
    Paging($(sender));
}

function Paging(sender) {
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { page_no: paging.startIndex, per_page_result: paging.pageSize, sortOrder: sortOrder, sortColumn: sortColumn, search: search, CategoryID: CategoryID, UserID: UserID },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Products/_Contest',
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

$(document).ready(function () {
    PageNumbering(totalCount);
});