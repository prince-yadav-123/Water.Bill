<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="NEW_USER.aspx.cs" Inherits="MainPage_NEW_USER" Title="NoidaJal::New User" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <center>
        <table cellpadding="1" cellspacing="0" width="500px">
            <tr>
                <td style="height: 30px;">
                </td>
            </tr>
            <tr>
                <td>
                    <div style="text-align: center; height: 450px" id="div_main" runat="server">
                        <fieldset>
                            <legend><b>Create New User</b></legend>
                            <table>
                                <tr>
                                    <td style="height: 10px;" colspan="2">
                                    </td>
                                </tr>
                                <tr>
                                    <td class="pageLabel">
                                        User Id:
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_userid" CssClass="td_text" runat="server" MaxLength="20" Enabled="false"></asp:TextBox>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="pageLabel">
                                        Name:
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_name" runat="server" CssClass="td_text" MaxLength="20"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_name"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="pageLabel">
                                        Email Id:
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_email" CssClass="td_text" runat="server" MaxLength="20"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_email"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="pageLabel">
                                        Mobile No. :
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_mobNo" CssClass="td_text" runat="server" MaxLength="20"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_mobNo"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="pageLabel">
                                        Department:
                                    </td>
                                    <td class="pageControl">
                                        <asp:DropDownList ID="ddldepartment" CssClass="td_text" runat="server" MaxLength="20">
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                            ControlToValidate="ddldepartment" InitialValue="0"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="pageLabel">
                                        Password :
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_pass" TextMode="Password" CssClass="td_text" runat="server"
                                            MaxLength="20"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_pass"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="pageLabel">
                                        Confirm Password :
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_Cpass" TextMode="Password" CssClass="td_text" runat="server"
                                            MaxLength="20"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_Cpass"></asp:RequiredFieldValidator><br />
                                        <asp:CompareValidator ID="CompareValidator1" ControlToCompare="txt_pass" ControlToValidate="txt_Cpass"
                                            runat="server" ErrorMessage="Password not matched"></asp:CompareValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="pageLabel">
                                        Role:
                                    </td>
                                    <td class="pageControl">
                                        <asp:DropDownList ID="ddlrole" CssClass="td_text" runat="server">
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ErrorMessage="*"
                                            ControlToValidate="ddlrole" InitialValue="0"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td class="pageLabel">
                                        Devision:
                                    </td>
                                    <td class="pageControl">
                                        <asp:DropDownList ID="dll_devision" CssClass="td_text" runat="server">
                                            <asp:ListItem Value="1">JAL-1</asp:ListItem>
                                            <asp:ListItem Value="2">JAL-2</asp:ListItem>
                                            <asp:ListItem Value="3">JAL-3</asp:ListItem>
                                        </asp:DropDownList>
                                        
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right">
                                        &nbsp;
                                    </td>
                                    <td class="pageControl">
                                      &nbsp;
                                    </td>
                                </tr>
                                <tr>
                                    <td align="right">
                                        <asp:ImageButton ID="btnSave" runat="server" ImageUrl="~/images/save.png" OnClick="btnSave_Click"
                                            ToolTip="Click Save Record" AccessKey="s" />
                                    </td>
                                    <td class="pageControl">
                                        <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Reset.png" CausesValidation="false"
                                            OnClick="btnReset_Click" ToolTip="Click Reset Page" AccessKey="r" />
                                        <asp:ImageButton ID="btnclose" runat="server" ImageUrl="~/images/Cancel.png" CausesValidation="false"
                                            OnClick="btnclose_Click" ToolTip="Click Cancel" AccessKey="c" />
                                    </td>
                                </tr>
                            </table>
                        </fieldset>
                    </div>
                </td>
            </tr>
        </table>
    </center>
</asp:Content>
