function SetTab(sender) {
    var li = $(sender).parent().parent().find('li').removeClass('active');
    $(sender).parent().addClass('active');
}

function ShowProductPicture(sender, image, username) {
    var src = $('#primary-image').attr('src');
    src = src.substring(0, src.lastIndexOf('/') + 1);
    $('#primary-image').attr('src', src + image);
}

function OpenShop(sender, shop_id) {
    var go = true;
    $('.agree').each(function () {
        if (!$(this).is(':checked')) {
            go = false;
        }
    });
    if (!go) {
        Message('Please accept all terms and conditions before opening your shop', ActionStatus.Error);
        return false;
    }
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { shop_id: shop_id },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/OpenShop',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                window.location.href = data.Results[0];
                //setTimeout(function () { HideMessage() }, 3000);
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function ShopInformation(tab, shop_id) {
    var title = '';
    if (tab == 'SellerInfo') {
        title = 'Seller Information';
    }
    else if (tab == 'Policy') {
        title = 'Shop Policies';
    }
    else if (tab == 'MessageForBuyers') {
        title = 'Message For Buyers';
    }
    OpenPopupWindow({
        url: '/User/ShopInformation?shop_id=' + shop_id + '&tab=' + tab,
        width: 400,
        title: title
    });
}

function editProduct(sender, product_id) {
    //window.location
}

function Follow(id) {
    OpenPopupWindow({
        url: '/Home/Follow/' + id,
        width: 300,
        title: ""
    });
}

var FollowArray = [];
var FavoriteArray = [];

function UpdateFollowArr(sender) {
    FollowArray.length = 0;
    $('#follow li').each(function () {
        if ($(this).find('input[type=checkbox]').is(':checked')) {
            FollowArray.push($(this).find('input[type=checkbox]').val());
        }
    });
    $('#FollowArr').val(FollowArray.join(','));
}

function FollowComplete(obj) {
    ClosePopupWindow();
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}