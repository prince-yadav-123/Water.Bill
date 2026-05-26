<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Defaulter.aspx.cs" Inherits="MainPage_Defaulter" Title="NoidaJal::Defaulter" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
<style type="text/css">
.tblwrap
{
	display:block;
	padding:25px;
}
.tblwrap table
{
	width:100%;
}
.tblwrap table tr td.pageLabel, .tblwrap table tr td.pageControl, .tblwrap table tr td.btnControl
{
	padding:3px 8px;
	vertical-align: initial;
}
.tblwrap table tr td.pageControl .td_text
{
	display:block;
	padding:8px 14px;
	border:1px solid #d0d0d0;
	border-radius:7px;
	font-size:13px;
}
.btn-search
{
	display:inline-block;
	padding:8px 14px;
	background:#234f9b;
	border:1px solid #234f9b;
	color:#fff;
	border-radius:7px;
	font-size:13px;
	cursor:pointer;
}
.pdfimg
{
	height: 34px; border-width: 0px; vertical-align: bottom; margin-left: 8px;
}
.fv-error
{
	font-size:11px;
	margin-top:2px;
	display: inline-block;
}
</style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
<div class="tblwrap">
    <table cellpadding="1" cellspacing="0">
        <tr>
            <td>
                <fieldset>
                    <legend><b>Search Defaulter</b></legend>
                    <table>
                        <tr>
                            <td class="pageLabel" width="100">
                                From Amount
                            </td>
                            <td class="pageControl" width="250">
                                <asp:TextBox ID="txt_From_amt" CssClass="td_text" MaxLength="8" runat="server" AutoPostBack="True"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="Enter from amount*"
                                    ControlToValidate="txt_From_amt" class="fv-error"></asp:RequiredFieldValidator>
                            </td>
                            <td class="pageLabel" width="100">
                                To Amount
                            </td>
                            <td class="pageControl" width="180">
                                <asp:TextBox ID="txt_To_amt" CssClass="td_text" MaxLength="8" runat="server" AutoPostBack="True"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="Enter to amount*"
                                    ControlToValidate="txt_To_amt" class="fv-error"></asp:RequiredFieldValidator>
                            </td>
                            <td class="btnControl">
                                <asp:Button ID="Button1" runat="server" Text="Search" OnClick="Button1_Click" class="btn-search" />
                                
                                <asp:ImageButton ID="btnExport" runat="server" ImageUrl="~/images/pdf.png" OnClick="btnExport_Click" class="pdfimg"
                                 ToolTip="Click Export Data in PDF" AccessKey="e" CausesValidation="false"/>
                            </td>
                        </tr>
                    </table>
                </fieldset>
            </td>
        </tr>
        <tr>
            <td valign="top">
                <fieldset>
                    <legend>Defaulter List</legend>
                    <div style="max-width: 100%; overflow-x: auto;" id="div1" runat="server">
                        <asp:GridView ID="gvParentGrid" runat="server" AutoGenerateColumns="False" HeaderStyle-CssClass="gridHeader"
                            HeaderStyle-Width="" RowStyle-CssClass="gridRows" Width="100%">
                            <Columns>
                                <asp:BoundField DataField="CONS_NO" HeaderText="CONSUMER NO." />
                                <asp:BoundField DataField="CONS_NM1" HeaderText="CONSUMER NAME" />
                                <asp:BoundField DataField="FLAT_NO" HeaderText="FLAT NO" />
                                <asp:BoundField DataField="BLK_NO" HeaderText="BLACK NO." />
                                <asp:BoundField DataField="SECTOR" HeaderText="SECTOR" />
                                <asp:BoundField DataField="PLOT_SIZE" HeaderText="PLOT SIZE" />
                                <asp:BoundField DataField="Div_type" HeaderText="DIV TYPE" />
                                <asp:BoundField DataField="CON_TP" HeaderText="CONNECTION TYPE" />
                                <asp:BoundField DataField="CONN_DT" HeaderText="CONNECTION DATE"/>
                                <asp:BoundField DataField="MOB_NO" HeaderText="MOBILE NO" />
                                <asp:BoundField DataField="EMAIL_ID" HeaderText="EMAIL ID" />
                                <asp:BoundField DataField="BILL_DUE_DATE" HeaderText="BILL DUE DATE" />
                                <asp:BoundField DataField="LAST_PAID_DATE" HeaderText="LAST PAID DATE" />
                                <asp:BoundField DataField="LAST_PAID_AMOUNT" HeaderText="LAST PAID AMOUNT" />
                                <asp:BoundField DataField="PAYMENT_MODE" HeaderText="PAYMENT MODE" />
                                <asp:BoundField DataField="OUTSTANDING_AMOUNT" HeaderText="OUTSTANDING AMOUNT" />
                            </Columns>
                          <HeaderStyle CssClass="gridHeader" />
                           <PagerSettings Mode="NumericFirstLast" />
                          <RowStyle CssClass="gridRows" />
                        </asp:GridView>
                    </div>
                </fieldset>
            </td>
        </tr>
    </table>
    </div>
</asp:Content>
