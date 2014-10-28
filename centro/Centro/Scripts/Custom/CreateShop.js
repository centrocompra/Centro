// Getting hash from url if exists
var hash = window.location.hash.substring(1)

$(document).ready(function () {
    if (hash.toLowerCase() == 'shopavailability') {
        $('ul.tabs li:last-child').find('a').trigger('click');
    }
    else {
        $('ul.tabs li:first-child').find('a').trigger('click');
    }
});


/*********************** Shop Information Tab *****************************/
function FileSize() {
    var input, file, fileExt;
    // (Can't use `typeof FileReader === "function"` because apparently
    // it comes back as "object" on some browsers. So just see if it's there
    // at all.)
    if (!window.FileReader) {
        //debugger;
        file = document.getElementById("shopBanner");
        fileExt = file.value.substr(file.value.lastIndexOf('.') + 1);
        if (file.size > 2048000) {
            Message("File must be less than 2 MB", ActionStatus.Error);
            $('#upload').attr('disabled', 'disabled');
        }
        else if (fileExt.toLowerCase() != "jpeg" && fileExt.toLowerCase() != "jpg" && fileExt.toLowerCase() != "gif" && fileExt.toLowerCase() != "bmp" && fileExt.toLowerCase() != "png") {
            Message(file.name + " is an invalid image file, valid files are *.jpg, *.jpeg, *.gif, *.bmp, *.png", ActionStatus.Error);
            $('#upload').attr('disabled', 'disabled');
        }
        else {
            HideMessage();
            $('#upload').removeAttr('disabled');
        }
        return;
    }

    input = document.getElementById('shopBanner');
    file = input.files[0];
    fileExt = file.name.substr(file.name.lastIndexOf('.') + 1).toLowerCase();
    if (file.size > 2048000) {
        Message("File must be less than 2 MB", ActionStatus.Error);
        $('#upload').attr('disabled', 'disabled');
    }
    else if (fileExt.toLowerCase() != "jpeg" && fileExt.toLowerCase() != "jpg" && fileExt.toLowerCase() != "gif" && fileExt.toLowerCase() != "bmp" && fileExt.toLowerCase() != "png") {
        //$('.Message_DIV').show().find('span').text(file.name +" is an invalid image file, valid files are *.jpg, *.jpeg, *.gif, *.bmp, *.png");
        Message(file.name + " is an invalid image file, valid files are *.jpg, *.jpeg, *.gif, *.bmp, *.png", ActionStatus.Error);
        $('#upload').attr('disabled', 'disabled');
    }
    else {
        HideMessage();
        $('#upload').removeAttr('disabled');
    }
}

