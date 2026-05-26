<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="BlockWiseTotal_new.aspx.cs" Inherits="Report_BlockWiseTotal" Title="NoidaJal::New Connection Report" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <table cellpadding="1" cellspacing="0" width="720px">
        <tr>
            <td style="height: 30px;">
            </td>
        </tr>
        <tr>
            <td>
                <fieldset>
                    <legend><b>Search Connection </b></legend>
                    <table>
                        <tr>
                            <td colspan="5">
                                
                            </td>
                        </tr>
                        <tr>
                            <td class="pageLabel">
                                Sector:
                            </td>
                           <%-- <td class="pageLabel">
                                Due Date:
                            </td>
                            <td class="pageLabel">
                                Discount:
                            </td>
                            <td class="pageLabel">
                                Division:
                            </td>--%>
                            <td>
                            </td>
                        </tr>
                        <tr>
                            <td class="pageControl">
                                <asp:DropDownList ID="ddlSector" CssClass="td_text_ddl" runat="server">
                                </asp:DropDownList>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ErrorMessage="*"
                                    ControlToValidate="ddlSector" InitialValue="0"></asp:RequiredFieldValidator>
                            </td>
                           <%-- <td class="pageControl">
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
                                    ControlToValidate="ddl_diascont"></asp:RequiredFieldValidator>
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
                            </td>--%>
                            <td>
                                <asp:ImageButton ID="btnSearch" runat="server" ImageUrl="~/images/View.png" OnClick="btnSearch_Click"
                                    ToolTip="Click Search Record" AccessKey="s" CausesValidation="false" />
                            </td>
                        </tr>
                    </table>
                </fieldset>
            </td>
        </tr>
    </table>
</asp:Content>
