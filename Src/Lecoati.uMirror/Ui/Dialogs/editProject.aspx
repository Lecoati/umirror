<%@ Page Language="C#" MasterPageFile="/Umbraco/masterpages/umbracoPage.Master" AutoEventWireup="True" CodeBehind="editProject.aspx.cs" Inherits="Lecoati.uMirror.Ui.editProject" %>

<%@ Register Namespace="umbraco.uicontrols" Assembly="controls" TagPrefix="umb" %>
<%@ Register Namespace="umbraco.uicontrols.TreePicker" Assembly="controls" TagPrefix="umbtree" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">

    <link href="/umbraco_client/Editors/EditMacro.css?cdv=2" type="text/css" rel="stylesheet"/>

    <style>

        .success {
            color: #ffffff;
            text-shadow: 0 -1px 0 rgba(0, 0, 0, 0.25);
            background-color: #53a93f;
            background-image: -moz-linear-gradient(top, #53a93f, #53a93f);
            background-image: -webkit-gradient(linear, 0 0, 0 100%, from(#53a93f), to(#53a93f));
            background-image: -webkit-linear-gradient(top, #53a93f, #53a93f);
            background-image: -o-linear-gradient(top, #53a93f, #53a93f);
            background-image: linear-gradient(to bottom, #53a93f, #53a93f);
            background-repeat: repeat-x;
            border-color: #53a93f #53a93f #38712a;
            border-color: rgba(0, 0, 0, 0.1) rgba(0, 0, 0, 0.1) rgba(0, 0, 0, 0.25);
            filter: progid:DXImageTransform.Microsoft.gradient(startColorstr='#ff53a93f', endColorstr='#ff53a93f', GradientType=0);
            filter: progid:DXImageTransform.Microsoft.gradient(enabled=false);
            padding: 9px 12px 0px 12px;
            margin-bottom: 0;           
            border: 1px solid #cccccc;
            -webkit-border-radius: 2px;
            -moz-border-radius: 2px;
            border-radius: 2px;
        }

        .notice {
            color: #000000;
            background: #f2f2f2;
        }

    </style>

    <script type="text/javascript">

        $(function () {

            /* ************************************************* */
            /* uMIrror started */
            /* ************************************************* */
            if ($('#<%=hdnProjectIsStarted.ClientID%>').val() == "yes") {
                synchronizer.services.getapplock(function (retVal) {
                    if (retVal == true) {
                        //UmbClientMgr.mainTree().refreshTree('projects');
                        updateStatus()
                    }
                });
            }

            /* ************************************************* */
            /* Log */
            /* ************************************************* */
            $(".syncLogs").click(function () {
                $('.umb-panel-header ul li').removeClass('active');
                $('.umb-panel-header ul li:eq(2)').addClass('active');
                $('.umb-panel .umb-tab-pane').removeClass('active');
                $('.umb-panel .umb-tab-pane:eq(2)').addClass('active');
            })

            /* ************************************************* */
            /* Task */
            /* ************************************************* */
            ddlPeriodChange();

            /* ************************************************* */
            /* Method */
            /* ************************************************* */
            $('#<%=ddlAttMethods.ClientID%>').change(function (e) {
                if ($(this).val() == "") {
                    $('#<%=txtXmlFileName.ClientID%>').removeAttr("disabled");
                    $('#<%=txtXmlFileName.ClientID%>').val("");
                    $('#runSelectedMethod').hide();
                }
                else {
                    $('#<%=txtXmlFileName.ClientID%>').attr("disabled", "disabled");
                    $('#<%=txtXmlFileName.ClientID%>').val($(this).find("option[value='" + $(this).val() + "']").attr("class"));
                    $('#runSelectedMethod').show();
                }
            })

            if ($('#<%=ddlAttMethods.ClientID%>').val() != "") {
                $('#<%=txtXmlFileName.ClientID%>').attr("disabled", "disabled");
                $('#<%=txtXmlFileName.ClientID%>').val($('#<%=ddlAttMethods.ClientID%> option:selected').attr("class"));
                $('#runSelectedMethod').show();
            }
            else
            {
                $('#runSelectedMethod').hide();
            }

            if ($('#<%=ddlAttMethods.ClientID%> option').length > 1) {
                $('#<%=ddlAttMethods.ClientID%>').show();
            }
            else {
                $('#<%=ddlAttMethods.ClientID%>').hide();
            }

            $('#runSelectedMethod').click(function () {
                $(this).hide();
                $('#runSelectedMethodWait').show();
                synchronizer.services.startMethod($('#<%=ddlAttMethods.ClientID%>').val(), function (retVal) {
                    $('#runSelectedMethod').show();
                    $('#runSelectedMethodWait').hide();
                    if (retVal == "done")
                        alert("The method has been completed succefuly");
                    else
                        alert(retVal)
                });
            });

        });

        function updateStatus() {

            $('#<%=noticesuccess.ClientID%>').hide();
            $('#animation').show();

            synchronizer.services.getappnum(function (retVal) {
                if (retVal != '0') { jQuery('.lNum').html(retVal); }
            });

            synchronizer.services.getapplock(function (retVal) {
                if (retVal == true) {
                    $('#<%=noticesuccess.ClientID%>').hide();
                    $('#animation').show();
                    setTimeout('updateStatus();', 1000);
                }
                else {
                    //UmbClientMgr.mainTree().refreshTree('projects');
                    window.location = document.URL;
                }
            });
        };

        function ddlPeriodChange() {

            var valor = $('#<%=ddlPeriod.ClientID%> :selected').text();
            switch (valor) {
                case "hourly":
                    {
                        $("#tab02 .umb-pane:eq(1)").hide();
                        break;
                    }
                case "daily":
                    {
                        $("#tab02 .umb-pane:eq(1)").show();
                        $("#tab02 .umb-pane:eq(1) .umb-el-wrap:eq(0)").hide();
                        $("#tab02 .umb-pane:eq(1) .umb-el-wrap:eq(1)").hide();
                        $("#tab02 .umb-pane:eq(1) .umb-el-wrap:eq(2)").show();
                        break;
                    }
                case "weekly":
                    {
                        $("#tab02 .umb-pane:eq(1)").show();
                        $("#tab02 .umb-pane:eq(1) .umb-el-wrap:eq(0)").hide();
                        $("#tab02 .umb-pane:eq(1) .umb-el-wrap:eq(1)").show();
                        $("#tab02 .umb-pane:eq(1) .umb-el-wrap:eq(2)").show();
                        break
                    }
                case "monthly":
                    {
                        $("#tab02 .umb-pane:eq(1)").show();
                        $("#tab02 .umb-pane:eq(1) .umb-el-wrap:eq(0)").show();
                        $("#tab02 .umb-pane:eq(1) .umb-el-wrap:eq(1)").hide();
                        $("#tab02 .umb-pane:eq(1) .umb-el-wrap:eq(2)").show();
                        break
                    }
                default:
                    {
                        $("#tab02 .umb-pane:eq(1)").hide();
                    }
            }
        }

    </script>

</asp:Content>

<asp:Content ID="Content" ContentPlaceHolderID="body" runat="server">
    
    <asp:HiddenField ID="hdnProjectIsStarted" runat="server" />
    <asp:HiddenField ID="hdnProjectId" runat="server" />

    <umb:TabView ID="tabGeneral" runat="server" Visible="true" Width="552px" Height="692px" />

    <umb:Pane ID="divNotice" runat="server">
        <div id="noticesuccess" class="success" runat="server">
            <p>
                <asp:Label ID="SpanLastCompleted" runat="server" Text="Process never started"></asp:Label>&nbsp;
                <a href="#" class="syncLogs">(View logs)</a>
            </p>
        </div>
        <div id="animation" class="success notice" style="display:none">
            <div style="overflow:hidden; padding:5px">
                <div style=" float:left; width:45% ">
                    <img src="/umbraco/plugins/uMirror/images/ajax-loader.gif" style="margin-right:5px" />
                    <span><asp:Label ID="currentProName" runat="server" Text=""></asp:Label></span>
                </div>
                <div style=" font-size:10px; float:right; width:45%; text-align:right; padding:5px 0 0 0 ">
                    <span style="margin-right: 5px" class="lState" id="statusLabel"></span>
                    <span class="lNum"  id="Span1"></span>
                </div>
            </div>
        </div>
    </umb:Pane>

    <umb:Pane ID="NamePanel" runat="server">
        <div style="margin:6px 0 0 0"></div>
        <umb:PropertyPanel ID="PPanel1" runat="server" Text="Name:<br /><small>uMirror project's name<br />&nbsp;</small>">
            <asp:TextBox ID="txtName" runat="server" MaxLength="100" CssClass="guiInputText guiInputStandardSize"></asp:TextBox>
            <asp:RequiredFieldValidator runat="server" ErrorMessage="&nbsp;&nbsp;Please fill this field" ControlToValidate="txtName" ForeColor="Red"></asp:RequiredFieldValidator>
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="PPanel3" runat="server" Text="Xml File Path:<br /><small>ex: /App_Data/MyData.xml<br />&nbsp;</small>">
            <asp:DropDownList ID="ddlAttMethods" runat="server"></asp:DropDownList>
            <asp:TextBox ID="txtXmlFileName" runat="server" MaxLength="50" CssClass="guiInputText guiInputStandardSize"></asp:TextBox>
            &nbsp;&nbsp;<a href="#" id="runSelectedMethod" style="text-decoration:underline">Test method</a>
            <img id="runSelectedMethodWait" src="/umbraco/plugins/uMirror/images/ajax-loader.gif" style="display:none;" />
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="PPanel12" runat="server" Text="Parent Node:<br /><small>Umbraco parent node<br />&nbsp;</small>">
            <umbtree:SimpleContentPicker runat="server" ID="tree">
            </umbtree:SimpleContentPicker>
        </umb:PropertyPanel>
    </umb:Pane>

    <!-- Tab Advanced -->
    <umb:Pane ID="AdvancedPane" runat="server">
        <umb:PropertyPanel ID="PPanel4" runat="server" Text="Preview:<br/><small>Simulates the process without making any changes in BBDD</small>">
            <asp:CheckBox ID="chkPreview" runat="server" />
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="PPanel5" runat="server" Text="Record all BBDD changes in logs:<br/><small>By default, it stores only the most important operation</small>">
            <asp:CheckBox ID="chkLogAllAction" runat="server" />
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="PPanel6" runat="server" Text="Use legacy schema:<br/><small>To use with Umbraco's legacy schema</small>">
            <asp:CheckBox ID="chkOldSchema" runat="server" />
        </umb:PropertyPanel>
    </umb:Pane>

    <!-- Tab Task -->
    <umb:Pane ID="TaskPane" runat="server">
        <umb:PropertyPanel ID="PPanel7" runat="server" Text="Task period:">
             <asp:DropDownList onchange="ddlPeriodChange()" id="ddlPeriod" runat="server">
                <asp:ListItem Value="-1">none</asp:ListItem>
                <asp:ListItem Value="0">hourly</asp:ListItem>
                <asp:ListItem Value="1">daily</asp:ListItem>
                <asp:ListItem Value="2">weekly</asp:ListItem>
                <asp:ListItem Value="3">monthly</asp:ListItem>
            </asp:DropDownList>
        </umb:PropertyPanel>
    </umb:Pane>

    <umb:Pane ID="RangePane" runat="server">   
        <umb:PropertyPanel ID="PPanel8" runat="server" Text="Day of the month:">
            <asp:DropDownList id="ddlMonth" runat="server">
                <asp:ListItem Value="1">1</asp:ListItem>
                <asp:ListItem Value="2">2</asp:ListItem>
                <asp:ListItem Value="3">3</asp:ListItem>
                <asp:ListItem Value="4">4</asp:ListItem>
                <asp:ListItem Value="5">5</asp:ListItem>
                <asp:ListItem Value="6">6</asp:ListItem>
                <asp:ListItem Value="7">7</asp:ListItem>
                <asp:ListItem Value="8">8</asp:ListItem>
                <asp:ListItem Value="9">9</asp:ListItem>
                <asp:ListItem Value="10">10</asp:ListItem>
                <asp:ListItem Value="11">11</asp:ListItem>
                <asp:ListItem Value="12">12</asp:ListItem>
                <asp:ListItem Value="13">13</asp:ListItem>
                <asp:ListItem Value="14">14</asp:ListItem>
                <asp:ListItem Value="15">15</asp:ListItem>
                <asp:ListItem Value="16">16</asp:ListItem>
                <asp:ListItem Value="17">17</asp:ListItem>
                <asp:ListItem Value="18">18</asp:ListItem>
                <asp:ListItem Value="19">19</asp:ListItem>
                <asp:ListItem Value="20">20</asp:ListItem>
                <asp:ListItem Value="21">21</asp:ListItem>
                <asp:ListItem Value="22">22</asp:ListItem>
                <asp:ListItem Value="23">23</asp:ListItem>
                <asp:ListItem Value="24">24</asp:ListItem>
                <asp:ListItem Value="25">25</asp:ListItem>
                <asp:ListItem Value="26">26</asp:ListItem>
                <asp:ListItem Value="27">27</asp:ListItem>
                <asp:ListItem Value="28">28</asp:ListItem>
                <asp:ListItem Value="29">29</asp:ListItem>
                <asp:ListItem Value="30">30</asp:ListItem>
                <asp:ListItem Value="31">31</asp:ListItem>
            </asp:DropDownList>
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="PPanel9" runat="server" Text="Day of the week:">
            <asp:DropDownList id="ddlDay" runat="server">
                <asp:ListItem Value="monday">monday</asp:ListItem>
                <asp:ListItem Value="tuesday">tuesday</asp:ListItem>
                <asp:ListItem Value="wednesday">wednesday</asp:ListItem>
                <asp:ListItem Value="thursday">thursday</asp:ListItem>
                <asp:ListItem Value="friday">friday</asp:ListItem>
                <asp:ListItem Value="saturday6">saturday</asp:ListItem>
                <asp:ListItem Value="sunday">sunday</asp:ListItem>
            </asp:DropDownList>
        </umb:PropertyPanel>
        <umb:PropertyPanel ID="PPanel10" runat="server" Text="Start time:">
            <asp:DropDownList id="ddlHour" runat="server">
                <asp:ListItem Value="0">0</asp:ListItem>
                <asp:ListItem Value="1">1</asp:ListItem>
                <asp:ListItem Value="2">2</asp:ListItem>
                <asp:ListItem Value="3">3</asp:ListItem>
                <asp:ListItem Value="4">4</asp:ListItem>
                <asp:ListItem Value="5">5</asp:ListItem>
                <asp:ListItem Value="6">6</asp:ListItem>
                <asp:ListItem Value="7">7</asp:ListItem>
                <asp:ListItem Value="8">8</asp:ListItem>
                <asp:ListItem Value="9">9</asp:ListItem>
                <asp:ListItem Value="10">10</asp:ListItem>
                <asp:ListItem Value="11">11</asp:ListItem>
                <asp:ListItem Value="12">12</asp:ListItem>
                <asp:ListItem Value="13">13</asp:ListItem>
                <asp:ListItem Value="14">14</asp:ListItem>
                <asp:ListItem Value="15">15</asp:ListItem>
                <asp:ListItem Value="16">16</asp:ListItem>
                <asp:ListItem Value="17">17</asp:ListItem>
                <asp:ListItem Value="18">18</asp:ListItem>
                <asp:ListItem Value="19">19</asp:ListItem>
                <asp:ListItem Value="20">20</asp:ListItem>
                <asp:ListItem Value="21">21</asp:ListItem>
                <asp:ListItem Value="22">22</asp:ListItem>
                <asp:ListItem Value="23">23</asp:ListItem>
            </asp:DropDownList> 
            h
            <asp:DropDownList id="ddlMinute" runat="server">
                <asp:ListItem Value="1">1</asp:ListItem>
                <asp:ListItem Value="2">2</asp:ListItem>
                <asp:ListItem Value="3">3</asp:ListItem>
                <asp:ListItem Value="4">4</asp:ListItem>
                <asp:ListItem Value="5">5</asp:ListItem>
                <asp:ListItem Value="6">6</asp:ListItem>
                <asp:ListItem Value="7">7</asp:ListItem>
                <asp:ListItem Value="8">8</asp:ListItem>
                <asp:ListItem Value="9">9</asp:ListItem>
                <asp:ListItem Value="10">10</asp:ListItem>
                <asp:ListItem Value="11">11</asp:ListItem>
                <asp:ListItem Value="12">12</asp:ListItem>
                <asp:ListItem Value="13">13</asp:ListItem>
                <asp:ListItem Value="14">14</asp:ListItem>
                <asp:ListItem Value="15">15</asp:ListItem>
                <asp:ListItem Value="16">16</asp:ListItem>
                <asp:ListItem Value="17">17</asp:ListItem>
                <asp:ListItem Value="18">18</asp:ListItem>
                <asp:ListItem Value="19">19</asp:ListItem>
                <asp:ListItem Value="20">20</asp:ListItem>
                <asp:ListItem Value="21">21</asp:ListItem>
                <asp:ListItem Value="22">22</asp:ListItem>
                <asp:ListItem Value="23">23</asp:ListItem>
                <asp:ListItem Value="24">24</asp:ListItem>
                <asp:ListItem Value="25">25</asp:ListItem>
                <asp:ListItem Value="26">26</asp:ListItem>
                <asp:ListItem Value="27">27</asp:ListItem>
                <asp:ListItem Value="28">28</asp:ListItem>
                <asp:ListItem Value="29">29</asp:ListItem>
                <asp:ListItem Value="30">30</asp:ListItem>
                <asp:ListItem Value="31">31</asp:ListItem>
                <asp:ListItem Value="32">32</asp:ListItem>
                <asp:ListItem Value="33">33</asp:ListItem>
                <asp:ListItem Value="34">34</asp:ListItem>
                <asp:ListItem Value="35">35</asp:ListItem>
                <asp:ListItem Value="36">36</asp:ListItem>
                <asp:ListItem Value="37">37</asp:ListItem>
                <asp:ListItem Value="38">38</asp:ListItem>
                <asp:ListItem Value="39">39</asp:ListItem>
                <asp:ListItem Value="40">40</asp:ListItem>
                <asp:ListItem Value="41">41</asp:ListItem>
                <asp:ListItem Value="42">42</asp:ListItem>
                <asp:ListItem Value="43">43</asp:ListItem>
                <asp:ListItem Value="44">44</asp:ListItem>
                <asp:ListItem Value="45">45</asp:ListItem>
                <asp:ListItem Value="46">46</asp:ListItem>
                <asp:ListItem Value="47">47</asp:ListItem>
                <asp:ListItem Value="48">48</asp:ListItem>
                <asp:ListItem Value="49">49</asp:ListItem>
                <asp:ListItem Value="50">50</asp:ListItem>
                <asp:ListItem Value="51">51</asp:ListItem>
                <asp:ListItem Value="52">52</asp:ListItem>
                <asp:ListItem Value="53">53</asp:ListItem>
                <asp:ListItem Value="54">54</asp:ListItem>
                <asp:ListItem Value="55">55</asp:ListItem>
                <asp:ListItem Value="56">56</asp:ListItem>
                <asp:ListItem Value="57">57</asp:ListItem>
                <asp:ListItem Value="58">58</asp:ListItem>
                <asp:ListItem Value="59">59</asp:ListItem>
            </asp:DropDownList> 
            m
        </umb:PropertyPanel>
    </umb:Pane>

    <umb:Pane ID="TriggerProjectPane" runat="server">
        <umb:PropertyPanel ID="PPanel11" runat="server" Text="Next proyect to start:<br/><small>At the end of the sinchronization, the selected project wil be run</small>">
            <asp:DropDownList ID="ddlTriggerProject" runat="server" DataValueField="Alias" DataTextField="Alias">
            </asp:DropDownList>
        </umb:PropertyPanel>
    </umb:Pane>

    <!-- Tab Logs -->
    <umb:Pane ID="logPanel" runat="server">
        <asp:GridView ID="gvLogTypesList" runat="server" AutoGenerateColumns="False" AllowPaging="True" AllowSorting="True" PageSize="20" BackColor="#FFFFFF" BorderColor="#DEDFDE" BorderWidth="1" GridLines="Both" CellPadding="4" ForeColor="#000000" OnSorting="gvLogTypesList_Sorting" OnPageIndexChanging="gvLogTypesList_PageIndexChanging">
		    <RowStyle BackColor="#F7F7DE" />
		    <HeaderStyle BackColor="#6B696B" Font-Bold="True" ForeColor="#FFFFFF" HorizontalAlign="Center" Wrap="false" />
		    <AlternatingRowStyle BackColor="White" />
		    <PagerSettings Mode="NumericFirstLast" FirstPageText="First" LastPageText="Last" />
		    <PagerStyle BackColor="#6B696B" ForeColor="#FFFFFF" HorizontalAlign="Right" />
		    <FooterStyle BackColor="#CCCC99" />
		    <Columns>
			    <asp:TemplateField SortExpression="DateStamp asc" HeaderText="Log Date">
				    <HeaderStyle Width="10%" />
				    <ItemTemplate>
					    <%# convertToShortDateTime(Eval("Date","{0}"))%>
				    </ItemTemplate>
			    </asp:TemplateField>
			    <asp:TemplateField SortExpression="logHeader asc" HeaderText="Log Type">
				    <HeaderStyle Width="5%" />
				    <ItemTemplate>
					    <%# Eval("Level", "{0}")%>
				    </ItemTemplate>
			    </asp:TemplateField>
			    <asp:TemplateField SortExpression="logComment asc" HeaderText="Log Detail">
				    <HeaderStyle Width="85%" />
				    <ItemTemplate>
					    <%# Eval("Message", "{0}")%>
				    </ItemTemplate>
			    </asp:TemplateField>
		    </Columns>
		    <EmptyDataTemplate>
			    <strong>There are no logs to show.</strong>
            </EmptyDataTemplate>
	    </asp:GridView>

    </umb:Pane>

</asp:Content> 