function ShopInfo(sender, next) {
    if($('#TermsAndCondition').is(':checked')!=true){
        $('.TermsAndCondition').text('*Required');
        return false;
    }
    $('.TermsAndCondition').text('');
    setServices();
    setShopSpeciality();
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        form: $(sender).parents("form:first"),
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/ShopInfo',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                IsCompleted();
                //                if (old_shop_id == 0) {
                //                    setTimeout(function () {
                //                        window.location.href = '/User/Shop';
                //                    }, 1000);
                //                }
                //                else 
                if (next == 'next') {
                    //debugger;
                    var nextURL = '<a class="Sections" data-ajax="true" data-ajax-mode="replace" data-ajax-update="#ui-tabs" href="/User/_Sections?shop_id=shopId" onclick="SetTab(this)">Sections</a>';
                    nextURL = nextURL.replace('shopId', data.ID);
                    $(".tabs li:nth-child(2)").html(nextURL)
                    var a = $(".tabs li:nth-child(2)").find('a');
                    $(a).trigger('click');
                    SetTab(a);
                }
                else {
                    setTimeout(function () {
                        HideMessage();
                    }, 3000);
                }
            }
            else {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function DeleteBanner(sender, shop_id) {
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { shop_id: shop_id },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/DeleteBanner',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                window.location.href = '/User/Shop';
                IsCompleted();
            }
            else {// if (data.Status == ActionStatus.Error) {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

/*********************** Policy Information Tab *****************************/
var gotoSection = false;
function OnBegin() {
    Message('Processing...');
}
function savenNextPolicy() {
//    var a = $(".tabs li:nth-child(3)").find('a');
//    $(a).trigger('click');
//    SetTab(a);
    gotoSection = true;
    $('#submit').trigger('click');
}
function savenNextShopName() {
    var a = $(".tabs li:nth-child(3)").find('a');
    $(a).trigger('click');
    SetTab(a);
}
function gotoListItem() {
    location.href = '/User/ListItems';
}
function gotoGetPaid() {
    location.href = '/User/GetPaid';
}

function OnPolicyComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        IsCompleted();
        Policy = 'PaymentPolicy';
        if (gotoSection) {
            var a = $(".tabs li:nth-child(3)").find('a');
            $(a).trigger('click');
            SetTab(a);
            $("html,body").animate({ scrollTop: "0" }, "slow");
            gotoSection = false;            
        }
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

/*********************** Shop Section Tab *****************************/
/*
function OnSectionBegin() {
    var txt = $('#SectionName');
    if ($(txt).val().trim() == '') {
        $(txt).addClass('input-validation-error');
        $(txt).parent().find('span').text('Required.').show();
        return false;
    } else { $(txt).removeClass('input-validation-error'); $(txt).parent().find('span').text('').hide(); }
    if ($(txt).val().trim().length < 6 || $(txt).val().trim().length > 100) {
        $(txt).addClass('input-validation-error');
        $(txt).parent().find('span').text('Mininum 6 and Maximum 100 characters are allowed.').show();
        return false; 
    } else {$(txt).removeClass('input-validation-error');  $(txt).parent().find('span').text('').hide();}
}
*/



function OnSectionComplete(obj) {    
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        Section = '1';
        var section = '<li id="' + json.ID + '"><div val="' + json.ID + '" style="word-wrap: break-word;"><a href="javascript:;">' + $('#SectionName').val() + '</a><a href="javascript:;" val="' + $('#SectionName').val() + '" onclick="Edit(this)" class="edit"></a>' +
                            '<a href="javascript:;" onclick="Delete(this)" class="delete"></a></div></li>';
        var sectionOption = '<option value="' + json.ID + '">' + $('#SectionName').val() + '</option>';
        $('#sortable').append(section);
        $('#SectionList, #SortSection').append(sectionOption);
        $('#SectionName').val('');
        IsCompleted();
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

function Delete(sender) {
    var section_id = $(sender).parent().parent().attr('id');
    Message('Processing...');
    AjaxFormSubmit({
        type: "POST",
        validate: false,
        parentControl: $(sender).parents("form:first"),
        data: { section_id: section_id },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/ShopSectionDelete',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                $(sender).parent().parent().remove();
                $('#SectionList, #SortSection').find("option[value='" + section_id + "']").remove();
                IsCompleted();
            }
            else if (data.Status == ActionStatus.Error) {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

var text = "";

function Edit(sender) {
    text = $(sender).attr('val');
    console.log(text);
    var section_id = $(sender).parent().parent().attr('id');
    var edit = '<div val="' + section_id + '" style="word-wrap: break-word;">' +                    
                    '<input type="text" id="Name" maxlength="255" style="width:100px;" value="' + text + '" class="required" name="SectionName">' +
                    '<input type="image" title="Update" onclick="UpdateSection(this, ' + section_id + ')" src="/Content/images/save-sec.png">' +
                    '<input type="image" title="Cancel" onclick="Cancel(this, ' + section_id + ')" src="/Content/images/cancel-sec.png">' +
                '</div>';
    $(sender).parent().parent().html(edit);
    
   
}

function Cancel(sender, section_id) {
    var section = '<a href="javascript:;">' + text + '</a><a val="' + text + '" href="javascript:;" onclick="Edit(this)" class="edit"></a>' +
                            '<a href="javascript:;" onclick="Delete(this)" class="delete"></a>';
    $(sender).parent().html(section);
}

function UpdateSection(sender, section_id) {
    var name=$(sender).parent().find('#Name');
    if ($(name).val().trim().length < 6) {
        $(name).css('border', 'solid 1px red').css('margin-right','3px');
        return false;
    }
    else {
        $(name).css('border', '');
    }
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        data: { section_id: section_id, section_name: $(name).val() },
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/UpdateSection',
        success: function (data) {
            if (data.Status == ActionStatus.Successfull) {
                Message(data.Message, ActionStatus.Successfull);
                var section = '<a href="javascript:;">' + $(sender).parent().find('#Name').val() + '</a><a val="' + $(sender).parent().find('#Name').val() + '" href="javascript:;" onclick="Edit(this)" class="edit"></a>' +
                            '<a href="javascript:;" onclick="Delete(this)" class="delete"></a>';
                $('#SectionList, #SortSection').find("option[value='" + section_id + "']").text($(sender).parent().find('#Name').val());
                $(sender).parent().html(section);
            }
            else if (data.Status == ActionStatus.Error) {
                Message(data.Message, ActionStatus.Error);
            }
        }
    });
}

function SortInit(shop_id) {
    var order = [];
    $("#sortable").sortable({
        start: function (event, ui) {
            var start_pos = ui.item.index() + 1;
            ui.item.data('start_pos', start_pos);
        },
        update: function (event, ui) {
            var start_pos = ui.item.data('start_pos');
            var end_pos = $(ui.item).index() + 1;
            var order = [];
            $('#sortable li').each(function (e) {
                //add each li position to the array...     
                // the +1 is for make it start from 1 instead of 0
                order.push($(this).attr('id') + '=' + ($(this).index() + 1));
            });
            var positions = '';
            positions = order.join(',');
            //use the variable as you need!
            var sender = $(this);
            AjaxFormSubmit({
                type: "POST",
                validate: false,
                parentControl: $(sender).parents("form:first"),
                data: { shop_id: shop_id, positions: positions },
                messageControl: null,
                throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
                url: '/User/ShopSectionDisplayOrder',
                success: function (data) {
                    if (data.Status == ActionStatus.Successfull) {
                        Message(data.Message, ActionStatus.Successfull);
                    }
                    else if (data.Status == ActionStatus.Error) {
                        Message(data.Message, ActionStatus.Error);
                    }
                }
            });
        }
    });
    //$("#sortable").disableSelection();
}
function setServices() {
    var services = ''
    $('input[type=checkbox][name=ServiceOffered]').each(function () {
        if ($(this).is(':checked')) {
            services = services + $(this).attr("id") + ',';
        }
    });
    $('#ServiceId').val(services);
}

function setShopSpeciality() {
    var speciality = ''
    $('input[type=checkbox][name=ShopSpeciality]').each(function () {
        if ($(this).is(':checked')) {
            speciality = speciality + $(this).attr("id") + ',';
        }
    });
    $('#ShopSpecialties').val(speciality);
}

function removetag(sender) {
    $(sender).parent().remove();
    setMaterial();
    
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

function AddMaterial(sender) {
    var mat = $('#Materials').val();
    var material = $(sender).parent().find('input').val();
    if (NotExists(material, $('#material-add .material')) == true) {
        if (material.trim() != '') {
            $('#material-add').append('<div class="material"><span>' + material.trim() + '</span><label class="close" onclick="removetag(this)">x</label></div>');
            $(sender).parent().find('input').val('');
        }
        setMaterial();
    }
}
function setMaterial() {
    var material = ''
    $('.material span').each(function () {
        material = material + $(this).text() + ',';
    });
    $('#Materials').val(material);
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