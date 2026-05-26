<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="change_password.aspx.cs" Inherits="MainPage_change_password" Title="NoidaJal::Change Password" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <table cellpadding="1" cellspacing="0" width="500px" style="height: 450px; vertical-align: top;">
        <tr>
            <td style="height: 30px;">
            </td>
        </tr>
        <tr>
            <td style="width: 50%; text-align: center; vertical-align: top;">
                <fieldset>
                    <legend><b>Change Password</b></legend>
                    <table>
                        <tr>
                            <td style="height: 30px;">
                            </td>
                        </tr>
                        <tr>
                            <td class="pageLabel">
                                Current Password :
                            </td>
                            <td class="tr_pagecontrol">
                                <asp:TextBox ID="txt_Current_Password" runat="server" CssClass="td_text" TextMode="Password"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="pageLabel">
                                New Password :
                            </td>
                            <td class="tr_pagecontrol">
                                <asp:TextBox ID="txt_New_Password" runat="server" CssClass="td_text" TextMode="Password"></asp:TextBox>
                            </td>
                        </tr>
                        <tr>
                            <td class="pageLabel">
                                Re-Enter New Password :
                            </td>
                            <td class="tr_pagecontrol">
                                <asp:TextBox ID="txt_Re_Password" runat="server" CssClass="td_text" TextMode="Password"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                    ControlToValidate="txt_Re_Password"></asp:RequiredFieldValidator>
                            </td>
                        </tr>
                        <tr>
                            <td>
                            </td>
                            <td>
                                <asp:CompareValidator ID="CompareValidator1" runat="server" ErrorMessage="Passwords Do Not Match"
                                    ControlToValidate="txt_Re_Password" ControlToCompare="txt_New_Password"></asp:CompareValidator>
                            </td>
                        </tr>
                        <tr>
                            <td style="text-align: right">
                                <asp:ImageButton ID="btn_Save" runat="server" ImageUrl="~/images/save.png" OnClick="btn_Save_Click"
                                    ToolTip="Click Save Record" AccessKey="s" CausesValidation="false" />
                            </td>
                            <td class="tr_pagecontrol">
                                <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Reset.png" CausesValidation="false"
                                    OnClick="btn_Cancel_Click" ToolTip="Click Reset Page" AccessKey="r" />
                                <asp:ImageButton ID="btnclose" runat="server" ImageUrl="~/images/Cancel.png" CausesValidation="false"
                                    OnClick="btnclose_Click" ToolTip="Click Cancel" AccessKey="c" />
                            </td>
                        </tr>
                    </table>
                </fieldset>
            </td>
        </tr>
    </table>
</asp:Content>
