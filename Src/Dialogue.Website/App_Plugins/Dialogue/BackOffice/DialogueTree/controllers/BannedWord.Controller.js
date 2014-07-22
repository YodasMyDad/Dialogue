angular.module("umbraco").controller("Dialogue.BannedWordController",
    function ($scope, $location, bannedwordResource, notificationsService, dialogService) {
        
        $scope.reloademails = function () {
            $scope.emails = bannedwordResource.getAll();
        };


        $scope.add = function () {
            dialogService.open({ template: bannedwordResource.getView('bannedwordadd.html'), callback: emailDone, closeCallback: reloademails, dialogData: null });
            function emailDone(data) {
                $scope.reloademails();
            }
            function reloademails(data) {
                //Not needed
            }
        };

        $scope.delete = function (id) {
            bannedwordResource.delete(id).then(function (response) {
                $scope.reloademails();
                notificationsService.success("Word deleted!");
            });
        };
                
        $scope.reloademails();

    });

angular.module("umbraco").controller("Dialogue.AddBannedWordController",
    function ($scope, $location, bannedwordResource, notificationsService) {

        $scope.add = function () {
            bannedwordResource.add($scope.email).then(function (response) {
                if (response.data.Id > 0) {
                    notificationsService.success("Word added!");
                    $scope.email = null;
                    $scope.submit();
                }
            }); 
        };
    });

// assetsService.loadJs("/app_plugins/ingredient/highcharts.min.js")
// .then(function () {            
//     // Do all logic here
//});

//if (!angular.isArray($scope.model.value)) {
//    $scope.model.value = [];
//}
