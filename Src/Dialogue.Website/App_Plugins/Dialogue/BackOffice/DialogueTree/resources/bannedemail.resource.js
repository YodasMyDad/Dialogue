angular.module("umbraco.resources")
.factory("bannedemailResource", function ($http) {

    var serviceUrl = "backoffice/bannedEmail/bannedemailapi/";
    var myService = {};
    
    myService.getAll = function () {
        return $http.get(serviceUrl + "getall");
    };

    myService.search = function (name) {
        return $http.get(serviceUrl + "getByName?name=" + name);
    };

    myService.delete = function (id) {
        return $http.delete(serviceUrl + "Delete?id=" + id);
    };

    myService.update = function (email) {
        return $http.post(serviceUrl + "Update", email);
    };
    
    myService.add = function (email) {
        return $http.post(serviceUrl + "Add", email);
    };

    myService.getView = function(viewName) {
        return '/App_Plugins/Dialogue/backOffice/DialogueTree/partials/' + viewName;
    };

    return myService;
});