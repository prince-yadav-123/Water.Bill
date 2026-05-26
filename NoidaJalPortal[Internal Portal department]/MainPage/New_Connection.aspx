<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="New_Connection.aspx.cs" Inherits="New_Connection" Title="NoidaJal::New Connection" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div id="div_main" runat="server">
        <center>
            <table cellpadding="1" cellspacing="0" width="720px">
                <tr>
                    <td style="height: 30px;">
                    </td>
                </tr>
                <tr>
                    <td>
                        <fieldset>
                            <legend><b>Connection Details</b></legend>
                            <asp:UpdatePanel ID="UPanel1" runat="server">
                                <ContentTemplate>
                                    <table style="width: 100%;">
                                        <tr>
                                            <td class="pageLabel">
                                                Consumer No.
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_con_no" runat="server" CssClass="IDtxt" MaxLength="8" ReadOnly="true"
                                                    Enabled="false" Width="160px"></asp:TextBox>
                                            </td>
                                            <td class="pageLabel">
                                                Form No.
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_form_no" CssClass="td_text" runat="server" MaxLength="11" ></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator11" runat="server" ErrorMessage="*"
                                                    ControlToValidate="txt_form_no" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="pageLabel">
                                                Sector
                                            </td>
                                            <td class="pageControl">
                                                <asp:DropDownList ID="ddl_Sector" CssClass="td_text" runat="server" AutoPostBack="True"
                                                    OnSelectedIndexChanged="ddl_Sector_SelectedIndexChanged" Width="160px">
                                                </asp:DropDownList>
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="*"
                                                    ControlToValidate="ddl_Sector" InitialValue="0" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                            </td>
                                            <td class="pageLabel">
                                                Block
                                            </td>
                                            <td class="pageControl">
                                                <asp:DropDownList ID="ddl_Block" CssClass="td_text" runat="server" Width="160px">
                                                </asp:DropDownList>
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="*"
                                                    ControlToValidate="ddl_Block" InitialValue="0" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="pageLabel">
                                                Flat/Plot No.
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_House_No" CssClass="td_text" runat="server" MaxLength="6"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="*"
                                                    ControlToValidate="txt_House_No" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                            </td>
                                            <td class="pageLabel">
                                                Area(sq.mtr.)
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txtArea" CssClass="td_text" runat="server" MaxLength="10"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ErrorMessage="*"
                                                    ControlToValidate="txtArea" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="pageLabel">
                                                Pipe Size (MM) :
                                            </td>
                                            <td class="pageControl">
                                                <asp:DropDownList ID="ddl_pipe_size" CssClass="td_text" runat="server" Width="160px">
                                                </asp:DropDownList>
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator18" runat="server" ErrorMessage="*"
                                                    ControlToValidate="ddl_pipe_size" Enabled="false" SetFocusOnError="True" InitialValue="0"></asp:RequiredFieldValidator>
                                            </td>
                                            <td class="pageLabel">
                                                Connection Category
                                            </td>
                                            <td class="pageControl">
                                                <asp:DropDownList ID="ddl_Con_cat" CssClass="td_text" runat="server" Width="160px"
                                                    AutoPostBack="True" OnSelectedIndexChanged="ddl_Con_cat_SelectedIndexChanged">
                                                </asp:DropDownList>
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator13" runat="server" ErrorMessage="*"
                                                    ControlToValidate="ddl_Con_cat" InitialValue="0" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="pageLabel">
                                                Connection Sub-Type
                                            </td>
                                            <td class="pageControl">
                                                <asp:DropDownList ID="ddl_flat_type" CssClass="td_text" runat="server" Width="160px">
                                                </asp:DropDownList>
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator12" runat="server" ErrorMessage="*"
                                                    ControlToValidate="ddl_flat_type" InitialValue="0" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                            </td>
                                            <td class="pageLabel">
                                                Connection Type
                                            </td>
                                            <td class="pageControl">
                                                <asp:DropDownList ID="ddl_con_type" CssClass="td_text" runat="server" Width="160px">
                                                <asp:ListItem Value="0">--Select--</asp:ListItem>
                                                    <asp:ListItem Value="R">Regular</asp:ListItem>
                                                    <asp:ListItem Value="T">Temporary</asp:ListItem>
                                                     <asp:ListItem Value="M">RMC</asp:ListItem>
                                                        <asp:ListItem Value="S">Staff</asp:ListItem>
                                                </asp:DropDownList>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="pageLabel" colspan='4'>
                                                Any other Water Connection on Same Property ?
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="pageLabel">
                                                <asp:RadioButtonList ID="rdb_Previous_Conn" runat="server" RepeatDirection="Horizontal"
                                                    OnSelectedIndexChanged="rdb_Previous_Conn_SelectedIndexChanged" AutoPostBack="true"
                                                    Font-Size="10px">
                                                    <asp:ListItem Value="Y" >YES</asp:ListItem>
                                                    <asp:ListItem Value="N" Selected="True">NO</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                            <td class="pageControl">
                                                <asp:DropDownList ID="txt_Other_Conn_Details" CssClass="td_textfield" runat="server"
                                                    Enabled="false" Width="160px">
                                                   
                                                    <asp:ListItem Value="0">First</asp:ListItem>
                                                    <asp:ListItem Value="A">Second</asp:ListItem>
                                                    <asp:ListItem Value="B">Third</asp:ListItem>
                                                    <asp:ListItem Value="C">Fouth</asp:ListItem>
                                                    <asp:ListItem Value="D">Fifth</asp:ListItem>
                                                    <asp:ListItem Value="E">Sixth</asp:ListItem>
                                                    <asp:ListItem Value="F">Seventh</asp:ListItem>
                                                    <asp:ListItem Value="G">Eight</asp:ListItem>
                                                </asp:DropDownList>
                                            </td>
                                            
                                            <td class="pageLabel">Rid</td>
                                            <td class="pageLabel">
                                            <asp:TextBox ID="txtRid" runat="server" MaxLength="15" Width="160px"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="rfvRid" runat="server" ErrorMessage="*"
                                            ControlToValidate="txtRid" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                    </table>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </fieldset>
                    </td>
                </tr>
                <tr>
                    <td>
                        <fieldset>
                            <legend><b>Estimation Details</b></legend>
                            <table style="width: 100%;">
                                <tr>
                                    <td class="pageLabel">
                                        Estimation No:
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_Est_No" CssClass="td_text" runat="server" ToolTip="" MaxLength="10"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator16" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_Est_No" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                    </td>
                                    <td class="pageLabel">
                                        Estimation Amt.:
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_Est_Amt" CssClass="td_text" runat="server" ToolTip="" MaxLength="8"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator15" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_Est_Amt" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="pageLabel">
                                        Secutity :
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_security" CssClass="td_text" runat="server" ToolTip="" MaxLength="11"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_security" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                    </td>
                                    <td class="pageLabel">
                                        Estimation Date :
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_Est_date" CssClass="readonlytxt" runat="server" ToolTip="" MaxLength="11"></asp:TextBox>
                                        <cc1:CalendarExtender ID="CalendarExtender9" runat="server" Format="dd-MMM-yyyy"
                                            TargetControlID="txt_Est_date">
                                        </cc1:CalendarExtender>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator17" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_Est_date" SetFocusOnError="True"></asp:RequiredFieldValidator>
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
                            </table>
                        </fieldset>
                    </td>
                </tr>
                <tr>
                    <td>
                        <fieldset>
                            <legend><b>Document Details</b></legend>
                            <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                                <ContentTemplate>
                                    <table style="width: 100%;">
                                        <tr>
                                            <td rowspan='5' colspan="2" class="pageLabel" valign="top">
                                                <asp:CheckBoxList ID="chk_Doc_Provided" runat="server" OnSelectedIndexChanged="chk_Doc_Provided_SelectedIndexChanged"
                                                    AutoPostBack="true" Height="140px" Width="180px" CssClass="pageLabel">
                                                    <asp:ListItem Value="txt_Allotment_Date">Allotment Letter</asp:ListItem>
                                                    <asp:ListItem Value="txt_Compliance_Date">Compliance Letter</asp:ListItem>
                                                    <asp:ListItem Value="txt_Possession_Date">Possesion Letter</asp:ListItem>
                                                    <asp:ListItem Value="txt_SSI_Date">SSI Letter</asp:ListItem>
                                                    <asp:ListItem Value="rbaffyn">Affidavit</asp:ListItem>
                                                </asp:CheckBoxList>
                                            </td>
                                            <td class="pageLabel">
                                                Dated
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_Allotment_Date" CssClass="readonlytxt" runat="server" Enabled="False"
                                                    ToolTip="Allotment Date" MaxLength="11"></asp:TextBox>
                                                <cc1:CalendarExtender ID="CalendarExtender5" runat="server" Format="dd-MMM-yyyy"
                                                    TargetControlID="txt_Allotment_Date">
                                                </cc1:CalendarExtender>
                                                <asp:RequiredFieldValidator ID="Required14" runat="server" ErrorMessage="*" ControlToValidate="txt_Allotment_Date"
                                                    Enabled="false" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="pageLabel">
                                                Dated
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_Compliance_Date" CssClass="readonlytxt" runat="server" Enabled="False"
                                                    ToolTip="Compliance Date" MaxLength="11"></asp:TextBox>
                                                <cc1:CalendarExtender ID="CalendarExtender4" runat="server" Format="dd-MMM-yyyy"
                                                    TargetControlID="txt_Compliance_Date">
                                                </cc1:CalendarExtender>
                                                <asp:RequiredFieldValidator ID="Required11" runat="server" ErrorMessage="*" ControlToValidate="txt_Compliance_Date"
                                                    Enabled="false" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="pageLabel">
                                                Dated
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_Possession_Date" CssClass="readonlytxt" runat="server" Enabled="False"
                                                    ToolTip="Posession Date" MaxLength="11"></asp:TextBox>
                                                <cc1:CalendarExtender ID="CalendarExtender2" runat="server" Format="dd-MMM-yyyy"
                                                    TargetControlID="txt_Possession_Date">
                                                </cc1:CalendarExtender>
                                                <asp:RequiredFieldValidator ID="Required12" runat="server" ErrorMessage="*" ControlToValidate="txt_Possession_Date"
                                                    Enabled="false" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="pageLabel">
                                                Dated
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_SSI_Date" CssClass="readonlytxt" runat="server" Enabled="False"
                                                    ToolTip="SSI Date" MaxLength="11"></asp:TextBox>
                                                <cc1:CalendarExtender ID="CalendarExtender3" runat="server" Format="dd-MMM-yyyy"
                                                    TargetControlID="txt_SSI_Date">
                                                </cc1:CalendarExtender>
                                                <asp:RequiredFieldValidator ID="Required13" runat="server" ErrorMessage="*" ControlToValidate="txt_SSI_Date"
                                                    Enabled="false" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="pageLabel" colspan="2" class="pageControl">
                                                <asp:RadioButtonList ID="rbaffyn" runat="server" RepeatDirection="Horizontal" OnSelectedIndexChanged="rdb_Previous_Conn_SelectedIndexChanged"
                                                    AutoPostBack="true" Font-Size="10px">
                                                    <asp:ListItem Value="Y">YES</asp:ListItem>
                                                    <asp:ListItem Value="N">NO</asp:ListItem>
                                                </asp:RadioButtonList>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="pageLabel" colspan='2'>
                                                Issuing Officers Department/Designation&nbsp;
                                            </td>
                                            <td colspan='2' class="pageControl">
                                                <asp:TextBox ID="txt_Officer_Detail" CssClass="td_textfield" runat="server" TextMode="MultiLine"></asp:TextBox>
                                            </td>
                                        </tr>
                                    </table>
                                </ContentTemplate>
                            </asp:UpdatePanel>
                        </fieldset>
                    </td>
                </tr>
                <tr>
                    <td>
                        <fieldset>
                            <legend><b>Consumer Details</b></legend>
                            <table style="width: 100%;">
                                <tr>
                                    <td class="pageLabel">
                                        Consumer Name
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_Name" CssClass="td_text" runat="server" MaxLength="100"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_Name"></asp:RequiredFieldValidator>
                                    </td>
                                    <td class="pageLabel">
                                        Father Name
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_Fname" CssClass="td_text" runat="server" MaxLength="100"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_Fname"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="pageLabel">
                                        Mobile No.
                                    </td>
                                    <td class="pageControl">
                                      <asp:TextBox ID="txt_Mobile_No" CssClass="td_text" runat="server" MaxLength="12"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator10" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_Mobile_No"></asp:RequiredFieldValidator></br>
                                             <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server"
                                            ControlToValidate="txt_Mobile_No" ErrorMessage="Please enter valid Mobile number!"
                                            ValidationExpression="^([6-9]{1})([0-9]{9})$"></asp:RegularExpressionValidator>
                                    </td>
                                    <td class="pageLabel">
                                        Connection Date
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_conn_date" CssClass="readonlytxt" runat="server" ToolTip="Date of Connection"
                                            MaxLength="11"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator14" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_conn_date"></asp:RequiredFieldValidator>
                                        <cc1:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd-MMM-yyyy"
                                            TargetControlID="txt_conn_date">
                                        </cc1:CalendarExtender>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="pageLabel">
                                        Email id
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_email" CssClass="td_text" runat="server" MaxLength="30"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_email"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="pageLabel">
                                        Purpose of Connection
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_Purpose" CssClass="td_text" runat="server" TextMode="MultiLine"></asp:TextBox>
                                    </td>
                                    <td class="pageLabel">
                                        Address
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_Address" CssClass="td_text" runat="server" TextMode="MultiLine"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_Address"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                            </table>
                        </fieldset>
                    </td>
                </tr>
                <tr>
                    <td colspan="4" align="center">
                        <asp:ImageButton ID="btnSave" runat="server" ImageUrl="~/images/save.png" OnClick="btnSave_Click"
                            ToolTip="Click Save Record" AccessKey="s" />
                        <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Reset.png" CausesValidation="false"
                            OnClick="btnReset_Click" ToolTip="Click Reset Page" AccessKey="r" />
                        <asp:ImageButton ID="btnclose" runat="server" ImageUrl="~/images/Cancel.png" CausesValidation="false"
                            OnClick="btnclose_Click" ToolTip="Click Cancel" AccessKey="c" />
                    </td>
                </tr>
            </table>
        </center>
    </div>
</asp:Content>
