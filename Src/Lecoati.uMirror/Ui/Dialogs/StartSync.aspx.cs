using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco;
using umbraco.BasePages;
using System.Threading;
using synchronizer;
using Lecoati.uMirror.Core;
using System.Net;

namespace Lecoati.uMirror.Ui
{
    public partial class StartSync : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("/umbraco/plugins/uMirror/synchronizer.asmx"));
        }
        protected void Page_Load(object sender, EventArgs e)
        {

            int ProyectId = int.TryParse(Request["id"], out ProyectId) ? ProyectId : -1;
            hdnProjectId.Value = ProyectId.ToString();

            if (Synchronizer.appPro > -1)
            {
                Block.Visible = true;
                Wizard.Visible = false;
            }
            else
            {
                Block.Visible = false;
                Wizard.Visible = true;
            }

        }
    }
}