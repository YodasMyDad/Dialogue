using System;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using Dialogue.Logic.Models;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using umbraco.BusinessLogic.Actions;

namespace Dialogue.Logic.Controllers
{
    [Tree("dialogue", "dialogueTree", "Dialogue")]
    [PluginController("Dialogue")]
    public partial class DialogueTreeController : TreeController
    {
        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {
            //check if we're rendering the root node's children
            if (id == global::Umbraco.Core.Constants.System.Root.ToInvariantString())
            {
                //Nodes that we will return
                var nodes = new TreeNodeCollection();

                //Main Route
                const string mainRoute = "/dialogue/dialogueTree";

                //Add nodes
                var treeNodes = new List<SectionTreeNode>();
                treeNodes.Add(new SectionTreeNode { Id = "overview", Title = "Overview", Icon = "icon-axis-rotation-2", Route = string.Format("{0}/view/{1}", mainRoute, "overview") });
                treeNodes.Add(new SectionTreeNode { Id = "bannedemail", Title = "Banned Email", Icon = "icon-mailbox", Route = string.Format("{0}/view/{1}", mainRoute, "bannedemail") });

                foreach (var item in treeNodes)
                {
                    //When clicked - /App_Plugins/Diagnostics/backoffice/diagnosticsTree/edit.html
                    //URL in address bar - /developer/diagnosticsTree/General/someID
                    //var route = string.Format("/analytics/analyticsTree/view/{0}", item.Value);
                    var nodeToAdd = CreateTreeNode(item.Id, null, queryStrings, item.Title, item.Icon, false, item.Route);

                    //Add it to the collection
                    nodes.Add(nodeToAdd);
                }

                //Return the nodes
                return nodes;
            }

            //this tree doesn't suport rendering more than 1 level
            throw new NotSupportedException();
        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {
            var menu = new MenuItemCollection
                {
                    DefaultMenuAlias = ActionNew.Instance.Alias
                };
            menu.Items.Add<ActionNew>("Create");
            return menu;
        }
    }
}