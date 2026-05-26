<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Challan_Entry_BACK.aspx.cs" Inherits="MainPage_Challan" Title="NoidaJal::Challan Entry" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style type="text/css">
        .modalBackground
        {
            background-color: #EEE;
            filter: alpha(opacity=40);
            opacity: 0.4px;
            z-index: 999;
        }
        .modalPopup
        {
            background-color: #ffffdd;
            border-width: 3px;
            border-style: solid;
            border-color: Gray;
            padding: 3px;
            width: 650px;
        }
    </style>

    <script type="text/javascript" language="javascript">
    function f1()
    {
    window.showModalDialog("../Search/Search.aspx", "newwindow","center:yes;dialogWidth:800px;dialogHeight:750px; resizable:no;");
    }
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <center>
        <table cellpadding="1" cellspacing="0" width="800px" style="height: 450px; vertical-align: top;">
            <tr>
                <td style="height: 30px;">
                </td>
            </tr>
            <tr>
                <td valign="top">
                    <div style="text-align: center;" id="div_main" runat="server">
                        <table>
                            <tr id="tr1" runat="server">
                                <td>
                                    <fieldset>
                                        <legend>Property Details</legend>
                                        <table>
                                            <tr>
                                                <td class="pageLabel">
                                                    Consumer No.:
                                                </td>
                                                <td class="pageControl">
                                                    <asp:TextBox ID="txt_cons_no" runat="server" CssClass="td_text_dll" MaxLength="8"></asp:TextBox>
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
                                <td align="center">
                                    <asp:Panel ID="pnl_main" runat="server" Enabled="false">
                                        <fieldset>
                                            <legend>Property Details</legend>
                                            <table style="margin-top: 10px;">
                                                <tr>
                                                    <td class="pageLabel">
                                                        Recipt No.:
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:TextBox ID="txtRecipt_no" CssClass="readonlytxt" Enabled="false" runat="server"
                                                            MaxLength="12"></asp:TextBox>
                                                    </td>
                                                    <td class="pageLabel">
                                                        Consumer No:
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:Label ID="lblConsNo" runat="server" Text=""></asp:Label>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="pageLabel">
                                                        Consumer Name:
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:Label ID="lblConName" runat="server" BorderStyle="None"></asp:Label>
                                                    </td>
                                                    <td class="pageLabel">
                                                        Property No.:
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:Label ID="lblSector" runat="server" Text=""></asp:Label>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="pageLabel">
                                                        Email Id:
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:Label ID="lblEmailId" runat="server" Text=""></asp:Label>
                                                    </td>
                                                    <td class="pageLabel">
                                                        Mobile No.:
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:Label ID="lblMobNo" runat="server" Text=""></asp:Label>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="pageLabel">
                                                        Type Of Payment:
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:Label ID="lblPayType" runat="server" Text=""></asp:Label>
                                                    </td>
                                                    <td>
                                                    </td>
                                                    <td>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="pageLabel">
                                                        Bank Name:
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:DropDownList ID="ddl_bank" CssClass="td_text" runat="server">
                                                            <asp:ListItem Value="12">HDFC</asp:ListItem>
                                                        </asp:DropDownList>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ErrorMessage="*"
                                                            ControlToValidate="ddl_bank" InitialValue="0"></asp:RequiredFieldValidator>
                                                    </td>
                                                    <td class="pageLabel">
                                                        Bill Id:
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:TextBox ID="txt_billId" CssClass="td_text" MaxLength="10" runat="server" Text="0"></asp:TextBox>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="pageLabel">
                                                        Bill Period from :
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:TextBox ID="txt_bill_from" CssClass="readonlytxt" runat="server" MaxLength="11"></asp:TextBox>
                                                        <cc1:CalendarExtender ID="cal1" runat="server" Format="dd-MMM-yyyy" TargetControlID="txt_bill_from">
                                                        </cc1:CalendarExtender>
                                                    </td>
                                                    <td class="pageLabel">
                                                        Bill Period to :
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:TextBox ID="txt_bill_to" CssClass="readonlytxt" runat="server" MaxLength="12"></asp:TextBox>
                                                        <cc1:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd-MMM-yyyy"
                                                            TargetControlID="txt_bill_to">
                                                        </cc1:CalendarExtender>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="pageLabel">
                                                        Challan No. :
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:TextBox ID="txt_challan_no" CssClass="td_text" MaxLength="10" runat="server"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator11" runat="server" ErrorMessage="*"
                                                            ControlToValidate="txt_challan_no"></asp:RequiredFieldValidator>
                                                    </td>
                                                    <td class="pageLabel">
                                                        Challan Amt.(Rs.) :
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:TextBox ID="txtChallanAmt" CssClass="td_text" MaxLength="10" runat="server"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                                            ControlToValidate="txtChallanAmt"></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="pageLabel">
                                                        Due Date :
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:TextBox ID="txt_due_date" CssClass="readonlytxt" MaxLength="12" runat="server"></asp:TextBox>
                                                        <cc1:CalendarExtender ID="CalendarExtender4" runat="server" Format="dd-MMM-yyyy"
                                                            TargetControlID="txt_due_date">
                                                        </cc1:CalendarExtender>
                                                    </td>
                                                    <td class="pageLabel">
                                                        Paid Date :
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:TextBox ID="txt_paid_date" CssClass="readonlytxt" MaxLength="12" runat="server"></asp:TextBox>
                                                        <cc1:CalendarExtender ID="CalendarExtender5" runat="server" Format="dd-MMM-yyyy"
                                                            TargetControlID="txt_paid_date">
                                                        </cc1:CalendarExtender>
                                                    </td>
                                                </tr>
                                            </table>
                                        </fieldset>
                                        <table>
                                            <tr>
                                                <td colspan="4">
                                                    <fieldset>
                                                        <legend>Challan Details</legend>
                                                        <table width="100%">
                                                            <tr>
                                                                <td style="height: 10px;" colspan="4">
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td class="pageLabel">
                                                                    Sub Deopsit Head
                                                                </td>
                                                                <td class="pageLabel">
                                                                    Amount(Rs.)
                                                                </td>
                                                                <td class="pageLabel" id="td1" runat="server" visible="true">
                                                                    Arrear Period
                                                                </td>
                                                                <td class="pageLabel">
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td class="pageControl">
                                                                    <asp:DropDownList ID="ddlSubHead" CssClass="td_text" runat="server" AutoPostBack="True"
                                                                        OnSelectedIndexChanged="ddlSubHead_SelectedIndexChanged">
                                                                    </asp:DropDownList>
                                                                    <asp:RequiredFieldValidator ID="Required7" runat="server" ErrorMessage="*" ControlToValidate="ddlSubHead"
                                                                        InitialValue="0"></asp:RequiredFieldValidator>
                                                                </td>
                                                                <td class="pageControl">
                                                                    <asp:TextBox ID="txtAmt" CssClass="td_text_ddl" MaxLength="10" runat="server"></asp:TextBox>
                                                                    <asp:RequiredFieldValidator ID="Required8" runat="server" ErrorMessage="*" ControlToValidate="txtAmt"></asp:RequiredFieldValidator>
                                                                </td>
                                                                <td class="pageControl" id="td2" runat="server" visible="true">
                                                                    <asp:TextBox ID="txtArrearFrom" CssClass="readonlytxt" runat="server" Enabled="false"
                                                                        MaxLength="11"></asp:TextBox>
                                                                    <cc1:CalendarExtender ID="CalendarExtender2" runat="server" Format="dd-MMM-yyyy"
                                                                        TargetControlID="txtArrearFrom">
                                                                    </cc1:CalendarExtender>
                                                                    <asp:TextBox ID="txtArrearTo" CssClass="readonlytxt" runat="server" Enabled="false"
                                                                        MaxLength="11"></asp:TextBox>
                                                                    <cc1:CalendarExtender ID="CalendarExtender3" runat="server" Format="dd-MMM-yyyy"
                                                                        TargetControlID="txtArrearTo">
                                                                    </cc1:CalendarExtender>
                                                                </td>
                                                                <td class="pageControl">
                                                                    <asp:ImageButton ID="btnAdd" runat="server" ImageUrl="~/images/Add.png" OnClick="btnAdd_Click"
                                                                        ToolTip="Click Save Record" AccessKey="s" />
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td colspan="4">
                                                                    <asp:GridView ID="GridView1" runat="server" AutoGenerateColumns="False" HeaderStyle-CssClass="gridHeader"
                                                                        OnRowCommand="GridView1_RowCommand" RowStyle-CssClass="gridRows" Width="100%"
                                                                        autoupdateaftercallback="True" OnRowDataBound="GridView1_RowDataBound">
                                                                        <Columns>
                                                                            <asp:TemplateField HeaderText="Delete">
                                                                                <ItemTemplate>
                                                                                    <asp:ImageButton ID="btndeletecheque" runat="server" CausesValidation="False" CommandArgument="<%#Container.DataItemIndex+1 %>"
                                                                                        CommandName="Del" Height="15px" ImageUrl="~/images/deletebutton.png" />
                                                                                </ItemTemplate>
                                                                            </asp:TemplateField>
                                                                            <asp:BoundField DataField="Deposit_Id" HeaderText="Head ID" />
                                                                            <asp:BoundField DataField="Deposit_head" HeaderText="Head" />
                                                                            <asp:BoundField DataField="Deopsit_amt" HeaderText="Amount(Rs.)" />
                                                                            <asp:BoundField DataField="Date_From" HeaderText="Date From" />
                                                                            <asp:BoundField DataField="Date_To" HeaderText="Date To" />
                                                                        </Columns>
                                                                        <HeaderStyle CssClass="gridHeader" />
                                                                        <RowStyle CssClass="gridRows" />
                                                                    </asp:GridView>
                                                                </td>
                                                            </tr>
                                                        </table>
                                                    </fieldset>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td colspan="4" align="center">
                                                    <asp:ImageButton ID="btnSave" runat="server" ImageUrl="~/images/save.png" OnClick="btnSave_Click"
                                                        ToolTip="Click Save Record" AccessKey="s" CausesValidation="false" Visible="false" />
                                                    <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Reset.png" CausesValidation="false"
                                                        OnClick="btnReset_Click" ToolTip="Click Reset Page" AccessKey="r" Visible="false" />
                                                    <asp:ImageButton ID="btnclose" runat="server" ImageUrl="~/images/Cancel.png" CausesValidation="false"
                                                        OnClick="btnclose_Click" ToolTip="Click Cancel" AccessKey="c" Visible="false" />
                                                </td>
                                            </tr>
                                        </table>
                                    </asp:Panel>
                                    <%--<asp:Panel ID="pnl_popup" runat="server"  Style="background-color: #D3E7F4;
                                        width: 600px;">
                                        <asp:LinkButton ID="btnCancel" runat="server" Text="Close" />
                                        <uc2:UserControl ID="uc1" runat="server" />
                                    </asp:Panel>--%>
                                </td>
                            </tr>
                            <tr>
                                <td valign="top">
                                    <div style="text-align: center; width: 100%; overflow: scroll;" id="div1" runat="server">
                                        <fieldset>
                                            <legend>Challan View</legend>
                                            <asp:GridView ID="gvParentGrid" runat="server" Width="100%" HeaderStyle-CssClass="gridHeader"
                                                RowStyle-CssClass="gridRows" AutoGenerateColumns="false" OnRowDataBound="gvUserInfo_RowDataBound">
                                                <Columns>
                                                    <asp:BoundField DataField="SNO" HeaderText="S.No." />
                                                    <asp:BoundField DataField="RECEIPT_ID" HeaderText="Receipt Id" />
                                                    <asp:BoundField DataField="ALLOTE_NAME" HeaderText="Name" />
                                                    <asp:BoundField DataField="BANK_NAME" HeaderText="Bank Name" />
                                                    <asp:BoundField DataField="CHALLAN_ID" HeaderText="Challan Id" />
                                                    <asp:BoundField DataField="AMOUNT_PAID" HeaderText="Amt.(Rs.)" />
                                                    <asp:BoundField DataField="DEPOSIT_DATE" HeaderText="Date" DataFormatString="{0:dd-MMM-yyyy}" />
                                                    <asp:BoundField DataField="PROPERTY_NUMBER" HeaderText="Property No." />
                                                </Columns>
                                                <EmptyDataTemplate>
                                                    <table border="1" bordercolor="black" style="border-collapse: collapse;" align="center"
                                                        width="98%">
                                                        <tr class="gridHeader">
                                                            <td>
                                                                S.No.
                                                            </td>
                                                            <td>
                                                                Receipt Id
                                                            </td>
                                                            <td>
                                                                Name
                                                            </td>
                                                            <td>
                                                                Bank Name
                                                            </td>
                                                            <td>
                                                                Challan Id.
                                                            </td>
                                                            <td>
                                                                Amt.(Rs.)
                                                            </td>
                                                            <td>
                                                                Date
                                                            </td>
                                                            <td>
                                                                Property No.
                                                            </td>
                                                        </tr>
                                                        <tr>
                                                            <td colspan="8" cssclass="gridRows">
                                                                Records not available
                                                            </td>
                                                        </tr>
                                                    </table>
                                                </EmptyDataTemplate>
                                                <HeaderStyle CssClass="gridHeader" />
                                                <PagerSettings Mode="NumericFirstLast" />
                                                <RowStyle CssClass="gridRows" />
                                            </asp:GridView>
                                        </fieldset>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td valign="top">
                                    <div style="text-align: center; width: 800px; height: 250px; overflow: scroll;" id="div2"
                                        runat="server">
                                        <fieldset>
                                            <legend>Challan View</legend>
                                            <asp:GridView ID="GridView3" runat="server" Width="100%" HeaderStyle-CssClass="gridHeader"
                                                RowStyle-CssClass="gridRows">
                                                <HeaderStyle CssClass="gridHeader" />
                                                <PagerSettings Mode="NumericFirstLast" />
                                                <RowStyle CssClass="gridRows" />
                                            </asp:GridView>
                                        </fieldset>
                                    </div>
                                </td>
                            </tr>
                            <tr>
                                <td valign="top">
                                    <div style="text-align: center; width: 800px; overflow: scroll;" id="div3" runat="server">
                                        <fieldset>
                                            <legend>Challan View</legend>
                                            <asp:GridView ID="GridView2" runat="server" Width="100%" HeaderStyle-CssClass="gridHeader"
                                                RowStyle-CssClass="gridRows">
                                                <HeaderStyle CssClass="gridHeader" />
                                                <PagerSettings Mode="NumericFirstLast" />
                                                <RowStyle CssClass="gridRows" />
                                            </asp:GridView>
                                        </fieldset>
                                    </div>
                                </td>
                            </tr>
                        </table>
                    </div>
                </td>
            </tr>
        </table>
        <%--<cc1:ModalPopupExtender ID="pnlsalary1" runat="server" TargetControlID="link_find"
            PopupControlID="pnl_popup" CancelControlID="btnCancel" DropShadow="true" BackgroundCssClass="modalBackground">
        </cc1:ModalPopupExtender>--%>
    </center>
</asp:Content>
