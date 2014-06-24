using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using umbraco.BasePages;
using umbraco.uicontrols;
using umbraco.DataLayer;
using synchronizer;
using System.Reflection;
using Lecoati.uMirror.Core;
using Lecoati.uMirror.Pocos;
using Lecoati.uMirror.Bll;
using System.Web;

namespace Lecoati.uMirror.Ui
{
    /// <summary>
    /// Manages the edit page for Projects
    /// </summary>
    public partial class editProject : Umbraco.Web.UI.Pages.UmbracoEnsuredPage
    {
        private TabPage dataTabGeneral, /* dataTabAdvanced, */ dataTabTask, dataTabLogs;
        private IList<LogItem> umbracoLogList = new List<LogItem>();
        private string _projectAlias;

        #region Events
        
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            ScriptManager.GetCurrent(Page).Services.Add(new ServiceReference("/umbraco/plugins/uMirror/synchronizer.asmx"));
        }

        protected override void OnInit(EventArgs e)
        {

            base.OnInit(e);

            /// **************************************************************
            /// General tab
            /// **************************************************************
            dataTabGeneral = tabGeneral.NewTabPage("General");
            dataTabGeneral.Controls.Add(divNotice);
            dataTabGeneral.Controls.Add(NamePanel);
            dataTabGeneral.HasMenu = true;

            MenuButton saveGeneral = dataTabGeneral.Menu.NewButton(0);
            saveGeneral.CssClass = "btn btn-primary";
            saveGeneral.Text = "Save";
            saveGeneral.Click += new EventHandler(save_Click);

            //if (!Synchronizer.appLock)
            //{
            //    MenuImageButton start = dataTabGeneral.Menu.NewImageButton(1);
            //    start.CausesValidation = false;
            //    start.ImageUrl = umbraco.GlobalSettings.Path + "/plugins/uMirror/images/start.png";
            //    start.AlternateText = "Start synchronization";
            //    start.Click += new ImageClickEventHandler(start_Click);
            //}
            //else if (Synchronizer.appPro.ToString() == Request["id"])
            //{
            //    MenuImageButton stop = dataTabGeneral.Menu.NewImageButton(1);
            //    stop.CausesValidation = false;
            //    stop.ImageUrl = umbraco.GlobalSettings.Path + "/plugins/uMirror/images/stop.png";
            //    stop.AlternateText = "Start synchronization";
            //    stop.Click += new ImageClickEventHandler(stop_Click);
            //}

            /// **************************************************************
            /// Tab Advanced
            /// **************************************************************
            //dataTabAdvanced = tabGeneral.NewTabPage("Advanced");
            //dataTabAdvanced.Controls.Add(AdvancedPane);
            //dataTabAdvanced.HasMenu = true;

            //MenuImageButton saveAdvanced = dataTabAdvanced.Menu.NewImageButton();
            //saveAdvanced.ImageUrl = umbraco.GlobalSettings.Path + "/images/editor/save.gif";
            //saveAdvanced.AlternateText = "Save";
            //saveAdvanced.Click += new ImageClickEventHandler(save_Click);

            /// **************************************************************
            /// Tab Task
            /// **************************************************************
            dataTabTask = tabGeneral.NewTabPage("Task");
            dataTabTask.Controls.Add(TaskPane);
            dataTabTask.Controls.Add(RangePane);
            //dataTabTask.Controls.Add(TriggerProjectPane);
            dataTabTask.HasMenu = true;

            //ddlTriggerProject.Items.Clear();
            //ddlTriggerProject.Items.Add(new ListItem("none", ""));
            //foreach (Project item in new BllProject().GetOthersProjects(int.Parse(Request["id"])))
            //    ddlTriggerProject.Items.Add(new ListItem(item.Name, item.id.ToString()));

            /// **************************************************************
            /// Tab Logs
            /// **************************************************************
            dataTabLogs = tabGeneral.NewTabPage("Logs");
            dataTabLogs.Controls.Add(logPanel);

            /******************************************************************************************/
            ddlAttMethods.Items.Add(new ListItem("none", ""));
            IList<MethodInfo> methods = new BllProject().GetMethods();
            foreach (var method in methods)
            {
                String assemblyRef = method.ReflectedType.Assembly.FullName + ";" + method.Name;
                if (assemblyRef.Contains("App_Code")) assemblyRef = "App_Code" + assemblyRef.Substring(assemblyRef.IndexOf(","));
                String filePath = new BllProject().GetProjectFilePath(assemblyRef);
                if (!string.IsNullOrEmpty(filePath))
                {
                    ListItem item = new ListItem(method.Name, assemblyRef);
                    item.Attributes.Add("class", filePath);
                    ddlAttMethods.Items.Add(item);
                }
            }
            /****************************************************************************************/

        }

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {

                // Load project data
                LoadProject();

                // Load current project state
                CurrentState();

                // Reset cache
                Cache.Remove("dgCache");

                // Reset viewstate sort order
                ViewState["SortOrder"] = null;

                // Reset GridView Page to the top
                gvLogTypesList.PageIndex = 0;

                // Assign default column sort order
                GetLogs("DateStamp desc");

            }
        }

        void save_Click(object sender, EventArgs e)
        {
            SaveProject();
        }

        void start_Click(object sender, ImageClickEventArgs e)
        {
            ClientTools.OpenModalWindow("plugins/uMirror/dialogs/StartSync.aspx?id=" + Request["id"], "Start", true, 400, 200, 0, 0, null, null);
        }

        void stop_Click(object sender, ImageClickEventArgs e)
        {
            ClientTools.OpenModalWindow("plugins/uMirror/dialogs/CancelSync.aspx?id=" + Request["id"], "Start", true, 400, 200, 0, 0, null, null);
        }

        protected void gvLogTypesList_Sorting(object sender, GridViewSortEventArgs e)
        {
            gvLogTypesList.PageIndex = 0;
            GetLogs(SortOrder(e.SortExpression.ToString()));
        }

        protected void gvLogTypesList_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvLogTypesList.PageIndex = e.NewPageIndex;
            GetLogs(ViewState["SortOrder"].ToString());
        }
        
        #endregion

        #region Functions

        private void RefreshGrid()
        {
            // Reset cache
            Cache.Remove("dgCache");

            // Reset viewstate sort order
            ViewState["SortOrder"] = null;

            // Reset GridView Page to the top
            gvLogTypesList.PageIndex = 0;

            // Assign default column sort order
            GetLogs("DateStamp desc");
        }

        private void LoadProject()
        {

            if (string.IsNullOrEmpty(Request["id"]))
            {
                if (Request["alias"] != null && !string.IsNullOrEmpty(Request["alias"]))
                    txtName.Text = Request["alias"];
            }
            else
            {
                Project project = new BllProject().GetProject(int.Parse(Request["id"]));
                if (project != null)
                {
                    if (Synchronizer.appPro == project.id) hdnProjectIsStarted.Value = "yes";
                    _projectAlias = project.Name;
                    ViewState["projectAlias"] = project.Name;
                    txtName.Text = project.Name;
                    if (!string.IsNullOrEmpty(project.XmlFileName))
                        txtXmlFileName.Text = project.XmlFileName;
                    chkPreview.Checked = project.Preview;
                    chkLogAllAction.Checked = project.LogAllAction;
                    chkOldSchema.Checked = project.OldSchema;
                    ddlPeriod.SelectedValue = project.Period.ToString();
                    ddlMonth.SelectedValue = project.Dayofmonth;
                    ddlDay.SelectedValue = project.Dayofweek;
                    ddlHour.SelectedValue = project.StartHour.ToString();
                    ddlMinute.SelectedValue = project.StartMinute.ToString();
                    if (project.TriggerProyect != null) ddlTriggerProject.SelectedValue = project.TriggerProyect.ToString();
                    tree.Value = project.UmbRootId.ToString();

                    // Extension Method
                    if (!string.IsNullOrEmpty(project.ExtensionMethod))
                    {
                        ddlAttMethods.SelectedIndex = ddlAttMethods.Items.IndexOf(ddlAttMethods.Items.FindByValue(project.ExtensionMethod));
                        txtXmlFileName.Enabled = false;
                    }
                    txtXmlFileName.Text = new BllProject().GetProjectFilePath(project);

                    ClientTools.SetActiveTreeType(umbraco.cms.presentation.Trees.TreeDefinitionCollection.Instance.FindTree<loadProjects>().Tree.Alias)
                        .SyncTree("-1,init," + project.id.ToString(), true);

                }
            }

        }

        private void SaveProject()
        {

            try
            {
                int idReturned = -1;

                if (!string.IsNullOrEmpty(hdnProjectId.Value))
                {
                    idReturned = int.Parse(hdnProjectId.Value);
                }
                else if (!string.IsNullOrEmpty(Request["id"]))
                {
                    idReturned = int.Parse(Request["id"]);
                    hdnProjectId.Value = idReturned.ToString();
                }
                else
                {
                    idReturned = new BllProject().CreateEmptyProject(txtName.Text);
                    hdnProjectId.Value = idReturned.ToString();
                }

                Project project = new Project();
                project.id = idReturned;
                project.Name = txtName.Text;
                project.XmlFileName = txtXmlFileName.Text;
                project.Preview = chkPreview.Checked;
                project.LogAllAction = chkLogAllAction.Checked;
                project.OldSchema = chkOldSchema.Checked;
                project.Period = int.Parse(ddlPeriod.SelectedValue);
                project.Dayofmonth = ddlMonth.SelectedValue.ToString();
                project.Dayofweek = ddlDay.SelectedValue.ToString();
                project.StartHour = int.Parse(ddlHour.SelectedValue);
                project.StartMinute = int.Parse(ddlMinute.SelectedValue);
                if (!string.IsNullOrEmpty(ddlTriggerProject.SelectedValue)) project.TriggerProyect = int.Parse(ddlTriggerProject.SelectedValue);
                else project.TriggerProyect = null;
                if (!string.IsNullOrEmpty(tree.Value)) project.UmbRootId = int.Parse(tree.Value);
                else project.UmbRootId = null;
                project.ExtensionMethod = ddlAttMethods.SelectedValue;

                new BllProject().UpdateProject(project);

                ClientTools.SetActiveTreeType(umbraco.cms.presentation.Trees.TreeDefinitionCollection.Instance.FindTree<loadProjects>().Tree.Alias)
                           .SyncTree("-1,init," + project.id.ToString(), true)
                           .ShowSpeechBubble(Umbraco.Web.UI.SpeechBubbleIcon.Success, "Project saved", string.Empty);
            }
            catch (Exception ex)
            {
                ClientTools.ShowSpeechBubble(Umbraco.Web.UI.SpeechBubbleIcon.Error, "Error to save project", ex.Message);
            }
        }

        private void CurrentState()
        {
            if (string.IsNullOrEmpty(_projectAlias) && ViewState["projectAlias"] != null)
                _projectAlias = ViewState["projectAlias"].ToString();

            if (!string.IsNullOrEmpty(_projectAlias))
            {
                LogItem lastCompleted = new BllUmbracoLog().GetLastComleted(_projectAlias);
                if (lastCompleted != null)
                {
                    if (lastCompleted.Level != "Error")
                    {
                        noticesuccess.Attributes["class"] = "success";
                        SpanLastCompleted.Text = "Last process completed:  " + "<b>" + lastCompleted.Date + "</b> "   ;
                    }
                    else
                    {
                        noticesuccess.Attributes["class"] = "error";
                        SpanLastCompleted.Text = "Last process completed:  " + "<b>" + lastCompleted.Date + "</b> ";
                    }
                }
            }

            if (Synchronizer.appPro > -1)
            {
                currentProName.Text = "Currently running: <b>" + new BllProject().GetProject(Synchronizer.appPro).Name + "</b>";
            }
        }

        protected void GetLogs(string ColumnOrder)
        {
            //Set up Cache Object and determine if it exists
            umbracoLogList = (IList<LogItem>)Cache.Get("dgCache");

            //Assign ColumnOrder to ViewState
            ViewState["SortOrder"] = ColumnOrder;

            if (umbracoLogList == null)
            {
                if (string.IsNullOrEmpty(_projectAlias) && ViewState["projectAlias"] != null)
                    _projectAlias = ViewState["projectAlias"].ToString();

                if (!string.IsNullOrEmpty(_projectAlias))
                {
                    IList<LogItem> source = new BllUmbracoLog().GetLogs(_projectAlias);
                    if (source != null && source.Any())
                    { 
                        source = SortLogList(ColumnOrder, source);
                
                        //Insert DataTable into Cache object
                        Cache.Insert("dgCache", source);
                        gvLogTypesList.DataSource = source;
                    }
                }
            }
            else
            {
                if (ViewState["SortOrder"] != null)
                { 
                    ColumnOrder =  ViewState["SortOrder"].ToString();

                    umbracoLogList = SortLogList(ColumnOrder, umbracoLogList);

                    //Bind DataGrid from Cached DataView
                    gvLogTypesList.DataSource = umbracoLogList;
                }
            }
            gvLogTypesList.Style.Add("width", "100%");
            gvLogTypesList.DataBind();

        }

        private static IList<LogItem> SortLogList(string ColumnOrder, IList<LogItem> source)
        {
            if (source != null)
            {
                if (ColumnOrder == "logHeader asc")
                    source = source.OrderBy(l => l.Level).ToList<LogItem>();
                else if (ColumnOrder == "logHeader desc")
                    source = source.OrderByDescending(l => l.Level).ToList<LogItem>();

                else if (ColumnOrder == "DateStamp asc")
                    source = source.OrderBy(l => l.Date).ToList<LogItem>();
                else if (ColumnOrder == "DateStamp desc")
                    source = source.OrderByDescending(l => l.Date).ToList<LogItem>();

                else if (ColumnOrder == "logComment asc")
                    source = source.OrderBy(l => l.Message).ToList<LogItem>();
                else if (ColumnOrder == "logComment desc")
                    source = source.OrderByDescending(l => l.Message).ToList<LogItem>();
                else
                    source = source.OrderBy(l => l.Date).ToList<LogItem>();
                return source;
            }
            else
            {
                return null;
            }
        }

        protected string SortOrder(string strField)
        {
            string strSortOrder = string.Empty;

            if (strField == ViewState["SortOrder"].ToString())
            {
                strSortOrder = strField.Replace("asc", "desc");
            }
            else
            {
                strSortOrder = strField.Replace("desc", "asc");
            }

            return strSortOrder;
        }

        protected string convertToShortDateTime(string dDate)
        {
            string convertedDate = DateTime.Parse(dDate).ToShortDateString() + " - " + DateTime.Parse(dDate).ToLongTimeString();
            return convertedDate;
        }

        #endregion

    }
}