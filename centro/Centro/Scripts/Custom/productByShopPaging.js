var totalRecords = parseInt(totalCount);
var shop_id = parseInt(ShopId);
var shop_section_id = null;
var showProductTemplate = show_productTemplate;
var inactive = null
try {
    inactive = parseInt(InactiveListing);
} catch (e) {
    inactive = null;
}

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
        data: { shop_id: shop_id, shop_section_id: shop_section_id, page_no: paging.startIndex, per_page_result: paging.pageSize, sortColumn: sortColumn, sortOrder: sortOrder, showProductTemplate: show_productTemplate, inactive: inactive },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Shops/ProductsByShopPaging',
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
        sortOrder = 'Asc';
    }
    else {
        sortColumn = 'CreatedOn';
        sortOrder = 'Desc';
    }
    Paging(sender);
}
function FilterProducts(sender, shopSectionId) {
    shop_section_id = shopSectionId;
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

function UpdateProducts(action, sender,selectedtab) {
    var obj = new Object();
    obj.ActionID = action;
    obj.ProductID = new Array();
    obj.ShopId = shop_id;
    $('#Result').find('input[type=checkbox].checkbox_list').each(function () {

        if ($(this).is(':checked')) {
            obj.ProductID.push($(this).attr("id"));
        }
    });

    if (obj.ProductID.length > 0) {

        AjaxFormSubmit({
            type: "POST",
            validate: false,
            parentControl: $(sender).parents("form:first"),
            data: postifyData({
                            obj: obj,
                            selectedTab: selectedtab
                            }),
            messageControl: null,
            throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
            url: '/User/UpdateProducts',
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    Message(data.Message, ActionStatus.Successfull);
                    $('#Result').html(data.Results[0]);
                    PageNumbering(data.Results[1]);
                  
                }
                else {
                  
                    $('.Message_DIV').find('span').text('Something went wrong');
                }
            }
        });
    }
    else {

        Message("Please select products to perform this action.", ActionStatus.Error);

        setTimeout(function () {
            HideMessage();
        }, 3000);

    }
}
