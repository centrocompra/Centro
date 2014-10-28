var ajaxcall =
{
    SitePath: '',
    data: '',
    url: '',
    callbackfunction: '',
    fileElementId: '',
    AjaxRequest: false,
    callback: true,
    async: false,
    folder: '',
    filename: '',
    Call: function () {
        if (ajaxcall.AjaxRequest == true) {
            alert(ajaxcall.AjaxRequest);
            return;
        }
        else {
            try {
                ajaxcall.AjaxRequest == true;
                $.ajax({
                    type: "POST",
                    url: ajaxcall.url,
                    data: ajaxcall.data,
                    contentType: "application/json; Characterset=utf-8",
                    dataType: "json",
                    async: false,
                    success: function (data) {
                        if (ajaxcall.callback == true) {
                            ajaxcall.callbackfunction(data);
                        }
                    },
                    error: function (request, status, error) {
                        debugger;
                        //alert("Exception Handling :  \n" + request.responseText);
                        if(error!=null)
                            alert('Unable to process the request at this moment! Please try again later.');
                        else
                            alert(error);
                    },
                    complete: function () {
                        ajaxcall.AjaxRequest = false;
                    }
                });
            }
            catch (e) {
                ajaxcall.AjaxRequest == false;
                // alert("Error Catch : " + e.Description + '\n' + 'Message: ' + e.Message);
            }
        }
    },
    AjaxFileUpload: function () {
        $.ajaxFileUpload({
            type: "POST",
            url: "../GenericHandlers/FileUploader.ashx?path=" + ajaxcall.folder,
            dataType: 'json',
            async: false,
            secureuri: false,
            fileElementClass: ajaxcall.fileElementClass,
            success: function (data) {
                var data = data.toString();
                ajaxcall.filename = data.substring(6, data.length - 7);
                alert(ajaxcall.filename);
                return true;
            }
        });
    }
};
//Set the cookie
function setCookie(c_name, value, expire) {
    var exdate = new Date();
    exdate.setDate(exdate.getDate() + expire);
    document.cookie = c_name + "=" + escape(value) + ((expire == null) ? "" : ";expires=" + exdate.toGMTString());
}

//Get the cookie content
function getCookie(c_name) {
    if (document.cookie.length > 0) {
        c_start = document.cookie.indexOf(c_name + "=");
        if (c_start != -1) {
            c_start = c_start + c_name.length + 1;
            c_end = document.cookie.indexOf(";", c_start);
            if (c_end == -1) {
                c_end = document.cookie.length;
            }
            return unescape(document.cookie.substring(c_start, c_end));
        }
    }
    return '';
}

String.prototype.trim = function () { return this.replace(/^\s\s*/, '').replace(/\s\s*$/, ''); };
String.prototype.ltrim = function () { return this.replace(/^\s+/, ''); }
String.prototype.rtrim = function () { return this.replace(/\s+$/, ''); }
String.prototype.fulltrim = function () { return this.replace(/(?:(?:^|\n)\s+|\s+(?:$|\n))/g, '').replace(/\s+/g, ' '); }

function watermark(control, text) {
    $('#' + control).val(text).addClass('watermarkOn');
    $("#" + control).focus(function () {
        $(this).filter(function () {
            return $(this).val() == "" || $(this).val() == text;
        }).removeClass("watermarkOn").val("");
    });
    $("#" + control).blur(function () {
        $(this).filter(function () {
            return $(this).val() == ""
        }).addClass("watermarkOn").val(text);
    });
}


$(document).ready(function () {
    $("#Loading").ajaxStart(function () {
        $(this).show();
    });

    $("#Loading").ajaxStop(function () {
        $(this).hide();
    });
});