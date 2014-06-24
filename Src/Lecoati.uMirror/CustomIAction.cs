using umbraco.interfaces;

public class ActionExportProject : IAction
{
    //create singleton
    private static readonly ActionExportProject instance = new ActionExportProject();
    private ActionExportProject() { }
    public static ActionExportProject Instance
    {
        get { return instance; }
    }

    #region IAction Members

    public char Letter
    {
        get { return 'p'; }
    }

    public bool ShowInNotifier
    {
        get { return false; }
    }

    public bool CanBePermissionAssigned
    {
        get { return false; }
    }

    public string Icon
    {
        get { return "out"; }
    }

    public string Alias
    {
        get { return "uMirrorExport"; }
    }

    public string JsSource
    {
        get { return "function IActionProxy_uMirrorExport() { UmbClientMgr.openModalWindow('plugins/uMirror/dialogs/ExportProyect.aspx?id=' + UmbClientMgr.mainTree().getActionNode().nodeId,  'Import Project', true,  400,  200)}"; }
    }   

    public string JsFunctionName
    {
        get { return "IActionProxy_uMirrorExport()"; }
    }

    #endregion
}

public class ActionImportProject : IAction
{
    //create singleton
    private static readonly ActionImportProject instance = new ActionImportProject();
    private ActionImportProject() { }
    public static ActionImportProject Instance
    {
        get { return instance; }
    }

    #region IAction Members

    public char Letter
    {
        get { return 'q'; }
    }

    public bool ShowInNotifier
    {
        get { return false; }
    }

    public bool CanBePermissionAssigned
    {
        get { return false; }
    }

    public string Icon
    {
        get { return "download-alt"; }
    }

    public string Alias
    {
        get { return "uMirrorImport"; }
    }

    public string JsSource
    {
        get { return "function IActionProxy_uMirrorImport() { UmbClientMgr.openModalWindow('plugins/uMirror/dialogs/ImportProyect.aspx?id=' + UmbClientMgr.mainTree().getActionNode().nodeId,  'Import Project', true,  400,  200)}"; }
    }

    public string JsFunctionName
    {
        get { return "IActionProxy_uMirrorImport()"; }
    }

    #endregion
}

public class ActionSyncStart : IAction
{
    //create singleton
    private static readonly ActionSyncStart instance = new ActionSyncStart();
    private ActionSyncStart() { }
    public static ActionSyncStart Instance
    {
        get { return instance; }
    }

    #region IAction Members

    public char Letter
    {
        get { return 'r'; }
    }

    public bool ShowInNotifier
    {
        get { return false; }
    }

    public bool CanBePermissionAssigned
    {
        get { return false; }
    }

    public string Icon
    {
        get { return "next"; }
    }

    public string Alias
    {
        get { return "uMirrorStart"; }
    }

    public string JsSource
    {
        get { return "function IActionProxy_uMirrorStart() { UmbClientMgr.openModalWindow('plugins/uMirror/dialogs/StartSync.aspx?id=' + UmbClientMgr.mainTree().getActionNode().nodeId,  'Import Project', true,  400,  200); return false;}"; }
    }

    public string JsFunctionName
    {
        get { return "IActionProxy_uMirrorStart()"; }
    }

    #endregion
}