<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Print_bill.aspx.cs" Inherits="Report_Print_bill" Title="NoidaJal::Bill" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <table style="height: 450px; width: 80%">
        <tr>
            <td valign="top">
                <fieldset>
                    <legend><b>Generate Bill</b></legend>
                    <table>
                        <tr>
                            <td>
                                &nbsp;
                            </td>
                           
                            <td align="center" class="pageLabel" colspan="3">
                                <asp:RadioButtonList ID="rbView" runat="server" RepeatDirection="Horizontal">
                                    <asp:ListItem Selected="True" Value="S" Enabled="false">Summary</asp:ListItem>
                                    <asp:ListItem Value="BB" >Bulk Bill </asp:ListItem>
                                    <asp:ListItem Value="P">Print Bill Summary</asp:ListItem>
                                     <asp:ListItem Value="D" >Defaulter (Lacs)</asp:ListItem>
                                </asp:RadioButtonList>
                            </td>
                            
                            <td>
                                &nbsp;
                            </td>
                        </tr>
                        <tr>
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
