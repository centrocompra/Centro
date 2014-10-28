function SetTab(sender) {
    var li = $(sender).parent().parent().find('li').removeClass('active');
    $(sender).parent().addClass('active');
}
function SetTabEdit() {
    $("div.tabs-outer ul.tabs").find("li").removeClass('active');
    $("div.tabs-outer ul.tabs li:first").addClass("active");
}
var shipping_count = 0;
var skip_shipping_ids = '';
var select = '';


function ChangeManufacture(sender) {
    var value = $(sender).val();
    if (value == 2) {
        $('#Manufacturer').val('');
        $('#div-Manufacturer').show();
    }
    else {
        $('#Manufacturer').val(value);
        $('#div-Manufacturer').hide();
    }
}

function CheckManufacturer(sender) {
    var value = $(sender).val();
    if (value != "1") {
        $(sender).parent().find("span.field-validation-valid:first").addClass("field-validation-error").html("Centro only allows original products to be posted and that all users must adhere to the terms of use (link), and privacy agreement (link) and agree to be legally liable and waive Replictity of any liability..etc").show();
    }
    else {
        $(sender).parent().find("span.field-validation-valid:first").addClass("field-validation-error").html("").hide();
    }
}

function AddMoreShippingLocation(sender) {
    shipping_count = $('#Shipping_Count').val();
    shipping_count++;
    if (select == '') select = $('#ShipFromId');
    var newSelect = $(select).clone();
    var ul = '<ul class="formRow">' +
                '<li class="col1 no-border">' +
                    '<select data-val-required="Required." data-val="true" style="width:170px;" name="ShipTo_' + shipping_count + '" class="input-box required shipTo">' +
                        $(newSelect).html() +
                    '</select>' +
                    '<span data-valmsg-replace="true" data-valmsg-for="ShipTo_' + shipping_count + '" class="field-validation-error"><span for="ShipTo_' + shipping_count + '" generated="true" class=""></span></span>' +
                '</li>' +
                '<li class="col2">' +
                    '<span class="currency_code">$</span><input data-val-range-min="0" data-val-range-max="100000" data-val-range="Invalid price." data-val-required="Required." data-val="true" type="text" id="Cost_' + shipping_count + '" name="Cost_' + shipping_count + '" class="input-box required number ml5" style="width:50px;" />' +
                    '<span data-valmsg-replace="true" data-valmsg-for="Cost_' + shipping_count + '" class="field-validation-error"><span for="Cost_' + shipping_count + '" generated="true" class=""></span></span>' +
                '</li>' +
                '<li class="col3">' +
                    '<span class="currency_code">$</span><input data-val-range-min="0" data-val-range-max="100000" data-val-range="Invalid price." data-val-required="Required." data-val-number="The field must be a number."  data-val="true" id="CostWithOther_' + shipping_count + '"  name="CostWithOther_' + shipping_count + '" class="input-box required number ml5" type="text" style="width:50px;" />' +
                    '<span data-valmsg-replace="true" data-valmsg-for="CostWithOther_' + shipping_count + '" class="field-validation-error"><span for="CostWithOther_' + shipping_count + '" generated="true" class=""></span></span>' +
                '</li>' +
              '<li class="col4">';
    if (shipping_count > 1)
        ul = ul + '<img src="/images/close-btn.png" alt="Remove" onclick="removeShipTo(this, ' + shipping_count + ' );"></li>';
    else
        ul = ul + '</li>';
    $('.ship-to').append(ul);
    $('#Shipping_Count').val(shipping_count);
    $('.ship-to').find("select[name=ShipTo_" + shipping_count + "]").val('');
    ResetUnobtrusiveValidation($('form'));
}

function removeShipTo(sender, row_num) {
    //shipping_count--;
    $(sender).parent().parent().remove();
    //$('#Shipping_Count').val(shipping_count);
    skip_shipping_ids = skip_shipping_ids + ',' + row_num;
    $('#Skip_Shipping_Rownum').val(skip_shipping_ids);
}

function OnProductBegin() {
    //console.log('checked=' + $('#IsDownloadable').is(':checked'));
    $('.field-validation-error-custom').remove();
    if ($('#IsDownloadable').is(':checked')) {
        //console.log('length=' + $('.uploaded-files').find('div').length);
        if ($('.uploaded-files').find('div').length > 0) {
            $('#product_file_msg').html('');
        }
        else {
            $('#product_file_msg').html('<span class="" for="productfile" generated="true">Required.</span>').show();
            return false;
        }
    }
    Message('Processing...');
    return CheckExisting();
}

function OnComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        if (json.Results[0] == "Manage Listing") {
            next = true;
        }
        if (next) {
            var a = $(".tabs li:nth-child(2)").find('a');
            $(a).trigger('click');
            SetTab(a);
        }
        else {
            var a = $(".tabs li:nth-child(1)").find('a');
            $(a).trigger('click');
            SetTab(a);
            setTimeout(function () {
                HideMessage();
            }, 3000);
        }
        IsCompleted();
    }
    else {
        if (json.ID == 0) {
            var countries = json.Results[0].split(',');
            HideMessage();
            $('.col1 select').each(function () {
                if ($.inArray($(this).val(), countries) >= 0) {
                    $(this).parent().append('<span class="field-validation-error-custom">Add <a target="_blank" href="/User/GetPaid">sales tax</a> for this country.</span>');
                    $('html, body').animate({
                        scrollTop: $(this).offset().top
                    }, 200);
                }
            });            
        }
        else
            Message(json.Message, ActionStatus.Error);
    }
}

function AddMaterial2(sender) {
    var mat = $('#Materials').val();
    var material = $(sender).parent().find('input').val();
    if (material.indexOf(">") >= 0 || material.indexOf("<") >= 0)
    {
        $('#error_material').show();
    }
    else
    {
        $('#error_material').hide();
        if (NotExists(material, $('#material-add .material')) == true) {
            if (material.trim() != '') {
                $('#material-add').append('<div class="material"><span>' + material.trim() + '</span><label class="close" onclick="removetags(this)">x</label></div>');
                $(sender).parent().find('input').val('');
            }
            setMaterial();
        }
    }
}

function Addtag2(sender) {
    //debugger;
    var tag = $(sender).parent().find('input').val();
    if (tag.indexOf(">") >= 0 || tag.indexOf("<") >= 0) {
        $('#error_Tag').show();
    }
    else {
        $('#error_Tag').hide();
        if (NotExists(tag, $('#tags-add .tag')) == true) {
            if (tag.trim() != '') {
                $('#tags-add').append('<div class="tag"><span>' + tag.trim() + '</span><label class="close" onclick="removetags(this)">x</label></div>');
                $(sender).parent().find('input').val('');
            }
            setTag();

        }
    }
}

function removetags(sender) {
    $(sender).parent().remove();
    setMaterial();
    setTag();
}

//function setMaterial() {
//    var material = ''
//    $('.material span').each(function () {
//        material = material + $(this).text() + ',';
//    });
//    $('#Materials').val(material);
//}

function setTag() {
    var tags = ''
    $('.tag span').each(function () {
        tags = tags + $(this).text() + ',';
    });
    $('#Tags').val(tags);
}

function NotExists(value, control) {
    var flag = true;
    $(control).find('span').each(function () {
        if (value.trim() == $(this).text().trim()) {
            alert('Already added!!!');
            flag = false;
        }
    });
    return flag;
}

function UploadProductPicture(sender, form) {
    //    var newForm = document.createElement('FORM');
    //    newForm.method = 'POST';
    //    newForm.enctype = "multipart/form-data";
    //    newForm.style = "display:none";

    var newForm = $("<FORM>");
    newForm.attr({ method: "POST", enctype: "multipart/form-data" }).hide();
    newForm.appendTo($("body"));

    var $this = $(sender), $clone = $this.clone();
    $this.after($clone).appendTo($(newForm));

    var progress = '<div class="progress"><div class="bar"><div class="percent">0%</div ></div ></div>';
    //$('#div-photos').append('&nbsp;<img name="img_loading" src="/Content/Images/loading.gif" />');
    $('#div-photos').append(progress);

    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(newForm),
        form: $(newForm),
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/UploadProductPicture',
        success: function (data) {
            $('#div-photos').find("img[name=img_loading]").remove();
            if (data.Status == ActionStatus.Successfull) {
                if (data.Results == null || data.Results == 'null') {
                    Message(data.Message, ActionStatus.Error);
                }
                else {
                    var div = $('#div-photos');
                    var image = '';
                    image = '<div class="image-box"><img src="/Temp/' + data.Results[0] + '/' + data.Results[2] + '" /><label onclick="DeletePic(this)" class="close" >x</label></div>';
                    $(div).append(image);
                    HideMessage();
                }
                $(newForm).remove();
                $('.progress').remove();
            }
            else {
                Message(data.Message, ActionStatus.Error);
                $(newForm).remove();
                $('.progress').remove();
            }
        },
        bar: $('.bar'),
        percent: $('.percent'),
        status: $('#status')
    });

}
var file_count = 0;
function UploadProductFile(sender, form) {
    var newForm = $("<FORM>");
    newForm.attr({ method: "POST", enctype: "multipart/form-data" }).hide();
    newForm.appendTo($("body"));

    file_count++;
    var $this = $(sender), $clone = $this.clone();
    $this.after($clone).appendTo($(newForm));
    $('.uploaded-files').append('<div class="' + file_count + '">&nbsp;<img src="/Content/Images/loading.gif" /></div>');
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(newForm),
        form: $(newForm),
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/UploadProductFile',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $('#product_file_msg').text('');
                if (data.Results == null || data.Results == 'null') {
                    Message(data.Message, ActionStatus.Error);
                }
                else {
                    $('#DownloadURL').val(data.Results[0]);
                    var str = '<div class="image-box">' + data.Results[1] + '<label class="close" onclick=DeleteFile(this,"' + data.Results[0] + '")>x</label></div>';
                    $('.uploaded-files').find('.' + file_count).html(str);
                    HideMessage();
                    $(newForm).remove();
                }
            }
            else {
                $('.uploaded-files').find('.' + file_count).remove();
                Message(data.Message, ActionStatus.Error);
                $(newForm).remove();
            }
        }
    });
}

