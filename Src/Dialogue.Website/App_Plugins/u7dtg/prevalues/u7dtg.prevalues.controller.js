angular.module("umbraco")
    .controller("u7dtg.prevaluesController",
    function ($scope) {

        if (!$scope.model.value) {
            $scope.model.value = {
                columns: []
            }
        }

        $scope.addColumn = function () {
            $scope.model.value.columns.push({
                title: "",
                alias: "",
                type: "textbox",
                props: {}
            });
        }

        $scope.removeColumn = function (index) {
            $scope.model.value.columns.splice(index, 1);
        }

        $scope.moveUp = function(index){
            if (index != 0) {
                $scope.model.value.columns[index] = $scope.model.value.columns.splice(index - 1, 1, $scope.model.value.columns[index])[0];
            }
        }
        $scope.moveDown = function (index) {
            if (index != $scope.model.value.length - 1) {
                $scope.model.value.columns[index] = $scope.model.value.columns.splice(index + 1, 1, $scope.model.value.columns[index])[0];
            }
        }

        $scope.addDropDownOption = function (column) {
            if (!column.props.options) {
                column.props.options = [];
            }
            column.props.options.push({
                text: "",
                value : ""
            });
        }

        $scope.optionRemoveRow = function (column,index) {
            column.props.options.splice(index, 1);
        }

        $scope.optionMoveUp = function (column,index) {
            if (index != 0) {
                column.props.options[index] = column.props.options.splice(index - 1, 1, column.props.options[index])[0];
            }
        }
        $scope.optionMoveDown = function (column,index) {
            if (index != $scope.model.value.length - 1) {
                column.props.options[index] = column.props.options.splice(index + 1, 1, column.props.options[index])[0];
            }
        }        
    });