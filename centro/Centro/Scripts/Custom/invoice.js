
function OnCreateComplete(obj) {
    var json = $.parseJSON(obj.responseText);
    if (json.Status == ActionStatus.Successfull) {
        Message(json.Message, ActionStatus.Successfull);
        row_count = 0;
        delere_rows.length = 0;
        window.location.href = '/Message/MyCustomOrder/' + requestId;
    }
    else {
        Message(json.Message, ActionStatus.Error);
    }
}

var row_count = 0;
var delere_rows = [];
function addRow(sender) {
    row_count++;
    var table = $('.add-task').find('ul');
    var str = '<li> ' +
                    '<span class="col"><input type="text"  data-val-required="*Required." data-val-length-min="3" data-val-length-max="50" data-val-length="Mininum 3 and Maximum 50 characters are allowed." data-val="true"  style="width:185px;" placeholder="Title" name="InvoiceItems[' + row_count + '].Title" class="input-box"> <span data-valmsg-replace="true" data-valmsg-for="InvoiceItems[' + row_count + '].Title" class="field-validation-error"> <span data-valmsg-replace="true" data-valmsg-for="InvoiceItems[' + row_count + '].Title" class="field-validation-valid"></span></span></span>' +
                    '<span class="col"><input type="text" data-val-length-max="500" data-val-length="Maximum 500 characters are allowed." data-val="true" style="width:230px;" placeholder="Description" name="InvoiceItems[' + row_count + '].Description" class="input-box">  <span data-valmsg-replace="true" data-valmsg-for="InvoiceItems[' + row_count + '].Description" class="field-validation-valid"></span></span></span>' +
                    '<span class="col"><input type="text"  onchange="Calculate(this);" data-val-required="*Required." data-val-number="The field Amount must be a number." data-val="true" style="width:60px;" data-val-range-min="0" data-val-range-max="100000" data-val-range="Invalid Price." placeholder="Amount" name="InvoiceItems[' + row_count + '].Amount" class="input-box amount" /> <span data-valmsg-replace="true" data-valmsg-for="InvoiceItems[' + row_count + '].Amount" class="field-validation-error"><span data-valmsg-replace="true" data-valmsg-for="InvoiceItems[' + row_count + '].Amount" class="field-validation-valid"></span></span></span> ' +
                    '<span class="col2"><input type="button" value="Remove" class="button2" onclick="deleteRow(this,' + row_count + ')"></span> ' +
                '</li>';


    $(table).append(str);
    ResetUnobtrusiveValidation($('#form0'));
}

function Calculate(sender) {
    var sum = 0;
    $('.amount').each(function () {
        var amt = isNaN(parseFloat($(this).val())) ? 0 : parseFloat($(this).val());
        sum += amt;
    });
    sum = Math.round(sum * 100) / 100;
    $('.amount-seller').text('$' + sum.toFixed(2) + ' USD');
    var feeAmt = (IsPercentage == true || IsPercentage == 'true') ? sum * parseFloat(fee) / 100.00 : fee;
    feeAmt = Math.round(feeAmt * 100) / 100;
    $('.fee').text('$' + feeAmt.toFixed(2) + ' USD');
    $('.total').text('$' + (Math.round((feeAmt + sum) * 100) / 100).toFixed(2) + ' USD');
}

function deleteRow(sender, count) {
    delere_rows.push(count);
    $('#DeletedItems').val(delere_rows.join(','));
    $(sender).parent().parent().hide();
}

function SetTab(sender) {
    var li = $(sender).parent().parent().find('li').removeClass('active');
    $(sender).parent().addClass('active');
}

function DeleteInvoice(invoice_id) {
    if (confirm("Are you sure to delete this invoice")) {
        $.ajax({
            url: "/Message/DeleteInvoice",
            type: "Post",
            data: { InvoiceId: invoice_id },
            success: function (data) {
                if (data.Status == ActionStatus.Successfull) {
                    $('#tr_' + invoice_id).remove();
                    Message(data.Message, ActionStatus.Successfull);
                    if ($('.trInvoice').size() == 0) {
                        if ($('.tablehead').size() > 0) {
                            $('.tablehead').remove();
                            $('.table-listing').html('No Invoice Found');
                        }
                        else {
                            $('.actv-contrat-tbl').children('table').html('tr class="even"><td colspan="5">No Invoices found</td></tr>');
                        }

                    }
                }
                else {
                    Message(data.Message, ActionStatus.Successfull);
                }
            }
        });
    }
}