function ajaxCall(url, data, cbSuccess, cbFailure, cbError) {
    ajaxCoreCall(url, "application/json; charset=utf-8", true, JSON.stringify(data), cbSuccess, cbFailure, cbError);
}

function ajaxCallSynchronous(url, data, cbSuccess, cbFailure, cbError) {
    ajaxCoreCallSynchronous(url, "application/json; charset=utf-8", true, JSON.stringify(data), cbSuccess, cbFailure, cbError);
}

function ajaxCoreCall(url, contentType, processData, data, cbSuccess, cbFailure, cbError) {
    var errorMsg = "Error has been occured. Please contact administration.";
    $.ajax({
        type: "POST",
        url: url,
        contentType: contentType,
        processData: processData,
        data: data,
        success: function (response) {
            console.log("a");
            if (typeof (response) !== "object" && typeof (response) === "string" && response != "" && response.replace("\"", "").replace("\"", "") === "SessionTimeout") {
                window.location.href = siteUrl+"login";
            }
            else if (cbSuccess) { cbSuccess(response); }
        },
        failure: function (response) {  //alert(errorMsg);
            if (cbFailure) { cbFailure(response); } else { console.log(response.responseText) }
        },
        error: function (response) {
            if (cbError) { cbError(response); } else { console.log(response.responseText) }
        }
    });
}

function ajaxCoreCallSynchronous(url, contentType, processData, data, cbSuccess, cbFailure, cbError) {
    var errorMsg = "Error has been occured. Please contact administration.";
    $.ajax({
        type: "POST",
        url: url,
        contentType: contentType,
        processData: processData,
        data: data,
        async: false,
        success: function (response) {
            console.log("a");
            if (typeof (response) !== "object" && typeof (response) === "string" && response != "" && response.replace("\"", "").replace("\"", "") === "SessionTimeout") {
                window.location.href = siteUrl+"login";
            }
            else if (cbSuccess) { cbSuccess(response); }
        },
        failure: function (response) {  //alert(errorMsg);
            if (cbFailure) { cbFailure(response); } else { console.log(response.responseText) }
        },
        error: function (response) {
            if (cbError) { cbError(response); } else { console.log(response.responseText) }
        }
    });
}


// User load partial views
function LoadPartial(partialViewContainer, url, data, isloading, action, className, reqData) {
    if (data === null || data === "") {
        $(partialViewContainer).load(url, function (responseText, textStatus, req) {
            if (responseText != "" && responseText.replace("\"", "").replace("\"", "") === "SessionTimeout") {
                window.location.href = siteUrl + "login";
            }
            else if (isloading) {
                HideLoader();
                if (action != null || action != "") {
                    if (action === "Export") {
                        ExportTable(className, reqData);
                    }
                }
            }
            else {
                //console.log($(responseText));
                if (action != null || action != "") {
                    if (action === "Export") {
                        ExportTable(className, reqData);
                    }
                }
            }
        });
    }
    else {
        $(partialViewContainer).load(url, data, function (responseText, textStatus, req) {
            if (responseText != "" && responseText.replace("\"", "").replace("\"", "") === "SessionTimeout") {
                window.location.href = siteUrl+"login";
            }
            else if (isloading) {
                HideLoader();
                if (action != null || action != "") {
                    if (action === "Export") {
                        ExportTable(className, reqData);
                    }
                }
            }
            else {
                //console.log($(responseText));
                if (action != null || action != "") {
                    if (action === "Export") {
                        ExportTable(className, reqData);
                    }
                }
            }
        });
    }
}

function NumberValue(str)
{
    var regex = new RegExp("[^0-9]");
    if (regex.test(str))
    {
        return false;
    } else
    {
        return true;
    }
}

function CardTitleValue(str) {
   
    //var regex = new RegExp("^([a-zA-Z\s]{1,19})*$");
    //var regex = new RegExp("^([a-zA-Z\s*])*$");
    //var regex = new RegExp("/^[a-zA-Z]+$/");
    //var regex = new RegExp("^[^-\s][a-zA-Z\s]+$");
    //if (regex.test(str) == false)
    //if (/^[a-zA-Z ]*$/.test(str)) {
    //    return true;
    //} else {
    //    return false;
    //}

    return str !== "" && /^[a-zA-Z ]*$/.test(str);
}

function ShowLoader() {
    $('#overlay').css('display', 'block');
}

function HideLoader() {
    $('#overlay').css('display', 'none');
}

function CheckStrLength(value, length) {
    return value.length <= length;
}

