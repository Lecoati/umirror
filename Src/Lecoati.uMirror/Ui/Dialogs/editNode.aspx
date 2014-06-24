<%@ Page Language="C#" MasterPageFile="/Umbraco/masterpages/umbracoPage.Master" AutoEventWireup="true" CodeBehind="editNode.aspx.cs" Inherits="Lecoati.uMirror.Ui.editNode" %>
<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link href="/umbraco_client/Editors/EditMacro.css?cdv=2" type="text/css" rel="stylesheet"/>
    <link type="text/css" href="/umbraco/plugins/uMirror/css/smoothness/jquery-ui-1.9.1.custom.min.css" rel="stylesheet" />	
	<link type="text/css" href="/umbraco/plugins/uMirror/css/uMirror.css" rel="Stylesheet" />
    <script src="/umbraco/plugins/uMirror/scripts/jquery-1.8.2.js"></script>
    <script src="/umbraco/plugins/uMirror/scripts/jquery-ui.js"></script>

    <script type="text/javascript">

        $(function () {

            // Accordion
            $("#<%=accordion.ClientID%>").accordion({
                heightStyle: "content"
            });

            // Control identifier
            $(".dllSuggestElement").change(function () {
                initDllSuggestElement($(this), true);
            })

            $(".dllSuggestElement").each(function () {
                initDllSuggestElement($(this), false);
            })

            // Control identifier
            $("#<%=ddlUmbIdentifierProperty.ClientID%>").change(function () {
                initDllIdentifierProperty();
            })

            $("#<%=DDLIdentifier.ClientID%>").change(function () {
                initDllIdentifierProperty();
            })

            $("#<%=txtXmlIdentifierXPath.ClientID%>").keyup(function () {
                
                if ($("#<%=ddlUmbIdentifierProperty.ClientID%>").val() != "") {
                    var control = "#<%=accordion.ClientID%> .node_property input." + $("#<%=ddlUmbIdentifierProperty.ClientID%>").val();
                    $(control).parent().parent().parent().find("input:eq(0)").val($("#<%=txtXmlIdentifierXPath.ClientID%>").val());
                }
            });

            initDllIdentifierProperty();

            // Init dllSuggestElement
            function initDllSuggestElement(control, init) {
                var input = control.next('input');
                input.next('span').css('visibility', 'hidden');
                if (control.val() == '') {
                    input.val('');
                    input.hide();
                }
                else if (control.val() != '##other##') {
                    input.val(control.val());
                    input.hide();
                }
                else {
                    if (init) input.val('');
                    input.show();
                }
                //initDllIdentifierProperty();
            }

            // Init identifier Property
            function initDllIdentifierProperty() {
                $("#<%=accordion.ClientID%> .node_property input").removeAttr("disabled");
                $("#<%=accordion.ClientID%> .node_property select").removeAttr("disabled");                
                if ($("#<%=ddlUmbIdentifierProperty.ClientID%>").val() != "") {
                    var control = "#<%=accordion.ClientID%> .node_property input." + $("#<%=ddlUmbIdentifierProperty.ClientID%>").val();
                    $(control).parent().parent().parent().find("select").attr("disabled", "disabled");
                    $(control).parent().parent().parent().find("select").val($("#<%=DDLIdentifier.ClientID%>").val());
                    var inputId = $(control).parent().parent().parent().find("input:eq(0)");
                    inputId.val($("#<%=txtXmlIdentifierXPath.ClientID%>").val());
                }
            }

        });
    
    </script>