function DeleteFile(sender, file) {
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { filename: file },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/DeleteTempProductFile',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $(sender).parent().remove();
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function DeletePic(sender, picId) {
    if (picId == undefined)
        picId = null;
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { filename: $(sender).parent().find('img').attr('src'), pic_id: picId },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/DeleteTempProductPicture',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                $(sender).parent().remove();
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function CheckExisting() {
    var values = [];
    $('.ship-to option:selected').each(function () {
        values.push($(this).attr('value'));
    });
    //values.push($('#ShipFromId').val());
    var sorted_arr = values.sort();
    var duplicates = [];
    for (var i = 0; i < values.length - 1; i++) {
        if (sorted_arr[i + 1] == sorted_arr[i]) {
            duplicates.push(sorted_arr[i]);
        }
    }
    if (duplicates.length > 0) {
        Message('Duplicate countries in Delivers to!!!', ActionStatus.Error);
        return false;
    }
    else {
        return true;
    }
}
var next = false;
function SaveNNext(sender) {
    next = true;
    $('#addListingFormBtn').trigger('click');
}

function savenNextShopName() {
    window.location.href = '/User/GetPaid'
}

function IsDownloadableCheck(sender) {
    if ($(sender).is(':checked')) {
        $('#dl_shipping_details').hide();
        $('#item_download_url').show();
        $('#item_quantity_typeI').hide();
    }
    else {
        $('#dl_shipping_details').show();
        $('#item_download_url').hide();
        $('#item_quantity_typeI').show();
    }
}
function SortBySection(sender) {
    var value = $(sender).val();
    if (value != "") {
        $(".product-grid tbody tr").hide();
        $(".product-grid tbody tr#section_" + value).show();
    }
    else {
        $(".product-grid tbody tr").show();
    }
}

function SetIsDownloadViaShip(sender) {
    if ($(sender).val() == true || $(sender).val() == 'true') {
        $('.IsDownloadViaShip').show();
        $('#item_quantity_typeI').hide();
        $('.Condition').hide();
    }
    else {
        $('#SendDownloadVia').val('');
        $('#SendDownloadViaProp').val('');
        $('.IsDownloadViaShip').hide();
        $('#item_quantity_typeI').show();
        $('.Condition').show();
    }
}

var keywords = '';

function OnCategoryChange(sender, subcategory, type) {
    var $this = $(sender);
    $("select#" + subcategory).find('option').remove().end().append('<option value="">Select a sub category...</option>').val('');
    $("select#" + type).find('option').remove().end().append('<option value="">Select a type...</option>').val('');
    if ($this.val() != "" && $this.val() != undefined) {

        AjaxFormSubmit({
            type: "Post",
            validate: false,
            messageControl: null,
            throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
            url: '/User/SubCategoriesGet',
            data: { ID: $(sender).val() },
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    if (data.Results == null || data.Results == 'null') {
                        Message(data.Message, ActionStatus.Error);
                    }
                    else {
                        FillSelectList($("select#" + subcategory), data.Results[0], false);
                        HideMessage();
                    }
                }
                else {
                    Message(data.Message, ActionStatus.Error);
                }
            }
        });
    }
}

function OnSubCategoryChange(sender, type) {
    var $this = $(sender);
    $("select#" + type).find('option').remove().end().append('<option value="">Select a type...</option>').val('');
    OnTypeChange($("select#" + type), $(sender).attr('id'));
    if ($this.val() != "" && $this.val() != undefined) {

        AjaxFormSubmit({
            type: "Post",
            validate: false,
            messageControl: null,
            throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
            url: '/User/TypesGet',
            data: { ID: $(sender).val() },
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    if (data.Results == null || data.Results == 'null') {
                        Message(data.Message, ActionStatus.Error);
                    }
                    else {
                        FillSelectList($("select#" + type), data.Results[0], false);
                        HideMessage();
                    }
                }
                else {
                    Message(data.Message, ActionStatus.Error);
                }
            }
        });
    }
}

function OnTypeChange(sender, subcategory) {
    var sub = $('#' + subcategory).find('option:selected');
    var type = $(sender).find('option:selected');
    if (sub.val() != '')
        keywords = sub.text();
    if (type.val() != '')
        keywords += ',' + type.text();
    $('#Keywords').val(keywords);
    //console.log(keywords);
}