//To be used for empty values of Main Address,Landline & Mobile
function CheckMainMobile(selectedval, Mobile,Mobile1,Mobile2,Mobile3)
{
    if (selectedval == "Mobile" && Mobile == "")
    {
        return false;
    }
    else if (selectedval == "Mobile 1" && Mobile1 == "")
    {
        return false;
    }
    else if (selectedval == "Mobile 2" && Mobile2 == "")
    {
        return false;
    }
    else if (selectedval == "Mobile 3" && Mobile3 == "")
    {
        return false;
    }
    else
    {
        return true;
    }
}


function CheckMainAddress(selectedval, AccountAddress, CustomerAddress1, CustomerAddress2)
{
    if (selectedval == "Account Address" && AccountAddress == "") {
        return false;
    }
    else if (selectedval == "Customer Address 1" && CustomerAddress1 == "")
    {
        return false;
    }
    else if (selectedval == "Customer Address 1" && CustomerAddress1 == "")
    {
        return false;
    }
    else if (selectedval == "Customer Address 2" && CustomerAddress2 == "")
    {
        return false;
    }
    else
    {
        return true;
    }
}

function CheckMainLandline(selectedval, Mobile, Mobile1, Mobile2, Mobile3) { }

function ExportExcel(filename, elemid) {
    //creating a temporary HTML link element (they support setting file names)
    var a = document.createElement('a');
    //getting data from our div that contains the HTML table
    var data_type = 'data:application/vnd.ms-excel';
    var table_div = document.getElementById(elemid);
    var table_html = table_div.outerHTML.replace(/ /g, '%20');
    a.href = data_type + ', ' + table_html;
    //setting the file name
    a.download = filename;
    //triggering the function
    a.click();
    //just in case, prevent default behaviour
    e.preventDefault();
}


var tableToExcel = (function () {
    var uri = 'data:application/vnd.ms-excel;base64,'
        , template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table>{table}</table></body></html>'
    , base64 = function (s) { return window.btoa(unescape(encodeURIComponent(s))) }
    , format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) }
    return function (table, name) {
        if (!table.nodeType) table = document.getElementById(table)
        var ctx = { worksheet: name || 'Worksheet', table: table.innerHTML }
        var a = document.createElement('a')
        a.href = uri + base64(format(template, ctx))
        a.download = name;
        a.click();
        //window.location.href = uri + base64(format(template, ctx))
    }
})()


function CheckLength(value, length) {
    var result = true;
    if (!value) {
        result = false;
    }
    if (value.length < length) {
        result = false;
    }
    if (value.length > length) {
        result = false;
    }

    return result;
}

function ExportTable(tblID, filename) {
    $(tblID).tableExport({
        headings: true,                    // (Boolean), display table headings (th/td elements) in the <thead>
        footers: true,                     // (Boolean), display table footers (th/td elements) in the <tfoot>
        formats: ["xlsx", "csv"],    // (String[]), filetypes for the export
        fileName: filename,                    // (id, String), filename for the downloaded file
        bootstrap: true,                   // (Boolean), style buttons using bootstrap
        position: "well",                // (top, bottom), position of the caption element relative to table
        ignoreRows: null,                  // (Number, Number[]), row indices to exclude from the exported file
        ignoreCols: null,                 // (Number, Number[]), column indices to exclude from the exported file
        ignoreCSS: ".tableexport-ignore",  // (selector, selector[]), selector(s) to exclude from the exported file

    });
}

function exportPDF(tblID, filename) {
    var doc = new jsPDF('p', 'pt', 'a4');
    doc.orientation = "landscape";
    doc.format = [279.4, 431.8];
    //A4 - 595x842 pts
    //https://www.gnu.org/software/gv/manual/html_node/Paper-Keywords-and-paper-size-in-points.html

    var specialElementHandlers = {
        // element with id of "bypass" - jQuery style selector
        '.no-export': function (element, renderer) {
            // true = "handled elsewhere, bypass text extraction"
            return true;
        }
    };
    //Html source 
    var source = document.getElementById(tblID).innerHTML;
    var margins = {
        top: 15,
        bottom: 15,
        left: 10,
        width: 170
    };
    doc.fromHTML(
      source, // HTML string or DOM elem ref.
      margins.left,
      margins.top, {
          'width': margins.width,
          'elementHandlers': specialElementHandlers
      },
      function (dispose) {
          // dispose: object with X, Y of the last line add to the PDF 
          //          this allow the insertion of new lines after html
          doc.save(filename + '.pdf');
      }, margins);
}

$(document).on('keypress', '.address-limit', function (e) {
    if (e.keyCode == 13) {
        e.preventDefault();
    }
});