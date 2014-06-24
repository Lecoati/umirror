<%@ Page MasterPageFile="/Umbraco/masterpages/umbracoDialog.Master" Language="C#" AutoEventWireup="true" CodeBehind="ImportProyect.aspx.cs" Inherits="Lecoati.uMirror.Ui.ImportProyect" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="Content2" runat="server" ContentPlaceHolderID="body">
    
    <input id="tempFile" type="hidden" name="tempFile" runat="server" />

    <cc1:Pane runat="server">
        <p>
            <span class="guiDialogNormal">To import a synchronizer project, find the ".umr" file on your computer by clicking the "Browse" button and click "Import".</span>
        </p> 
        <p>
            <input id="documentTypeFile" type="file" runat="server" />
            </p>
    </cc1:Pane>

    <cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
        <asp:Panel ID="Wizard" runat="server" Visible="True">
            <a href="#" class="btn btn-link" onclick="UmbClientMgr.mainWindow().UmbClientMgr.closeModalWindow();UmbClientMgr.closeModalWindow(); return False;"><%=umbraco.ui.Text("cancel")%></a>
            <asp:Button ID="submit" Text="Import" CssClass="btn btn-primary" click="UmbClientMgr.mainWindow().UmbClientMgr.closeModalWindow();UmbClientMgr.closeModalWindow(); return False;" runat="server"></asp:Button>
        </asp:Panel>
        <asp:Panel ID="done" runat="server" Visible="False">
            Your project has been imported!
        </asp:Panel>


    </cc1:Pane>

</asp:Content>