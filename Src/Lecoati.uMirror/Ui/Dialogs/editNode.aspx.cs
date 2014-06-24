using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.uicontrols;
using umbraco.BasePages;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;
using System.Xml;
using System.Xml.XPath;
using System.IO;
using Lecoati.uMirror.Core;
using Lecoati.uMirror.Pocos;
using Lecoati.uMirror.Bll;
using System.Web;
using umbraco.uicontrols.TreePicker;

namespace Lecoati.uMirror.Ui
{
    public partial class editNode : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {
        private TabPage dataTabGeneral/*, dataTabAdvanced*/;

        private static IDictionary<string, string> suggestedElement;

        #region Events

        protected override void OnInit(EventArgs e)
        {

            Umbraco.Core.Models.ContentType DocType = null;
            Node node = null;
            Project project = null;

            if (!string.IsNullOrEmpty(Request["id"]))
            {
                node = new BllNode().GetNode(int.Parse(Request["id"]));
                DocType = (Umbraco.Core.Models.ContentType)Umbraco.Core.ApplicationContext.Current.Services.ContentTypeService.GetContentType(node.UmbDocumentTypeAlias);
                project = new BllProject().GetProjectByNode(node);
            }
            else if (!string.IsNullOrEmpty(hdnNodeId.Value.ToString()))
            {
                node = new BllNode().GetNode(int.Parse(hdnNodeId.Value));
                DocType = (Umbraco.Core.Models.ContentType)Umbraco.Core.ApplicationContext.Current.Services.ContentTypeService.GetContentType(node.UmbDocumentTypeAlias);
                project = new BllProject().GetProjectByNode(node);
            }
            else if (!string.IsNullOrEmpty(Request["type"]))
            {
                node = new Node();
                DocType = (Umbraco.Core.Models.ContentType)Umbraco.Core.ApplicationContext.Current.Services.ContentTypeService.GetContentType(int.Parse(Request["type"]));
                if (!string.IsNullOrEmpty(Request["projectID"]))
                    project = new BllProject().GetProject(int.Parse(Request["projectID"]));
                else if (!string.IsNullOrEmpty(Request["parentID"]))
                    project = new BllProject().GetProject(new BllNode().GetNode(int.Parse(Request["parentID"])).ProjectId);
            }

            if (node.id > 0)
            {
                Node tmpNode = node;
                string path = "," + tmpNode.id;
                while (tmpNode.ParentId != null)
                {
                    path = "," + tmpNode.ParentId + path;
                    tmpNode = new BllNode().GetNode(tmpNode.ParentId);
                }
                path = "-1,init," + tmpNode.ProjectId + path;

                ClientTools.SetActiveTreeType(umbraco.cms.presentation.Trees.TreeDefinitionCollection.Instance.FindTree<loadProjects>().Tree.Alias)
                           .SyncTree(path, true);
            }

            suggestedElement = SuggestElementAndAttribute(Server.MapPath(new BllProject().GetProjectFilePath(project)));

            if (!Page.IsPostBack)
            {

                /* Elements */
                string xpathPrefix = "//";
                if (node.ParentId != null || !string.IsNullOrEmpty(Request["parentID"])) xpathPrefix = "./";
                DDLElement.Items.Add(new ListItem("- Select an element -", ""));
                foreach (KeyValuePair<string, string> index in suggestedElement.Where(r => r.Value.Substring(0, 1) != "@"))
                    DDLElement.Items.Add(new ListItem(xpathPrefix + index.Value, xpathPrefix + index.Key));
                DDLElement.Items.Add(new ListItem("Other...", "##other##"));

                if (!string.IsNullOrEmpty(node.XmlDocumentXPath))
                {
                    txtXmlDocumentXPath.Text = node.XmlDocumentXPath;
                    if (DDLElement.Items.IndexOf(DDLElement.Items.FindByValue(node.XmlDocumentXPath)) > 0)
                        DDLElement.SelectedValue = node.XmlDocumentXPath;
                    else
                        DDLElement.SelectedIndex = DDLElement.Items.Count - 1;
                }

                /* Primary key */
                DDLIdentifier.Items.Add(new ListItem("- Select an element -", ""));
                foreach (KeyValuePair<string, string> index in suggestedElement)
                    DDLIdentifier.Items.Add(new ListItem(index.Value, index.Key));
                DDLIdentifier.Items.Add(new ListItem("Other...", "##other##"));

                if (!string.IsNullOrEmpty(node.XmlIdentifierXPath))
                {
                    txtXmlIdentifierXPath.Text = node.XmlIdentifierXPath;
                    if (DDLIdentifier.Items.IndexOf(DDLIdentifier.Items.FindByValue(node.XmlIdentifierXPath)) > 0)
                        DDLIdentifier.SelectedValue = node.XmlIdentifierXPath;
                    else
                        DDLIdentifier.SelectedIndex = DDLIdentifier.Items.Count - 1;
                }

                /* Node name */
                DDLNodeName.Items.Add(new ListItem("- Select an element -", ""));
                foreach (KeyValuePair<string, string> index in suggestedElement)
                    DDLNodeName.Items.Add(new ListItem(index.Value, index.Key));
                DDLNodeName.Items.Add(new ListItem("Other...", "##other##"));

                if (!string.IsNullOrEmpty(node.XmlNodeNameXPath))
                {
                    txtXmlNodeNameXPath.Text = node.XmlNodeNameXPath;
                    if (DDLNodeName.Items.IndexOf(DDLNodeName.Items.FindByValue(node.XmlNodeNameXPath)) > 0)
                        DDLNodeName.SelectedValue = node.XmlNodeNameXPath;
                    else
                        DDLNodeName.SelectedIndex = DDLNodeName.Items.Count - 1;
                }

                /* Advanced option */
                txtXmlDocumentXPath.Style.Add("display", "none");

                txtLevel.Text = node.LevelNumber.ToString();
                txtTruncateNodeName.Text = node.TruncateNodeName.ToString();
                chkOnlyAdd.Checked = node.OnlyAdd;
                chkNeverDelete.Checked = node.NeverDelete;
                chkCompareNodeName.Checked = node.IgnoreNodeName;

                // Id node exists
                if (DocType != null)
                {
                    LoadDdlUmbIdentifierProperty(DocType.Id);
                    ddlUmbIdentifierProperty.SelectedValue = node.UmbIdentifierProperty;
                }
            }

            // Fill node properties
            FillNodeProperties(DocType, node);

            /// Tab General
            dataTabGeneral = tabGeneral.NewTabPage(DocType.Alias);
            dataTabGeneral.HasMenu = true;

            dataTabGeneral.Controls.Add(PaneSource);
            dataTabGeneral.Controls.Add(PaneDestination);
            dataTabGeneral.Controls.Add(PaneProperties);

            MenuButton saveGeneral = dataTabGeneral.Menu.NewButton(0);
            saveGeneral.CssClass = "btn btn-primary";
            saveGeneral.Text = "Save";
            saveGeneral.Click += new EventHandler(saveGeneral_Click);

            /// Tab Advanced
            //dataTabAdvanced = tabGeneral.NewTabPage("Advanced");
            //dataTabAdvanced.Controls.Add(Pane5);
            //dataTabAdvanced.HasMenu = true;

            //MenuImageButton saveAdvanced = dataTabAdvanced.Menu.NewImageButton();
            //saveAdvanced.ImageUrl = umbraco.GlobalSettings.Path + "/images/editor/save.gif";
            //saveAdvanced.AlternateText = "Save";
            //saveAdvanced.Click += new ImageClickEventHandler(saveAdvanced_Click);

        }

