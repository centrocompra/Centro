var totalRecords = parseInt(totalCount);
var filter = false, sortOrder = 'Desc', sortColumn = 'CreatedOn';
paging.pageSize = 12;
$(document).ready(function () {
    PageNumbering(totalRecords);
});

function Paging(sender) {
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { page_no: paging.startIndex, per_page_result: paging.pageSize, sortOrder: sortOrder, sortColumn: sortColumn, search: '', category: category },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Admin/Product/ProductsPaging',
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

function SearchProducts(sender) {
    var value = $('#ddlCategory :selected').text();
    if (value != '') {
        category = value;
        Paging(sender);
    }    
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

function SetAsFeatured(sender) {
    var allProducts = [];
    var selectedProducts = [];
    $('#Result input[type=checkbox].featured').each(function () {
        allProducts.push($(this).attr('value'));
        if ($(this).is(':checked')) {
            selectedProducts.push($(this).attr('value'));
        }
    });
//    if (selectedProducts.length <= 0) {
//        Message('Please select at-least one product', ActionStatus.Error);
//        return false;
//    }
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { allProducts: allProducts.join(','), selectedProducts: selectedProducts.join(',') },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Admin/Product/SetAsFeatured',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                setTimeout(function () { HideMessage(); }, 3000);
            }
            else {
                Message('Something went wrong', ActionStatus.Error);
            }
        }
    });
}
function CheckAll(sender) {
    if ($(sender).is(':checked')) {
        $('#Result').find('input[type=checkbox].pro').attr('checked', 'checked');
    }
    else {
        $('#Result').find('input[type=checkbox].pro').removeAttr('checked', 'checked');
    }
}


function MoveToCategory(sender, category) {
    var CatID = $(category).val();
    var selectedProducts = [];
    $('#Result input[type=checkbox].pro').each(function () {
        if ($(this).is(':checked')) {
            selectedProducts.push($(this).attr('id'));
        }
    });
    var pids = selectedProducts.join(',');
    if (selectedProducts.length <= 0) {
        Message('Please select at-least one product', ActionStatus.Error);
        return false;
    }
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { CategoryID: CatID, Pids: pids },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Admin/Product/MoveToCategory',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                 setTimeout(function () { HideMessage(); }, 3000);
            }
            else {
                Message('Something went wrong', ActionStatus.Error);
            }
        }
    });
}