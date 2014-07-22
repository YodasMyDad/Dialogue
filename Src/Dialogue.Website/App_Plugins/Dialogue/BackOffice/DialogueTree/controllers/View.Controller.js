angular.module("umbraco").controller("Dialogue.ViewController",
    function ($scope, $routeParams) {

        //Currently loading /umbraco/general.html
        //Need it to look at /App_Plugins/
        //$scope.dateFilter = settingsResource.getDateFilter();
        //$scope.$watch('dateFilter', function () {
        //    console.log("parent watch");
        //});

        var viewName = $routeParams.id;
        viewName = viewName.replace('%20', '-').replace(' ', '-');

        $scope.templatePartialURL = '../App_Plugins/Dialogue/backoffice/dialogueTree/partials/' + viewName + '.html';
        $scope.sectionName = $routeParams.id;


    });