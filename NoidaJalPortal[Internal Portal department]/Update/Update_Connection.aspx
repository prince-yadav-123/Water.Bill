<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Update_Connection.aspx.cs" Inherits="Update_connection" Title="NoidaJal::Connection Update" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">

    <script type="text/javascript" language="javascript">
    function f1()
    {
    window.showModalDialog("../Search/Search.aspx", "newwindow","center:yes;dialogWidth:800px;dialogHeight:750px; resizable:no;");
    }
    </script>

    <style type="text/css">
        .style1
        {
            font-size: 8pt;
            color: #8b0000;
            font-weight: bold;
            text-align: left;
            vertical-align: middle;
            font-family: Verdana;
            height: 17px;
        }
        .style2
        {
            text-align: left;
            vertical-align: top;
            padding-left: 5px;
            font-family: Verdana;
            width: 125px;
        }
    </style>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div>
        <center>
            <table cellpadding="1" cellspacing="0" width="750px" style="height: 450px; vertical-align: top;">
                <tr>
                    <td style="height: 30px;">
                    </td>
                </tr>
                <tr>
                    <td style="height: 100%;" valign="top">
                        <div style="text-align: center;" id="div_main" runat="server">
                            <table>
                                <tr>
                                    <td>
                                        <fieldset>
                                            <legend><b>Connection Details</b></legend>
                                            <table>
                                                <tr>
                                                    <td style="height: 10px;" colspan="4">
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="pageLabel">
                                                        Connection No.:
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:TextBox ID="txt_con_no" runat="server" CssClass="td_text" Enabled="true" MaxLength="8"></asp:TextBox>
                                                    </td>
                                                    <td>
                                                        <asp:ImageButton ID="btnSearch" runat="server" ImageUrl="~/images/View.png" OnClick="btnSearch_Click"
                                                            ToolTip="Click Search Record" AccessKey="s" CausesValidation="false" />
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="3">
                                                        <asp:LinkButton ID="link_find" CausesValidation="false" runat="server" OnClientClick="javascript:f1();">Find Consumer No ?</asp:LinkButton>
                                                    </td>
                                                </tr>
                                            </table>
                                        </fieldset>
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan='4'>
                                        <asp:Panel ID="pnl_details" runat="server" Enabled="false">
                                            <table>
                                                <tr>
                                                    <td>
                                                        <fieldset>
                                                            <legend><b>Connection Details</b></legend>
                                                            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                                                <ContentTemplate>
                                                                    <table>
                                                                        <tr>
                                                                            <td style="height: 10px;" colspan="4">
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Form No. :
                                                                            </td>
                                                                            <td class="pageLabel">
                                                                                <asp:Label ID="lbl_form_no" runat="server"></asp:Label>
                                                                            </td>
                                                                            <td class="pageLabel">
                                                                        R-Id :
                                                                    </td>
                                                                    <td class="pageControl">
                                                                        <asp:TextBox ID="txtrid" CssClass="td_text" runat="server" ToolTip="" MaxLength="10"></asp:TextBox>
                                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ErrorMessage="*"
                                                                            ControlToValidate="txt_CessAmt" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                                                    </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Sector :
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:DropDownList ID="ddl_Sector" CssClass="td_text" runat="server" AutoPostBack="True"
                                                                                    OnSelectedIndexChanged="ddl_Sector_SelectedIndexChanged">
                                                                                </asp:DropDownList>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="ddl_Sector" InitialValue="0"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                            <td class="pageLabel">
                                                                                Block :
                                                                            </td>
                                                                            <td class="style2">
                                                                                <asp:DropDownList ID="ddl_Block" CssClass="td_text" runat="server">
                                                                                </asp:DropDownList>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="ddl_Block" InitialValue="0"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                House No. :
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:TextBox ID="txt_House_No" CssClass="td_text" runat="server"></asp:TextBox>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="txt_House_No"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                            <td class="pageLabel">
                                                                                Area(sq.mtr.) :
                                                                            </td>
                                                                            <td class="style2">
                                                                                <asp:TextBox ID="txtArea" CssClass="td_text" runat="server"></asp:TextBox>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="txtArea"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Pipe Size :
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <%--<asp:TextBox ID="txt_pipe_size" CssClass="td_text" runat="server"></asp:TextBox>--%>
                                                                                <asp:DropDownList ID="ddl_pipe_size" CssClass="td_text" runat="server" Width="160px">
                                                                                </asp:DropDownList>
                                                                            </td>
                                                                            <td class="pageLabel">
                                                                                Connection Category:
                                                                            </td>
                                                                            <td class="style2">
                                                                                <asp:DropDownList ID="ddl_Con_cat" CssClass="td_text" runat="server" AutoPostBack="True"
                                                                                    OnSelectedIndexChanged="ddl_Con_cat_SelectedIndexChanged">
                                                                                </asp:DropDownList>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator12" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="ddl_Con_cat" InitialValue="0"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Connection Sub-Type:
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:DropDownList ID="ddl_flat_type" CssClass="td_text" runat="server">
                                                                                </asp:DropDownList>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator13" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="ddl_flat_type" InitialValue="0"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                            <td class="pageLabel">
                                                                                Connection Type:
                                                                            </td>
                                                                            <td class="style2">
                                                                                <asp:DropDownList ID="ddl_con_type" CssClass="td_text" runat="server">
                                                                                <asp:ListItem Value="0">--Select--</asp:ListItem>
                                                                                    <asp:ListItem Value="R">Regular</asp:ListItem>
                                                                                    <asp:ListItem Value="T">Temporary</asp:ListItem>
                                                                                    <asp:ListItem Value="M">RMC</asp:ListItem>
                                                                                    <asp:ListItem Value="S">Staff</asp:ListItem>
                                                                                </asp:DropDownList>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="style1" colspan='4'>
                                                                                Any other Water Connection on Same Property ?
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageControl">
                                                                                <asp:RadioButtonList ID="rdb_Previous_Conn" runat="server" RepeatDirection="Horizontal"
                                                                                    OnSelectedIndexChanged="rdb_Previous_Conn_SelectedIndexChanged" AutoPostBack="true"
                                                                                    Font-Size="10px">
                                                                                    <asp:ListItem Value="Y">YES</asp:ListItem>
                                                                                    <asp:ListItem Value="N">NO</asp:ListItem>
                                                                                </asp:RadioButtonList>
                                                                            </td>
                                                                            <td colspan='3' class="pageControl">
                                                                                <asp:TextBox ID="txt_Other_Conn_Details" CssClass="td_textfield" runat="server" Enabled="false">
                                                                                    
                                                                                </asp:TextBox>
                                                                            </td>
                                                                            
                                                                            </td>
                                                                    
                                                                        </tr>
                                                                    </table>
                                                                </ContentTemplate>
                                                            </asp:UpdatePanel>
                                                        </fieldset>
                                                        <fieldset>
                                                            <legend><b>Estimation Details</b></legend>
                                                            <table>
                                                                <tr>
                                                                    <td style="height: 10px;" colspan="4">
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="pageLabel">
                                                                        Estimation No:
                                                                    </td>
                                                                    <td class="pageControl">
                                                                        <asp:TextBox ID="txt_Est_No" CssClass="td_text" runat="server"></asp:TextBox>
                                                                    </td>
                                                                    <td class="pageLabel">
                                                                        Estimation Amt.:
                                                                    </td>
                                                                    <td class="pageControl">
                                                                        <asp:TextBox ID="txt_Est_Amt" CssClass="td_text" runat="server"></asp:TextBox>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="pageLabel">
                                                                        Secutity :
                                                                    </td>
                                                                    <td class="pageControl">
                                                                        <asp:TextBox ID="txt_security" CssClass="td_text" runat="server"></asp:TextBox>
                                                                    </td>
                                                                    <td class="pageLabel">
                                                                        Estimation Date :
                                                                    </td>
                                                                    <td class="pageControl">
                                                                        <asp:TextBox ID="txt_Est_date" CssClass="readonlytxt" runat="server"></asp:TextBox>
                                                                        <cc1:CalendarExtender ID="CalendarExtender9" runat="server" Format="dd-MMM-yyyy"
                                                                            TargetControlID="txt_Est_date">
                                                                        </cc1:CalendarExtender>
                                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator17" runat="server" ErrorMessage="*"
                                                                            ControlToValidate="txt_Est_date" Enabled="false"></asp:RequiredFieldValidator>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="pageLabel">
                                                                        Monthly Charges :
                                                                    </td>
                                                                    <td class="pageControl">
                                                                        <asp:TextBox ID="txt_monthlyCharges" CssClass="td_text" runat="server" ToolTip=""
                                                                            MaxLength="8"></asp:TextBox>
                                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator19" runat="server" ErrorMessage="*"
                                                                            ControlToValidate="txt_monthlyCharges" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                                                    </td>
                                                                    <td class="pageLabel">
                                                                        Cess Amt. :
                                                                    </td>
                                                                    <td class="pageControl">
                                                                        <asp:TextBox ID="txt_CessAmt" CssClass="td_text" runat="server" ToolTip="" MaxLength="8"></asp:TextBox>
                                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator20" runat="server" ErrorMessage="*"
                                                                            ControlToValidate="txt_CessAmt" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td style="height: 10px;" colspan="4">
                                                                    </td>
                                                                </tr>
                                                            </table>
                                                        </fieldset>
                                                        <fieldset>
                                                            <legend><b>Document Details</b></legend>
                                                            <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                                                                <ContentTemplate>
                                                                    <table>
                                                                        <tr>
                                                                            <td style="height: 10px;" colspan="4">
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td rowspan='5' colspan="2" cssclass="pageLabel">
                                                                                <asp:CheckBoxList ID="chk_Doc_Provided" runat="server" OnSelectedIndexChanged="chk_Doc_Provided_SelectedIndexChanged"
                                                                                    AutoPostBack="true" Height="130px" Width="180px" CssClass="pageLabel">
                                                                                    <asp:ListItem Value="txt_Allotment_Date">Allotment Letter</asp:ListItem>
                                                                                    <asp:ListItem Value="txt_Compliance_Date">Compliance Letter</asp:ListItem>
                                                                                    <asp:ListItem Value="txt_Possession_Date">Possesion Letter</asp:ListItem>
                                                                                    <asp:ListItem Value="txt_SSI_Date">SSI Letter</asp:ListItem>
                                                                                    <asp:ListItem Value="rbaffyn">Affidavit</asp:ListItem>
                                                                                </asp:CheckBoxList>
                                                                            </td>
                                                                            <td class="pageLabel">
                                                                                Dated:
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:TextBox ID="txt_Allotment_Date" CssClass="td_text" runat="server" Enabled="False"></asp:TextBox>
                                                                                <cc1:CalendarExtender ID="CalendarExtender5" runat="server" Format="dd-MMM-yyyy"
                                                                                    TargetControlID="txt_Allotment_Date">
                                                                                </cc1:CalendarExtender>
                                                                                <asp:RequiredFieldValidator ID="Required14" runat="server" ErrorMessage="*" ControlToValidate="txt_Allotment_Date"
                                                                                    Enabled="false"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Dated:
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:TextBox ID="txt_Compliance_Date" CssClass="td_text" runat="server" Enabled="False"></asp:TextBox>
                                                                                <cc1:CalendarExtender ID="CalendarExtender4" runat="server" Format="dd-MMM-yyyy"
                                                                                    TargetControlID="txt_Compliance_Date">
                                                                                </cc1:CalendarExtender>
                                                                                <asp:RequiredFieldValidator ID="Required11" runat="server" ErrorMessage="*" ControlToValidate="txt_Compliance_Date"
                                                                                    Enabled="false"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Dated:
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:TextBox ID="txt_Possession_Date" CssClass="td_text" runat="server" Enabled="False"></asp:TextBox>
                                                                                <cc1:CalendarExtender ID="CalendarExtender2" runat="server" Format="dd-MMM-yyyy"
                                                                                    TargetControlID="txt_Possession_Date">
                                                                                </cc1:CalendarExtender>
                                                                                <asp:RequiredFieldValidator ID="Required12" runat="server" ErrorMessage="*" ControlToValidate="txt_Possession_Date"
                                                                                    Enabled="false"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Dated:
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:TextBox ID="txt_SSI_Date" CssClass="td_text" runat="server" Enabled="False"></asp:TextBox>
                                                                                <cc1:CalendarExtender ID="CalendarExtender3" runat="server" Format="dd-MMM-yyyy"
                                                                                    TargetControlID="txt_SSI_Date">
                                                                                </cc1:CalendarExtender>
                                                                                <asp:RequiredFieldValidator ID="Required13" runat="server" ErrorMessage="*" ControlToValidate="txt_SSI_Date"
                                                                                    Enabled="false"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td colspan="2" class="pageControl">
                                                                                <asp:RadioButtonList ID="rbaffyn" runat="server" RepeatDirection="Horizontal" OnSelectedIndexChanged="rdb_Previous_Conn_SelectedIndexChanged"
                                                                                    AutoPostBack="true" Font-Size="10px">
                                                                                    <asp:ListItem Value="Y">YES</asp:ListItem>
                                                                                    <asp:ListItem Value="N">NO</asp:ListItem>
                                                                                </asp:RadioButtonList>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel" colspan='2'>
                                                                                Issuing Officer's Department/Designation :
                                                                            </td>
                                                                            <td colspan='2' class="pageControl">
                                                                                <asp:TextBox ID="txt_Officer_Detail" CssClass="td_textfield" runat="server" TextMode="MultiLine"></asp:TextBox>
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </ContentTemplate>
                                                            </asp:UpdatePanel>
                                                        </fieldset>
                                                        <fieldset>
                                                            <legend><b>Consumer Details</b></legend>
                                                            <asp:UpdatePanel ID="UpdatePanel3" runat="server">
                                                                <ContentTemplate>
                                                                    <table>
                                                                        <tr>
                                                                            <td style="height: 10px;" colspan="4">
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Purpose of Connection :
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:TextBox ID="txt_Purpose" Width="155px" CssClass="td_textfield" TextMode="MultiLine"
                                                                                    runat="server"></asp:TextBox>
                                                                            </td>
                                                                            <td class="pageLabel" colspan="2">
                                                                                &nbsp;
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Name :
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:TextBox ID="txt_Name" CssClass="td_text" runat="server"></asp:TextBox>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="txt_Name"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                            <td class="pageLabel">
                                                                                Father Name :
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:TextBox ID="txt_Fname" CssClass="td_text" runat="server" MaxLength="25"></asp:TextBox>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="txt_Fname"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Mobile No. :
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:TextBox ID="txt_Mobile_No" CssClass="td_text" runat="server" MaxLength="10"></asp:TextBox>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator10" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="txt_Mobile_No"></asp:RequiredFieldValidator>
                                                                                <br></br>
                                                                                     <asp:RegularExpressionValidator ID="RegularExpressionValidatortxt_Mobile_No" runat="server"
                                                                                     ControlToValidate="txt_Mobile_No" ErrorMessage="Please enter valid Mobile number!"
                                                                                      ValidationExpression="^([6-9]{1})([0-9]{9})$"></asp:RegularExpressionValidator>
                                                                            </td>
                                                                            <td class="pageLabel">
                                                                                Address :
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:TextBox ID="txt_Address" CssClass="td_text" runat="server" TextMode="MultiLine"></asp:TextBox>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="txt_Address"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Connection Date:
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:TextBox ID="txt_conn_date" CssClass="td_text" runat="server"></asp:TextBox>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator14" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="txt_conn_date"></asp:RequiredFieldValidator>
                                                                                <cc1:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd-MMM-yyyy"
                                                                                    TargetControlID="txt_conn_date">
                                                                                </cc1:CalendarExtender>
                                                                            </td>
                                                                            <td class="pageLabel">
                                                                                Email id :
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:TextBox ID="txt_email" CssClass="td_text" runat="server" Width="174px"></asp:TextBox>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="txt_email"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td colspan="4" align="center">
                                                                                &nbsp;
                                                                            </td>
                                                                        </tr>
                                                                    </table>
                                                                </ContentTemplate>
                                                            </asp:UpdatePanel>
                                                        </fieldset>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="4" align="center">
                                                        <asp:ImageButton ID="btnSave" runat="server" ImageUrl="~/images/Update.png" OnClick="btnSave_Click"
                                                            ToolTip="Click Search Record" AccessKey="s" />
                                                        <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Reset.png" CausesValidation="false"
                                                            OnClick="btnReset_Click" ToolTip="Click Reset Page" AccessKey="r" />
                                                        <asp:ImageButton ID="btnclose" runat="server" ImageUrl="~/images/Cancel.png" CausesValidation="false"
                                                            OnClick="btnclose_Click" ToolTip="Click Cancel" AccessKey="c" />
                                                    </td>
                                                </tr>
                                            </table>
                                        </asp:Panel>
                                    </td>
                                </tr>
                            </table>
                        </div>
                    </td>
                </tr>
            </table>
        </center>
    </div>
</asp:Content>
