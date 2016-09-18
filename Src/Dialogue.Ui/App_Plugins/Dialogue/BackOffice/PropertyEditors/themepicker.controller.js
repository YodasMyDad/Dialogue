angular.module('umbraco').controller("Dialogue.PropertyEditors.ThemePicker", function ($scope, umbRequestHelper, $http) {

    var serviceUrl = "backoffice/dialogue/PropertyEditors/";
    umbRequestHelper.resourcePromise(
            $http.get(serviceUrl + "GetThemes"),
            'Failed to retrieve themes for Dialogue')
        .then(function (data) {
            $scope.themes = data;
        });

});