using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using umbraco;

namespace Lct_uMirror.Ui
{
    public partial class CancelSync : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {

        public CancelSync()
	    {
            CurrentApp = umbraco.BusinessLogic.DefaultApps.settings.ToString();
	    }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("/umbraco/plugins/uMirror/synchronizer.asmx"));
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request["id"] != "")
            {
                int ProyectId = int.Parse(Request["id"]);
                if (Page.IsPostBack)
                {
                    if (ProyectId > 0)
                    {
                        Umbraco.Web.UI.Pages.ClientTools c = new Umbraco.Web.UI.Pages.ClientTools((Page)HttpContext.Current.CurrentHandler);
                        c.CloseModalWindow();
                    }
                }
            }
        }
    }
}