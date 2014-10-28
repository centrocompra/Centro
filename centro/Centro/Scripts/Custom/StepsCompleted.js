var Policy = '', Section='';
function SetTab(sender) {
//    console.log(Section);
//    if ($(sender).hasClass('Sections')) {
//        if (Policy == '' || Policy == undefined) {
//            var li = $(sender).parent().parent().find('li').removeClass('active');
//            $('.Policies').parent().addClass('active');
//        } else {
//            $(sender).parent().parent().find('li').removeClass('active');
//            $('.Sections').parent().addClass('active');
//        }
//    }
//    else if ($(sender).hasClass('ShopName')) {
//        if (Policy == '' || Policy == undefined) {
//            // var li = $(sender).parent().parent().find('li').removeClass('active');
//            // $('.Sections').parent().addClass('active');
//        } else if (Section == '' || Section == undefined) {
//            $(sender).parent().parent().find('li').removeClass('active');
//            $('.Sections').parent().addClass('active');
//        }
//        else{
//            $(sender).parent().parent().find('li').removeClass('active');
//            $('.ShopName').parent().addClass('active');
//        }
//    }
//    else{
//        var li = $(sender).parent().parent().find('li').removeClass('active');
//        $(sender).parent().addClass('active');
//    }
}

function IsCompleted() {
    sender = $('#sortable');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { shop_id: shop_id },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/IsCompleted',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                /* Check for uppar tab section completeness */
                console.log(data.ID);
                if (data.ID > 0) {
                    switch (data.ID) {
                        case 1:
                            $('.create-shop-steps li:nth-child(1)').removeClass('selected').addClass('completed');
                            $('.create-shop-steps li:nth-child(2)').find('a').attr('href', '/User/GetPaid');
                            break;
                        case 2:
                            $('.create-shop-steps li:nth-child(1)').removeClass('selected').addClass('completed');
                            $('.create-shop-steps li:nth-child(2)').removeClass('selected').addClass('completed');
                            $('.create-shop-steps li:nth-child(2)').find('a').attr('href', '/User/GetPaid');
                            $('.create-shop-steps li:nth-child(3)').removeClass('selected').removeClass('completed');
                            $('.create-shop-steps li:nth-child(3)').find('a').attr('href', '/User/ListItems');
                            $('.create-shop-steps li:nth-child(4)').removeClass('selected').removeClass('completed');
                            $('.create-shop-steps li:nth-child(4)').find('a').attr('href', 'javascript:;');
                            break;
                        case 3:
                            $('.create-shop-steps li:nth-child(1)').removeClass('selected').addClass('completed');
                            $('.create-shop-steps li:nth-child(2)').removeClass('selected').addClass('completed');
                            $('.create-shop-steps li:nth-child(2)').find('a').attr('href', '/User/GetPaid');
                            $('.create-shop-steps li:nth-child(3)').removeClass('selected').addClass('completed');
                            $('.create-shop-steps li:nth-child(3)').find('a').attr('href', '/User/ListItems');
                            $('.create-shop-steps li:nth-child(4)').removeClass('selected').removeClass('completed');
                            $('.create-shop-steps li:nth-child(4)').find('a').attr('href', '/User/PreviewShop');
                            //$('.create-shop-steps li:nth-child(4)').removeClass('selected');//.addClass('completed');
                            //$('.create-shop-steps li:nth-child(4)').find('a').attr('href', '/User/PreviewShop');
                            break;
                    }
                }
                else {
                    $('.create-shop-steps li:nth-child(1)').removeClass('completed').addClass('selected');
                    $('.create-shop-steps li:nth-child(2)').removeClass('selected').removeClass('completed');
                    $('.create-shop-steps li:nth-child(2)').find('a').attr('href', 'javascript:;');
                    $('.create-shop-steps li:nth-child(3)').removeClass('completed');
                    $('.create-shop-steps li:nth-child(3)').find('a').attr('href', 'javascript:;');
                    $('.create-shop-steps li:nth-child(4)').removeClass('completed');
                    $('.create-shop-steps li:nth-child(4)').find('a').attr('href', 'javascript:;');
                }
            }
        }
    });
}