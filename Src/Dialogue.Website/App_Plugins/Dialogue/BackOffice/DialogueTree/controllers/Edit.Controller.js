angular.module("umbraco").controller("Dialogue.EditController",
    function ($scope, $routeParams) {

        //Currently loading /umbraco/general.html
        //Need it to look at /App_Plugins/

        var viewName = $routeParams.id;
        viewName = viewName.replace('%20', '-').replace(' ', '-');

        $scope.templatePartialURL = '../App_Plugins/Dialogue/backoffice/dialogueTree/partials/' + viewName + '.html';
        $scope.sectionName = $routeParams.id;

    });