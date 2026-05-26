<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Manual_calculation.aspx.cs" Inherits="MainPage_Manual_calculation"
    Title="NoidaJal:Manual Claculation" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <table cellpadding="1" cellspacing="0">
        <tr>
            <td style="height: 30px;">
            </td>
        </tr>
        <tr>
            <td>
                <fieldset>
                    <legend><b>Manual Calculation</b></legend>
                    <table>
                        <tr>
                            <td class="pageLabel">
                                From :
                            </td>
                            <td class="pageControl">
                                <asp:TextBox ID="txt_from" runat="server" CssClass="td_text" MaxLength="20"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="*"
                                    ControlToValidate="txt_from"></asp:RequiredFieldValidator>
                                <cc1:CalendarExtender ID="cal1" runat="server" Format="dd-MMM-yyyy" TargetControlID="txt_from">
                                </cc1:CalendarExtender>
                            </td>
                            <td class="pageLabel">
                                To :
                            </td>
                            <td class="pageControl">
                                <asp:TextBox ID="txt_to" runat="server" CssClass="td_text" MaxLength="20"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="*"
                                    ControlToValidate="txt_to"></asp:RequiredFieldValidator>
                                <cc1:CalendarExtender ID="cal2" runat="server" Format="dd-MMM-yyyy" TargetControlID="txt_to">
                                </cc1:CalendarExtender>
                            </td>
                            <td class="pageLabel">
                                Amount :
                            </td>
                            <td class="pageControl">
                                <asp:TextBox ID="txt_amt" runat="server" CssClass="td_text" MaxLength="20"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="*"
                                    ControlToValidate="txt_amt"></asp:RequiredFieldValidator>
                            </td>
                        </tr>
                        <tr>
                            <td class="pageLabel">
                                Cess Amount :
                            </td>
                            <td class="pageControl">
                                <asp:TextBox ID="txt_Cess" runat="server" CssClass="td_text" MaxLength="20"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="*"
                                    ControlToValidate="txt_Cess"></asp:RequiredFieldValidator>
                            </td>
                            <td class="pageLabel">
                                Bill Type :
                            </td>
                            <td class="pageControl">
                                <asp:DropDownList ID="ddl_bill_type" CssClass="td_text" runat="server" 
                                    AutoPostBack="True" onselectedindexchanged="ddl_bill_type_SelectedIndexChanged">
                                    <asp:ListItem Value="0" Selected="True">--Select--</asp:ListItem>
                                    <asp:ListItem Value="1">Only</asp:ListItem>
                                    <asp:ListItem Value="2">Continue</asp:ListItem>
                                </asp:DropDownList>
                            </td>
                            <td align="center" colspan="2">
                                &nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td style="height: 30px;" colspan="6">
                            </td>
                        </tr>
                        <tr>
                            <td class="pageLabel">
                                Arrear Amt. :
                            </td>
                            <td class="pageControl">
                                <asp:TextBox ID="txt_arear" CssClass="td_text" runat="server" MaxLength="20"></asp:TextBox>
                            </td>
                            <td class="pageLabel">
                                Arrear Int. :
                            </td>
                            <td class="pageControl">
                                <asp:TextBox ID="txt_arrear_int" CssClass="td_text" runat="server" MaxLength="20"></asp:TextBox>
                            </td>
                            <td class="pageLabel">
                                Cess Amt. :
                            </td>
                            <td class="pageControl">
                                <asp:TextBox ID="txt_CessAmt" CssClass="td_text" runat="server" MaxLength="20"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="pageLabel">
                                Cess Int. :
                            </td>
                            <td class="pageControl">
                                <asp:TextBox ID="txt_cess_int" CssClass="td_text" runat="server" MaxLength="20"></asp:TextBox>
                            </td>
                            <td colspan="4">
                            </td>
                        </tr>
                       
                    </table>
                </fieldset>
            </td>
        </tr>
    </table>
</asp:Content>
