<%@ Control Language="c#" AutoEventWireup="True" CodeBehind="createNodeProject.ascx.cs" Inherits="Lecoati.uMirror.Ui.createNodeProyect" TargetSchema="http://schemas.microsoft.com/intellisense/ie5" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<!-- Project -->
<asp:Panel ID="PanelProject" runat="server" Visible="false">
    
    <cc1:Pane runat="server">
        <cc1:PropertyPanel runat="server" Text="Name">
             <asp:TextBox id="rename" CssClass="bigInput input-large-type input-block-level" Runat="server"></asp:TextBox><asp:RequiredFieldValidator id="RequiredFieldValidator2" ErrorMessage="*" ControlToValidate="rename" runat="server">*</asp:RequiredFieldValidator>   
        </cc1:PropertyPanel> 
    </cc1:Pane>

    <cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
        <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow();"><%=umbraco.ui.Text("cancel")%></a>
        <asp:Button id="Button1" Runat="server" CssClass="btn btn-primary" onclick="sbmt_Click"></asp:Button>
    </cc1:Pane>

</asp:Panel>


<!-- Node -->
<asp:Panel ID="PanelNode" runat="server" Visible="false">
    
    <cc1:Pane runat="server">
        <span><%=umbraco.ui.Text("choose")%> <%=umbraco.ui.Text("documentType")%>:<br /></span>
        <asp:ListBox cssClass="bigInput" Rows="1" SelectionMode="Single" ID="nodeType" runat="server"></asp:ListBox>
        <asp:TextBox runat="server" Style="visibility: hidden; display: none;" ID="Textbox1" />
    </cc1:Pane>

    <cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
        <a href="#" class="btn btn-link" onclick="UmbClientMgr.closeModalWindow()"><%=umbraco.ui.Text("cancel")%></a>
        <asp:Button id="sbmt" Runat="server" CssClass="btn btn-primary" onclick="sbmt_Click"></asp:Button>
    </cc1:Pane>

</asp:Panel>
