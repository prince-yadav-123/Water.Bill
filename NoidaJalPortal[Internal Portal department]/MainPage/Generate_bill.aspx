<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Generate_bill.aspx.cs" Inherits="Update_Generate_bill" Title="NoidaJal::Generate Bill" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">

    <script type="text/javascript" language="javascript">
    function f1()
    {
    window.showModalDialog("../Search/Search.aspx", "newwindow","center:yes;dialogWidth:800px;dialogHeight:750px; resizable:no;");
    }
    

function GetAmount(x)
{
    var a1 = x;
   
    var a = document.getElementById(a1).value;	
    var b = document.getElementById(b1).value;
    var c = a*b;
    document.getElementById(c1).value = c;

}
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <center>
        <table cellpadding="1" cellspacing="0" width="720px">
            <tr>
                <td style="height: 30px;">
                </td>
            </tr>
            <tr>
                <td>
                    <fieldset>
                        <legend><b>Connection Details</b></legend>
                        <table style="width: 50%;">
                            <tr>
                                <td class="pageLabel">
                                    Consumer No.
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_cons_no" runat="server" CssClass="td_text" MaxLength="8"></asp:TextBox>
                                </td>
                                <td class="pageControl">
                                    <asp:ImageButton ID="btnSearch" runat="server" ImageUrl="~/images/View.png" OnClick="btnSearch_Click"
                                        ToolTip="Click Search Record" AccessKey="s" CausesValidation="false" />
                                </td>
                            </tr>
                            <tr>
                                <td>
                                </td>
                                <td>
                                    <asp:LinkButton ID="link_find" CausesValidation="false" runat="server" OnClientClick="javascript:f1();">Find Consumer No ?</asp:LinkButton>
                                </td>
                                <td>
                                </td>
                            </tr>
                        </table>
                    </fieldset>
                </td>
            </tr>
            <tr>
                <td>
                    <fieldset>
                        <legend><b>Connection Details</b></legend>
                        <asp:UpdatePanel ID="UPanel1" runat="server">
                            <ContentTemplate>
                                <table style="width: 100%;">
                                    <tr>
                                        <td class="pageLabel">
                                            Bill No.
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_bill_no" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                        <td class="pageLabel">
                                            Due Date
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_due_date" runat="server" CssClass="td_text"></asp:TextBox>
                                            <cc1:CalendarExtender ID="cal1" runat="server" Format="dd-MMM-yyyy" TargetControlID="txt_due_date">
                                            </cc1:CalendarExtender>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_due_date"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Consumer Name:
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_Cons_nm" runat="server" TextMode="MultiLine" CssClass="td_text"></asp:TextBox>
                                        </td>
                                        <td class="pageLabel">
                                            Flat Type:
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_Flat_type" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Sector:
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_sector" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                        <td class="pageLabel">
                                            Category
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_category" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Block No.:
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_block_no" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                        <td class="pageLabel">
                                            Bill Period From :
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_bill_from" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator12" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_bill_from"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Flat No.:
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_Flat_No" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                        <td class="pageLabel">
                                            Bill Period Upto:
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_bill_to" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_bill_to"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </fieldset>
                </td>
            </tr>
            <tr>
                <td>
                    <fieldset>
                        <legend><b>Bill Details</b></legend>
                        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                            <ContentTemplate>
                                <table style="width: 75%;">
                                    <tr>
                                        <td class="pageLabel">
                                            Min Charge per Month(Rs.)
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_min_charges" runat="server" CssClass="td_text" AutoPostBack="True"
                                                OnTextChanged="txt_min_charges_TextChanged"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_min_charges"></asp:RequiredFieldValidator>
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_t_min_charge" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Rebate(%)
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_rebate_per" runat="server" CssClass="td_text" AutoPostBack="True"
                                                OnTextChanged="txt_rebate_per_TextChanged"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_rebate_per"></asp:RequiredFieldValidator>
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_rebate_t_amt" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Bill Date:
                                        </td>
                                        <td class="pageControl">
                                            &nbsp;
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_bill_date" runat="server" CssClass="td_text"></asp:TextBox>
                                            <cc1:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd-MMM-yyyy"
                                                TargetControlID="txt_bill_date">
                                            </cc1:CalendarExtender>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_bill_date"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Cess Amt.:
                                        </td>
                                        <td class="pageControl">
                                            &nbsp;
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_cess_amt" runat="server" CssClass="td_text"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_cess_amt"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Arrear Amt. Upto:
                                        </td>
                                        <td class="pageControl">
                                            &nbsp;
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_arear_amt" runat="server" CssClass="td_text"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_arear_amt"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Interest Amt. Upto:
                                        </td>
                                        <td class="pageControl">
                                            &nbsp;
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_intrest_amt" runat="server" CssClass="td_text"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_intrest_amt"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Other Credit (-):
                                        </td>
                                        <td class="pageControl">
                                            &nbsp;
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_adv_amt" runat="server" CssClass="td_text">0</asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_adv_amt"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </fieldset>
                </td>
            </tr>
            <tr>
                <td colspan="4" align="center">
                    <asp:ImageButton ID="btnSave" runat="server" ImageUrl="~/images/save.png" OnClick="btnSave_Click"
                        ToolTip="Click Save Record" AccessKey="s" />
                    <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Reset.png" CausesValidation="false"
                        OnClick="btnReset_Click" ToolTip="Click Reset Page" AccessKey="r" />
                    <asp:ImageButton ID="btnclose" runat="server" ImageUrl="~/images/Cancel.png" CausesValidation="false"
                        OnClick="btnclose_Click" ToolTip="Click Cancel" AccessKey="c" />
                    <asp:Button ID="btnPrint" runat="server" Text="Print" CausesValidation="false" ToolTip="Click Print"
                        AccessKey="p" OnClick="btnPrint_Click" Visible="false" />
                </td>
            </tr>
        </table>
    </center>
</asp:Content>
