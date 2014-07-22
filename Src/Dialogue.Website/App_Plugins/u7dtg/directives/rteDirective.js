angular.module("umbraco")
    .directive("u7dtRte",function(){
    return {
        restrict: 'A',
        link: function (scope, element, attr, ctrl) {

            //console.log(element.length);

            element.on('hidden.bs.modal', function () {
                $("#contentwrapper").css("z-index", '10');
                $("#contentcolumn").css("z-index", '10');
                scope.$apply(function () {
                    //console.log($scope.rtEditors[scope.selectedEditorIndex].value);
                    scope.selectedEditorRow[scope.selectedEditorProperty] = scope.rtEditors[scope.selectedEditorIndex].value;
                });
            });
            element.on('show.bs.modal', function () {
                $("#contentwrapper").css("z-index", 'auto');
                $("#contentcolumn").css("z-index", 'auto');
            })


            //$('a[data-toggle="tab"]', element).on('click', function (e) {
            //    console.log("A");
            //    e.preventDefault()
            //    $(this).tab('show')
            //})
        }
    }
});