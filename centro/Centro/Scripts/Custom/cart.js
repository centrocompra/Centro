function checkout(sender, shop_id, user_id) {
    var country_id = $('#select_' + shop_id).val();
    var state_id = $('#select_state_' + shop_id).val();
    if (country_id == undefined) country_id = null;
    var note = $('#note_' + shop_id).val();
    if (note == 'You can enter any info needed to complete your order or write a note to the shop') {
        note = null;
    }
    if (country_id == '') {
        $('#select_' + shop_id).css('border-color', 'red');
        $('#select_' + shop_id).parent().append('<span class="field-validation-error select">Required.</span>');
    }
    else if (country_id == '1' && state_id == '') {
        $('#select_state_' + shop_id).css('border-color', 'red');
        $('#select_state_' + shop_id).parent().append('<span class="field-validation-error select">Required.</span>');
    }
    else {
        AjaxFormSubmit({
            type: "POST",
            validate: true,
            parentControl: $(sender).parents("form:first"),
            data: { shop_id: shop_id, country_id: country_id, state_id: state_id, user_id: user_id, note: note },
            messageControl: null,
            throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
            url: '/Payment/PreCheckout',
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    window.location.href = '/Payment/Checkout/' + data.Results[0];
                }
                else {
                    Message(data.Message, ActionStatus.Error);
                }
            }
        });
    }
}

function ChageProductQuantity(sender, shop_id, product_id) {
    var quantity = $(sender).val();
    addToCart(sender, shop_id, product_id, quantity, true);
}

function addToCart(sender, shop_id, product_id, quantity, update_quantity_from_dropDown) {
    if (update_quantity_from_dropDown != null && update_quantity_from_dropDown != undefined) {
        update_quantity_from_dropDown = update_quantity_from_dropDown;
    }
    else {update_quantity_from_dropDown = false; }
    /* Add product to cart */
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { shop_id: shop_id, product_id: product_id, quantity: quantity, update_quantity_from_dropDown: update_quantity_from_dropDown },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Cart/AddToCart',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                //onsole.log(data.Results[1]);
                if (update_quantity_from_dropDown == true)
                    UpdateCart($('#select_' + shop_id), shop_id);
                else
                    window.location.href = data.Results[0];
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function changeCountry(sender, shop_id, states) {
    var countryID = $(sender).val();
    if (countryID == 1) {
        getUSAStates('select_state_' + shop_id, states);
        return false;
    }
    else {
        $('#select_state_' + shop_id).parent().hide();
        UpdateCart(sender, shop_id, null);
    }
}

function changeState(sender, shop_id) {
    HideMessage();
    var stateID = $('#select_state_' + shop_id).val();
    if (stateID != '') {
        $(sender).parent().parent().parent().find('.currency-value').html('&nbsp;<img src="/Content/images/loading.gif" />');
        $(sender).parent().parent().parent().find('.action a').removeAttr('onclick');
        var tax = 1.0;
        AjaxFormSubmit({
            type: "POST",
            validate: true,
            parentControl: $(sender).parents("form:first"),
            data: { StateID: stateID, ShopID: shop_id },
            messageControl: null,
            throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
            url: '/Cart/GetUSASTatesTax ',
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    //var taxjson = $.parseJSON(data.Results[0]);
                    //if (taxjson.results.length > 0) {
                        //debugger;
                        //console.log(taxjson.results[0].taxSales);
                    //tax = parseFloat(taxjson.results[0].taxSales) * 100;
                        tax = parseFloat(data.Results[0]);
                        UpdateCart(sender, shop_id, tax, stateID);
                    //}
                    //else {
                      //  $(sender).parent().parent().parent().find('.currency-value').html('');
                       // Message('Sales tax API does not have data for selected state!!!', ActionStatus.Error);
                   // }
                }
                else {

                }
            }
        });
        //
    }
}

function UpdateCart(sender, shop_id, tax, state_id) {
    var countryID = $('#select_' + shop_id).val();
    var stateID = $('#select_state_' + shop_id).val();
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { ship_to_country: countryID, ship_to_state: stateID, shop_id: shop_id, state_tax: tax, stateID: state_id },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Cart/UpdateToCart',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $('.cart').html(data.Results[0]);
                $("#select_" + shop_id).val(countryID);
                if (countryID == 1) {
                    $(".select_state_" + shop_id).show();
                    getUSAStates('select_state_' + shop_id);
                    $('#select_state_' + shop_id).val(stateID);
                }
                $('.cart-link').find('a').text('[' + data.Results[1] + '] CART');
                if (data.Results[1] == '0') { $('.cart-link').find('a').attr('href', 'javascript:;'); }
            }
            else {
                $('.cart').html('No product added to cart yet!');
            }
        }
    });
}

function RemoveProduct(sender, shop_id, product_id) {
    var countryID = $("#select_" + shop_id).val();
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { shop_id: shop_id, product_id: product_id },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Cart/DeleteProductFromCart',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                UpdateCart($("#select_" + shop_id), shop_id);
            }
            else {
                $('.cart').html('No product added to cart yet!');
            }
        }
    });
}

function RemoveShop(sender, shop_id) {
    var countryID = $("#select_" + shop_id).val();
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { shop_id: shop_id },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/Cart/DeleteShopFromCart',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                UpdateCart($("#select_" + shop_id), shop_id);
            }
            else {
                $('.cart').html('No product added to cart yet!');
            }
        }
    });
}

$(document).ready(function () {
    $('.Countries').each(function () {
        var countryID = $(this).val();
        var ShopID = $(this).attr('id').split('_')[1];
        if (countryID > 1) {
            $(this).trigger('change');
        } else {
            $('#select_state_' + ShopID).trigger('change');
        }
    });
});