angular.module("umbraco").controller("Imulus.ArchetypeController", function ($scope, $http, assetsService, angularHelper, notificationsService, $timeout) {

    //$scope.model.value = "";
    $scope.model.hideLabel = $scope.model.config.hideLabel == 1;

    //get a reference to the current form
    $scope.form = $scope.form || angularHelper.getCurrentForm($scope);

    //set the config equal to our prevalue config
    $scope.model.config = $scope.model.config.archetypeConfig;

    //ini the model
    $scope.model.value = $scope.model.value || getDefaultModel($scope.model.config);
    init();

    //helper to get $eval the labelTemplate
    $scope.getFieldsetTitle = function(fieldsetConfigModel, fieldsetIndex) {
        if(!fieldsetConfigModel)
            return "";
        var fieldset = $scope.model.value.fieldsets[fieldsetIndex];
        var fieldsetConfig = $scope.getConfigFieldsetByAlias(fieldset.alias);
        var template = fieldsetConfigModel.labelTemplate;

        if (template.length < 1)
            return fieldsetConfig.label;

        var rgx = /{{(.*?)}}*/g;
        var results;
        var parsedTemplate = template;

        while ((results = rgx.exec(template)) !== null) {
            var propertyAlias = results[1];
            var propertyValue = $scope.getPropertyValueByAlias(fieldset, propertyAlias);
            parsedTemplate = parsedTemplate.replace(results[0], propertyValue);
        }

        return parsedTemplate;
    };

    //sort config
    $scope.sortableOptions = {
        axis: 'y',
        cursor: "move",
        handle: ".handle",
        update: function (ev, ui) {
            $scope.setDirty();
        },
        stop: function (ev, ui) {

        }
    };

    //handles a fieldset add
    $scope.addRow = function (fieldsetAlias, $index) {
        if ($scope.canAdd())
        {
            if ($scope.model.config.fieldsets)
            {
                var newFieldset = getEmptyRenderFieldset($scope.getConfigFieldsetByAlias(fieldsetAlias));

                if (typeof $index != 'undefined')
                {
                    $scope.model.value.fieldsets.splice($index + 1, 0, newFieldset);
                }
                else
                {
                    $scope.model.value.fieldsets.push(newFieldset);
                }
            }
            $scope.setDirty();

            newFieldset.collapse = $scope.model.config.enableCollapsing ? true : false;
            $scope.focusFieldset(newFieldset);
        }
    }

    $scope.removeRow = function ($index) {
        if ($scope.canRemove()) {
            if (confirm('Are you sure you want to remove this?')) {
                $scope.setDirty();
                $scope.model.value.fieldsets.splice($index, 1);
            }
        }
    }

    //helpers for determining if a user can do something
    $scope.canAdd = function ()
    {
        if ($scope.model.config.maxFieldsets)
        {
            return countVisible() < $scope.model.config.maxFieldsets;
        }

        return true;
    }

    //helper that returns if an item can be removed
    $scope.canRemove = function ()
    {
        return countVisible() > 1 
            || ($scope.model.config.maxFieldsets == 1 && $scope.model.config.fieldsets.length > 1)
            || $scope.model.config.startWithAddButton;
    }

    //helper that returns if an item can be sorted
    $scope.canSort = function ()
    {
        return countVisible() > 1;
    }

    //helpers for determining if the add button should be shown
    $scope.showAddButton = function () {
        return $scope.model.config.startWithAddButton
            && countVisible() === 0;
            ///&& $scope.model.config.fieldsets.length == 1;
    }

    //helper, ini the render model from the server (model.value)
    function init() {
        $scope.model.value = removeNulls($scope.model.value);
        addDefaultProperties($scope.model.value.fieldsets);
    }

    function addDefaultProperties(fieldsets)
    {
        _.each(fieldsets, function (fieldset)
        {
            fieldset.collapse = false;
            fieldset.isValid = true;
        });
    }

    //helper to get the correct fieldset from config
    $scope.getConfigFieldsetByAlias = function(alias) {
        return _.find($scope.model.config.fieldsets, function(fieldset){
            return fieldset.alias == alias;
        });
    }

    //helper to get a property by alias from a fieldset
    $scope.getPropertyValueByAlias = function(fieldset, propertyAlias) {
        var property = _.find(fieldset.properties, function(p) {
            return p.alias == propertyAlias;
        });
        return (typeof property !== 'undefined') ? property.value : '';
    };

    $scope.isCollapsed = function(fieldset)
    {
        if(typeof fieldset.collapse === "undefined")
        {
            fieldset.collapse = true;
        }
        return fieldset.collapse;
    }

    //helper for expanding/collapsing fieldsets
    $scope.focusFieldset = function(fieldset){
        fixDisableSelection();

        if (!$scope.model.config.enableCollapsing) {
            return;
        }

        var iniState;

        if(fieldset)
        {
            iniState = fieldset.collapse;
        }

        _.each($scope.model.value.fieldsets, function(fieldset){
            fieldset.collapse = true;
        });

        if(!fieldset && $scope.model.value.fieldsets.length == 1)
        {
            $scope.model.value.fieldsets[0].collapse = false;
            return;
        }

        if(iniState && fieldset)
        {
            fieldset.collapse = !iniState;
        }
    }

    //ini the fieldset expand/collapse
    $scope.focusFieldset();

    //developerMode helpers
    $scope.model.value.toString = stringify;

    //encapsulate stringify (should be built into browsers, not sure of IE support)
    function stringify() {
        return JSON.stringify(this);
    }

    //watch for changes
    $scope.$watch('model.value', function (v) {
        if ($scope.model.config.developerMode) {
            console.log(v);
            if (typeof v === 'string') {
                $scope.model.value = JSON.parse(v);
                $scope.model.value.toString = stringify;
            }
        }
    });

    //helper to count what is visible
    function countVisible()
    {
        return $scope.model.value.fieldsets.length;
    }

    // helper to get initial model if none was provided
    function getDefaultModel(config) {
        if (config.startWithAddButton)
            return { fieldsets: [] };

        return { fieldsets: [getEmptyRenderFieldset(config.fieldsets[0])] };
    }

    //helper to add an empty fieldset to the render model
    function getEmptyRenderFieldset (fieldsetModel) {
        return {alias: fieldsetModel.alias, collapse: false, isValid: true, properties: []};
    }

    //helper to ensure no nulls make it into the model
    function removeNulls(model){
        if(model.fieldsets){
            _.each(model.fieldsets, function(fieldset, index){
                if(!fieldset){
                    model.fieldsets.splice(index, 1);
                    removeNulls(model);
                }
            });

            return model;
        }
    }

    // Hack for U4-4281 / #61
    function fixDisableSelection() {
        $timeout(function() {
            $('.archetypeEditor .controls')
                .bind('mousedown.ui-disableSelection selectstart.ui-disableSelection', function(e) {
                    e.stopImmediatePropagation();
                });
        }, 1000);
    }

    //helper to lookup validity when given a fieldsetIndex and property alias
    $scope.getPropertyValidity = function(fieldsetIndex, alias)
    {
        if($scope.model.value.fieldsets[fieldsetIndex])
        {
            var property = _.find($scope.model.value.fieldsets[fieldsetIndex].properties, function(property){
                return property.alias == alias;
            });
        }

        return (typeof property == 'undefined') ? true : property.isValid;
    }

    // helper to force the current form into the dirty state
    $scope.setDirty = function () {
        if($scope.form) {
            $scope.form.$setDirty();
        }
    }

    //custom js
    if ($scope.model.config.customJsPath) {
        assetsService.loadJs($scope.model.config.customJsPath);
    }

    //archetype css
    assetsService.loadCss("/App_Plugins/Archetype/css/archetype.css");

    //custom css
    if($scope.model.config.customCssPath)
    {
        assetsService.loadCss($scope.model.config.customCssPath);
    }
});

