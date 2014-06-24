using System.Collections.Generic;
using System.Web;
using umbraco.cms.presentation.Trees;
using umbraco.BusinessLogic.Actions;
using umbraco.interfaces;
using umbraco.businesslogic;
using umbraco.cms.businesslogic.web;
using Lecoati.uMirror.Core;
using Lecoati.uMirror.Pocos;
using Lecoati.uMirror.Bll;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Core;

namespace Lecoati.uMirror
{

    [Tree("developer", "nodes", "Nodes", sortOrder: 100, initialize: false)]
    public class loadNodes : BaseTree
    {
        public loadNodes(string application) : base(application) { }
        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {
            rootNode.Icon = FolderIcon;
            rootNode.OpenIcon = FolderIconOpen;
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
        }
        public override void Render(ref XmlTree tree)
        {
            string parentId = HttpContext.Current.Request["parentnodeId"];
            string projectId = HttpContext.Current.Request["projectId"];

            if (projectId != null && projectId != "" && int.Parse(projectId) >= 0)
            {

                IList<Node> syncList = new List<Node>();
                if (parentId != null && parentId != "" && int.Parse(parentId) >= 0)
                    syncList = new BllNode().GetNodes(int.Parse(parentId));
                else
                    syncList = new BllNode().GetNodesByProyect(int.Parse(projectId));

                foreach (Node node in syncList)
                {
                    
                    XmlTreeNode synNode = XmlTreeNode.Create(this);
                    synNode.NodeID = node.id.ToString();

                    ContentType DocType = (ContentType)ApplicationContext.Current.Services.ContentTypeService.GetContentType(node.UmbDocumentTypeAlias);
                    if (DocType != null)
                    {
                        synNode.Text = DocType.Name;
                        synNode.Icon = DocType.Icon;
                    }
                    synNode.NodeType = "initnodes";
                    synNode.Action = "javascript:openNode(" + node.id.ToString() + ")";

                    // If the node has a child, create icon for the tree
                    synNode.Source = "/umbraco/tree.aspx?rnd=500&id=" + node.id.ToString() + "&treeType=nodes&contextMenu=true&isDialog=false&projectid=" + projectId + "&parentnodeId=" + node.id.ToString();

                    tree.Add(synNode);
                }

            }
        }

        public override void RenderJS(ref System.Text.StringBuilder Javascript)
        {
            Javascript.Append(
            @"
                function openNode(id)
                {
                    parent.right.document.location.href = 'plugins/uMirror/dialogs/editNode.aspx?id=' + id; 
                } 
            ");
        }
        protected override void CreateAllowedActions(ref List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionNew.Instance);
            actions.Add(ActionDelete.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionRefresh.Instance);
        }

    }
}