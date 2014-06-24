using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.cms.businesslogic.web;
using umbraco.presentation.create;
using umbraco.BasePages;
using umbraco;
using umbraco.BusinessLogic;
using System.Web;
using Umbraco.Core.Logging;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Models;
using umbraco.cms.presentation.Trees;

namespace Lecoati.uMirror.Ui
{
    public partial class createNodeProyect : UserControl
    {

        #region Web Form Designer generated code

        protected override void OnInit(EventArgs e)
        {
            //
            // CODEGEN: This call is required by the ASP.NET Web Form Designer.
            //
            InitializeComponent();
            base.OnInit(e);
        }

        /// <summary>
        ///		Required method for Designer support - do not modify
        ///		the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
        }

        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string id = Request["nodeId"];
                if (id == "init")
                {
                    // Use Panel Proyect
                    PanelProject.Visible = true;
                    Button1.Text = umbraco.ui.Text("create");
                }
                else
                {
                    // Panel Node
                    PanelNode.Visible = true;
                    sbmt.Text = umbraco.ui.Text("create");

                    int counter = 0;
                    foreach (ContentType dt in ApplicationContext.Current.Services.ContentTypeService.GetAllContentTypes())
                    {
                        ListItem li = new ListItem();
                        li.Text = dt.Name;
                        li.Value = dt.Id.ToString();

                        nodeType.Items.Add(li);
                        counter++;
                    }
                    if (nodeType.Items.Count == 0)
                    {
                        sbmt.Enabled = false;
                    }
                }
            }
        }

        protected void sbmt_Click(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                string returnUrl = string.Empty;

                // To create Project
                string id = Request["nodeId"];
                if (id == "init")
                {
                    int nodeId = Request["nodeId"] != "init" ? int.Parse(Request["nodeId"]) : -1;
                    returnUrl = umbraco.presentation.create.dialogHandler_temp.Create(
                        Request["nodeType"],
                        nodeId,
                        rename.Text);

                    BasePage.Current.ClientTools
                        .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadProjects>().Tree.Alias)
                        .ChangeContentFrameUrl(returnUrl)
                        .CloseModalWindow();
                }
                else // To Create Node
                {
                    returnUrl = umbraco.presentation.create.dialogHandler_temp.Create(
                        Request["nodeType"],
                        int.Parse(nodeType.SelectedValue),
                        int.Parse(Request["nodeID"]),
                        "text") + "&docType=" + nodeType.SelectedValue + "&docTypeName=" + nodeType.SelectedItem;

                    BasePage.Current.ClientTools
                        .SetActiveTreeType(TreeDefinitionCollection.Instance.FindTree<loadProjects>().Tree.Alias)
                        .ChangeContentFrameUrl(returnUrl)
                        .CloseModalWindow();

                }



            }
        }

    }
}