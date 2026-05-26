<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="JAL_Bulk_BILL_Generation.aspx.cs" Inherits="Report_Print_bill" Title="NoidaJal::Bill" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <table style="height: 450px; width: 80%">
        <tr>
            <td valign="top">
                <fieldset>
                    <legend><b>Generate Bill</b></legend>
                    <table>
                        <tr id="tr1" runat="server">
                            <td class="pageLabel">
                                Sector :
                            </td>
                            <td class="pageControl">
                                <asp:DropDownList ID="ddl_Sector" CssClass="td_text" runat="server" Width="160px"
                                    AutoPostBack="True" OnSelectedIndexChanged="ddl_Sector_SelectedIndexChanged">
                                </asp:DropDownList>
                            </td>
                            <td class="pageLabel">
                                Block :
                            </td>
                            <td class="pageControl">
                                <asp:DropDownList ID="ddl_block" CssClass="td_text" runat="server" Width="160px">
                                </asp:DropDownList>
                            </td>
                            <td class="pageLabel">
                                Due DAte:
                            </td>
                            <td class="pageControl">
                                <asp:TextBox ID="txt_due_date" CssClass="readonlytxt" runat="server"></asp:TextBox>
                                <cc1:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd-MMM-yyyy"
                                    TargetControlID="txt_due_date">
                                </cc1:CalendarExtender>
                            </td>
                            <td class="pageControl">
                                <asp:ImageButton ID="btnView" runat="server" Text="View" ImageUrl="~/images/View.png"
                                    OnClick="btnView_Click"></asp:ImageButton>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="7">
                                <asp:Panel ID="trShow" runat="server" Visible="false">
                                    <asp:Image ID="Image1" runat="server" ImageUrl="~/images/wait_animated.gif" />
                                </asp:Panel>
                            </td>
                        </tr>
                    </table>
                </fieldset>
            </td>
        </tr>
    </table>
</asp:Content>
