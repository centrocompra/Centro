var totalRecords = parseInt(totalCount);
var filter = false, sortOrder = 'Desc', sortColumn = 'CreatedOn';
paging.pageSize = 6;
$(document).ready(function () {
    PageNumbering(totalRecords);
});

function SeeAll(sender) {
    Message('Processing...');
    paging.pageSize = 16;
    paging.startIndex = 1;
    paging.currentPage = 0;
    Paging(sender);
    $('.home-specific').remove();
}

function Paging(sender) {
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { page_no: paging.startIndex, per_page_result: paging.pageSize, sortOrder: sortOrder, sortColumn: sortColumn, search: '' },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Products/FeaturedProductsPaging',
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

function SortProducts(sender) {
    if ($(sender).val() != '') {
        sortColumn = $(sender).val();
        sortOrder = $(sender).attr('order')!=''?$(sender).attr('order'): 'Asc';
    }
    else {
        sortColumn = 'ProductID';
        sortOrder = 'Desc';
    }    
    Paging(sender);
}

function sort(sender) {
    if ($(sender).hasClass('up')) {
        sortOrder = "Asc";
        $(sender).removeClass('up').addClass('down');
        $(sender).attr('src', MasterAbsoluteURLs.siteUrl + 'images/desc.gif');
    }
    else {
        sortOrder = "Desc";
        $(sender).removeClass('down').addClass('up');
        $(sender).attr('src', MasterAbsoluteURLs.siteUrl + 'images/asc.gif');
    }
    sortColumn = $(sender).parent().find('span').attr('class')
    Paging($(sender));
}

function UserFilter(sender) {
    filter = true;
    //paging.pageSize = $('#pageSize').val();
    Paging(sender);
}
