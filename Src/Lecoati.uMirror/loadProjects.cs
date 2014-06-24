using umbraco.cms.presentation.Trees;
using Lecoati.uMirror.Core;
using umbraco.interfaces;
using umbraco.BusinessLogic.Actions;
using umbraco.businesslogic;
using Lecoati.uMirror.Bll;
using Lecoati.uMirror.Pocos;
using System.Linq;
using System;

namespace Lecoati.uMirror
{

    [Tree("developer", "projects", "uMirror", sortOrder: 100, initialize:true)]
    public class loadProjects: BaseTree
    {

        public loadProjects(string application): base(application) { }

        protected override void CreateRootNode(ref XmlTreeNode rootNode)
        {

            rootNode.Icon = "icon-folder";
            //rootNode.Icon = "../../plugins/uMirror/images/folder.png";
            //rootNode.OpenIcon = "../../plugins/uMirror/images/folder.png";
            rootNode.NodeType = "init" + TreeAlias;
            rootNode.NodeID = "init";
        }

        public override void RenderJS(ref System.Text.StringBuilder Javascript)
        {
            Javascript.Append(
                @"
                    function openProject(id)
                    {
                        UmbClientMgr.contentFrame('plugins/uMirror/dialogs/editProject.aspx?id=' + id);
                    } 
               ");
        }

        public override void Render(ref XmlTree tree)
        {
            foreach (Project project in new BllProject().GetAllProjects())
            {
                var synNode = XmlTreeNode.Create(this);
                synNode.NodeID = project.id.ToString();
                synNode.NodeType = "initprojects";
                synNode.Text = project.Name;
                synNode.Icon = "icon-untitled";
                synNode.OpenIcon = "icon-untitled";
                //synNode.Icon = "../../plugins/uMirror/images/project.png";
                OnBeforeNodeRender(ref tree, ref synNode, EventArgs.Empty);
                synNode.Action = "javascript:openProject(" + project.id.ToString() + ")";
                if (synNode != null)
                {
                    synNode.Source = "/umbraco/tree.aspx?rnd=5&id=" + project.id.ToString() + "&treeType=nodes&contextMenu=true&isDialog=false&projectId=" + project.id.ToString();
                    tree.Add(synNode);
                }
                OnAfterNodeRender(ref tree, ref synNode, EventArgs.Empty);
            }
        }

        protected override void CreateRootNodeActions(ref System.Collections.Generic.List<IAction> actions)
        {
            actions.Clear();
            actions.Add(ActionNew.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionImportProject.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionRefresh.Instance);
        }

        protected override void CreateAllowedActions(ref System.Collections.Generic.List<IAction> actions)
        {
            actions.Add(ActionNew.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionSyncStart.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionDelete.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionExportProject.Instance);
            actions.Add(ContextMenuSeperator.Instance);
            actions.Add(ActionRefresh.Instance);
        }

    }
}