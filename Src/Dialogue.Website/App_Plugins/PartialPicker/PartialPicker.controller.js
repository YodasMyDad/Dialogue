angular.module("umbraco")
    .controller("PartialPicker.controller",
    function ($scope, $log, partialpickerResource) {
        partialpickerResource.getAll($scope.model.config.folderpath).then(function (response) {
            $scope.partials = response.data;
        });
        $scope.saveSelectedFile = function () {
            $scope.model.value = $scope.partialFile;
        };

        //$scope.limitChars = function(){
        //    var limit = parseInt($scope.model.config.limit);

        //    if ($scope.model.value.length > limit )
        //    {
        //        $scope.info = ‘You cannot write more then ‘ + limit  + ‘ characters!’;
        //        $scope.model.value = $scope.model.value.substr(0, limit );
        //    }
        //    else
        //    {
        //        $scope.info = ‘You have ‘ + (limit - $scope.model.value.length) + ‘ characters left.’;
        //    }
        //};
    });