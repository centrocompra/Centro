var paging = {
    startIndex: 1,
    currentPage: 0,
    pageSize: 20,
    pagingWrapper: 'pagingWrapper',
    first: 'first',
    last: 'last',
    previous: 'previous',
    next: 'next',
    numeric: 'numeric',
    pageInfo: 'pageInfo',
    PagingFunction:''
}

function PageNumbering(TotalRecords) {    
    var totalPages = 0;
    /**** Setting Total Records & Page Size *************/
    totalPages = parseInt((parseInt(TotalRecords) + parseInt(paging.pageSize) - 1) / parseInt(paging.pageSize));
    /**** Setting Total Records & Page Size *************/
    if (TotalRecords == 0 || totalPages == 1) { // in case there are no records or only one page
        $("." + paging.pagingWrapper).css("display", 'none'); // hide the paging 
    }
    else {
        $("." + paging.pagingWrapper).css("display", ''); // show the paging 
    }
    /*  Creating Pagination */
    /*  Code Commented because client don't want numbered paging */

    var LastIndex = parseInt(paging.startIndex + paging.pageSize); // this is the last displaying record
    if (LastIndex > TotalRecords) { // in case that last page includes records less than the size of the page 
        LastIndex = TotalRecords;
    }
    //$("." + paging.pageInfo).html("Page ( <b>" + parseInt(paging.currentPage + 1) + "</b> of <b>" + totalPages + "</b> )      Displaying <b>" + parseInt(startIndex) + "-" + LastIndex + "</b> of <b>" + TotalRecords + "</b> Records."); // displaying current records interval  and currnet page infromation 
    if (paging.currentPage > 0) {
        $('.' + paging.first).unbind('click'); // rmove previous click events
        $('.' + paging.first).removeClass('PageInActive'); // remove the inactive page style
        $('.' + paging.first).addClass('PageActive'); // make it active
        $('.' + paging.first).click(function () { // set goto page to first page 
            GotoPage(0, this);
        });
        $('.' + paging.previous).unbind('click');
        $('.' + paging.previous).removeClass('PageInActive');
        $('.' + paging.previous).addClass('PageActive');
        $('.' + paging.previous).click(function () {
            GotoPage(paging.currentPage - 1, this); // set the previous page next value  to current page - 1
        });
    }
    else {
        $('.' + paging.first).removeClass('PageActive');
        $('.' + paging.first).addClass('PageInActive');
        $('.' + paging.first).unbind('click');
        $('.' + paging.previous).removeClass('PageActive');
        $('.' + paging.previous).addClass('PageInActive');
        $('.' + paging.previous).unbind('click');
    }
    if (paging.currentPage < totalPages - 1) { // if you are not displaying the last index 
        $('.' + paging.next).unbind('click');
        $('.' + paging.next).removeClass('PageInActive');
        $('.' + paging.next).addClass('PageActive');
        $('.' + paging.next).click(function () {
            GotoPage(paging.currentPage + 1, this);
        });

        $('.' + paging.last).unbind('click');
        $('.' + paging.last).removeClass('PageInActive');
        $('.' + paging.last).addClass('PageActive');
        $('.' + paging.last).click(function () {
            GotoPage(totalPages - 1, this);
        });
    } else {
        $('.' + paging.next).removeClass('PageActive');
        $('.' + paging.next).addClass('PageInActive');
        $('.' + paging.next).unbind('click');
        $('.' + paging.last).removeClass('PageActive');
        $('.' + paging.last).addClass('PageInActive');
        $('.' + paging.last).unbind('click');
    }
    // displaying the numeric pages by default there are 10 numeric pages 
    var firstPage = 0;
    var lastPage = 10;
    if (paging.currentPage >= 5) {
        lastPage = paging.currentPage + 5;
        firstPage = paging.currentPage - 5
    }
    if (lastPage > totalPages) {
        lastPage = totalPages;
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
            pagesString += "<span class='PageActive' onclick='GotoPage(" + i + ", this)' > " + parseInt(i + 1) + "</span>" // add goto page event
        }
    }
    $("." + paging.numeric).html(pagesString);
    /**** Loading data and binding grid *******************/
}


/*  This function will call if user click on numbered paging links. */
function GotoPage(page, sender) {
    paging.currentPage = page;
    paging.startIndex = page * paging.pageSize + 1;
    if (typeof (onGotoPage) != "undefined")
        onGotoPage(page, sender);
    if (paging.PagingFunction.trim().length > 0) {
        window[paging.PagingFunction]();
    }
    else
        Paging(paging.startIndex, paging.pageSize, sender);
}

