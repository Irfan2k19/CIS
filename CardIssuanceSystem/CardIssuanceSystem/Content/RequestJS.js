$(function () {

});

$(document).on('click', '#btnFilterRequestData', function (e) {
    FilterRequestData();
});

function FilterRequestData(e) {
    var data = new Object();
    data.From = $('.from_date').val();
    data.To = $('.to_date').val();
    data.RequestNumber = $('.request_number').val();
    data.CIFNumber = $('.cif_number').val();
    data.AccountNumber = $('.account_number').val();
    data.RequestType = $('#hfrequest_type').val();
    ShowLoader();
    LoadPartial(".divTableData", FilterRequestDataUrl, data, false, null, null, null);
    HideLoader();
}