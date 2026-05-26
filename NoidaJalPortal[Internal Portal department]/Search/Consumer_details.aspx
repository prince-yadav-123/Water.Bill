<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Consumer_details.aspx.cs" Inherits="Consumer_details" Title="NOIDA JAL::Consumer Details" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <table style="width: 850px; height: 450px;">
        <tr>
            <td valign="top">
                <fieldset>
                    <legend></legend>
                    <table>
                        <tr>
                            <td class="pageLabel">
                                Consumer No.:
                            </td>
                            <td>
                                <asp:TextBox ID="txt_cons_no" runat="server" MaxLength="8" CssClass="td_text"></asp:TextBox>
                            </td>
                            <td align="center" class="pageLabel">
                                <asp:ImageButton ID="btnView" runat="server" ImageUrl="~/images/View.png" CausesValidation="true"
                                    Visible="true" OnClick="btnView_Click" />
                            </td>
                        </tr>
                    </table>
                </fieldset>
                <asp:Panel ID="pnl_main" runat="server" Visible="false">
                    <fieldset>
                        <legend>Consumer Details</legend>
                        <table width="100%">
                            <tr>
                                <td class="pageLabel">
                                    Consumer No.:
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblconsumerNo" runat="server" Text=""></asp:Label>
                                </td>
                                <td class="pageLabel">
                                    Connection Category :
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblconnCategory" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Consumer Name :
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblconsumerName" runat="server" Text=""></asp:Label>
                                </td>
                                <td class="pageLabel">
                                    Flat Type :
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblflattype" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Address :
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lbladdress" runat="server" Text=""></asp:Label>
                                </td>
                                <td class="pageLabel">
                                    Plot Size :
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblPlotsize" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Connection Type :
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblconnType" runat="server" Text=""></asp:Label>
                                </td>
                                <td class="pageLabel">
                                    Pipe Size :
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblpipesize" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Registration No.:
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblRegNo" runat="server" Text=""></asp:Label>
                                </td>
                                <td class="pageLabel">
                                    Estimation Date :
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblEstDate" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Connection Date :
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblConnDate" runat="server" Text=""></asp:Label>
                                </td>
                                <td class="pageLabel">
                                    No-Due Amount (Rs.) :
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblNoDues" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Estimation No. :
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblEstNo" runat="server" Text=""></asp:Label>
                                </td>
                                <td class="pageLabel">
                                    No-Due Issued Upto :
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblNoduesUpto" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    Estimation Amount (Rs.):
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblEstAmt" runat="server" Text=""></asp:Label>
                                </td>
                                <td class="pageLabel">
                                    Due Amount (Rs.) :
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblDueAmt" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                            <tr>
                                <td class="pageLabel">
                                    &nbsp;
                                </td>
                                <td>
                                    &nbsp;
                                </td>
                                <td class="pageLabel">
                                    Due Date :
                                </td>
                                <td class="pageControl">
                                    <asp:Label ID="lblDuesDate" runat="server" Text=""></asp:Label>
                                </td>
                            </tr>
                        </table>
                    </fieldset>
                    <table width="100%">
                        <tr>
                            <td>
                                <fieldset>
                                    <legend>Bill Details</legend>
                                    <asp:GridView ID="gvChildGrid" runat="server" AutoGenerateColumns="false" Width="100%"
                                        HeaderStyle-CssClass="gridHeader" RowStyle-CssClass="gridRows" OnRowDataBound="gvChildGrid_RowDataBound">
                                        <Columns>
                                            <asp:BoundField DataField="SNO" HeaderText="S.No." />
                                            <asp:BoundField DataField="CONS_NO" HeaderText="Consumer No." />
                                            <asp:BoundField DataField="BILL_NO" HeaderText="Bill No." />
                                            <asp:BoundField DataField="BILL_DATE_FROM" HeaderText="Bill From" DataFormatString="{0:dd-MMM-yyyy}" />
                                            <asp:BoundField DataField="BILL_DATE_TO" HeaderText="Bill To" DataFormatString="{0:dd-MMM-yyyy}" />
                                            <asp:BoundField DataField="MIN_TOTAL_AMT" HeaderText="Bill Amt." />
                                            <asp:BoundField DataField="BILL_DATE" HeaderText="Bill Date" DataFormatString="{0:dd-MMM-yyyy}" />
                                            <asp:TemplateField ItemStyle-Width="20px">
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="linkPrint" runat="server" OnCommand="lbRefund_Command">Print</asp:LinkButton>
                                                </ItemTemplate>
                                                <ItemStyle Width="20px"></ItemStyle>
                                            </asp:TemplateField>
                                            <asp:TemplateField ItemStyle-Width="20px">
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="linkEdit" runat="server" OnCommand="lbRefund1_Command">Edit</asp:LinkButton>
                                                </ItemTemplate>
                                                <ItemStyle Width="20px"></ItemStyle>
                                            </asp:TemplateField>
                                            <asp:TemplateField ItemStyle-Width="40px">
                                                <ItemTemplate>
                                                    <asp:LinkButton ID="linkNew" runat="server" OnCommand="lbRefund2_Command">New Bill</asp:LinkButton>
                                                </ItemTemplate>
                                                <ItemStyle Width="40px"></ItemStyle>
                                            </asp:TemplateField>
                                        </Columns>
                                        <EmptyDataTemplate>
                                            <table border="1" bordercolor="black" style="border-collapse: collapse;" align="center"
                                                width="90%">
                                                <tr class="gridHeader">
                                                    <td>
                                                        S.No.
                                                    </td>
                                                    <td>
                                                        Bill No.
                                                    </td>
                                                    <td>
                                                        Bill To
                                                    </td>
                                                    <td>
                                                        Rebate
                                                    </td>
                                                    <td>
                                                        Cess Amt
                                                    </td>
                                                    <td>
                                                        Bill Date
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td colspan="6" cssclass="gridRows">
                                                        Records not available
                                                    </td>
                                                </tr>
                                            </table>
                                        </EmptyDataTemplate>
                                    </asp:GridView>
                                </fieldset>
                            </td>
                        </tr>
                        <tr>
                            <td colspan="4" style="vertical-align: top; text-align: center;">
                                <fieldset>
                                    <legend>Challan Details</legend>
                                    <asp:Panel ID="pnl1" runat="server" Style="width: 100%; margin-left: 20; padding-left: 20;">
                                        <asp:GridView ID="gvChallan" runat="server" AutoGenerateColumns="False" Width="100%"
                                            EmptyDataText="No Data Found !!!" HeaderStyle-CssClass="gridHeader" RowStyle-CssClass="gridRows">
                                            <HeaderStyle CssClass="gridHeader" />
                                            <RowStyle CssClass="gridRows" />
                                            <AlternatingRowStyle CssClass="gridRows" />
                                            <Columns>
                                                <asp:BoundField DataField="S_no" HeaderText="S#" />
                                                <asp:BoundField DataField="BL_PER_FR" HeaderText="Bill Period From" DataFormatString="{0:dd-MMM-yyyy}" />
                                                <asp:BoundField DataField="BL_PER_TO" HeaderText="Bill Period To" DataFormatString="{0:dd-MMM-yyyy}" />
                                                <asp:BoundField DataField="BILL_AMT" HeaderText="Bill Amt(Rs.)" />
                                                <asp:BoundField DataField="SURCHARGE" HeaderText="Surcharge(Rs.)" />
                                                <asp:BoundField DataField="PAID_AMT" HeaderText="Paid Amt(Rs.)" />
                                                <asp:BoundField DataField="PAY_DATE" HeaderText="Pay Date" DataFormatString="{0:dd-MMM-yyyy}" />
                                                <asp:BoundField DataField="RECP_NO" HeaderText="Receipt No" />
                                                <asp:BoundField DataField="T_FEE" HeaderText="Transfer Fees" />
                                                <asp:BoundField DataField="BNK_CD" HeaderText="Bank Code" />
                                                <asp:BoundField DataField="BR_NM" HeaderText="Branch Name" />
                                            </Columns>
                                        </asp:GridView>
                                    </asp:Panel>
                                </fieldset>
                            </td>
                        </tr>
                    </table>
                </asp:Panel>
            </td>
        </tr>
    </table>
</asp:Content>
