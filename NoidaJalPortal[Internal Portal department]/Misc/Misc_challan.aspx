<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Misc_challan.aspx.cs" Inherits="MainPage_Challan" Title="NoidaJal::Rate Master" %>

<%@ Register Assembly="Anthem" Namespace="Anthem" TagPrefix="anthem" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
    <style type="text/css">
        .WaterMarkedTextBox
        {
            height: 16px;
            width: 168px;
            padding: 2px 2 2 2px;
            border: 1px solid #BEBEBE;
            background-color: #F0F8FF;
            color: gray;
            font-size: 8pt;
            text-align: center;
        }
        .fieldSet
        {
            height: 450px;
            width: 130px;
        }
        .fieldSet1
        {
            height: 450px;
        }
        .WaterMarkedddl
        {
            height: 25px;
            width: 168px;
            padding: 2px 2 2 2px;
            border: 1px solid #BEBEBE;
            background-color: #F0F8FF;
            color: gray;
            font-size: 8pt;
            text-align: center;
        }
        .WaterMarkedTextBoxPSW
        {
            background-position: center;
            height: 16px;
            width: 168px;
            padding: 2px 2 2 2px;
            border: 1px solid #BEBEBE;
            background-color: #F0F8FF;
            color: white;
            vertical-align: middle;
            text-align: right;
            background-image: url(Images/psw_wMark.png);
            background-repeat: no-repeat;
        }
        .NormalTextBox
        {
            height: 16px;
            width: 168px;
        }
    </style>

    <script language="javascript" type="text/javascript">
        function Focus(objname, waterMarkText) {
            obj = document.getElementById(objname);
            if (obj.value == waterMarkText) {
                obj.value = "";
                obj.className = "NormalTextBox";
                if (obj.value == "User ID" || obj.value == "" || obj.value == null) {
                    obj.style.color = "black";
                }
            }
        }
        function Blur(objname, waterMarkText) {
            obj = document.getElementById(objname);
            if (obj.value == "") {
                obj.value = waterMarkText;
                if (objname != "txtPwd") {
                    obj.className = "WaterMarkedTextBox";
                }
                else {
                    obj.className = "WaterMarkedTextBoxPSW";
                }
            }
            else {
                obj.className = "NormalTextBox";
            }

            if (obj.value == "User ID" || obj.value == "" || obj.value == null) {
                obj.style.color = "gray";
            }
        }
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <center>
        <asp:UpdateProgress ID="UpdateProgress2" runat="server" AssociatedUpdatePanelID="updtPnlWarErr"
            DisplayAfter="0">
            <ProgressTemplate>
                <div id="SD" align="center" valign="middle" runat="server" style="position: absolute;
                    left: 40%; top: 30%; visibility: visible; vertical-align: middle; border-style: inset;
                    border-color: black; background-color: #c8d1d4;">
                    <asp:Image ID="Image1" runat="server" ImageUrl="~/images/loadingcontent.gif" />
                    <br />
                    Please Wait....
                </div>
            </ProgressTemplate>
        </asp:UpdateProgress>
        <asp:UpdatePanel ID="updtPnlWarErr" runat="server" RenderMode="Inline">
            <ContentTemplate>
                <div id="div_find" runat="server">
                    <table cellpadding="1" cellspacing="0" width="850px" height="490px">
                        <tr>
                            <td valign="top">
                                <table style="width: 110px;">
                                    <tr>
                                        <td align="left" valign="top">
                                            <fieldset class="fieldSet">
                                                <legend>Search By</legend>
                                                <table>
                                                    <tr>
                                                        <td>
                                                            <asp:CheckBoxList ID="Checlst1" runat="server" AutoPostBack="True" OnSelectedIndexChanged="CheckBoxList1_SelectedIndexChanged">
                                                                <asp:ListItem Value="RECP_NO">Challan</asp:ListItem>
                                                                <asp:ListItem Value="Pay_date">Paid Date</asp:ListItem>
                                                                <asp:ListItem Value="paid_amt">Amount</asp:ListItem>
                                                                <asp:ListItem Value="BNK_CD">Bank</asp:ListItem>
                                                            </asp:CheckBoxList>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td>
                                                            <asp:TextBox ID="RECP_NO" onfocus="Focus(this.id,'Challan No.')" onblur="Blur(this.id,'Challan No.')"
                                                                Width="100px" CssClass="WaterMarkedTextBox" runat="server" Visible="false">Challan No.</asp:TextBox>
                                                            <asp:TextBox ID="Pay_date" onfocus="Focus(this.id,'From Date')" onblur="Blur(this.id,'From Date')"
                                                                Width="100px" CssClass="WaterMarkedTextBox" runat="server" Visible="false">From Date</asp:TextBox>
                                                            <asp:TextBox ID="Pay_date1" onfocus="Focus(this.id,'To Date')" onblur="Blur(this.id,'To Date')"
                                                                Width="100px" CssClass="WaterMarkedTextBox" runat="server" Visible="false">To Date</asp:TextBox>
                                                            <asp:TextBox ID="paid_amt" onfocus="Focus(this.id,'Paid Amount')" onblur="Blur(this.id,'Paid Amount')"
                                                                Width="100px" CssClass="WaterMarkedTextBox" runat="server" Visible="false">Paid Amount</asp:TextBox>
                                                            <asp:TextBox ID="BNK_CD1" onfocus="Focus(this.id,'Bank')" onblur="Blur(this.id,'Bank')"
                                                                Width="100px" CssClass="WaterMarkedTextBox" runat="server" Visible="false">Bank</asp:TextBox>
                                                            <asp:DropDownList ID="BNK_CD" onfocus="Focus(this.id,'Bank')" onblur="Blur(this.id,'Bank')"
                                                                CssClass="WaterMarkedddl" Width="105px" runat="server" Visible="false">
                                                            </asp:DropDownList>
                                                        </td>
                                                    </tr>
                                                    <tr>
                                                        <td class="pageControl">
                                                            <asp:ImageButton ID="btnView" runat="server" Text="View" ImageUrl="~/images/View.png"
                                                                OnClick="btnView_Click"></asp:ImageButton>
                                                        </td>
                                                    </tr>
                                                </table>
                                            </fieldset>
                                        </td>
                                        <td valign="top">
                                            <fieldset class="fieldSet1">
                                                <legend>
                                                    <asp:Label ID="lbl_lgnd" runat="server" Text=""></asp:Label></legend>
                                                <div style="text-align: center; width: 700px; height: 440px; overflow: scroll;" id="div1"
                                                    runat="server">
                                                    <asp:GridView ID="gvParentGrid" runat="server" HeaderStyle-CssClass="gridHeader"
                                                        RowStyle-CssClass="gridRows" AutoGenerateColumns="False" DataKeyNames="RECP_NO"
                                                        OnRowCancelingEdit="gvParentGrid_RowCancelingEdit" OnRowEditing="gvParentGrid_RowEditing"
                                                        OnRowUpdating="gvParentGrid_RowUpdating" OnRowDataBound="gvParentGrid_RowDataBound"
                                                        Width="775px">
                                                        <Columns>
                                                            <asp:BoundField DataField="receipt_id" HeaderText="Receipt Id" ReadOnly="true" />
                                                            <asp:TemplateField HeaderText="Consumer No.">
                                                                <EditItemTemplate>
                                                                    <asp:TextBox runat="server" ID="txt_cons_no" Width="80px" Text='<%# Eval("CONS_NO")%>'
                                                                        AutoPostBack="True" MaxLength="8" OnTextChanged="txt_cons_no_TextChanged"></asp:TextBox>
                                                                </EditItemTemplate>
                                                                <ItemTemplate>
                                                                    <%# Eval("CONS_NO")%>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                             <asp:BoundField DataField="deposeter_name" HeaderText="DEPOSETER_NAME" ReadOnly="true" />
                                                            <asp:BoundField DataField="Property_no" HeaderText="Property No." ReadOnly="true" />
                                                            <asp:BoundField DataField="BL_PER_FR" HeaderText="From" DataFormatString="{0:dd-MMM-yyyy}"
                                                                ReadOnly="true" />
                                                            <asp:BoundField DataField="BL_PER_TO" HeaderText="To" DataFormatString="{0:dd-MMM-yyyy}"
                                                                ReadOnly="true" />
                                                            <asp:BoundField DataField="pay_date" HeaderText="Pay Date" DataFormatString="{0:dd-MMM-yyyy}"
                                                                ReadOnly="true" />
                                                            <asp:BoundField DataField="paid_amt" HeaderText="Paid Amt." ReadOnly="true" />
                                                            <asp:BoundField DataField="RECP_NO" HeaderText="Challan No." ReadOnly="true" />
                                                            <asp:BoundField DataField="BNK_CD" HeaderText="Bank" ReadOnly="true" />
                                                            <asp:BoundField DataField="ADDRESS" 
