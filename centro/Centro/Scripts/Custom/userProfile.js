function selectslot(sender) {
    if ($(sender).hasClass('available')) {
        $(sender).removeClass('available');
    }
    else if ($(sender).hasClass('selected')) {
        $(sender).removeClass('selected');
    }
    else {
        $(sender).addClass('selected');
    }

    var slots = [];
    $('.main-chart tr td div').each(function () {
        if ($(this).hasClass('selected') || $(this).hasClass('available')) {
            slots.push($(this).attr('row') + ',' + $(this).attr('col'));
        }
    });
    var list = slots.join(':');
    $('#ShopAvailability').val(list);
}


var Profile = {
    OnProfileComplete: function (obj) {
        var json = $.parseJSON(obj.responseText);
        if (json.Status == ActionStatus.Successfull) {
            Message(json.Message, ActionStatus.Successfull);
            setTimeout(function () {
                window.location.href = '/User/Profile';
            }, 100);
        }
        else {
            Message(json.Message, ActionStatus.Error);
        }
    }
};

function UploadProfilePicture(sender, form) {
    var $this = $(sender), $clone = $this.clone();
    $this.after($clone).appendTo($('#' + form));
    // Message('Processing...', MessageType.None);
    var progress = '<div class="progress"><div class="bar"><div class="percent">0%</div ></div ></div>';
    $("div.loading_ind").html(progress);
    AjaxFormSubmit({
        type: "POST",
        validate: true,
        parentControl: $(sender).parents("form:first"),
        form: $('#' + form),
        messageControl: null,
        throbberPosition: { my: "left center", at: "right center", of: sender, offset: "5 0" },
        url: '/User/UploadProfilePicture',
        success: function (data) {
            $("div.loading_ind").html('');
            if (data.Status == ActionStatus.Successfull) {
                if (data.Results == null || data.Results == 'null') {
                    Message(data.Message, ActionStatus.Error);
                }
                else {
                    $("#avatar_img").attr("src", "/Temp/" + data.Results[0] + "/" + data.Results[2]);
                    HideMessage();
                }
                $('.progress').remove();
            }
            else {
                Message(data.Message, ActionStatus.Error);
                $('.progress').remove();
            }
        },
        bar: $('.bar'),
        percent: $('.percent'),
        status: null
    });
    $('#' + form).html('');
}

function NotExistsService(value, control) {
    var flag = true;
    $(control).find('span').each(function () {
        console.log(value.trim() + " --- " + $(this).text().trim());
        if (value.trim() == $(this).text().trim()) {
            alert('Already added!!!');
            flag = false;
        }
    });
    return flag;
}

function AddServices(sender) {    
    var mat = $('#servicesOffered').val();
    var services = $(sender).parent().find('select option:selected').text();
    if (NotExistsService(services, $('#services-add .material')) == true) {
        if (services.trim() != '') {
            $('#services-add').append('<div class="material"><span id="' + mat + '">' + services.trim() + '</span><label class="close" onclick="removetag(this)"></label></div>');
            //$(sender).parent().find('input').val('');
        }
        setServices();
    }
}

function setServices() {
    var services = [];
    $('.material span').each(function () {
        services.push($(this).attr('id'));
    });
    $('#Services').val(services.join(','));
}

function removetag(sender) {
    $(sender).parent().remove();
    setServices();

}
