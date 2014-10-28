var totalRecords = parseInt(totalCount);
var ServiceId = parseInt(ActiveServiceID);


var filter = false, sortOrder = 'Desc', sortColumn = 'CreatedOn';
paging.pageSize = 8;
$(document).ready(function () {
    PageNumbering(totalRecords);
});

function Paging(sender) {

    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { service_id: ServiceId,  page_no: paging.startIndex, per_page_result: paging.pageSize, sortColumn: sortColumn, sortOrder: sortOrder},
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Shops/ShopsByServicesPaging',
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

function SortShops(sender) {
    if ($(sender).val() != '') {
        sortColumn = $(sender).val();
        sortOrder = 'Desc';
    }
    else {
        sortColumn = 'CreatedOn';
        sortOrder = 'Desc';
    }
    Paging(sender);
}
function FilterProducts(sender) {
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
function CheckAll(sender) {
    if ($(sender).is(':checked')) {
        $('#Result').find('input[type=checkbox]').attr('checked', 'checked');
    }
    else {
        $('#Result').find('input[type=checkbox]').removeAttr('checked', 'checked');
    }
}

function ShowMore(sender, user_id) {
    var fulltext = $(sender).parent().find('input#' + user_id).val();
    $(sender).parent().find('span').text(fulltext);
    $(sender).remove();
}