angular.module("umbraco").controller("Imulus.ArchetypeConfigController", function ($scope, $http, assetsService, dialogService, archetypePropertyEditorResource) {

    //$scope.model.value = "";
    //console.log($scope.model.value);

    //define empty items
    var newPropertyModel = '{"alias": "", "remove": false, "collapse": false, "label": "", "helpText": "", "dataTypeGuid": "0cc0eba1-9960-42c9-bf9b-60e150b429ae", "value": ""}';
    var newFieldsetModel = '{"alias": "", "remove": false, "collapse": false, "labelTemplate": "", "icon": "", "label": "", "properties": [' + newPropertyModel + ']}';
    var defaultFieldsetConfigModel = JSON.parse('{"showAdvancedOptions": false, "startWithAddButton": false, "hideFieldsetToolbar": false, "enableMultipleFieldsets": false, "hideFieldsetControls": false, "hidePropertyLabel": false, "maxFieldsets": null, "enableCollapsing": true, "fieldsets": [' + newFieldsetModel + ']}');

    //ini the model
    $scope.model.value = $scope.model.value || defaultFieldsetConfigModel;

    //ini the render model
    initConfigRenderModel();

    //get the available datatypes
    archetypePropertyEditorResource.getAllDataTypes().then(function(data) {
        $scope.availableDataTypes = data;
    });

    //iconPicker
    $scope.selectIcon = function(fieldset){
        var dialog = dialogService.iconPicker({
            callback: function(data){
                fieldset.icon = data;
            }
        });
    }

    //config for the sorting
    $scope.sortableOptions = {
        axis: 'y',
        cursor: "move",
        handle: ".handle",
        update: function (ev, ui) {

        },
        stop: function (ev, ui) {

        }
    };

    //function that determines how to manage expanding/collapsing fieldsets
    $scope.focusFieldset = function(fieldset){
        var iniState;

        if(fieldset)
        {
            iniState = fieldset.collapse;
        }

        _.each($scope.archetypeConfigRenderModel.fieldsets, function(fieldset){
            if($scope.archetypeConfigRenderModel.fieldsets.length == 1 && fieldset.remove == false)
            {
                fieldset.collapse = false;
                return;
            }

            if(fieldset.label)
            {
                fieldset.collapse = true;
            }
            else
            {
                fieldset.collapse = false;
            }
        });

        if(iniState)
        {
            fieldset.collapse = !iniState;
        }
    }

    //ini the fieldsets
    $scope.focusFieldset();

    //function that determines how to manage expanding/collapsing properties
    $scope.focusProperty = function(properties, property){
        var iniState;

        if(property)
        {
            iniState = property.collapse;
        }

        _.each(properties, function(property){
            if(property.label)
            {
                property.collapse = true;
            }
            else
            {
                property.collapse = false;
            }
        });

        if(iniState)
        {
            property.collapse = !iniState;
        }
    }

    //ini the properties
    _.each($scope.archetypeConfigRenderModel.fieldsets, function(fieldset){
            $scope.focusProperty(fieldset.properties);
    });

    //setup JSON.stringify helpers
    $scope.archetypeConfigRenderModel.toString = stringify;

    //encapsulate stringify (should be built into browsers, not sure of IE support)
    function stringify() {
        return JSON.stringify(this);
    }

    //watch for changes
    $scope.$watch('archetypeConfigRenderModel', function (v) {
        //console.log(v);
        if (typeof v === 'string') {
            $scope.archetypeConfigRenderModel = JSON.parse(v);
            $scope.archetypeConfigRenderModel.toString = stringify;
        }
    });

    $scope.autoPopulateAlias = function(s) {
        var modelType = s.hasOwnProperty('fieldset') ? 'fieldset' : 'property';
        var modelProperty = s[modelType];

        if (!modelProperty.aliasIsDirty) {
            modelProperty.alias = modelProperty.label.toUmbracoAlias();
        }
    }

    $scope.markAliasDirty = function(s) {
        var modelType = s.hasOwnProperty('fieldset') ? 'fieldset' : 'property';
        var modelProperty = s[modelType];

        if (!modelProperty.aliasIsDirty) {
            modelProperty.aliasIsDirty = true;;
        }
    }

    //helper that returns if an item can be removed
    $scope.canRemoveFieldset = function ()
    {
        return countVisibleFieldset() > 1;
    }

    //helper that returns if an item can be sorted
    $scope.canSortFieldset = function ()
    {
        return countVisibleFieldset() > 1;
    }

    //helper that returns if an item can be removed
    $scope.canRemoveProperty = function (fieldset)
    {
        return countVisibleProperty(fieldset) > 1;
    }

    //helper that returns if an item can be sorted
    $scope.canSortProperty = function (fieldset)
    {
        return countVisibleProperty(fieldset) > 1;
    }

    $scope.getDataTypeNameByGuid = function (guid) {
        if ($scope.availableDataTypes == null) // Might not be initialized yet?
            return "";
        
        var dataType = _.find($scope.availableDataTypes, function(d) {
            return d.guid == guid;
        });

        return dataType == null ? "" : dataType.name;
    }

    //helper to count what is visible
    function countVisibleFieldset()
    {
        var count = 0;

        _.each($scope.archetypeConfigRenderModel.fieldsets, function(fieldset){
            if (fieldset.remove == false) {
                count++;
            }
        });

        return count;
    }

    //determines how many properties are visible
    function countVisibleProperty(fieldset)
    {
        var count = 0;

        for (var i in fieldset.properties) {
            if (fieldset.properties[i].remove == false) {
                count++;
            }
        }

        return count;
    }

    //handles a fieldset add
    $scope.addFieldsetRow = function ($index, $event) {
        $scope.archetypeConfigRenderModel.fieldsets.splice($index + 1, 0, JSON.parse(newFieldsetModel));
        $scope.focusFieldset();
    }

    //rather than splice the archetypeConfigRenderModel, we're hiding this and cleaning onFormSubmitting
    $scope.removeFieldsetRow = function ($index) {
        if ($scope.canRemoveFieldset()) {
            if (confirm('Are you sure you want to remove this?')) {
                $scope.archetypeConfigRenderModel.fieldsets[$index].remove = true;
            }
        }
    }

    //handles a property add
    $scope.addPropertyRow = function (fieldset, $index) {
        fieldset.properties.splice($index + 1, 0, JSON.parse(newPropertyModel));
    }

    //rather than splice the archetypeConfigRenderModel, we're hiding this and cleaning onFormSubmitting
    $scope.removePropertyRow = function (fieldset, $index) {
        if ($scope.canRemoveProperty(fieldset)) {
            if (confirm('Are you sure you want to remove this?')) {
                fieldset.properties[$index].remove = true;
            }
        }
    }

    //helper to ini the render model
    function initConfigRenderModel()
    {
        $scope.archetypeConfigRenderModel = $scope.model.value;

        _.each($scope.archetypeConfigRenderModel.fieldsets, function(fieldset){

            fieldset.remove = false;
            if (fieldset.alias.length > 0)
                fieldset.aliasIsDirty = true;

            if(fieldset.label)
            {
                fieldset.collapse = true;
            }

            _.each(fieldset.properties, function(fieldset){
                fieldset.remove = false;
                if (fieldset.alias.length > 0)
                    fieldset.aliasIsDirty = true;
            });
        });
    }

    //sync things up on save
    $scope.$on("formSubmitting", function (ev, args) {
        syncModelToRenderModel();
    });

    //helper to sync the model to the renderModel
    function syncModelToRenderModel()
    {
        $scope.model.value = $scope.archetypeConfigRenderModel;
        var fieldsets = [];

        _.each($scope.archetypeConfigRenderModel.fieldsets, function(fieldset){
            //check fieldsets
            if (!fieldset.remove) {
                fieldsets.push(fieldset);

                var properties = [];

                _.each(fieldset.properties, function(property){
                   if (!property.remove) {
                        properties.push(property);
                    }
                });

                fieldset.properties = properties;
            }
        });

        $scope.model.value.fieldsets = fieldsets;
    }

    //archetype css
    assetsService.loadCss("/App_Plugins/Archetype/css/archetype.css");
});