</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="body" runat="server">
    
    <asp:HiddenField ID="hdnNodeId" runat="server" />
    <asp:HiddenField ID="hdnProperty" runat="server" />
    
    <umb:TabView ID="tabGeneral" runat="server" Visible="true" Width="552px" Height="692px" />

    <umb:Pane ID="PaneSource" runat="server" Text="Source (xml)">
        <div style="margin:6px 0 0 0"></div>
        <umb:PropertyPanel ID="PPElementsSelector" runat="server" Text="Elements:<br /><small>XPath to select the set of elements to import<br />&nbsp;</small>">
            <asp:DropDownList ID="DDLElement" cssClass="dllSuggestElement" runat="server"></asp:DropDownList>
            <asp:TextBox ID="txtXmlDocumentXPath" runat="server" MaxLength="100" CssClass="guiInputText guiInputStandardSize"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="&nbsp;&nbsp;Please fill this field" ControlToValidate="txtXmlDocumentXPath"  ForeColor="Red"></asp:RequiredFieldValidator>
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="PPanel4" runat="server" Text="Primary Key:<br /><small>XPath to select the Primary key value<br />&nbsp;</small>">
            <asp:DropDownList ID="DDLIdentifier" cssClass="dllSuggestElement" runat="server"></asp:DropDownList>
            <asp:TextBox ID="txtXmlIdentifierXPath" runat="server" MaxLength="100" CssClass="guiInputText guiInputStandardSize"></asp:TextBox>
            <asp:RequiredFieldValidator runat="server" ErrorMessage="&nbsp;&nbsp;Please fill this field" ControlToValidate="txtXmlIdentifierXPath"  ForeColor="Red"></asp:RequiredFieldValidator>
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="PPanel5" runat="server" Text="Node Name:<br /><small>XPath to select the node name value</small>">
            <asp:DropDownList ID="DDLNodeName" cssClass="dllSuggestElement" runat="server"></asp:DropDownList>
            <asp:TextBox ID="txtXmlNodeNameXPath" runat="server" MaxLength="100" CssClass="guiInputText guiInputStandardSize"></asp:TextBox>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="&nbsp;&nbsp;Please fill this field" ControlToValidate="txtXmlNodeNameXPath"  ForeColor="Red"></asp:RequiredFieldValidator>
        </umb:PropertyPanel>
    </umb:Pane>
    
    <umb:Pane ID="PaneDestination" runat="server" Text="Destination">
        <div style="margin:6px 0 0 0"></div>
        <umb:PropertyPanel ID="PPKeyProperty" runat="server" Text="Foreign Key Property:<br /><small>Property where the foreign key will be stored<br />&nbsp;</small>">
            <asp:DropDownList ID="ddlUmbIdentifierProperty" runat="server" ></asp:DropDownList>
            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="&nbsp;&nbsp;Please fill this field" ControlToValidate="ddlUmbIdentifierProperty"  ForeColor="Red"></asp:RequiredFieldValidator>
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="PPanelCompareNodeName" runat="server" Text="Ignore Node Name:<br /><small>Do not compare and update the node name<br />&nbsp;</small>">
            <asp:CheckBox ID="chkCompareNodeName" runat="server" />
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="PPanel8" runat="server" Text="Ignore Delete:<br/><small>Only add and update nodes, never delete</small>">
            <asp:CheckBox ID="chkNeverDelete" runat="server" />
        </umb:PropertyPanel>
    </umb:Pane>

    <umb:Pane ID="PaneProperties" runat="server" Text="Properties">
        <asp:Panel ID="accordion" runat="server"></asp:Panel>
    </umb:Pane>

    <umb:Pane ID="Pane5" runat="server">
        <umb:PropertyPanel ID="PPanel6" runat="server" Text="levels<br/><small>Number of levels to ignore between father and children, ex: in case you are using datefolder (year/month), level=2</small>">
            <asp:TextBox ID="txtLevel" runat="server" MaxLength="100" CssClass="guiInputText guiInputStandardSize"></asp:TextBox><asp:RangeValidator
                ID="RangeValidator1" runat="server" ErrorMessage="&nbsp;&nbsp;Please enter a number" 
                MinimumValue="0" MaximumValue="10000000" ControlToValidate="txtLevel" ForeColor="Red" 
                Type="Integer"></asp:RangeValidator>
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="PPanel7" runat="server" Text="Crop node name (number of letter)<br/><small>Crop automatically the node name to avoid too long url</small>">
            <asp:TextBox ID="txtTruncateNodeName" runat="server" MaxLength="100" CssClass="guiInputText guiInputStandardSize"></asp:TextBox><asp:RangeValidator
                ID="RangeValidator2" runat="server" ErrorMessage="&nbsp;&nbsp;Please enter a number" 
                MinimumValue="1" MaximumValue="100" ControlToValidate="txtTruncateNodeName" ForeColor="Red" 
                Type="Integer"></asp:RangeValidator>
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="PPanel9" runat="server" Text="Only add<br/><small>Do not update or delete, only add new node</small>">
            <asp:CheckBox ID="chkOnlyAdd" runat="server" />
        </umb:PropertyPanel>
    </umb:Pane>

    
</asp:Content>
