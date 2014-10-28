/*******************************************************
Author: Jitendra Pancholi
Created On: 22/02/2012
Modified On : 05/03/2012
*******************************************************/
// Paging Class
var paging = {
    startIndex: 1,
    pageSize: 4,
    page: 1,
    totalRecors: 0,
    currentPage: 0,
    totalPages: 0,
    sortExp: 'Id',
    isSort: false,
    asc: false,
    searchExp: '',
    gridContent: '',
    pagingWrapper: '',
    gridTemplate: '',
    gridHeader: '',
    gridTemplateHeader: '',
    gridResultPerPageDropdown: '',
    first: '',
    last: '',
    previous: '',
    next: '',
    numeric: '',
    pageInfo: '',
    data: '',
    contentUrl: '',
    headerGetUrl: '',
    headerUpdateUrl: '',
    where: '',
    columns: '', // in csv format
    checked: 0,
    defaultColumns: 'Name, ID, Location, Code',
    bSuppressScroll: false,
    usersCustomColumns: [],
    blsMore: true,
    otherDataList1: [],
    /*  Function to fetch columns saved by user for the personalised setting. */
    GetColumns: function () {
        ajaxcall.data = "{defaultColumns:'" + paging.defaultColumns + "'}";
        ajaxcall.url = paging.headerGetUrl;
        ajaxcall.callbackfunction = paging.GetColumnsSuccess;
        ajaxcall.Call();
    },
    /*  Callback function for the GetColumns function. 
    If GetColumns function executes without error, control will come to this function */
    GetColumnsSuccess: function (result) {
        paging.usersCustomColumns.length = 0;
        $.each(result, function (i) {
            paging.usersCustomColumns.push(result[i].trim());
        });
    },
    ShowHideColumns: function () {
        paging.checked = 0;
        $('.th1, .td1').hide();
        //$('.th1, .td1').css('display', 'none').css('margin', '0').css('padding', '0').css('width', '0');
        $('.action').show();
        $.each(paging.usersCustomColumns, function (i) {
            $('.' + paging.usersCustomColumns[i] + ' input').attr('checked', 'checked');
            $('.' + paging.usersCustomColumns[i]).show();
            //$('.' + paging.usersCustomColumns[i]).removeAttr('style');
            paging.checked++;
        });
    },
    /* Function to update columns selected by user for the personalised setting */
    UpdateColumns: function () {
        ajaxcall.data = "{Columns:'" + paging.columns + "'}";
        ajaxcall.url = paging.headerUpdateUrl;
        ajaxcall.callbackfunction = paging.UpdateColumnsSuccess;
        ajaxcall.Call();
    },
    /*  Callback function for the UpdateColumns function. 
    If UpdateColumns function executes without error, control will come to this function */
    UpdateColumnsSuccess: function (result) {
        // paging.GetColumns();
        // Future use if required.
        paging.GetColumns();
    },
    /*  Paging function that will fetch data in paged format by applying sorting and searching */
    Paging: function (start, size) {
        ajaxcall.data = "{StartIndex:" + start + ",PageSize:" + size + ",SortExp:'" + paging.sortExp + "',Asc:'" + paging.asc + "',SearchExp:'" + paging.searchExp + "',Where:'" + paging.where + "'}";
        ajaxcall.url = paging.contentUrl;
        ajaxcall.callbackfunction = paging.PagingSuccess;
        ajaxcall.Call();
    },
    /*  Callback function for the Paging function. 
    If Paging function executes without error, control will come to this function.
    This function will populate the grid. */
    PagingSuccess: function (result) {
        //debugger;
        var jsonObj = jsonParse("(" + result + ")");
        /**** Setting Total Records & Page Size *************/
        paging.totalRecors = jsonObj.Count;
        paging.totalPages = parseInt((parseInt(paging.totalRecors) + parseInt(paging.pageSize) - 1) / parseInt(paging.pageSize));
        /**** Setting Total Records & Page Size *************/
        /**** Setting otherDataList1 ****************/
        paging.otherDataList1 = jsonObj.otherDataList1;
        /**** Setting otherDataList1 ****************/
        /**** Loading data and binding grid *******************/
        //console.log(paging.totalPages);
        if (paging.totalRecors == 0 || paging.totalPages == 1) { // in case there are no records or only one page
            // Finding total columns
            var totalColumns = 0;
            $("#" + paging.gridContent).parent().find('thead tr th').each(function () {
                totalColumns++;
            });
            // alert(totalColumns);
            $("#" + paging.gridContent).html("<tr><td colspan='" + totalColumns + "' style='padding:5px;text-align:center;'>No data found</td></tr>");
            $("#" + paging.pagingWrapper).css("display", 'none'); // hide the paging 

            $('#Loading').hide();

            return;
        } else {
            $("#" + paging.pagingWrapper).css("display", '');
        }
        $('.th1, .td1').show();
        if (paging.isSort)
            $("#" + paging.gridContent).empty();
        //        if (paging.blsMore) {
        //            $("#" + paging.gridTemplate).tmpl(jsonObj.Data).appendTo("#" + paging.gridContent);
        //        }
        //        else {
        $("#" + paging.gridContent).html('');
        //alert(jsonObj.Data);
        $("#" + paging.gridTemplate).tmpl(jsonObj.Data).appendTo("#" + paging.gridContent);
        // }
        paging.isSort = false;
        paging.bSuppressScroll = false;

        /*  Creating Pagination */
        /*  Code Commented because client don't want numbered paging */

        var LastIndex = parseInt(paging.startIndex + paging.pageSize); // this is the last displaying record
        if (LastIndex > paging.totalRecors) { // in case that last page includes records less than the size of the page 
            LastIndex = paging.totalRecors;
        }
        //$("#" + paging.pageInfo).html("Page ( <b>" + parseInt(paging.currentPage + 1) + "</b> of <b>" + paging.totalPages + "</b> )      Displaying <b>" + parseInt(startIndex) + "-" + LastIndex + "</b> of <b>" + paging.totalRecors + "</b> Records."); // displaying current records interval  and currnet page infromation 
        if (paging.currentPage > 0) {
            $('#' + paging.first).unbind('click'); // rmove previous click events
            $('#' + paging.first).removeClass('PageInActive'); // remove the inactive page style
            $('#' + paging.first).addClass('PageActive'); // make it active 
            $('#' + paging.first).click(function () { // set goto page to first page 
                paging.GotoPage(0);
            });
            $('#' + paging.previous).unbind('click');
            $('#' + paging.previous).removeClass('PageInActive');
            $('#' + paging.previous).addClass('PageActive');
            $('#' + paging.previous).click(function () {
                paging.GotoPage(paging.currentPage - 1); // set the previous page next value  to current page - 1
            });
        }
        else {
            $('#' + paging.first).removeClass('PageActive');
            $('#' + paging.first).addClass('PageInActive');
            $('#' + paging.first).unbind('click');
            $('#' + paging.previous).removeClass('PageActive');
            $('#' + paging.previous).addClass('PageInActive');
            $('#' + paging.previous).unbind('click');
        }
        if (paging.currentPage < paging.totalPages - 1) { // if you are not displaying the last index 
            $('#' + paging.next).unbind('click');
            $('#' + paging.next).removeClass('PageInActive');
            $('#' + paging.next).addClass('PageActive');
            $('#' + paging.next).click(function () {
                paging.GotoPage(paging.currentPage + 1);
            });

            $('#' + paging.last).unbind('click');
            $('#' + paging.last).removeClass('PageInActive');
            $('#' + paging.last).addClass('PageActive');
            $('#' + paging.last).click(function () {
                paging.GotoPage(paging.totalPages - 1);
            });
        } else {
            $('#' + paging.next).removeClass('PageActive');
            $('#' + paging.next).addClass('PageInActive');
            $('#' + paging.next).unbind('click');
            $('#' + paging.last).removeClass('PageActive');
            $('#' + paging.last).addClass('PageInActive');
            $('#' + paging.last).unbind('click');
        }
        // displaying the numeric pages by default there are 10 numeric pages 
        var firstPage = 0;
        var lastPage = 10;
        if (paging.currentPage >= 5) {
            lastPage = paging.currentPage + 5;
            firstPage = paging.currentPage - 5
        }
        if (lastPage > paging.totalPages) {
            lastPage = paging.totalPages;
            firstPage = lastPage - 10;
        }
        if (firstPage < 0) {
            firstPage = 0;
        }
        var pagesString = '';
        for (var i = firstPage; i < lastPage; i++) {
            if (i == paging.currentPage)
            { pagesString += "<span class='PageInActive currentPage' > " + parseInt(i + 1) + "</span>" }
            else {
                pagesString += "<span class='PageActive' onclick='paging.GotoPage(" + i + ")' > " + parseInt(i + 1) + "</span>" // add goto page event
            }
        }
        $("#" + paging.numeric).html(pagesString);


        $('#Loading').hide();

        /**** Loading data and binding grid *******************/
    },
    /*  This function will call if user click on numbered paging links. */
    GotoPage: function (page) {
        debugger;
        $('#Loading').show();
        paging.currentPage = page;
        paging.startIndex = page * paging.pageSize + 1;
        paging.Paging(paging.startIndex, paging.pageSize);

    },
    ResultsPerPage: function (count) {
        $('#Loading').show();
        paging.pageSize = count;
        paging.Paging(paging.startIndex, paging.pageSize);

    }
}

