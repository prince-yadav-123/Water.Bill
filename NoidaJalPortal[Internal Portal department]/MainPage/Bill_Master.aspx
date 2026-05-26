<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Bill_Master.aspx.cs" Inherits="MainPage_Bill_Master" Title="NoidaJal::Generate Bill" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <table style="height: 450px;">
        <tr>
            <td valign="top">
                <fieldset>
                    <legend><b>Generate Bill</b></legend>
                    <table>
                        <tr>
                            <td class="pageLabel">
                                Consumer No. :
                            </td>
                            <td class="pageLabel">
                                Sector :
                            </td>
                            <td class="pageLabel">
                                Division :
                            </td>
                            <td>
                            </td>
                        </tr>
                        <tr>
                            <td class="pageControl">
                                <asp:TextBox ID="txtConsName" runat="server" CssClass="td_text" MaxLength="8"></asp:TextBox>
                            </td>
                            <td class="pageControl">
                                <asp:DropDownList ID="ddl_Sector" CssClass="td_text" runat="server" Width="160px">
                                </asp:DropDownList>
                            </td>
                            <td class="pageControl">
                                <asp:DropDownList ID="ddl_Division" CssClass="td_text" runat="server" Width="160px">
                                    <asp:ListItem Value="1">JAL-1</asp:ListItem>
                                    <asp:ListItem Value="2">JAL-2</asp:ListItem>
                                    <asp:ListItem Value="3">JAL-3</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td class="pageControl">
                                <asp:ImageButton ID="btnView" runat="server" Text="View" ImageUrl="~/images/View.png"
                                    OnClick="btnView_Click"></asp:ImageButton>
                            </td>
                        </tr>
                    </table>
                </fieldset>
            </td>
        </tr>
    </table>
</asp:Content>
