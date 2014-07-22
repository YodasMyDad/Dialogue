angular.module("umbraco").controller("Dialogue.BannedEmailController",
    function ($scope, $location, bannedemailResource, notificationsService, dialogService) {
        
        $scope.reloademails = function () {
            $scope.emails = bannedemailResource.getAll();
        };


        $scope.add = function () {
            dialogService.open({ template: bannedemailResource.getView('bannedemailadd.html'), callback: emailDone, closeCallback: reloademails, dialogData: null });
            function emailDone(data) {
                $scope.reloademails();
            }
            function reloademails(data) {
                //Not needed
            }
        };

        $scope.delete = function (id) {
            bannedemailResource.delete(id).then(function (response) {
                $scope.reloademails();
                notificationsService.success("email deleted!");
            });
        };
                
        $scope.reloademails();

    });

angular.module("umbraco").controller("Dialogue.AddBannedEmailController",
    function ($scope, $location, bannedemailResource, notificationsService) {

        $scope.add = function () {
            bannedemailResource.add($scope.email).then(function (response) {
                if (response.data.Id > 0) {
                    notificationsService.success("email added!");
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
