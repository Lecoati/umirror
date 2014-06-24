<%@ Page MasterPageFile="/Umbraco/masterpages/umbracoDialog.Master" Language="C#" AutoEventWireup="true" CodeBehind="StartSync.aspx.cs" Inherits="Lecoati.uMirror.Ui.StartSync" %>
<%@ Register TagPrefix="cc1" Namespace="umbraco.uicontrols" Assembly="controls" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="body">
    <script type="text/javascript">
        function start() {

            $('.divButton').hide();
            $('.divWait').show();

            setTimeout(function () {
                UmbClientMgr.contentFrame("/umbraco/plugins/uMirror/dialogs/editProject.aspx?id=" + $("#<%=hdnProjectId.ClientID%>").val());
                if (parent.right != undefined)
                {
                    parent.right.document.location.href = "/umbraco/plugins/uMirror/dialogs/editProject.aspx?id=" + $("#<%=hdnProjectId.ClientID%>").val();
                }
                UmbClientMgr.mainWindow().UmbClientMgr.closeModalWindow();
                UmbClientMgr.closeModalWindow();
                return false;
            }, 100)
            synchronizer.services.start($("#<%=hdnProjectId.ClientID%>").val());

        }
    </script>

    <cc1:Pane runat="server">
        
        <asp:Panel ID="Wizard" runat="server" Visible="True">
            
            <cc1:Pane runat="server">
                <p>
                    <span class="guiDialogNormal">You are about to start a uMirror process.</span>
                </p>   
            </cc1:Pane>

            <cc1:Pane runat="server" CssClass="btn-toolbar umb-btn-toolbar">
                <div class="divButton" id="divButton" runat="server">  
                    <a href="#" class="btn btn-link" onclick="UmbClientMgr.mainWindow().UmbClientMgr.closeModalWindow();UmbClientMgr.closeModalWindow(); return False;"><%=umbraco.ui.Text("cancel")%></a>
                    <input type="button" value="Go!" class="btn btn-primary" onclick="javascript: start()" /> 
                </div>
                <div class="divWait" style="display:none" id="divWait" runat="server">   
                    <img src="/umbraco/plugins/uMirror/images/ajax-loader.gif" style="margin-right:5px" />
                </div>
            </cc1:Pane>

        </asp:Panel>
        
        <asp:Panel ID="Block" runat="server" Visible="True">
            <p>
                <span class="guiDialogNormal">Another process is curently running.</span>
            </p>   
        </asp:Panel>
         
    </cc1:Pane>

    <asp:HiddenField runat="server" id="hdnProjectId"></asp:HiddenField>
</asp:Content>