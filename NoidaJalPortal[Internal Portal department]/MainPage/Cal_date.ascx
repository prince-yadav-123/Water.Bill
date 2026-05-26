<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Cal_date.ascx.cs" Inherits="PageButton" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<center>
    <table cellpadding="1" cellspacing="0" width="100%" style="vertical-align: top; text-align: center;">
        <tr id="tr1" runat="server">
            <td align="center">
                <fieldset>
                    <legend>&nbsp;Update Calculation Date</legend>
                    <table>
                        <tr>
                            <td class="pageLabel">
                                Consumer No.:
                            </td>
                            <td class="pageLabel">
                                Calculation Date :
                            </td>
                            <td class="pageLabel">
                                &nbsp;
                            </td>
                        </tr>
                        <tr>
                            <td class="pageControl">
                                <asp:TextBox ID="txt_cons_no" runat="server" CssClass="td_text_search" Enabled="false"
                                    MaxLength="8" AutoPostBack="True" ></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"
                                    ControlToValidate="txt_cons_no"></asp:RequiredFieldValidator>
                            </td>
                            <td class="pageControl">
                                <asp:TextBox ID="txt_cal_date" runat="server" CssClass="td_text_search" MaxLength="12"></asp:TextBox>
                                <cc1:CalendarExtender ID="cal1" runat="server" Format="dd-MMM-yyyy" TargetControlID="txt_cal_date">
                                </cc1:CalendarExtender>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                    ControlToValidate="txt_cal_date"></asp:RequiredFieldValidator>
                            </td>
                            <td class="pageControl">
                                <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Save.png" CausesValidation="false"
                                    OnClick="btnReset_Click" ToolTip="Click Reset Page" AccessKey="r" />
                            </td>
                        </tr>
                    </table>
                </fieldset>
            </td>
        </tr>
    </table>
</center>
