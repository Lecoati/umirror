<%@ Page MasterPageFile="/Umbraco/masterpages/umbracoDialog.Master" Language="C#" AutoEventWireup="true" CodeBehind="ExportProyect.aspx.cs" Inherits="Lecoati.uMirror.Ui.ExportProyect" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="body">
    
    <cc1:Pane runat="server">
        
        <p>
            <span class="guiDialogNormal">To export a uMirror project, click "Export". </span>
        </p>
        
    </cc1:Pane>

    <cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
        <a href="#" class="btn btn-link" onclick="UmbClientMgr.mainWindow().UmbClientMgr.closeModalWindow();UmbClientMgr.closeModalWindow(); return False;"><%=umbraco.ui.Text("cancel")%></a>
        <asp:Button id="submit" Runat="server" text="export" CssClass="btn btn-primary"></asp:Button>
    </cc1:Pane>

</asp:Content>