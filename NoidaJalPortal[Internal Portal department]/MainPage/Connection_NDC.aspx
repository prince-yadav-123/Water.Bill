<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Connection_NDC.aspx.cs" Inherits="MainPage_Connection_transfer" Title="NoidaJal::Connection NDC" %>

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
                    <legend><b>Connection NDC</b></legend>
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
                        <table style="height: 175px">
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
                                    <asp:DropDownList ID="ddl_con_type" CssClass="readonlytxt" runat="server" Enabled="false">
                                        <asp:ListItem Value="0">--Select--</asp:ListItem>
                                        <asp:ListItem Value="R">Regular</asp:ListItem>
                                        <asp:ListItem Value="T">Temporary</asp:ListItem>
                                    </asp:DropDownList>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                </td>
                                <td class="pageControl">
                                </td>
                            </tr>
                        </table>
                    </fieldset>
                </asp:Panel>
            </td>
            <td>
                <asp:Panel ID="pnl_New" runat="server" Enabled="false">
                    <fieldset>
                        <legend><b>NDC Details</b></legend>
                        <table style="height: 175px;">
                            <tr>
                                <td class="pageLabel">
                                    NDC Date:
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_trans_date" runat="server" CssClass="td_text" MaxLength="11"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="*"
                                        ControlToValidate="txt_trans_date" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                    <cc1:CalendarExtender ID="cal1" runat="server" Format="dd-MMM-yyyy" TargetControlID="txt_trans_date">
                                    </cc1:CalendarExtender>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    NDC Amt(Rs.):
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_trans_amt" runat="server" CssClass="td_text" MaxLength="10"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="*"
                                        ControlToValidate="txt_trans_amt" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Challan No.:
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_challan_no" runat="server" CssClass="td_text" MaxLength="15"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="*"
                                        ControlToValidate="txt_challan_no" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Bank Name :
                                </td>
                                <td class="pageControl">
                                    <asp:DropDownList ID="ddl_bank_name" CssClass="td_text" runat="server">
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ErrorMessage="*"
                                        ControlToValidate="ddl_bank_name" InitialValue="0" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Challan Date:
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_challan_date" runat="server" CssClass="td_text" MaxLength="11"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ErrorMessage="*"
                                        ControlToValidate="txt_challan_date" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                    <cc1:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd-MMM-yyyy"
                                        TargetControlID="txt_challan_date">
                                    </cc1:CalendarExtender>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Security :
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_seuc" runat="server" CssClass="td_text" MaxLength="10"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                        ControlToValidate="txt_seuc" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    NDC Upto :
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_ndcUpto" runat="server" CssClass="td_text" MaxLength="10"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"
                                        ControlToValidate="txt_ndcUpto" SetFocusOnError="True"></asp:RequiredFieldValidator>
                                    <cc1:CalendarExtender ID="CalendarExtender2" runat="server" Format="dd-MMM-yyyy" TargetControlID="txt_ndcUpto">
                                    </cc1:CalendarExtender>
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
