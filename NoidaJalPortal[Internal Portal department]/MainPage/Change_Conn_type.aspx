<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Change_Conn_type.aspx.cs" Inherits="MainPage_Change_Conn_type" Title="NoidaJal::Connection Type Change" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">

    <script type="text/javascript" language="javascript">
    function f1()
    {
        window.showModalDialog("../Search/Search.aspx", "newwindow","center:yes;dialogWidth:800px;dialogHeight:750px; resizable:no;");
    }
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <table style="width: 90%;">
        <tr>
            <td colspan="2">
                <fieldset>
                    <legend><b>Change Connection Type</b></legend>
                    <table>
                        <tr>
                            <td class="pageLabel">
                                Consumer No.:
                            </td>
                            <td class="pageControl">
                                <asp:TextBox ID="txt_cons_no" CssClass="td_text" MaxLength="8" runat="server"></asp:TextBox>
                            </td>
                            <td class="pageControl">
                                <asp:ImageButton ID="btnSearch" runat="server" ImageUrl="~/images/View.png" OnClick="btnSearch_Click"
                                    ToolTip="Click Search Record" AccessKey="s" CausesValidation="false" />
                            </td>
                        </tr>
                        <tr>
                            <td>
                            </td>
                            <td>
                                <asp:LinkButton ID="link_find" CausesValidation="false" runat="server" OnClientClick="javascript:f1();">Find Consumer No ?</asp:LinkButton>
                            </td>
                            <td>
                            </td>
                        </tr>
                    </table>
                </fieldset>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Panel ID="pnl_old" runat="server" Enabled="false">
                    <fieldset>
                        <legend><b>Connection Details</b></legend>
                        <table style="height: 150px">
                            <tr>
                                <td class="pageLabel">
                                    Consumer Name:
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_cons_nm" runat="server" CssClass="readonlytxt" MaxLength="100"
                                        Enabled="false"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Father Name:
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_cons_fnm" runat="server" CssClass="readonlytxt" MaxLength="100"
                                        Enabled="false"></asp:TextBox>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Connection Category:
                                </td>
                                <td class="pageControl">
                                    <asp:DropDownList ID="ddl_Con_cat" CssClass="readonlytxt" runat="server" AutoPostBack="True"
                                        OnSelectedIndexChanged="ddl_Con_cat_SelectedIndexChanged" Enabled="false">
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
                                    <asp:DropDownList ID="ddl_flat_type" CssClass="readonlytxt" runat="server" Enabled="false">
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
                                    </asp:DropDownList>
                                </td>
                            </tr>
                        </table>
                    </fieldset>
                </asp:Panel>
            </td>
            <td>
                <asp:Panel ID="pnl_New" runat="server" Enabled="false">
                    <fieldset>
                        <legend><b>Connection Details</b></legend>
                        <table style="height: 150px;">
                            <tr>
                                <td class="pageLabel">
                                    Date:
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_change_date" runat="server" CssClass="td_text" MaxLength="11"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="*"
                                        ControlToValidate="txt_change_date" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                    <cc1:CalendarExtender ID="cal1" runat="server" Format="dd-MMM-yyyy" TargetControlID="txt_change_date">
                                    </cc1:CalendarExtender>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Security Amt(Rs.):
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_secu_amt" runat="server" CssClass="td_text" MaxLength="10"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="*"
                                        ControlToValidate="txt_secu_amt" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Estimation No.:
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_est_no" runat="server" CssClass="readonlytxt" MaxLength="15"
                                        Enabled="false"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="*"
                                        ControlToValidate="txt_est_no" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Estimation Amt(Rs.) :
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_Est_amt" runat="server" CssClass="td_text" MaxLength="10"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ErrorMessage="*"
                                        ControlToValidate="txt_Est_amt" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Monthly Rate :
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_monthly_rate" runat="server" CssClass="td_text" MaxLength="10"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                        ControlToValidate="txt_monthly_rate" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                        </table>
                    </fieldset>
                </asp:Panel>
            </td>
        </tr>
        <tr>
            <td colspan="2" align="center">
                <asp:ImageButton ID="btnSave" runat="server" ImageUrl="~/images/save.png" OnClick="btnSave_Click"
                    ToolTip="Click Save Record" AccessKey="s" Visible="false" />
                <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Reset.png" CausesValidation="false"
                    OnClick="btnReset_Click" ToolTip="Click Reset Page" AccessKey="r" Visible="false" />
                <asp:ImageButton ID="btnclose" runat="server" ImageUrl="~/images/Cancel.png" CausesValidation="false"
                    OnClick="btnclose_Click" ToolTip="Click Cancel" AccessKey="c" Visible="false" />
            </td>
        </tr>
    </table>
</asp:Content>
