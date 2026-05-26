<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Search_Record.aspx.cs" Inherits="MainPage_Search_Record" Title="NoidaJal::Search" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">

    <script language="javascript" type="text/javascript">
        function divexpandcollapse(divname) 
        {
                var div = document.getElementById(divname);
                var img = document.getElementById('img' + divname);
                if (div.style.display == "none") 
                {
                    div.style.display = "block";
                   img.src = "../images/minus1.jpg";
                } 
                else 
                {
                    div.style.display = "none";
                    img.src = "../images/plus1.jpg";
                }
        }
    </script>

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
                            <tr id="tr1" runat="server" visible="true">
                                <td class="pageLabel">
                                    Consumer No.
                                </td>
                                <%--<td class="pageLabel">
                                    Bill From :
                                </td>
                                <td class="pageLabel">
                                    Bill To :
                                </td>--%>
                                <td class="pageLabel">
                                    Sector :
                                </td>
                                <td class="pageLabel">
                                    Block :
                                </td>
                                <td class="pageLabel">
                                    Flat/Plot No.
                                </td>
                                <td>
                                    <asp:HiddenField ID="cons_no" runat="server" />
                                </td>
                            </tr>
                            <tr id="tr2" runat="server" visible="true">
                                <td align="center" class="pageControl">
                                    <asp:TextBox ID="txt_cons_no" runat="server" CssClass="td_text" MaxLength="8"></asp:TextBox>
                                </td>
                               <%-- <td class="pageControl">
                                    <asp:TextBox ID="txt_Bill_From" CssClass="td_text_ddl" runat="server"></asp:TextBox>
                                    <cc1:CalendarExtender ID="CalendarExtender2" runat="server" Format="dd-MMM-yyyy"
                                        TargetControlID="txt_Bill_To">
                                    </cc1:CalendarExtender>
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_Bill_To" CssClass="td_text_ddl" runat="server"></asp:TextBox>
                                    <cc1:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd-MMM-yyyy"
                                        TargetControlID="txt_Bill_From">
                                    </cc1:CalendarExtender>
                                </td>--%>
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
                                <td align="center" class="pageLabel">
                                    <asp:ImageButton ID="btnView" runat="server" ImageUrl="~/images/View.png" CausesValidation="true"
                                        Visible="true" OnClick="btnView_Click" />
                                    <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Reset.png" CausesValidation="false"
                                        OnClick="btnReset_Click" ToolTip="Click Reset Page" AccessKey="r" />
                                </td>
                            </tr>
                        </table>
                    </fieldset>
                </td>
            </tr>
            <tr>
                <td valign="top">
                    <div style="text-align: center; height: 400px; width: 100%; overflow: scroll;" id="div_main"
                        runat="server">
                        <fieldset>
                            <legend>Bill View</legend>
                            <asp:GridView ID="gvParentGrid" runat="server" DataKeyNames="CONS_NO" Width="100%"
                                HeaderStyle-CssClass="gridHeader" RowStyle-CssClass="gridRows" AutoGenerateColumns="false"
                                OnRowDataBound="gvUserInfo_RowDataBound">
                                <Columns>
                                    <asp:TemplateField ItemStyle-Width="20px">
                                        <ItemTemplate>
                                            <a href="JavaScript:divexpandcollapse('div<%# Eval("CONS_NO") %>');">
                                                <img id="imgdiv<%# Eval("CONS_NO") %>" width="9px" border="0" src="../images/plus1.jpg" />
                                            </a>
                                        </ItemTemplate>
                                        <ItemStyle Width="20px"></ItemStyle>
                                    </asp:TemplateField>
                                    <asp:BoundField DataField="CONS_NO" HeaderText="Consumer No." />
                                    <asp:BoundField DataField="CONS_NM1" HeaderText="Name" />
                                    <asp:BoundField DataField="property_no" HeaderText="Property No." />
                                    <asp:BoundField DataField="CONN_DT" HeaderText="Connection Date" DataFormatString="{0:dd-MMM-yyyy}" />
                                    <asp:BoundField DataField="MOB_NO" HeaderText="Mobile No." />
                                    <asp:BoundField DataField="DEV_TYPE" HeaderText="Division" />
                                    <asp:TemplateField>
                                        <ItemTemplate>
                                            <tr>
                                                <td colspan="100%">
                                                    <div id="div<%# Eval("CONS_NO") %>" style="display: none; position: relative; left: 15px;
                                                        overflow: auto">
                                                        <asp:GridView ID="gvChildGrid" runat="server" AutoGenerateColumns="false" Width="90%"
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
                                                                        <asp:LinkButton ID="linkNew" runat="server" OnCommand="lbRefund2_Command">Calculation</asp:LinkButton>
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
                                                    </div>
                                                </td>
                                            </tr>
                                        </ItemTemplate>
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
                                                Name
                                            </td>
                                            <td>
                                                Property No.
                                            </td>
                                            <td>
                                                Connection Date
                                            </td>
                                            <td>
                                                Mobile No.
                                            </td>
                                            <td>
                                                Division
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="6" cssclass="gridRows">
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
        </table>
    </center>
</asp:Content>