HeaderText="ADDRESS" ReadOnly="true" />
          <asp:CommandField ShowEditButton="True" />
                                                        </Columns>
                                                        <EmptyDataTemplate>
                                                            <table border="1" bordercolor="black" style="border-collapse: collapse;" align="center"
                                                                width="98%">
                                                                <tr class="gridHeader">
                                                                    <td>
                                                                        Consumer No.
                                                                    </td>
                                                                    <td>
                                                                        Flat No.
                                                                    </td>
                                                                    <td>
                                                                        Block
                                                                    </td>
                                                                    <td>
                                                                        Sector
                                                                    </td>
                                                                    <td>
                                                                        From
                                                                    </td>
                                                                    <td>
                                                                        To
                                                                    </td>
                                                                    <td>
                                                                        Bill Amt.
                                                                    </td>
                                                                    <td>
                                                                        Challan No.
                                                                    </td>
                                                                    <td>
                                                                        Bank
                                                                    </td>
                                                                    <td>
                                                                        Branch
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
                                                </div>
                                            </fieldset>
                                        </td>
                                    </tr>
                                </table>
                            </td>
                        </tr>
                    </table>
                </div>
            </ContentTemplate>
            <Triggers>
                <asp:AsyncPostBackTrigger ControlID="btnView" EventName="Click" />
                <asp:AsyncPostBackTrigger ControlID="Checlst1" EventName="SelectedIndexChanged" />
            </Triggers>
        </asp:UpdatePanel>
    </center>
</asp:Content>
