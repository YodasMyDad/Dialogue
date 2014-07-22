(function ($) {
    
    $(function () {
        
        // Hide un-necessary property panes
        $(".dtm select[id$=_ddlTemplates], .dtm select[id$=_ddlThumbnails], .dtm input[id$=_allowAtRoot]")
            .closest(".propertypane").hide();

        // Change the label to the allowed content types property as we are changing it's purpose
        $(".dtm table[id$=_lstAllowedContentTypes]").closest(".propertyItem")
            .children(".propertyItemheader").text("Apply to nodetypes");

        // Hide check boxes on allowed templates
        $("table[id$=lstAllowedContentTypes] td label:contains('Mixin')").closest("tr").hide();
        
        // Hide tabs on mixinMaster
        $(".mixinMaster .header li:gt(0)").hide();
        
        // Hide allowed child templates on mixin master
        $(".mixinMaster table[id$=lstAllowedContentTypes]").closest(".propertypane").hide();
        
        // Move the allow in root checkbox to the first tab for tidiness
        //var allowAtRoot = $(".mixinMaster input[id$=allowAtRoot]").closest(".propertypane");

        // Clean it up
        //allowAtRoot.html(allowAtRoot.html().substring( "<br>"));
        //allowAtRoot.insertAfter($(".propertypane:nth(0)"));
        


    });
    
}(jQuery));