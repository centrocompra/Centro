function NoShop() {
    OpenPopupWindow({
        url: '/User/NoShop',
        width: 300,
        title: ""
    });
}

function gotoAvailability() {
    window.location = '/User/Shop#ShopAvailability';
    $('ul.tabs li:nth-child(4)').find('a').trigger('click');
}

function GotoFavShops(sender, shops) {
    window.location = '/User/FavoriteProducts#' + shops;
    $('ul.tabs li:nth-child(2)').find('a').trigger('click');
}

function openLogin() {
    OpenPopupWindow({
        url: '/Home/_Login',
        width: 500,
        title: "Centro Signup"
    });
}

function OnBegin(obj) {
    Message('Processing...');
}

function OnComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        ClosePopupWindow();
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

function search(sender) {
    var search = $('div.search input[type=text]').val();
    var type = $('div.search select').val();
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { search: search, type: type },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Home/Search',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                IsCompleted();
                Message(data.Message, ActionStatus.Successfull);
                setTimeout(function () { HideMessage() }, 3000);
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function AddToFavorite(sender, id, forProduct) {
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { ID: id, ForProduct: forProduct },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Home/AddToFavorite',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                //Message(data.Message, ActionStatus.Successfull);
                $(sender).attr('title', 'Remove from favorites')
                         .addClass('userfav').removeClass('add-fav')
                         .attr('onclick', 'RemoveFromFavorite(this, ' + id + ',' + forProduct + ')')
                         .html('<img src="/Content/Images/heart1-min.png"><span>Remove from favorites</span>');

            }
            else {
                window.location.href = '/Home/Signin';
            }
        }
    });
}

function RemoveFromFavorite(sender, id, forProduct) {
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { ID: id, ForProduct: forProduct },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Home/RemoveFromFavorite',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                //Message(data.Message, ActionStatus.Successfull);
                $(sender).attr('title', 'Add to favorites')
                             .addClass('add-fav').removeClass('userfav')
                             .attr('onclick', 'AddToFavorite(this, ' + id + ', ' + forProduct + ')')
                             .html('<img src="/Content/Images/heart1-plus.png"><span>Add to favorites</span>');


            }
            else {
                window.location.href = '/Home/Signin';
            }
        }
    });
}

function SeeAllFollowers(sender) {
    $('#followers').removeClass('followed-by-top');
    $(sender).replaceWith('<a href="javascript:;" class="seeall" onclick="SeeLessFollowers(this)">View Less</a>');
}

function SeeLessFollowers(sender) {
    $('#followers').addClass('followed-by-top');
    $(sender).replaceWith('<a href="javascript:;" class="seeall" onclick="SeeAllFollowers(this)">View All</a>');
}