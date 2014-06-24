using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;

using Lecoati.uMirror.Core;
using Lecoati.uMirror.Pocos;
using Lecoati.uMirror.Bll;
using Umbraco.Core.IO;
using Umbraco.Web.UI.Pages;
using System.IO;

namespace Lecoati.uMirror.Ui
{
    public partial class ImportProyect : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (Page.IsPostBack && documentTypeFile.PostedFile.FileName != "")
            {

                string tempFileName = "justDelete_" + Guid.NewGuid().ToString() + ".umr";
                string fileName = Server.MapPath(SystemDirectories.Data + "/" + tempFileName);
                tempFile.Value = fileName;

                documentTypeFile.PostedFile.SaveAs(fileName);

                XmlDocument xd = new XmlDocument();
                xd.Load(fileName);

                Project importProyect = BllProject.DeSerialize(xd.OuterXml);
                new BllProject().ImportProject(importProyect);

                File.Delete(fileName);

                Wizard.Visible = false;
                done.Visible = true;

                if ((Page)HttpContext.Current.CurrentHandler != null)
                {
                    ClientTools.SetActiveTreeType(umbraco.cms.presentation.Trees.TreeDefinitionCollection.Instance.FindTree<loadProjects>().Tree.Alias)
                               .SyncTree("-1,init," + importProyect.id.ToString(), true)
                               .ShowSpeechBubble(Umbraco.Web.UI.SpeechBubbleIcon.Success, "Project imported", string.Empty).CloseModalWindow();

                    ClientTools.ChangeContentFrameUrl("plugins/uMirror/dialogs/editProject.aspx?id=" + importProyect.id.ToString());

                    Page.ClientScript.RegisterStartupScript(this.GetType(), "closemodale", "UmbClientMgr.mainWindow().UmbClientMgr.closeModalWindow();UmbClientMgr.closeModalWindow(); return False;", true);
                }                    

            }

        }
    }
}