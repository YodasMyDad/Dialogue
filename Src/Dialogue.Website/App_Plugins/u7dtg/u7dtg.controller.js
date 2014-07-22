angular.module("umbraco")
    .controller("u7dtg.editorController",
    function ($scope) {
        //console.log($scope.model.alias)
        if (!$scope.model.value) {
            $scope.model.value = [

            ]
        }
        
        var dtgContentPicker = {
            alias: 'u7dtgContentPicker',
            label: '',
            description: '',
            view: 'contentpicker',
        };

        var dtgMediaPicker = {
            alias: 'u7dtgMediaPicker',
            label: '',
            description: '',
            view: 'mediapicker',
            config: {
                multiPicker: '0'
            }
        };

        var dtgDatePicker = {
            alias: 'u7dtgDatePicker',
            label: '',
            description: '',
            view: 'datepicker',
            config: {
                format: 'yyyy-MM-dd',
                pickTime: false
            }
        };

        var dtgEditor = {
            alias: 'u7dtgRichtexteditor',
            label: '',
            description: '',
            view: 'rte',
            config: {
                editor: {
                    toolbar: ["code", "undo", "redo", "cut", "bold", "italic", "alignleft", "aligncenter", "alignright", "bullist", "numlist", "link", "umbmediapicker", "umbmacro", "table", "umbembeddialog"],
                    stylesheets: [],
                    dimensions: { height: 400 }
                }
            }
        };

        var maxRows = parseInt($scope.model.config.rows.rows) || 0; 0;

        var propertiesEditorswatchers = [];
        var rowObject = {};
        var resetProertiesEditors = function () {
            $scope.contentpickers = {};
            $scope.mediapickers = {};
            $scope.datepickers = {};
            $scope.rtEditors = [];
            rowObject = {};
            $scope.propertiesOrder = [];

            // clean watchers before set again.
            for (index = 0; index < propertiesEditorswatchers.length; ++index) {
                propertiesEditorswatchers[index]();
            }

            angular.forEach($scope.model.config.columns.columns, function (value, key) {
                rowObject[value.alias] = "";
                $scope.propertiesOrder.push(value.alias);
                var columnKey = key;
                var editorProperyAlias = value.alias;
                if (value.type == "rte") {
                    angular.forEach($scope.model.value, function (row, key) {
                        var rtEditor = angular.copy(dtgEditor);
                        rtEditor.alias = rtEditor.alias + $scope.model.alias + columnKey + key;
                        if (row[editorProperyAlias]) {
                            rtEditor.value = row[editorProperyAlias];
                        } else {
                            rtEditor.value = "";
                        }
                        $scope.rtEditors.push(rtEditor);
                    });
                }
                if (value.type == "mediapicker") {
                    angular.forEach($scope.model.value, function (row, key) {
                        var currentRow = row;
                        var mediapicker = angular.copy(dtgMediaPicker);
                        mediapicker.alias = mediapicker.alias + columnKey + key;
                        if (row[editorProperyAlias]) {
                            mediapicker.value = row[editorProperyAlias];
                        } else {
                            mediapicker.value = "";
                        }

                        if (value.props.multiple) {
                            mediapicker.config.multiPicker = '1';
                        }

                        $scope.mediapickers["c" + columnKey + "r" + key] = mediapicker;

                        var pickerWatch = $scope.$watch('mediapickers["c' + columnKey + 'r' + key + '"].value', function (newVal, oldVal) {
                            if (newVal || newVal != oldVal) {
                                $scope.model.value[key][editorProperyAlias] = newVal;
                            }
                        });
                        propertiesEditorswatchers.push(pickerWatch)
                    });
                }

                if (value.type == "contentpicker") {
                    angular.forEach($scope.model.value, function (row, key) {

                        var contentpicker = angular.copy(dtgContentPicker);
                        contentpicker.alias = contentpicker.alias + columnKey + key;
                        if (row[editorProperyAlias]) {
                            contentpicker.value = row[editorProperyAlias];
                        } else {
                            contentpicker.value = "";
                        }

                        $scope.contentpickers["c" + columnKey + "r" + key] = contentpicker;

                        var pickerWatch = $scope.$watch('contentpickers["c' + columnKey + 'r' + key + '"].value', function (newVal, oldVal) {
                            if (newVal || newVal != oldVal) {
                                $scope.model.value[key][editorProperyAlias] = newVal;
                            }
                        });
                        propertiesEditorswatchers.push(pickerWatch)
                    });
                }

                if (value.type == "datepicker") {
                    angular.forEach($scope.model.value, function (row, key) {
                        var currentRow = row;
                        var datepicker = angular.copy(dtgDatePicker);
                        datepicker.alias = datepicker.alias + $scope.model.alias + columnKey + key;
                        if (row[editorProperyAlias]) {
                            datepicker.value = row[editorProperyAlias];
                        } else {
                            datepicker.value = "";
                        }

                        if (value.props.format) {
                            datepicker.config.format = value.props.format;
                        }
                        if (value.props.time) {
                            datepicker.config.pickTime = true;
                            if (!value.props.format) {
                                datepicker.config.format = "yyyy-MM-dd hh:mm:ss"
                            }
                        }

                        $scope.datepickers["c" + columnKey + "r" + key] = datepicker;

                        var pickerWatch = $scope.$watch('datepickers["c' + columnKey + 'r' + key + '"].value', function (newVal, oldVal) {
                            if (newVal || newVal != oldVal) {
                                $scope.model.value[key][editorProperyAlias] = newVal;
                            }
                        });
                        propertiesEditorswatchers.push(pickerWatch)
                    });
                }


            });
        }

        resetProertiesEditors();
        

        // Check for deleted columns
        angular.forEach($scope.model.value, function (row, key) {
            angular.forEach(row, function (value, alias) {
                if ($scope.propertiesOrder.indexOf(alias) == -1) {
                    delete row[alias];
                }
            });
        });

        $scope.addRow = function () {
            if (maxRows == 0 || $scope.model.value.length < maxRows) {
                $scope.model.value.push(angular.copy(rowObject));
                resetProertiesEditors();
            }
            else {
                alert("Max rows is - " + maxRows);
            }
        }

        $scope.removeRow = function (index) {
            $scope.model.value.splice(index, 1);
            resetProertiesEditors();
        }

        $scope.moveUp = function (index) {
            if (index != 0) {
                $scope.model.value[index] = $scope.model.value.splice(index - 1, 1, $scope.model.value[index])[0];
            }
            resetProertiesEditors();
        }
        $scope.moveDown = function (index) {
            if (index != $scope.model.value.length - 1) {
                $scope.model.value[index] = $scope.model.value.splice(index + 1, 1, $scope.model.value[index])[0];
            }
            resetProertiesEditors();
        }

        $scope.selectedEditorIndex = null;
        $scope.selectedEditorRow = null;
        $scope.selectedEditorProperty = null;
        $scope.selectedEditorTitle = "";

        $scope.editorOpen = function (row, property) {
            var selectedEditorRowIndex = $scope.model.value.indexOf(row);
            var selectedEditorColumnIndex = $scope.propertiesOrder.indexOf(property);
            $scope.selectedEditorTitle = $scope.model.config.columns.columns[selectedEditorColumnIndex].title
            angular.forEach($scope.rtEditors, function (value, key) {
                if (value.alias == 'u7dtgRichtexteditor' + $scope.model.alias + selectedEditorColumnIndex + selectedEditorRowIndex) {
                    $scope.selectedEditorIndex = key;
                    
                }
            });
            $scope.selectedEditorRow = row;
            $scope.selectedEditorProperty = property;
        }


      


    

       
});
