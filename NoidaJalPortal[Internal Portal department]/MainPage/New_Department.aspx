<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="New_Department.aspx.cs" Inherits="MainPage_New_Department" Title="NoidaJal::New Department" %>

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
                            <legend><b>New Connection/Sub-Connection Type</b></legend>
                            <table>
                                <tr>
                                    <td style="height: 30px;">
                                    </td>
                                </tr>
                                <tr>
                                    <td colspan='2' align="center">
                                        <asp:RadioButtonList ID="rb_property" runat="server" RepeatDirection="Horizontal"
                                            AutoPostBack="True" OnSelectedIndexChanged="rb_property_SelectedIndexChanged">
                                            <asp:ListItem Value="D">Property</asp:ListItem>
                                            <asp:ListItem Value="S">Sub-Property</asp:ListItem>
                                        </asp:RadioButtonList>
                                    </td>
                                </tr>
                                <tr id="tr1" runat="server" visible="false">
                                    <td class="pageLabel">
                                        Property Type:
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_property_type" runat="server" CssClass="td_text" MaxLength="20"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_property_type"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr id="tr2" runat="server" visible="false">
                                    <td class="pageLabel">
                                        Property Type:
                                    </td>
                                    <td class="pageControl">
                                        <asp:DropDownList ID="ddl_department" CssClass="td_text" runat="server">
                                        </asp:DropDownList>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"
                                            ControlToValidate="ddl_department" InitialValue="0"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr id="tr3" runat="server" visible="false">
                                    <td class="pageLabel">
                                        Sub-Property Type :
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_subpro_typr" CssClass="td_text" runat="server" MaxLength="20"></asp:TextBox>
                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                            ControlToValidate="txt_subpro_typr"></asp:RequiredFieldValidator>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="height: 30px;">
                                    </td>
                                </tr>
                                <tr id="tr4" runat="server" visible="false">
                                    <td align="right">
                                        <asp:ImageButton ID="btnSave" runat="server" ImageUrl="~/images/save.png" OnClick="btnSave_Click"
                                            ToolTip="Click Save Record" AccessKey="s" CausesValidation="false" />
                                    </td>
                                    <td class="pageControl">
                                        <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Reset.png" CausesValidation="false"
                                            OnClick="btn_Cancel_Click" ToolTip="Click Reset Page" AccessKey="r" />
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
