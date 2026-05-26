<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="New_Entry_Update.aspx.cs" Inherits="MainPage_New_Entry" Title="NoidaJal::New Connection Entry" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <table cellpadding="1" cellspacing="0" width="600px">
        <tr>
            <td style="height: 30px;">
            </td>
        </tr>
        <tr>
            <td>
                <fieldset>
                    <legend><b>Connection Details</b></legend>
                    <table >
                        <tr>
                            <td class="pageLabel">
                                Consumer No.
                            </td>
                            <td class="pageControl">
                                <asp:TextBox ID="txt_cons_no" CssClass="td_text" runat="server" MaxLength="8"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="*"
                                    ControlToValidate="txt_cons_no"></asp:RequiredFieldValidator>
                            </td>
                            <td>
                                <asp:ImageButton ID="btnSearch" runat="server" ImageUrl="~/images/View.png" OnClick="btnSearch_Click"
                                    ToolTip="Click Search Record" AccessKey="s" CausesValidation="false" />
                            </td>
                        </tr>
                    </table>
                </fieldset>
            </td>
        </tr>
        <tr>
            <td>
                <asp:Panel ID="pnlMain" Width="600px" runat="server" Enabled="false">
                    <table width="100%">
                        <tr>
                            <td>
                                <fieldset>
                                    <legend><b>Connection Details</b></legend>
                                    <asp:UpdatePanel ID="UPanel1" runat="server">
                                        <ContentTemplate>
                                            <table width="100%">
                                                <tr>
                                                    <td class="pageLabel">
                                                        Sector:
                                                    </td>
                                                    <td class="pageLabel">
                                                        Due Date:
                                                    </td>
                                                    <td class="pageLabel">
                                                        Discount:
                                                    </td>
                                                    <td class="pageLabel">
                                                        Division:
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="pageControl">
                                                        <asp:DropDownList ID="ddlSector" CssClass="td_text_ddl" runat="server">
                                                        </asp:DropDownList>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ErrorMessage="*"
                                                            ControlToValidate="ddlSector" InitialValue="0"></asp:RequiredFieldValidator>
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:TextBox ID="txt_dueDate" CssClass="td_text_ddl" runat="server"></asp:TextBox>
                                                        <cc1:CalendarExtender ID="CalendarExtender5" runat="server" Format="dd-MMM-yyyy"
                                                            TargetControlID="txt_dueDate">
                                                        </cc1:CalendarExtender>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator11" runat="server" ErrorMessage="*"
                                                            ControlToValidate="txt_dueDate"></asp:RequiredFieldValidator>
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:DropDownList ID="ddl_diascont" CssClass="td_text_ddl" runat="server">
                                                            <asp:ListItem Value="0">0</asp:ListItem>
                                                            <asp:ListItem Value="10">10</asp:ListItem>
                                                            <asp:ListItem Value="20">20</asp:ListItem>
                                                            <asp:ListItem Value="30">30</asp:ListItem>
                                                            <asp:ListItem Value="40">40</asp:ListItem>
                                                            <asp:ListItem Value="50">50</asp:ListItem>
                                                            <asp:ListItem Value="60">60</asp:ListItem>
                                                            <asp:ListItem Value="70">70</asp:ListItem>
                                                            <asp:ListItem Value="80">80</asp:ListItem>
                                                            <asp:ListItem Value="90">90</asp:ListItem>
                                                            <asp:ListItem Value="100">100</asp:ListItem>
                                                        </asp:DropDownList>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="*"
                                                            ControlToValidate="ddl_diascont" InitialValue="0"></asp:RequiredFieldValidator>
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:DropDownList ID="ddl_divType" CssClass="td_text_ddl" runat="server">
                                                            <asp:ListItem Value="0">---Select---</asp:ListItem>
                                                            <asp:ListItem Value="1">JAL-1</asp:ListItem>
                                                            <asp:ListItem Value="2">JAL-2</asp:ListItem>
                                                            <asp:ListItem Value="3">JAL-3</asp:ListItem>
                                                        </asp:DropDownList>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="*"
                                                            ControlToValidate="ddl_divType" InitialValue="0"></asp:RequiredFieldValidator>
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
                                    <table width="100%">
                                        <tr>
                                            <td class="pageLabel">
                                                Consumer Name
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_Name" CssClass="td_text" runat="server" TextMode="MultiLine"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="*"
                                                    ControlToValidate="txt_Name"></asp:RequiredFieldValidator>
                                            </td>
                                            <td class="pageLabel">
                                                Block:
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_block" CssClass="td_text" MaxLength="4" runat="server" ></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                                    ControlToValidate="txt_block"></asp:RequiredFieldValidator>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="pageLabel">
                                                Flat No.:
                                            </td>
                                            <td valign="middle">
                                                <asp:TextBox ID="txt_flat_No" CssClass="td_text" MaxLength="4" runat="server" ></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator10" runat="server" ErrorMessage="*"
                                                    ControlToValidate="txt_flat_No"></asp:RequiredFieldValidator>
                                            </td>
                                            <td class="pageLabel">
                                                To Be Paid Amt.:
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_Paidamt" CssClass="td_text" runat="server"></asp:TextBox>
                                                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"
                                                    ControlToValidate="txt_Paidamt"></asp:RequiredFieldValidator>
                                                <asp:HiddenField ID="HiddenNew" runat="server" />
                                            </td>
                                        </tr>
                                        <tr>
                                            <td>
                                            </td>
                                            <td colspan="2">
                                                <asp:ImageButton ID="btnSave" runat="server" ImageUrl="~/images/save.png" OnClick="btnSave_Click"
                                                    ToolTip="Click Save Record" AccessKey="s" />
                                                <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Reset.png" CausesValidation="false"
                                                    OnClick="btnReset_Click" ToolTip="Click Reset Page" AccessKey="r" />
                                                <asp:ImageButton ID="btnclose" runat="server" ImageUrl="~/images/Cancel.png" CausesValidation="false"
                                                    OnClick="btnclose_Click" ToolTip="Click Cancel" AccessKey="c" />
                                            </td>
                                            <td>
                                            </td>
                                        </tr>
                                    </table>
                                </fieldset>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </td>
        </tr>
    </table>
</asp:Content>