$(document).ready(function () {
    $('.arrow-up').live('click', function () { // Sort by Ascending on selected column
        sortPageSize = paging.page * paging.pageSize;
        $('#Loading').show();
        paging.isSort = true;
        paging.sortExp = $(this).parent().parent().attr('value');
        paging.asc = true;
        paging.Paging(paging.startIndex, paging.pageSize);
     
        $('.arrow-up-down').html('<div class="arrow-down" title="descending"></div>');

        var checkFunc = typeof (gridloaded);
        if (checkFunc != "undefined") {
            gridloaded();
        }
    });
    $('.arrow-down').live('click', function () { // Sort by Descending on selected column        
        sortPageSize = paging.page * paging.pageSize;
        $('#Loading').show();
        paging.isSort = true;
        paging.sortExp = $(this).parent().parent().attr('value');
        paging.asc = false;
        paging.Paging(paging.startIndex, paging.pageSize);
      
        $(this).parent().attr("class", "current");
        $('.arrow-up-down').html('<div class="arrow-down" title="descending"></div>');
        $('.current').addClass('arrow-up-down');
        $('.current').html('<div class="arrow-up" title="ascending"></div>');
        $('.current').removeClass('current');

        var checkFunc = typeof (gridloaded);
        if (checkFunc != "undefined") {
            gridloaded();
        }
    });
    $('#ddlPerPage').live('change', function () {
        var count = $('#ddlPerPage option:selected').val();
        paging.ResultsPerPage(count);
    });
});