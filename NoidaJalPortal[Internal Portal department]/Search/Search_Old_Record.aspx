<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Search_Old_Record.aspx.cs" Inherits="MainPage_Search_Record" Title="NoidaJal::Search" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <center>
        <table cellpadding="1" cellspacing="0" width="800px" style="vertical-align: top;">
            <tr>
                <td style="height: 30px;">
                </td>
            </tr>
            <tr>
                <td valign="top">
                    <fieldset>
                        <legend></legend>
                        <table>
                            <tr id="tr2" runat="server" visible="true">
                                <td class="pageLabel">
                                    Consumer No.
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_cons_no" runat="server" MaxLength="8" CssClass="td_text_search">0</asp:TextBox>
                                </td>
                                <td align="center" class="pageLabel">
                                    <asp:ImageButton ID="btnView" runat="server" ImageUrl="~/images/View.png" CausesValidation="true"
                                        Visible="true" OnClick="btnView_Click" />
                                </td>
                            </tr>
                        </table>
                    </fieldset>
                </td>
            </tr>
            <tr>
                <td valign="top">
                    <div style="text-align: center;  width: 100%; overflow: scroll;" id="div_main"
                        runat="server">
                        <fieldset>
                            <legend>Challan View</legend>
                            <asp:GridView ID="gvParentGrid" runat="server"  Width="100%"
                                HeaderStyle-CssClass="gridHeader" RowStyle-CssClass="gridRows" AutoGenerateColumns="false"
                                OnRowDataBound="gvUserInfo_RowDataBound">
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
                    <div style="text-align: center;  width: 800px; overflow: scroll;" id="div1"
                        runat="server">
                        <fieldset>
                            <legend>Challan View</legend>
                            <asp:GridView ID="GridView1" runat="server" Width="100%" HeaderStyle-CssClass="gridHeader"
                                RowStyle-CssClass="gridRows" >
                                <HeaderStyle CssClass="gridHeader" />
                                <PagerSettings Mode="NumericFirstLast" />
                                <RowStyle CssClass="gridRows" />
                            </asp:GridView>
                        </fieldset>
                    </div>
                </td>
            </tr>
        </table>
    </center>
</asp:Content>
