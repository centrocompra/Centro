var totalRecords = parseInt(totalCount);
var filter = false, sortOrder = 'Desc', sortColumn = 'CreatedOn';
$(document).ready(function () {
    PageNumbering(totalRecords);
});

function Paging(sender) {
    var search_login = '';
    if (filter) {
        search_login = $('#login').val().trim();
    }
    Message('Loading...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { page_no: paging.startIndex, per_page_result: paging.pageSize, sortOrder: sortOrder, sortColumn: sortColumn, username: search_login },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Admin/User/UserPaging',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $('#Result').html(data.Results[0]);
                PageNumbering(data.Results[1]);
                HideMessage();
            }
            else {
                $('.Message_DIV').find('span').text('Something went wrong');
            }
        }
    });
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

function CheckAll(sender) {
    if ($(sender).is(':checked')) {
        $('#Result').find('input[type=checkbox]').attr('checked', 'checked');
    }
    else {
        $('#Result').find('input[type=checkbox]').removeAttr('checked', 'checked');
    }
}

function UserFilter(sender) {
    filter = true;
    //paging.pageSize = $('#pageSize').val();
    Paging(sender);
}
function UpdateUsers(sender) {
    if ($(sender).val() != "0") {
        var obj = new Object();
        obj.ActionID = $(sender).val();
        obj.UserID = new Array();

        $('#Result').find('input[type=checkbox]').each(function () {

            if ($(this).is(':checked')) {
                obj.UserID.push($(this).attr("id"));
            }
        });

        if (obj.UserID.length > 0) {

            AjaxFormSubmit({
                type: "POST",
                validate: false,
                parentControl: $(sender).parents("form:first"),
                data: postifyData(obj),
                messageControl: null,
                throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
                url: '/Admin/User/UpdateUser',
                success: function (data) {
                    if (data.Status == ActionStatus.Successfull) {
                        setTimeout(function () {
                            Message('Redirecting...', ActionStatus.Successfull);
                            window.location.href = data.Results[0];
                        }, 100);
                    }
                    else {
                        $('.Message_DIV').find('span').text('Something went wrong');
                    }
                }
            });
        }
    }
}