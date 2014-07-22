//adds the resource to umbraco.resources module:
angular.module('umbraco.resources').factory('partialpickerResource',
    function ($q, $http) {
        //the factory object returned
        return {
            //this cals the Api Controller we setup earlier
            getAll: function (folder) {
                return $http.get("backoffice/Apt/PartialPickerApi/GetFiles", {
                    params: { folder: folder }
                });
            }
        };
    }
);