angular.module("umbraco.directives").directive('archetypeProperty', function ($compile, $http, archetypePropertyEditorResource, umbPropEditorHelper, $timeout, $rootScope, $q) {

    function getFieldsetByAlias(fieldsets, alias)
    {
        return _.find(fieldsets, function(fieldset){
            return fieldset.alias == alias;
        });
    }

    function getPropertyIndexByAlias(properties, alias){
        for (var i in properties)
        {
            if (properties[i].alias == alias) {
                return i;
            }
        }
    }

    function getPropertyByAlias(fieldset, alias){
        return _.find(fieldset.properties, function(property){
            return property.alias == alias; 
        });
    }

    //helper that returns a JS ojbect from 'value' string or the original string
    function jsonOrString(value, developerMode, debugLabel){
        if(value && typeof value == 'string'){
            try{
                if(developerMode == '1'){
                    console.log("Trying to parse " + debugLabel + ": " + value); 
                }
                value = JSON.parse(value);
            }
            catch(exception)
            {
                if(developerMode == '1'){
                    console.log("Failed to parse " + debugLabel + "."); 
                }
            }
        }

        if(value && developerMode == '1'){
            console.log(debugLabel + " post-parsing: ");
            console.log(value); 
        }

        return value;
    }

    var linker = function (scope, element, attrs, ngModelCtrl) {
        var configFieldsetModel = getFieldsetByAlias(scope.archetypeConfig.fieldsets, scope.fieldset.alias);
        var view = "";
        var label = configFieldsetModel.properties[scope.propertyConfigIndex].label;
        var dataTypeGuid = configFieldsetModel.properties[scope.propertyConfigIndex].dataTypeGuid;
        var config = null;
        var alias = configFieldsetModel.properties[scope.propertyConfigIndex].alias;
        var defaultValue = configFieldsetModel.properties[scope.propertyConfigIndex].value;
        var umbracoPropertyAlias = scope.umbracoPropertyAlias;
        // initialize container for invalid fieldset property identifiers (store on ngModelCtrl to separate Archetype validations, e.g. when there two Archetype properties on the same document)
        if(ngModelCtrl.invalidProperties == null) {
            ngModelCtrl.invalidProperties = [];
        }

        //try to convert the defaultValue to a JS object
        defaultValue = jsonOrString(defaultValue, scope.archetypeConfig.developerMode, "defaultValue");

        //grab info for the selected datatype, prepare for view
        archetypePropertyEditorResource.getDataType(dataTypeGuid).then(function (data) {
            //transform preValues array into object expected by propertyeditor views
            var configObj = {};
            _.each(data.preValues, function(p) {
                configObj[p.key] = p.value;
            });
            config = configObj;

            //determine the view to use [...] and load it
            archetypePropertyEditorResource.getPropertyEditorMapping(data.selectedEditor).then(function(propertyEditor) {
                var pathToView = umbPropEditorHelper.getViewPath(propertyEditor.view);

                //load in the DefaultPreValues for the PropertyEditor, if any
                var defaultConfigObj =  {};
                if (propertyEditor.hasOwnProperty('defaultPreValues') && propertyEditor.defaultPreValues != null) {
                    _.extend(defaultConfigObj, propertyEditor.defaultPreValues);
                }

                var mergedConfig = _.extend(defaultConfigObj, config);

                loadView(pathToView, mergedConfig, defaultValue, alias, umbracoPropertyAlias, scope, element, ngModelCtrl, validateProperty);
            });
        });

        scope.$on("formSubmitting", function (ev, args) {
            // "hard" validate to highlight any erroneous entries
            _.each(scope.fieldset.properties, function (property) {
                validateProperty(scope.fieldset, property, true);
            });
        });

        scope.$on("formSubmitted", function (ev, args) {
            // reset the nested fieldset validation state after submit
            ngModelCtrl.invalidProperties = [];
        });

        // need to pass the property fieldset here to clear any invalid state of the fieldset when validating a single fieldset property
        // - it's the Umbraco way to hide the invalid state when altering an invalid property, even if the new value isn't valid either
        function validateProperty(fieldset, property, markAsInvalid) {
            var valid = true;
            fieldset.isValid = true;
            property.isValid = true;
            var propertyConfig = getPropertyByAlias(configFieldsetModel, property.alias);
            if (propertyConfig) {
                // use property.value !== property.value to check for NaN values on numeric inputs
                if (propertyConfig.required && (property.value == null || property.value === "" || property.value !== property.value)) {
                    valid = false;
                }
                // issue 116: RegEx validate property value
                // Only validate the property value if anything has been entered - RegEx is considered a supplement to "required".
                if (valid == true && propertyConfig.regEx && property.value) {
                    var regEx = new RegExp(propertyConfig.regEx);
                    if (regEx.test(property.value) == false) {
                        valid = false;
                    }
                }
                // only mark the property as invalid when doing a "hard" validation
                if (valid == false && markAsInvalid == true) {
                    property.isValid = false;
                }
            }

            // handle nested fieldset validation by storing the identifier of all invalid fieldset properties 
            var fieldsetIdentifier = scope.umbracoPropertyAlias + "_" + scope.fieldsetIndex;
            var propertyIdentifier = fieldsetIdentifier + "_" + property.alias;
            var propertyIdentifierIndex = ngModelCtrl.invalidProperties.indexOf(propertyIdentifier);
            if (valid == false) {
                if (propertyIdentifierIndex == -1) {
                    ngModelCtrl.invalidProperties.push(propertyIdentifier);
                }
            }
            else {
                if (propertyIdentifierIndex != -1) {
                    ngModelCtrl.invalidProperties.splice(propertyIdentifierIndex, 1);
                }
            }
            
            if (markAsInvalid) {
                // mark the entire fieldset as invalid if there are any invalid properties in the fieldset, otherwise mark it as valid
                fieldset.isValid =
                    _.find(ngModelCtrl.invalidProperties, function (item) {
                        return item.indexOf(fieldsetIdentifier) == 0
                    }) == null;
            }

            // set invalid state if one or more fieldsets contain invalid properties
            ngModelCtrl.$setValidity('validation', ngModelCtrl.invalidProperties.length == 0);
        }
    }

    function loadView(view, config, defaultValue, alias, umbracoPropertyAlias, scope, element, ngModelCtrl, validateProperty) {
        if (view)
        {
            $http.get(view).success(function (data) {
                if (data) {
                    if (scope.archetypeConfig.developerMode == '1')
                    {
                        console.log(scope);
                    }

                    //define the initial model and config
                    scope.form = scope.umbracoForm;
                    scope.model = {};
                    scope.model.config = {};

                    //ini the property value after test to make sure a prop exists in the renderModel
                    var renderModelPropertyIndex = getPropertyIndexByAlias(scope.archetypeRenderModel.fieldsets[scope.fieldsetIndex].properties, alias);

                    if (!renderModelPropertyIndex)
                    {
                        scope.archetypeRenderModel.fieldsets[scope.fieldsetIndex].properties.push(JSON.parse('{"alias": "' + alias + '", "value": "' + defaultValue + '"}'));
                        renderModelPropertyIndex = getPropertyIndexByAlias(scope.archetypeRenderModel.fieldsets[scope.fieldsetIndex].properties, alias);
                    }
                    scope.model.value = scope.archetypeRenderModel.fieldsets[scope.fieldsetIndex].properties[renderModelPropertyIndex].value;

                    //set the config from the prevalues
                    scope.model.config = config;

                    //some items need an alias
                    scope.model.alias = "archetype-property-" + umbracoPropertyAlias + "-" + scope.fieldsetIndex + "-" + scope.propertyConfigIndex;

                    //watch for changes since there is no two-way binding with the local model.value
                    scope.$watch('model.value', function (newValue, oldValue) {
                        scope.archetypeRenderModel.fieldsets[scope.fieldsetIndex].properties[renderModelPropertyIndex].value = newValue;

                        // call validation method for the property when the value changes 
                        // use "soft" validation to mimic the default umbraco validation style (show error highlights on submit, not while entering data)
                        validateProperty(scope.archetypeRenderModel.fieldsets[scope.fieldsetIndex], scope.archetypeRenderModel.fieldsets[scope.fieldsetIndex].properties[renderModelPropertyIndex], false);
                    });

                    element.html(data).show();
                    $compile(element.contents())(scope);

                    $timeout(function() {
                        var def = $q.defer();
                        def.resolve(true);
                        $rootScope.$apply();
                    }, 500);
                }
            });
        }
    }

    return {
        require: "^ngModel",
        restrict: "E",
        replace: true,
        link: linker,
        scope: {
            property: '=',
            propertyConfigIndex: '=',
            archetypeConfig: '=',
            fieldset: '=',
            fieldsetIndex: '=',
            archetypeRenderModel: '=',
            umbracoPropertyAlias: '=',
            umbracoForm: '='
        }
    }
});
angular.module("umbraco.directives").directive('archetypeCustomView', function ($compile, $http) {
    var linker = function (scope, element, attrs) {

        var config = scope.archetype.config.archetypeConfig;

        var view = "/App_plugins/Archetype/views/archetype.default.html";
        if(config.customViewPath) {
            view = config.customViewPath;
        }

        $http.get(view).then(function(data) {

            scope.model = {};
            scope.model.value = scope.archetype.value;
            scope.model.config = scope.archetype.config;

            element.html(data.data).show();

            $compile(element.contents())(scope);
        });
    }

    return {
        restrict: "A",
        replace: true,
        link: linker,
        scope: {
            archetype: "="
        }
    }
});
angular.module("umbraco.directives").directive('archetypeLocalize', function (archetypeLocalizationService) {
	var linker = function (scope, element, attrs){

		var key = scope.key;
        
        archetypeLocalizationService.localize(key).then(function(value){
        	if(value){
        		element.html(value);
        	}
        });
	}   

	return {
	    restrict: "E",
	    rep1ace: true,
	    link: linker,
	    scope: {
	    	key: '@'
	    }
	}
});
angular.module('umbraco.services').factory('archetypeLocalizationService', function($http, $q, userService){
    var service = {
        resourceFileLoaded: false,
        dictionary: {},
        localize: function(key) {
            var deferred = $q.defer();

            if(service.resourceFileLoaded){
                var value = service._lookup(key);
                deferred.resolve(value);
            }
            else{
                service.initLocalizedResources().then(function(dictionary){
                   var value = service._lookup(key);
                   deferred.resolve(value); 
                });
            } 

            return deferred.promise;
        },
        _lookup: function(key){
            return service.dictionary[key];
        },
        initLocalizedResources:function () {
            var deferred = $q.defer();
            userService.getCurrentUser().then(function(user){
                $http.get("/App_plugins/Archetype/langs/" + user.locale + ".js") 
                    .then(function(response){
                        service.resourceFileLoaded = true;
                        service.dictionary = response.data;

                        return deferred.resolve(service.dictionary);
                    }, function(err){
                        return deferred.reject("Lang file missing");
                    });
            });
            return deferred.promise;
        }
    }

    return service;
});
angular.module('umbraco.resources').factory('archetypePropertyEditorResource', function($q, $http, umbRequestHelper){
    return { 
        getAllDataTypes: function() {
            // Hack - grab DataTypes from Tree API, as `dataTypeService.getAll()` isn't implemented yet
            return umbRequestHelper.resourcePromise(
                $http.get("/umbraco/backoffice/ArchetypeApi/ArchetypeDataType/GetAll"), 'Failed to retrieve datatypes from tree service'
            );
        },
        getDataType: function(guid) {
        	return umbRequestHelper.resourcePromise(
        		$http.get("/umbraco/backoffice/ArchetypeApi/ArchetypeDataType/GetByGuid?guid=" + guid), 'Failed to retrieve datatype'
    		);
        },
        getPropertyEditorMapping: function(alias) {
            return umbRequestHelper.resourcePromise(
                $http.get("/umbraco/backoffice/ArchetypeApi/ArchetypeDataType/GetAllPropertyEditors"), 'Failed to retrieve datatype mappings'
            ).then(function (data) {
                var result = _.find(data, function(d) {
                    return d.alias === alias;
                });

                if (result != null) 
                    return result;

                return "";
            });
        }
    }
}); 