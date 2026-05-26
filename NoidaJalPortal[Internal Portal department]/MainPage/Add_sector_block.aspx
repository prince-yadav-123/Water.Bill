<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Add_sector_block.aspx.cs" Inherits="MainPage_Add_sector_block" Title="Untitled Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <table style="width: 90%;">
        <tr>
            <td>
                <asp:Panel ID="pnl_old" runat="server">
                    <fieldset>
                        <legend><b>Sector/Block Details</b></legend>
                        <table >
                            <tr>
                                <td colspan='2' style="height: 30px;">
                                </td>
                            </tr>
                            <tr>
                                <td colspan='2' align="center">
                                    <asp:RadioButtonList ID="rb_property" runat="server" RepeatDirection="Horizontal"
                                        AutoPostBack="True" OnSelectedIndexChanged="rb_property_SelectedIndexChanged">
                                        <asp:ListItem Value="S">Sector</asp:ListItem>
                                        <asp:ListItem Value="B">Block</asp:ListItem>
                                    </asp:RadioButtonList>
                                </td>
                            </tr>
                            <tr id="tr1" runat="server" visible="false">
                                <td class="pageLabel">
                                    Sector:
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_Sector" runat="server" CssClass="td_text" MaxLength="20"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="*"
                                        ControlToValidate="txt_Sector"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr id="tr2" runat="server" visible="false">
                                <td class="pageLabel">
                                    Sector:
                                </td>
                                <td class="pageControl">
                                    <asp:DropDownList ID="ddl_sector" runat="server" CssClass="td_text" MaxLength="5">
                                    </asp:DropDownList>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                        ControlToValidate="ddl_sector"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                            <tr id="tr3" runat="server" visible="false">
                                <td class="pageLabel">
                                    Block:
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_block" runat="server" CssClass="td_text" MaxLength="5"></asp:TextBox>
                                    <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"
                                        ControlToValidate="txt_block"></asp:RequiredFieldValidator>
                                </td>
                            </tr>
                        </table>
                    </fieldset>
                </asp:Panel>
            </td>
        </tr>
        <tr id="tr4" runat="server" visible="false">
            <td colspan="2" align="center">
                <asp:ImageButton ID="btnSave" runat="server" ImageUrl="~/images/save.png" OnClick="btnSave_Click"
                    ToolTip="Click Save Record" AccessKey="s" />
                <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Reset.png" CausesValidation="false"
                    OnClick="btnReset_Click" ToolTip="Click Reset Page" AccessKey="r" />
                <asp:ImageButton ID="btnclose" runat="server" ImageUrl="~/images/Cancel.png" CausesValidation="false"
                    OnClick="btnclose_Click" ToolTip="Click Cancel" AccessKey="c" />
            </td>
        </tr>
    </table>
</asp:Content>
