var USAStatesOptions = {
    Alabama: 1,
    Alaska: 2,
    Arizona: 4,
    Arkansas: 5,
    California: 9,
    Colorado: 10,
    Connecticut: 11,
    Delaware: 12,
    District_of_Columbia: 13,
    Florida: 15,
    Georgia: 16,
    Hawaii: 18,
    Idaho: 19,
    Illinois: 20,
    Indiana: 21,
    Iowa: 22,
    Kansas: 23,
    Kentucky: 24,
    Louisiana: 25,
    Maine: 26,
    Maryland: 28,
    Massachusetts: 29,
    Michigan: 30,
    Minnesota: 31,
    Mississippi: 32,
    Missouri: 33,
    Montana: 34,
    Nebraska: 35,
    Nevada: 36,
    New_Hampshire: 37,
    New_Jersey: 38,
    New_Mexico: 39,
    New_York: 40,
    North_Carolina: 41,
    North_Dakota: 42,
    Ohio: 44,
    Oklahoma: 45,
    Oregon: 46,
    Pennsylvania: 48,
    Rhode_Island: 50,
    South_Carolina: 51,
    South_Dakota: 52,
    Tennessee: 53,
    Texas: 54,
    Utah: 55,
    Vermont: 56,
    Virginia: 58,
    Washington: 59,
    West_Virginia: 60,
    Wisconsin: 61,
    Wyoming: 62
}

//function getUSAStates(selectID) {
//    var USAStatesSelect = $('#' + selectID);
//    $.each(USAStatesOptions, function (val, text) {
//        USAStatesSelect.append(
//            $('<option></option>').val(text).html(val.replace('_', ' ').replace('_', ' '))
//        );
//    });
//    $('#' + selectID).parent().show();
//}

function getUSAStates(selectID, stateIds) {
    var states = stateIds.split(',');
    var USAStatesSelect = $('#' + selectID);
    $.each(USAStatesOptions, function (val, text) {
        if (states.indexOf(text.toString()) != -1) {
            USAStatesSelect.append(
            $('<option></option>').val(text).html(val.replace('_', ' ').replace('_', ' '))
        );
        }
    });
    $('#' + selectID).parent().show();
}