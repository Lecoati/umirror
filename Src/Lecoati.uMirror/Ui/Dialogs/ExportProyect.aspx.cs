using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco;
using System.Xml;
using umbraco.BasePages;
using Lecoati.uMirror.Core;
using Lecoati.uMirror.Pocos;
using Lecoati.uMirror.Bll;

namespace Lecoati.uMirror.Ui
{

    public partial class ExportProyect : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {

        protected void Page_Load(object sender, EventArgs e)
        {

            if (Page.IsPostBack)
            {
                int ProyectId = int.Parse(Request["id"]);
                if (ProyectId > 0)
                {

                    Project proyect = new BllProject().GetProject(ProyectId, true);

                    string strXML = BllProject.Serialize(proyect);

                    if (!string.IsNullOrEmpty(strXML))
                    {
                         
                        Response.AddHeader("Content-Disposition", "attachment;filename=" + proyect.Name.Replace(" ", "") + ".umr");
                        Response.ContentType = "application/octet-stream";

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(strXML);

                        foreach (XmlNode index in doc.SelectNodes("//EntityKey")) index.ParentNode.RemoveChild(index);

                        XmlWriterSettings writerSettings = new XmlWriterSettings();
                        writerSettings.Indent = true;

                        XmlWriter xmlWriter = XmlWriter.Create(Response.OutputStream, writerSettings);
                        doc.Save(xmlWriter);

                        Response.End();

                    }
                }
            }
            
        }


    }
}