        protected void Page_Load(object sender, EventArgs e)
        {
        }

        void saveGeneral_Click(object sender, EventArgs e)
        {
            SaveNode();
        }

        protected void ddlUmbIdentifierProperty_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hdnProperty.Value) == false)
            {
                ddlUmbIdentifierProperty.SelectedValue = hdnProperty.Value;
            }
        }

        void saveAdvanced_Click(object sender, ImageClickEventArgs e)
        {
            SaveNode();
        }

        #endregion

        #region Functions

        private void SaveNode()
        {
            
            try
            {

                Umbraco.Core.Models.ContentType DocType = null;
                Node node = null;

                if (!string.IsNullOrEmpty(hdnNodeId.Value))
                {
                    node = new BllNode().GetNode(int.Parse(hdnNodeId.Value));
                    DocType = (Umbraco.Core.Models.ContentType)Umbraco.Core.ApplicationContext.Current.Services.ContentTypeService.GetContentType(node.UmbDocumentTypeAlias);
                }
                else if (!string.IsNullOrEmpty(Request["id"]))
                {
                    node = new BllNode().GetNode(int.Parse(Request["id"]));
                    hdnNodeId.Value = node.id.ToString();
                    DocType = (Umbraco.Core.Models.ContentType)Umbraco.Core.ApplicationContext.Current.Services.ContentTypeService.GetContentType(node.UmbDocumentTypeAlias);
                }
                else if (!string.IsNullOrEmpty(Request["type"]))
                {
                    DocType = (Umbraco.Core.Models.ContentType)Umbraco.Core.ApplicationContext.Current.Services.ContentTypeService.GetContentType(int.Parse(Request["type"]));
                    if (!string.IsNullOrEmpty(Request["projectID"]))
                    {
                        node = new BllNode().GetNode(new BllNode().CreateEmptyNode(int.Parse(Request["projectID"]), int.Parse(Request["type"])));
                        hdnNodeId.Value = node.id.ToString();
                    }
                    else if (!string.IsNullOrEmpty(Request["parentID"]))
                    {
                        node = new BllNode().GetNode(new BllNode().CreateChildNode(int.Parse(Request["parentID"]), int.Parse(Request["type"])));
                        hdnNodeId.Value = node.id.ToString();
                    }   
                }

                if (node != null)
                {

                    if (node.ProjectId == null && node.ParentId != null)
                        node.ProjectId = new BllNode().GetNode(node.ParentId).ProjectId;

                    node.XmlDocumentXPath = txtXmlDocumentXPath.Text;
                    node.UmbDocumentTypeAlias = DocType.Alias;

                    node.XmlIdentifierXPath = txtXmlIdentifierXPath.Text;
                    node.XmlNodeNameXPath = txtXmlNodeNameXPath.Text;
                    if (string.IsNullOrEmpty(txtLevel.Text) == false)
                        node.LevelNumber = int.Parse(txtLevel.Text);
                    else
                        node.LevelNumber = null;

                    if (string.IsNullOrEmpty(txtTruncateNodeName.Text) == false)
                        node.TruncateNodeName = int.Parse(txtTruncateNodeName.Text);
                    else
                        node.TruncateNodeName = null;

                    node.NeverDelete = chkNeverDelete.Checked;
                    node.OnlyAdd = chkOnlyAdd.Checked;
                    node.IgnoreNodeName = chkCompareNodeName.Checked;

                    SetNodeProperties(node);

                    if (string.IsNullOrEmpty(hdnProperty.Value) == false)
                        node.UmbIdentifierProperty = hdnProperty.Value;
                    else
                        node.UmbIdentifierProperty = ddlUmbIdentifierProperty.SelectedValue;

                    new BllNode().UpdateNode(node);

                    string path = "," + node.id;
                    while (node.ParentId != null)
                    {
                        path = "," + node.ParentId + path;
                        node = new BllNode().GetNode(node.ParentId);
                    }
                    path = "-1,init," + node.ProjectId + path;

                    ClientTools.SetActiveTreeType(umbraco.cms.presentation.Trees.TreeDefinitionCollection.Instance.FindTree<loadProjects>().Tree.Alias)
                               .SyncTree(path, true);

                    ClientTools.ShowSpeechBubble(Umbraco.Web.UI.SpeechBubbleIcon.Success, "Node saved", string.Empty);

                }

            }
            catch (Exception ex)
            {
                ClientTools.ShowSpeechBubble(Umbraco.Web.UI.SpeechBubbleIcon.Error, "Node not saved", ex.Message);
            }
        }

        private IDictionary<string, string> SuggestElementAndAttribute(string xmlFilePath)
        {

            IList<string> attributes = new List<string>();
            IList<string> elements = new List<string>();
            
            if (File.Exists(xmlFilePath))
            {
                XDocument currentDoc = XDocument.Load(xmlFilePath);                
                foreach (String name in currentDoc.Root.DescendantNodes().OfType<XElement>().Select(

                    x => !string.IsNullOrEmpty(x.GetPrefixOfNamespace(x.Name.Namespace)) ? x.GetPrefixOfNamespace(x.Name.Namespace) + ":" + x.Name.LocalName : x.Name.LocalName ).Distinct())
                {
                    elements.Add(name);
                    string cleanName = name.Substring(name.LastIndexOf(":") + 1);
                    if (currentDoc.Descendants(cleanName).Any())
                    {
                        attributes = attributes.Concat(currentDoc.Descendants(cleanName).ToArray()[0].Attributes().Select(r => r.Name.LocalName).ToList()).ToList();
                    }
                }

                attributes = attributes.Distinct().ToList().OrderBy(r => r).ToList();
                elements = elements.Distinct().ToList().OrderBy(r => r).ToList();
            }

            IDictionary<string, string> results = new Dictionary<string, string>();
            foreach (string index in elements) results.Add(index, index);
            foreach (string index in attributes) results.Add("@" + index, "@" + index);

            return results;

        }

        private void FillNodeProperties(Umbraco.Core.Models.ContentType DocType, Node node)
        {

            IList<Umbraco.Core.Models.PropertyType> listProperties = DocType.CompositionPropertyTypes.ToList();
            foreach (Umbraco.Core.Models.PropertyType prop in listProperties)
            {

                // Get value from db
                Property dbProp = null;
                if (node.Properties != null && node.Properties.Any())
                    dbProp = node.Properties.SingleOrDefault(p => p.UmbPropertyAlias == prop.Alias);

                Panel divPanel = new Panel();

                // Add UmbPropertyAlias
                PropertyPanel pnlProperty = new PropertyPanel();
                pnlProperty.Text = "Alias";
                Label lblUmbPropertyAlias = new Label();
                lblUmbPropertyAlias.Text = prop.Alias;
                pnlProperty.Controls.Add(lblUmbPropertyAlias);
                divPanel.Controls.Add(pnlProperty);
                pnlProperty.Visible = false;

                // Add XmlPropertyXPath
                PropertyPanel ppanelXmlPropertyXPath = new PropertyPanel();
                ppanelXmlPropertyXPath.Text = "Property Selector:<br/><small>XPath to select the property value</small>";

                DropDownList ddlXmlPropertyPath = new DropDownList();
                ddlXmlPropertyPath.CssClass = "dllSuggestElement";
                ddlXmlPropertyPath.Style.Add("font-size", "12px");
                ddlXmlPropertyPath.Style.Add("margin-right", "5px");
                ddlXmlPropertyPath.Style.Add("font-family", "Trebuchet MS,Lucida Grande,verdana,arial");
                ddlXmlPropertyPath.Items.Add(new ListItem("- Select an element -", ""));
                foreach (KeyValuePair<string, string> index in suggestedElement)
                    ddlXmlPropertyPath.Items.Add(new ListItem(index.Value, index.Key));
                ddlXmlPropertyPath.Items.Add(new ListItem("Other...", "##other##"));

                TextBox txtXmlPropertyXPath = new TextBox();
                txtXmlPropertyXPath.Style.Add("font-size", "11px");
                txtXmlPropertyXPath.Style.Add("font-family", "Trebuchet MS,Lucida Grande,verdana,arial");
                txtXmlPropertyXPath.CssClass = "guiInputText guiInputStandardSize " + prop.Alias;

                if (dbProp != null)
                {
                    if (!string.IsNullOrEmpty(dbProp.XmlPropertyXPath))
                    {
                        txtXmlPropertyXPath.Text = dbProp.XmlPropertyXPath;
                        if (ddlXmlPropertyPath.Items.IndexOf(DDLNodeName.Items.FindByValue(dbProp.XmlPropertyXPath)) > 0)
                            ddlXmlPropertyPath.SelectedValue = dbProp.XmlPropertyXPath;
                        else
                            ddlXmlPropertyPath.SelectedIndex = DDLNodeName.Items.Count - 1;
                    }
                }
                else
                {
                    if (ddlXmlPropertyPath.Items.IndexOf(DDLNodeName.Items.FindByValue(prop.Alias)) > 0)
                    {
                        txtXmlPropertyXPath.Text = prop.Alias;
                        ddlXmlPropertyPath.SelectedValue = prop.Alias;
                    }
                }

                ppanelXmlPropertyXPath.Controls.Add(ddlXmlPropertyPath);
                ppanelXmlPropertyXPath.Controls.Add(txtXmlPropertyXPath);
                divPanel.Controls.Add(ppanelXmlPropertyXPath);

                // Add compareValue
                PropertyPanel ppanelCompareValue = new PropertyPanel();
                ppanelCompareValue.Text = "Ignore:<br/><small>Do not compare and update this property</small>";
                CheckBox chkCompareValue = new CheckBox();
                if (dbProp != null)
                    chkCompareValue.Checked = (bool)dbProp.Ignore;
                if (dbProp == null && true)
                    chkCompareValue.Checked = false;

                ppanelCompareValue.Controls.Add(chkCompareValue);
                divPanel.Controls.Add(ppanelCompareValue);

                // Media picker
                if (prop.DataTypeId.ToString() == "ead69342-f06d-4253-83ac-28000225583b")
                {
                    PropertyPanel ppanelSimpleMediaPicker = new PropertyPanel();
                    ppanelSimpleMediaPicker.Text = "Media Parent Folder<br/><small>Media folder where the file will be saved</small>";
                    umbraco.uicontrols.TreePicker.SimpleMediaPicker mp = new umbraco.uicontrols.TreePicker.SimpleMediaPicker();
                    ppanelSimpleMediaPicker.Controls.Add(mp);
                    divPanel.Controls.Add(ppanelSimpleMediaPicker);
                    if (dbProp != null && dbProp.MediaParent > 0)
                        mp.Value = dbProp.MediaParent.ToString();
                }

                // Add XmlInit
                //PropertyPanel ppanelXmlInit = new PropertyPanel();
                //ppanelXmlInit.Text = "Init in Xml<br/><small>Get property value in Xml</small>";
                //CheckBox chkXmlInit = new CheckBox();
                //if (dbProp != null && dbProp.InitLikeXml)
                //    chkXmlInit.Checked = (bool)dbProp.InitLikeXml;
                //else
                //    chkXmlInit.Checked = false;

                //ppanelXmlInit.Controls.Add(chkXmlInit);
                //divPanel.Controls.Add(ppanelXmlInit);

                if (node.UmbIdentifierProperty == prop.Alias)
                {
                    txtXmlPropertyXPath.Text = node.XmlIdentifierXPath;
                    //txtXmlPropertyXPath.Enabled = false;
                    chkCompareValue.Enabled = false;
                }

                // For the accordion to work
                HtmlGenericControl heading = new HtmlGenericControl("h3");
                HtmlAnchor anchor = new HtmlAnchor();
                anchor.HRef = "#";
                anchor.InnerText = prop.Alias;
                heading.Controls.Add(anchor);

                accordion.Controls.Add(heading);

                divPanel.CssClass = "node_property";
                accordion.Controls.Add(divPanel);

            }

        }

        private void SetNodeProperties(Node node)
        {

            node.Properties = new Property[]{};

            foreach (Control cont in accordion.Controls)
            {
                if (cont.GetType().Name == "Panel")
                {
                    // Get property pannel XmlPropertyXPath [1]
                    PropertyPanel ppanelXmlPropertyXPath = (PropertyPanel)cont.Controls[1];

                    // Get Textbox XmlPropertyXPath
                    if (cont.Controls[0] != null && cont.Controls[0].Controls[0] != null)
                    {

                        string umbPropAlias = ((Label)cont.Controls[0].Controls[0]).Text;
                        Property dbProp = node.Properties.SingleOrDefault(p => p.UmbPropertyAlias == umbPropAlias);
                        TextBox txtXmlPropertyXPath = (TextBox)ppanelXmlPropertyXPath.Controls[1];

                        // Get checkbox CompareValue
                        PropertyPanel ppanelCompareValue = (PropertyPanel)cont.Controls[2];
                        CheckBox chkCompareValue = (CheckBox)ppanelCompareValue.Controls[0];

                        // Get media picker
                        int mediaFolder = -1;
                        if (cont.Controls.Count > 3)
                        {
                            PropertyPanel ppanelSimpleMediaPicker = (PropertyPanel)cont.Controls[3];
                            SimpleMediaPicker mp = (SimpleMediaPicker)ppanelSimpleMediaPicker.Controls[0];
                            int.TryParse(mp.Value, out mediaFolder); 
                        }

                        // Get checkbox InitLikeXml
                        //PropertyPanel ppanelInitLikeXml = (PropertyPanel)cont.Controls[3];
                        //CheckBox chkInitLikeXml = (CheckBox)ppanelInitLikeXml.Controls[0];

                        // Check if exists in db
                        if (dbProp == null)
                        {
                            Property prop = new Property();
                            prop.UmbPropertyAlias = umbPropAlias;
                            prop.XmlPropertyXPath = txtXmlPropertyXPath.Text;
                            prop.Ignore = chkCompareValue.Checked;
                            prop.MediaParent = mediaFolder;
                            //prop.InitLikeXml = chkInitLikeXml.Checked;
                            // New property, check for id=0 to see if it doesn't exists in db
                            node.Properties = node.Properties.Concat(new Property[] { prop }).ToArray();
                        }
                        else
                        {
                            // Existing property, check of id
                            dbProp.XmlPropertyXPath = txtXmlPropertyXPath.Text;
                            dbProp.Ignore = chkCompareValue.Checked;
                            //dbProp.InitLikeXml = chkInitLikeXml.Checked;
                        }
                    }
                }
            }
        }

        private void LoadDdlUmbIdentifierProperty(int docTypeId)
        {
            Umbraco.Core.Models.ContentType docType = (Umbraco.Core.Models.ContentType)Umbraco.Core.ApplicationContext.Current.Services.ContentTypeService.GetContentType(docTypeId);
            IList<Umbraco.Core.Models.PropertyType> listProperties = docType.CompositionPropertyTypes.ToList();
            ddlUmbIdentifierProperty.DataSource = listProperties;
            ddlUmbIdentifierProperty.DataTextField = "Alias";
            ddlUmbIdentifierProperty.Items.Clear();
            ddlUmbIdentifierProperty.SelectedValue = null;
            ddlUmbIdentifierProperty.DataBind();
            ddlUmbIdentifierProperty.Items.Insert(0, new ListItem("- Select an element -", ""));
        }

        #endregion

        protected void DDLElement_SelectedIndexChanged(object sender, EventArgs e)
        {

            /*******************************************************************************************************/

            if (!string.IsNullOrEmpty(DDLElement.SelectedValue))
            {
                if (DDLElement.SelectedIndex == DDLElement.Items.Count - 1)
                {
                    txtXmlDocumentXPath.Style.Add("display", "inline");
                    txtXmlDocumentXPath.Text = "";
                }
                else
                {
                    txtXmlDocumentXPath.Style.Add("display", "none");
                    txtXmlDocumentXPath.Text = DDLElement.SelectedValue;

                    int id = int.Parse(Request["id"]);
                    Node node = new BllNode().GetNode(id);

                    Project project = new BllProject().GetProjectByNode(node);
                    String xmlFilePath = Server.MapPath(new BllProject().GetProjectFilePath(project));

                    if (File.Exists(xmlFilePath))
                    {
                        XDocument currentDoc = XDocument.Load(xmlFilePath);
                        var elements = currentDoc.XPathSelectElements(DDLElement.SelectedValue);
                        DDLIdentifier.Items.Clear();
                        DDLIdentifier.Items.Add(new ListItem("- Select a property -", ""));
                        if (elements.Any())
                        {
                            foreach (XName name in elements.First().Descendants().OfType<XElement>().Select(x => x.Name).Distinct())
                            {
                                DDLIdentifier.Items.Add(new ListItem(name.LocalName, name.LocalName));
                                DDLNodeName.Items.Add(new ListItem(name.LocalName, name.LocalName));
                            }

                            foreach (XAttribute name in elements.First().Attributes())
                            {
                                DDLIdentifier.Items.Add(new ListItem("@" + name.Name, "@" + name.Name));
                                DDLNodeName.Items.Add(new ListItem("@" + name.Name, "@" + name.Name));
                            }
                        }
                    }
                }
            }
            else
            {
                txtXmlDocumentXPath.Style.Add("display", "none");
                txtXmlDocumentXPath.Text = string.Empty;
            }
            /*******************************************************************************************************/

        }

    }
}