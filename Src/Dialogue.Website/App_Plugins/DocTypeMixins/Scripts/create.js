(function ($) {
    
    $(function () {
        
        // Remove Mixins from Master DocType drop down on create pop up
        $("#body_ctl00_masterType option:contains('Mixin'):not(:contains('Mixins'))").remove();

        // Hide the create template check box if you select to create a new Mixin
        $("#body_ctl00_masterType").change(function () {
            var makeTemplate = $("#body_ctl00_createTemplate");
            if (this.options[this.selectedIndex].text == "Mixins") {
                makeTemplate.data("lastValue", makeTemplate.attr("checked"));
                makeTemplate.removeAttr("checked");
                makeTemplate.parent().hide();
            }
            else {
                if (makeTemplate.data("lastValue")) {
                    makeTemplate.attr("checked", makeTemplate.data("lastValue"));
                    makeTemplate.removeData("lastValue");
                }
                makeTemplate.parent().show();
            }
        });

        // Hide the create template check box when we are adding a doc type under the Mixin doc type
        $("h3:contains('Mixins')").parent().find("input[id$=createTemplate]").removeAttr("checked");
        $("h3:contains('Mixins')").parent().find("input[id$=createTemplate]").parent().hide();

    });
    
}(jQuery));