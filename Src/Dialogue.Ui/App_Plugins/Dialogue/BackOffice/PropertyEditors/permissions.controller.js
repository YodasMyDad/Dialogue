angular.module('umbraco').controller("Dialogue.PropertyEditors.Permissions", function ($scope, umbRequestHelper, $http, editorState) {

    var serviceUrl = "backoffice/dialogue/PropertyEditors/";
    $scope.dialogueCheckedPermissions = [];
    $scope.disableAll = false;

    // Get Permission Model
    $http.get(serviceUrl + "EditPermissions", {
        params: { categoryId: editorState.current.id }
    }).then(function (response) {
        $scope.dialoguePermissions = response.data;

        // Get the current checked permissions, add them to
        // dialogueCheckedPermissions so it auto checks the checkboxes
        if ($scope.dialoguePermissions.CurrentPermissions.length > 0) {
            $scope.dialogueCheckedPermissions = $scope.dialoguePermissions.CurrentPermissions;
        }
    });

    // Update permissions when user clicks a check box
    $scope.updateDialoguePermissions = function (permissionId, categoryId, groupId) {
        // Create a key to use for the list of checkboxes
        var formatKey = $scope.formatPermissionIds(permissionId, categoryId, groupId);

        // disable all checkboxes
        $scope.disableAll = true;

        // create a model to send to the server
        var ajaxEditPermissionViewModel = {};
        ajaxEditPermissionViewModel.Permission = permissionId;
        ajaxEditPermissionViewModel.MemberGroup = groupId;
        ajaxEditPermissionViewModel.Category = categoryId;

        // Do we add or remove the permission
        if ($scope.dialogueCheckedPermissions.indexOf(formatKey) > -1) {
            // Remove permission
            ajaxEditPermissionViewModel.HasPermission = false;
        } else {
            // Add Permission
            ajaxEditPermissionViewModel.HasPermission = true;
        }

        // Post the model to the update permissions webapi method
        $http.post(serviceUrl + "UpdatePermission", JSON.stringify(ajaxEditPermissionViewModel),
        {
            headers: {
                'Content-Type': 'application/json; charset=utf-8'
            }
        }
        ).success(function () {
            
            // All good. Update the local list of permissions
            if (ajaxEditPermissionViewModel.HasPermission) {
                $scope.dialogueCheckedPermissions.push(formatKey);
            } else {
                $scope.dialogueCheckedPermissions.splice($scope.dialogueCheckedPermissions.indexOf(formatKey), 1);
            }

        });
        //TODO Failure
        $scope.disableAll = false;
    }

    $scope.formatPermissionIds = function (permissionId, categoryId, groupId) {
        return permissionId + '_' + categoryId + '_' + groupId;
    }

    // We don't need to save anything as it's all done when click
    $scope.model.value = "";
});