angular.module("umbraco")
    .controller("CssPicker.controller",
    function ($scope, $log, cssPickerResource) {
        cssPickerResource.getAll($scope.model.config.folderpath).then(function (response) {
            $scope.cssfiles = response.data;
        });
        $scope.saveSelectedFile = function () {
            $scope.model.value = $scope.cssFile;
        };
    });