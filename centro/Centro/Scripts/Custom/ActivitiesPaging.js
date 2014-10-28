var filter = false, sortOrder = 'Desc', sortColumn = 'CreatedOn', search = '', CategoryID = null;
paging.pageSize = 12;

function SortContest(sender, ID,UserId) {
    CategoryID = ID;
    UserID = UserId;
    sortColumn = $(sender).val();
    sortOrder = $('option:selected', sender).attr('order');
    Paging($(sender));
}

function Paging(sender) {
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { page_no: paging.startIndex, per_page_result: paging.pageSize, sortOrder: sortOrder, sortColumn: sortColumn, search: search, UserID: UserID },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/_Activities',
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