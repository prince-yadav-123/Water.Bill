<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Rate_mater.aspx.cs" Inherits="MainPage_Challan" Title="NoidaJal::Rate Master" %>

<%@ Register Assembly="Anthem" Namespace="Anthem" TagPrefix="anthem" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <center>
        <table cellpadding="1" cellspacing="0" width="850px">
            <tr>
                <td valign="top">
                    <table>
                        <tr>
                            <td align="center">
                                <fieldset>
                                    <legend>Rate Details</legend>
                                    <table style="margin-top: 10px; width: 100%;">
                                        <tr>
                                            <td class="pageLabel">
                                                Conn. Category :
                                            </td>
                                            <td class="pageControl">
                                                <asp:DropDownList ID="ddl_Con_cat" CssClass="td_text" runat="server" Width="160px"
                                                    AutoPostBack="True" OnSelectedIndexChanged="ddl_Con_cat_SelectedIndexChanged">
                                                </asp:DropDownList>
                                            </td>
                                            <td class="pageLabel">
                                                Pipe Size :
                                            </td>
                                            <td class="pageControl">
                                                <asp:DropDownList ID="ddl_pipe_size" CssClass="td_text" runat="server" Width="160px"
                                                    OnSelectedIndexChanged="ddl_pipe_size_SelectedIndexChanged" AutoPostBack="true">
                                                </asp:DropDownList>
                                            </td>
                                            <td class="pageLabel">
                                                Area Start :
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_areastart" CssClass="td_text" runat="server" MaxLength="6"></asp:TextBox>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="pageLabel">
                                                Area End :
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_areaend" CssClass="td_text" runat="server" MaxLength="6"></asp:TextBox>
                                            </td>
                                            <td class="pageLabel">
                                                Regular Rate :
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_r_rate" CssClass="td_text" MaxLength="10" runat="server"></asp:TextBox>
                                            </td>
                                            <td class="pageLabel">
                                                Temp. Rate :
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_t_rate" CssClass="td_text" MaxLength="10" runat="server"></asp:TextBox>
                                            </td>
                                        </tr>
                                        <tr>
                                            <td class="pageLabel">
                                                Cess Rate :
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_cess_rate" CssClass="td_text" MaxLength="10" runat="server"></asp:TextBox>
                                            </td>
                                            <td class="pageLabel">
                                                Eff. Date From :
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_eff_date_fr" CssClass="readonlytxt" MaxLength="12" runat="server"></asp:TextBox>
                                                <cc1:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd-MMM-yyyy"
                                                    TargetControlID="txt_eff_date_fr">
                                                </cc1:CalendarExtender>
                                            </td>
                                            <td class="pageLabel">
                                                Eff. Date To :
                                            </td>
                                            <td class="pageControl">
                                                <asp:TextBox ID="txt_eff_date_to" CssClass="readonlytxt" MaxLength="12" runat="server"></asp:TextBox>
                                                <cc1:CalendarExtender ID="CalendarExtender2" runat="server" Format="dd-MMM-yyyy"
                                                    TargetControlID="txt_eff_date_to">
                                                </cc1:CalendarExtender>
                                            </td>
                                        </tr>
                                    </table>
                                </fieldset>
                            </td>
                        </tr>
                        <tr>
                            <td align="center">
                                <asp:ImageButton ID="btnSave" runat="server" ImageUrl="~/images/save.png" OnClick="btnSave_Click"
                                    ToolTip="Click Save Record" AccessKey="s" CausesValidation="false" />
                                <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Reset.png" CausesValidation="false"
                                    OnClick="btnReset_Click" ToolTip="Click Reset Page" AccessKey="r" />
                                <asp:ImageButton ID="btnclose" runat="server" ImageUrl="~/images/Cancel.png" CausesValidation="false"
                                    OnClick="btnclose_Click" ToolTip="Click Cancel" AccessKey="c" />
                            </td>
                        </tr>
                        <tr>
                            <td valign="top">
                                <div style="text-align: center; width: 100%; overflow: scroll;" id="div1" runat="server">
                                    <fieldset>
                                        <legend>
                                            <asp:Label ID="lblCtg" runat="server" Text=""></asp:Label>
                                            RATE VIEW</legend>
                                        <asp:GridView ID="gvParentGrid" runat="server" Width="100%" HeaderStyle-CssClass="gridHeader"
                                            RowStyle-CssClass="gridRows" AutoGenerateColumns="False" DataKeyNames="SID" OnRowCancelingEdit="gvParentGrid_RowCancelingEdit"
                                            OnRowDeleting="gvParentGrid_RowDeleting" OnRowEditing="gvParentGrid_RowEditing"
                                            OnRowUpdating="gvParentGrid_RowUpdating">
                                            <Columns>
                                                <asp:BoundField DataField="SID" HeaderText="SId" ReadOnly="true" />
                                                <asp:BoundField DataField="ID" HeaderText="Id" ReadOnly="true" />
                                                <asp:BoundField DataField="AREA_START" HeaderText="Area Start" />
                                                <asp:BoundField DataField="AREA_END" HeaderText="Area End" />
                                                <asp:BoundField DataField="REGULAR" HeaderText="Regular Rate" />
                                                <asp:BoundField DataField="TEMPORARY" HeaderText="Temp. Rate" />
                                                <asp:BoundField DataField="PIPE_SIZE" HeaderText="Pipe Size" />
                                                <asp:BoundField DataField="CESS_RATE" HeaderText="Cess" />
                                                <asp:BoundField DataField="EFF_FROM" HeaderText="Eff. Date From" DataFormatString="{0:dd-MMM-yyyy}" />
                                                <asp:BoundField DataField="EFF_TO" HeaderText="Eff.Date To" DataFormatString="{0:dd-MMM-yyyy}" />
                                                <asp:CommandField ShowEditButton="True" />
                                                <asp:CommandField ShowDeleteButton="True" />
                                            </Columns>
                                            <EmptyDataTemplate>
                                                <table border="1" bordercolor="black" style="border-collapse: collapse;" align="center"
                                                    width="98%">
                                                    <tr class="gridHeader">
                                                        <td>
                                                            SId
                                                        </td>
                                                        <td>
                                                            Id
                                                        </td>
                                                        <td>
                                                            Area Start
                                                        </td>
                                                        <td>
                                                            Area End
                                                        </td>
                                                        <td>
                                                            Regular Rate
                                                        </td>
                                                        <td>
                                                            Temp. Rate
                                                        </td>
                                                        <td>
                                                            Pipe Size
                                                        </td>
                                                        <td>
                                                            Cess
                                                        </td>
                                                        <td>
                                                            Eff. Date From
                                                        </td>
                                                        <td>
                                                            Eff. Date To
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
                    </table>
                </td>
            </tr>
        </table>
    </center>
</asp:Content>
