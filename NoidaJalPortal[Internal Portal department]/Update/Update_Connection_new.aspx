<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Update_Connection_new.aspx.cs" Inherits="Update_connection" Title="NoidaJal::Connection Update" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div>
        <center>
            <table cellpadding="1" cellspacing="0" width="750px" style="height: 450px; vertical-align: top;">
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
                                                        <asp:TextBox ID="txt_con_no" runat="server" CssClass="td_text" Enabled="true" MaxLength="9"></asp:TextBox>
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
                                                                                Name :
                                                                            </td>
                                                                            <td class="pageLabel">
                                                                               <asp:TextBox ID="txt_name" CssClass="td_text" runat="server" TextMode="MultiLine"></asp:TextBox>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="txt_name"></asp:RequiredFieldValidator>
                                                                            </td>
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
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Block :
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:DropDownList ID="ddl_Block" CssClass="td_text" runat="server">
                                                                                </asp:DropDownList>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="ddl_Block" InitialValue="0"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                            <td class="pageLabel">
                                                                                House No. :
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:TextBox ID="txt_House_No" CssClass="td_text" runat="server"></asp:TextBox>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="txt_House_No"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Area(sq.mtr.) :
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:TextBox ID="txtArea" CssClass="td_text" runat="server"></asp:TextBox>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="txtArea"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                            <td class="pageLabel">
                                                                                Pipe Size :
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <%--<asp:TextBox ID="txt_pipe_size" CssClass="td_text" runat="server"></asp:TextBox>--%>
                                                                                <asp:DropDownList ID="ddl_pipe_size" CssClass="td_text" runat="server">
                                                                                </asp:DropDownList>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Connection Category:
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:DropDownList ID="ddl_Con_cat" CssClass="td_text" runat="server" AutoPostBack="True"
                                                                                    OnSelectedIndexChanged="ddl_Con_cat_SelectedIndexChanged">
                                                                                </asp:DropDownList>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator12" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="ddl_Con_cat" InitialValue="0"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                            <td class="pageLabel">
                                                                                Connection Sub-Type:
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:DropDownList ID="ddl_flat_type" CssClass="td_text" runat="server">
                                                                                </asp:DropDownList>
                                                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator13" runat="server" ErrorMessage="*"
                                                                                    ControlToValidate="ddl_flat_type" InitialValue="0"></asp:RequiredFieldValidator>
                                                                            </td>
                                                                        </tr>
                                                                        <tr>
                                                                            <td class="pageLabel">
                                                                                Connection Type:
                                                                            </td>
                                                                            <td class="pageControl">
                                                                                <asp:DropDownList ID="ddl_con_type" CssClass="td_text" runat="server">
                                                                                    <asp:ListItem Value="0">--Select--</asp:ListItem>
                                                                                    <asp:ListItem Value="R">Regular</asp:ListItem>
                                                                                    <asp:ListItem Value="T">Temporary</asp:ListItem>
                                                                                    <asp:ListItem Value="M">RMC</asp:ListItem>
                                                                                    <asp:ListItem Value="S">Staff</asp:ListItem>
                                                                                    <asp:ListItem Value="D">Disconnection</asp:ListItem>
                                                                                    <asp:ListItem Value="CC">CourtCase</asp:ListItem>
                                                                                </asp:DropDownList>
                                                                            </td>
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
                                                                        </tr>
                                                                        <tr>
                                                                        <td class="pageLabel">Narration</td><td><asp:TextBox ID="txtNarration" runat="server" TextMode="MultiLine" Width="300px"></asp:TextBox></td>
                                                                        <td class="pageLabel">
                                                        Rid.:
                                                    </td>
                                                     <td class="pageLabel">
                                                        <asp:TextBox ID="txtrid" runat="server" CssClass="td_text" Enabled="true" 
                                                             MaxLength="12" required="required"></asp:TextBox>
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
                                                                    <td class="pageLabel">
                                                                        Calculation Date :
                                                                    </td>
                                                                    <td class="pageControl">
                                                                        <asp:TextBox ID="txt_cal_date" CssClass="readonlytxt" runat="server" ToolTip="" MaxLength="11"></asp:TextBox>
                                                                        <cc1:CalendarExtender ID="CalendarExtender2" runat="server" Format="dd-MMM-yyyy"
                                                                            TargetControlID="txt_cal_date">
                                                                        </cc1:CalendarExtender>
                                                                    </td>
                                                                    <td class="pageLabel">
                                                                        NDC UPTO :
                                                                    </td>
                                                                    <td class="pageControl">
                                                                        <asp:TextBox ID="txt_ndc_date" CssClass="readonlytxt" runat="server" ToolTip="" MaxLength="11"></asp:TextBox>
                                                                        <cc1:CalendarExtender ID="CalendarExtender3" runat="server" Format="dd-MMM-yyyy"
                                                                            TargetControlID="txt_ndc_date">
                                                                        </cc1:CalendarExtender>
                                                                    </td>
                                                                </tr>
                                                                <tr>
                                                                    <td class="pageLabel">
                                                                        Ledger Date :
                                                                    </td>
                                                                    <td class="pageControl">
                                                                        <asp:TextBox ID="txt_ledger_date" CssClass="readonlytxt" runat="server" ToolTip=""
                                                                            MaxLength="11"></asp:TextBox>
                                                                        <cc1:CalendarExtender ID="CalendarExtender4" runat="server" Format="dd-MMM-yyyy"
                                                                            TargetControlID="txt_ledger_date">
                                                                        </cc1:CalendarExtender>
                                                                    </td>
                                                                    <td class="pageLabel">
                                                                        T/R Date :
                                                                    </td>
                                                                    <td class="pageControl">
                                                                        <asp:TextBox ID="txt_type_change_date" CssClass="readonlytxt" runat="server" ToolTip=""
                                                                            MaxLength="11"></asp:TextBox>
                                                                        <cc1:CalendarExtender ID="CalendarExtender5" runat="server" Format="dd-MMM-yyyy"
                                                                            TargetControlID="txt_type_change_date">
                                                                        </cc1:CalendarExtender>
                                                                    </td>
                                                                </tr>
                                                            </table>
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
                                                        <asp:HiddenField ID="lbl_form_no" runat="server" />
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
