$(function () {

});

function AuthorizeSystemRequest(URL, ID, STATUS, COMMENTS) {
    var url = URL;
    var Id = ID;
    var status = STATUS;
    var comments = COMMENTS;

    if (status === "C" || status === "R") {
        if (!comments) {
            alert("Please provide comments before proceed");
            return false;
        }
    }

    ShowLoader();
    ajaxCall(url, { ID: Id, AuthorizationStatus: status, AuthorizationComments: comments }, function (resp) {
        if (typeof (resp) === "object" && resp != null) {
            HideLoader();
            if (resp.IsSuccess) {
                alert(resp.ErrorMessage);
                location.reload();
            }
            else {
                alert(resp.ErrorMessage);
            }
        }
    }, function (fail) {
        HideLoader();
        alert("Failure");
    }, function (err) {
        HideLoader();
        alert("Error");
    });
}