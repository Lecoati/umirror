using System.Net.Http.Formatting;
using umbraco;
using umbraco.BusinessLogic.Actions;
using Umbraco.Core;
using Umbraco.Web.Models.Trees;
using Umbraco.Web.Mvc;
using Umbraco.Web.Trees;
using uMirror.core.DataStore;

namespace uMirror.core
{

    [Tree("developer", "uMirror", "uMirror", sortOrder: 100, initialize: true)]
    [PluginController("uMirror")]
    public class UMirrorController : TreeController
    {

        protected override TreeNodeCollection GetTreeNodes(string id, FormDataCollection queryStrings)
        {

            TreeNodeCollection nodes = new TreeNodeCollection();

            if (id == Constants.System.Root.ToInvariantString())
            {
                foreach (Project project in Store.GetAllProjects())
                {
                    nodes.Add(CreateTreeNode("project_" + project.id.ToString(), id, queryStrings, project.Name, "icon-untitled", true, routePath : "/developer/uMirror/edit/project_" + project.id.ToString()));
                }
            }
            else {
                if (id.StartsWith("project_"))
                {
                    foreach (Node node in Store.GetNodesByProject(int.Parse(id.Replace("project_", ""))))
                    {
                        nodes.Add(CreateTreeNode("node_" + node.id.ToString(), id, queryStrings, node.UmbDocumentTypeAlias, node.UmbDocumentTypeIcon, true, routePath: "/developer/uMirror/edit/node_" + node.id.ToString()));
                    }
                }
                else {
                    foreach (Node node in Store.GetNodes(int.Parse(id.Replace("node_", ""))))
                    {
                        nodes.Add(CreateTreeNode("node_" + node.id.ToString(), id, queryStrings, node.UmbDocumentTypeAlias, node.UmbDocumentTypeIcon, true, routePath: "/developer/uMirror/edit/node_" + node.id.ToString()));
                    }
                }
            }

            return nodes;

        }

        protected override MenuItemCollection GetMenuForNode(string id, FormDataCollection queryStrings)
        {

            var menu = new MenuItemCollection();
            
            if (id == Constants.System.Root.ToInvariantString())
            {
                // root actions              
                menu.Items.Add<CreateChildEntity, ActionNew>(ui.Text("actions", ActionNew.Instance.Alias));
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
                return menu;
            }
            else
            {
                menu.Items.Add<CreateChildEntity, ActionNew>("Create");
                if (id.StartsWith("project_"))
                    menu.Items.Add<StartAction>("Start");
                menu.Items.Add<ActionDelete>(ui.Text("actions", ActionDelete.Instance.Alias));
                menu.Items.Add<RefreshNode, ActionRefresh>(ui.Text("actions", ActionRefresh.Instance.Alias), true);
            }
            return menu;

        }


    }

}