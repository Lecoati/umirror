<%@ Page MasterPageFile="/Umbraco/masterpages/umbracoDialog.Master"  Language="C#" AutoEventWireup="true" CodeBehind="CancelSync.aspx.cs" Inherits="Lecoati.uMirror.Ui.CancelSync" %>

<asp:Content ID="Content1" runat="server" ContentPlaceHolderID="body">
    <script type="text/javascript">
        function stop() {
            synchronizer.services.stop();
        }
    </script>
    <table class="propertyPane" id="Table1" cellspacing="0" cellpadding="4" width="360" border="0" runat="server">
      <tr>
        <td class="propertyContent" colspan="2">
          <asp:Panel ID="Wizard" runat="server" Visible="True">
            <p>
                <span class="guiDialogNormal">It seems that something bad is happening, we will try to cancel it. If the process persists, you always can kill the application manually through the IIS or server in use.</span>
            </p>            
            <asp:Button ID="submit" OnClientClick="javascript:stop()" Text="go!" runat="server"></asp:Button> <em><%= umbraco.ui.Text("or") %></em> <a href="#" onclick="UmbClientMgr.closeModalWindow(); return false;"><%= umbraco.ui.Text("cancel") %></a>
          </asp:Panel>
        </td>
      </tr>
    </table>
</asp:Content>
