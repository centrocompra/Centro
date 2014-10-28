var filter = false, sortOrder = 'Desc', sortColumn = 'CreatedOn', search = '';
paging.pageSize = 20;

function SortContest(sender) {
    sortColumn = $(sender).attr('col');
    sortOrder = sortOrder == 'Desc' ? 'Asc' : 'Desc';
    Paging($(sender));
}

function Paging(sender) {
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { page_no: paging.startIndex, per_page_result: paging.pageSize, sortOrder: sortOrder, sortColumn: sortColumn, search: search, CategoryID: null, UserID: null},
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Admin/Contest/_Contest',
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
var ids = [];

function Deactivate(sender) {
    ids.length = 0;
    $('input[type=checkbox].pro').each(function () {
        if ($(this).is(':checked')) {
            ids.push($(this).attr('id'));
        }
    });
    if (ids.length <= 0) {
        Message('Please select at-least one challenge', ActionStatus.Error);
        return false;
    }
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { Ids: ids.join(',') },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Admin/Contest/DeactivateContest',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                setTimeout(function () {
                    window.location.href = window.location.href;
                }, 3000);
            }
            else {
                Message('Something went wrong', ActionStatus.Error);
            }
        }
    });
}

function CheckAll(sender) {
    if ($(sender).is(':checked')) {
        $('#Result').find('input[type=checkbox]').attr('checked', 'checked');
    }
    else {
        $('#Result').find('input[type=checkbox]').removeAttr('checked', 'checked');
    }
}


function Activate(sender) {
    ids.length = 0;
    $('input[type=checkbox].pro').each(function () {
        if ($(this).is(':checked')) {
            ids.push($(this).attr('id'));
        }
    });
    if (ids.length <= 0) {
        Message('Please select at-least one challenge', ActionStatus.Error);
        return false;
    }
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { Ids: ids.join(',') },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Admin/Contest/ActivateContest',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                setTimeout(function () {
                    window.location.href = window.location.href;
                }, 3000);
            }
            else {
                Message('Something went wrong', ActionStatus.Error);
            }
        }
    });
}

function searchContest(sender) {
    search = $('#contest').val();
    Paging(sender);
}


