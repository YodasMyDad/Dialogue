(function ($) {
    
    $(function () {
        
        // Add drag and drop tree support
        if (window === top)
        {
            // Wrap the rebuild tree method to reinitialize the doc types tree
            UmbClientMgr.mainTree().rebuildTree = wrap(UmbClientMgr.mainTree().rebuildTree,
                false,
                function ()
                {
                    // Reinitialize the tree
                    initTree();
                    
                    // Force any loaded doc type nodes to reinitialize
                    $(".umbTree li[umb:type='nodeTypes']:has(ul)").each(function () {
                        setupDocTypeNode(this);
                    });
                },
                UmbClientMgr.mainTree());
            
            // Wrap the shift app method to detect an app change and destroy the drag and drop
            UmbClientMgr.appActions().shiftApp = wrap(UmbClientMgr.appActions().shiftApp,
                function () {
                    // Destroy drag and drop feature
                    $(".umbTree li[umb:type='nodeTypes'] a.ui-draggable").draggable("destroy");
                    $(".umbTree li[umb:type='nodeTypes'] a.ui-droppable").droppable("destroy");
                },
                false,
                UmbClientMgr.appActions());

            // First load so initialize tree, Initialize the tree
            initTree();
        }

    });
    
    // Helper method to allow us to hijack methods
    function wrap (functionToWrap, before, after, thisObject) {
        return function () {
            var args = Array.prototype.slice.call(arguments),
                result;
            if (before) before.apply(thisObject || this, args);
            result = functionToWrap.apply(thisObject || this, args);
            if (after) after.apply(thisObject || this, args);
            return result;
        };
    };

    // Hack the tree to support drag and drop
    function initTree()
    {
        // Disable the trees draggable features (it doesn't seem to be used anywhere anyway)
        UmbClientMgr.mainTree()._tree.settings.types.default.draggable = false;
        
        // Register onopen callback to setup doc type nodes
        UmbClientMgr.mainTree()._tree.settings.callback.onopen = wrap(UmbClientMgr.mainTree()._tree.settings.callback.onopen,
            false,
            function (NODE, TREE_OBJ) {
                setupDocTypeNode(NODE);
            },
            UmbClientMgr.mainTree()._tree);

        UmbClientMgr.mainTree().getNodeDef = wrap(UmbClientMgr.mainTree().getNodeDef,
            function(N) {
                // There is a problem with jQuery d&d messing with metadata so just clear the cache
                // it does mean the value is worked out every time, but it shouldn't be much of a 
                // performance hit.
                $(N).children("a.ui-draggable,a.ui-droppable").data("metadata", false);
            },
            false,
            UmbClientMgr.mainTree());
    }
    
    // Set's up drag and drop functionality for dtm nodes
    function setupDocTypeNode(node)
    {
        var $node = $(node);
        
        // Check node is a doc type node
        if ($node.attr("umb:type") == "nodeTypes")
        {
            // Hook up draggables
            $node.find("> ul > li > a:not(.ui-draggable):contains('Mixin')").each(function () {
                // Make the node is not the mixins top level node
                if ($(this).text().indexOf("Mixins") === -1) {
                    // Make the node draggable
                    $(this).draggable({
                        scope: "mixin",
                        helper: "clone",
                        appendTo: "body",
                        revert: true,
                        revertDuration: 0,
                        zIndex: 99999,
                        start: function (event, ui)
                        {
                            // Position the helper to the bottom right 
                            // so you can see what you are doing
                            $(ui.helper).css("margin-left", event.clientX - $(event.target).offset().left + 10);
                            $(ui.helper).css("margin-top", event.clientY - $(event.target).offset().top + 10);
                        }
                    });
                }
            });
            // Hook up droppables
            $node.find("> ul > li > a:not(.ui-droppable):not(:contains('Mixin'))").each(function () {
                // Make the node droppable
                $(this).droppable({
                    scope: "mixin",
                    greedy: true,
                    pointer: "touch",
                    over: function (e, ui) {
                        $(e.target).addClass("mixin-droppable-hover");
                    },
                    out: function (e, ui) {
                        $(e.target).removeClass("mixin-droppable-hover");
                    },
                    deactivate: function (event, ui) {
                        $(".mixin-droppable-hover").removeClass("mixin-droppable-hover");
                    },
                    drop: function (e, ui) {
                        var mixinId = $(ui.draggable).parent().attr("id");
                        var targetId = $(e.target).parent().attr("id");
                        
                        $.getJSON("/base/DTM/ApplyMixin/" + mixinId + "/" + targetId, function(data) {
                            top.UmbSpeechBubble.ShowMessage('success', 'Mixin applied', '<div style=\'padding-right:5px;\'>Mixin <strong>'+ data.MixinName +'</strong> successfully applied to doc type <strong>'+ data.TargetName +'</strong></div>');
                        });
                    }
                });
            });
        }
    }

}(jQuery));