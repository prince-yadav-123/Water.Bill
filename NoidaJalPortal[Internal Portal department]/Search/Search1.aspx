<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Search1.aspx.cs" Inherits="Search_Search1" Title="NoidaJal::Consumer Search" %>

<%@ Register Src="~/Search/PageButton.ascx" TagPrefix="uc2" TagName="UserControl" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <div>
        <center>
            <table cellpadding="1" cellspacing="0" width="90%" style="vertical-align: top; text-align: center;">
                <tr id="tr1" runat="server">
                    <td align="center">
                        <fieldset>
                            <legend>&nbsp;Search By</legend>
                            <table>
                                <tr>
                                    <td class="pageLabel">
                                        Name :
                                    </td>
                                    <td class="pageLabel">
                                        Sector :
                                    </td>
                                    <td class="pageLabel">
                                        Block :
                                    </td>
                                    <td class="pageLabel">
                                        Flat/Plot No. :
                                    </td>
                                    <td class="pageLabel">
                                        Mobile No.:
                                    </td>
                                    <td class="pageLabel">
                                        &nbsp;
                                    </td>
                                </tr>
                                <tr>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txtConsName" runat="server" CssClass="td_text_search" MaxLength="12"></asp:TextBox>
                                    </td>
                                    <td class="pageControl">
                                        <asp:DropDownList ID="ddlSector" runat="server" CssClass="td_text_search" AutoPostBack="True"
                                            OnSelectedIndexChanged="ddlSector_SelectedIndexChanged" Style="height: 22px">
                                        </asp:DropDownList>
                                    </td>
                                    <td class="pageControl">
                                        <asp:DropDownList ID="ddlBlock" runat="server" CssClass="td_text_search">
                                        </asp:DropDownList>
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txtFlatNo" runat="server" CssClass="td_text_search">0</asp:TextBox>
                                    </td>
                                    <td class="pageControl">
                                        <asp:TextBox ID="txt_mob_no" runat="server" CssClass="td_text_search"></asp:TextBox>
                                    </td>
                                    <td class="pageControl">
                                        <asp:ImageButton ID="btnView" runat="server" Text="View" ImageUrl="~/images/View.png"
                                            OnClick="btnView_Click"></asp:ImageButton>
                                    </td>
                                </tr>
                            </table>
                        </fieldset>
                    </td>
                </tr>
                <tr id="trSearch" runat="server">
                    <td>
                        <fieldset>
                            <legend>Search Details</legend>
                            <asp:GridView ID="gridSearch" runat="server" AutoGenerateColumns="False" HeaderStyle-CssClass="gridHeader"
                                RowStyle-CssClass="gridRows" Width="100%" AllowPaging="True" OnPageIndexChanging="gridSearch_PageIndexChanging"
                                PageSize="30" AllowSorting="True" OnRowDataBound="gridSearch_RowDataBound">
                                <Columns>
                                    <asp:BoundField DataField="SNo" HeaderText="S#" />
                                    <asp:BoundField DataField="CONS_NO" HeaderText="Consumer No." />
                                    <asp:BoundField DataField="CONS_NM1" HeaderText="Name" />
                                    <asp:BoundField DataField="CONN_DT" HeaderText="Connection Date" DataFormatString="{0:dd-MMM-yyyy}" />
                                    <asp:BoundField DataField="Property_No" HeaderText="Property No." />
                                    <asp:BoundField DataField="DEV_TYPE" HeaderText="Division" />
                                </Columns>
                                <EmptyDataTemplate>
                                    <table border="1" bordercolor="black" style="border-collapse: collapse;" align="center"
                                        width="90%">
                                        <tr class="gridHeader">
                                            <td>
                                                S#
                                            </td>
                                            <td>
                                                Name
                                            </td>
                                            <td>
                                                Connection Date
                                            </td>
                                            <td>
                                                Division
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="4" cssclass="gridRows">
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
                    </td>
                </tr>
            </table>
        </center>
    </div>
</asp:Content>
