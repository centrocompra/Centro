var totalRecords = parseInt(totalCount);
var filter = false, sortOrder = 'Desc', sortColumn = 'Name';
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
        data: { page_no: paging.startIndex, per_page_result: paging.pageSize, sortOrder: sortOrder, sortColumn: sortColumn, name: search_login },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Admin/Categories/TypesPaging',
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
function UpdateCategory(sender) {
    if ($(sender).val() != "0") {
        var obj = new Object();
        obj.ActionID = $(sender).val();
        obj.CategoryID = new Array();

        $('#Result').find('input[type=checkbox]').each(function () {

            if ($(this).is(':checked')) {
                obj.CategoryID.push($(this).attr("id"));
            }
        });

        if (obj.CategoryID.length > 0) {

            AjaxFormSubmit({
                type: "POST",
                validate: false,
                parentControl: $(sender).parents("form:first"),
                data: postifyData(obj),
                messageControl: null,
                throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
                url: '/Admin/Categories/UpdateCategories',
                success: function (data) {
                    if (data.Status == ActionStatus.Successfull) {
                        setTimeout(function () {
                            Message('Redirecting...', ActionStatus.Successfull);
                            window.location.href = data.Results[0];
                        }, 100);
                    }
                    else {
                        if (data.Results[1] != '' && data.Results[1] != undefined && data.Results[1] != null) {
                            var category = data.Results[1].split(',');
                            $.each(category, function () {
                                $('.' + this).addClass('failure-td');
                            });
                            Message(data.Message, ActionStatus.Error);
                        }
                    }
                }
            });
        }
    }
}

var checked = [];

function DeleteTypes(sender) {
    $('#Result').find('input[type=checkbox]').each(function () {
        if ($(this).is(':checked')) {
            checked.push($(this).attr('id'));
        }
    });
    if (checked.length <= 0) {
        Message('Please select at-least one type.', ActionStatus.Error);
        return false;
    }
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: {Ids:checked.join(',')},
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Admin/Categories/DeleteType',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                setTimeout(function () {
                    Message('Redirecting...', ActionStatus.Successfull);
                    window.location.href = window.location.href;
                }, 1000);
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}