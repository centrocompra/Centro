paging.PagingFunction = 'MessagePaging';
var totalRecords = 0;
if (totalCount != null)
    totalRecords = parseInt(totalCount);

var filter = false, sortOrder = 'Desc', sortColumn = 'CreatedOn', search = $('#Search').val();
if (search == 'Search Messages') search = '';
paging.pageSize = 20;
$(document).ready(function () {
    //alert(message_type);
    if (message_type == 'Archived') {
        IsArchived = true;
    }
    // PageNumbering(totalRecords);
    if (message_type == 'Request') {
        $('ul.custom-requests li:first').find('a').trigger('click');
    }
    else {
        MessagePaging($('#previous'));
    }
});

function MessagePaging(sender) {
    sortColumn = 'CreatedOn'
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { page_no: paging.startIndex, per_page_result: paging.pageSize, sortOrder: sortOrder, sortColumn: sortColumn, search: search, message_type: message_type, IsRead: IsRead, IsArchived: IsArchived },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Message/_Message',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $('#Result').html(data.Results[0]);
                PageNumbering(data.Results[1]);
                totalRecords = data.Results[1]
                $('#CountInbox').text(data.Results[2]);
                HideMessage();
                //$.getScript("/Scripts/custom/messages.js", function (data, textStatus, jqxhr) { });
                //$.getScript("/Scripts/custom/paging.js", function (data, textStatus, jqxhr) { });
                // $.getScript("/Scripts/custom/messagePaging.js", function (data, textStatus, jqxhr) { });
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
        sortOrder = 'Asc';
    }
    else {
        sortColumn = 'ProductID';
        sortOrder = 'Desc';
    }    
    MessagePaging(sender);
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
    MessagePaging($(sender));
}

function searchMessage(sender) {
    Message('Processing...');    
    search = $('#Search').val().replace("'", "\'\'");
    if (search == 'Search Messages') search = '';
    //paging.pageSize = $('#pageSize').val();
    MessagePaging(sender);
}

function searching(sender, e) {
    if (e.keyCode == 13)
        searchMessage(sender);
}