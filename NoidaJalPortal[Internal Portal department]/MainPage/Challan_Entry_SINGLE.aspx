<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true" EnableEventValidation="false"
    CodeFile="Challan_Entry_SINGLE.aspx.cs" Inherits="MainPage_Challan" Title="NoidaJal::Challan Entry" %>

<%@ Register Assembly="Anthem" Namespace="Anthem" TagPrefix="anthem" %>
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
    function calculate()
    {
        var a=0;
        var text1="";
        var c=document.getElementById('<%=htax.ClientID %>').value;
        var grossamount=document.getElementById('<%=txtChallanAmt.ClientID %>').value;
        for(var i=0;i<c;i++)
        {
            var x="ctl00_ContentPlaceHolder1_txt"+i.toString();
            text1=x;
            var val=document.getElementById(x).value;
            if(val=="")
            {
                a=a+parseFloat(val);
                document.getElementById(x).value=0;
            }
            if(val!="0" )
            {
                a=a+parseFloat(val);
            }
        }
        if(parseFloat(a)<=parseFloat(grossamount ))
        {
            var net=parseFloat(grossamount)-parseFloat(a);            
           // document.getElementById('<%=txtChallanAmt.ClientID %>').value=net;
        }
        else
        {
            alert('Enter Valid head Amount');
             document.getElementById(text1).value=0;
            return false;
            document.getElementById('<%=btnSave.ClientID %>').Visible=true;
        }
    }
    </script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <center>
        <table cellpadding="1" cellspacing="0" width="800px" style="height: 450px; vertical-align: top;">
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
                                                    <asp:TextBox ID="txt_cons_no" runat="server" CssClass="td_text_dll" MaxLength="9"></asp:TextBox>
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
                                                        Cons. No:
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:Label ID="lblConsNo" runat="server" Text=""></asp:Label>
                                                    </td>
                                                    <td class="pageLabel">
                                                        Name:
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:Label ID="lblConName" runat="server" BorderStyle="None"></asp:Label>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="pageLabel">
                                                        Property No.:
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:Label ID="lblSector" runat="server" Text=""></asp:Label>
                                                    </td>
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
                                                        Type:
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:Label ID="lblPayType" runat="server" Text=""></asp:Label>
                                                    </td>
                                                    <td class="pageLabel">
                                                        Bank :
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
                                                        Bill from :
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:TextBox ID="txt_bill_from" CssClass="readonlytxt" runat="server" MaxLength="11"></asp:TextBox>
                                                        <cc1:CalendarExtender ID="cal1" runat="server" Format="dd-MMM-yyyy" TargetControlID="txt_bill_from">
                                                        </cc1:CalendarExtender>
                                                    </td>
                                                    <td class="pageLabel">
                                                        Bill to :
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:TextBox ID="txt_bill_to" CssClass="readonlytxt" runat="server" MaxLength="12"></asp:TextBox>
                                                        <cc1:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd-MMM-yyyy"
                                                            TargetControlID="txt_bill_to">
                                                        </cc1:CalendarExtender>
                                                    </td>
                                                    <td class="pageLabel">
                                                        Challan No. :
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:TextBox ID="txt_challan_no" CssClass="td_text" MaxLength="15" runat="server"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator11" runat="server" ErrorMessage="*"
                                                            ControlToValidate="txt_challan_no"></asp:RequiredFieldValidator>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td class="pageLabel">
                                                        Amt.(Rs.) :
                                                    </td>
                                                    <td class="pageControl">
                                                        <asp:TextBox ID="txtChallanAmt" CssClass="td_text" MaxLength="10" runat="server"></asp:TextBox>
                                                        <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                                            ControlToValidate="txtChallanAmt"></asp:RequiredFieldValidator>
                                                    </td>
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
                                        <table width="100%">
                                            <tr>
                                                <td>
                                                    <fieldset>
                                                        <legend>Challan Details</legend>
                                                        <table>
                                                            <tr>
                                                                <td colspan="5">
                                                                    <asp:Panel ID="Panel1" runat="server">
                                                                        <div id="tbltax" runat="server">
                                                                        </div>
                                                                    </asp:Panel>
                                                                </td>
                                                            </tr>
                                                            <tr>
                                                                <td class="pageLabel">
                                                                    Arrear From :
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txt_arrear_from" CssClass="readonlytxt" MaxLength="12" runat="server"></asp:TextBox>
                                                                    <cc1:CalendarExtender ID="CalendarExtender2" runat="server" Format="dd-MMM-yyyy"
                                                                        TargetControlID="txt_arrear_from">
                                                                    </cc1:CalendarExtender>
                                                                </td>
                                                                <td class="pageLabel">
                                                                    Arrear To :
                                                                </td>
                                                                <td>
                                                                    <asp:TextBox ID="txt_arrear_to" CssClass="readonlytxt" MaxLength="12" runat="server"></asp:TextBox>
                                                                    <cc1:CalendarExtender ID="CalendarExtender3" runat="server" Format="dd-MMM-yyyy"
                                                                        TargetControlID="txt_arrear_to">
                                                                    </cc1:CalendarExtender>
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
                                                    <asp:HiddenField ID="htax" runat="server" />                                                    
                                                    <asp:HiddenField ID="txtFlatno" runat="server" />
                                                    <asp:HiddenField ID="txtSector" runat="server" />
                                                    <asp:HiddenField ID="txtBlock" runat="server" />
                                                     <asp:HiddenField ID="txtsrNo" runat="server" />
                                                    <asp:ImageButton ID="btnSave" runat="server" ImageUrl="~/images/save.png" OnClick="btnSave_Click"
                                                        ToolTip="Click Save Record" AccessKey="s" CausesValidation="false" />
                                                    <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Reset.png" CausesValidation="false"
                                                        OnClick="btnReset_Click" ToolTip="Click Reset Page" AccessKey="r" />
                                                    <asp:ImageButton ID="btnclose" runat="server" ImageUrl="~/images/Cancel.png" CausesValidation="false"
                                                        OnClick="btnclose_Click" ToolTip="Click Cancel" AccessKey="c" />
                                                        
                                                    <asp:ImageButton ID="btnExport" runat="server" ImageUrl="~/images/Export.png" OnClick="btnExport_Click" Width="40px" Height="30px"
                                                    ToolTip="Click Export Data in PDF" AccessKey="e" CausesValidation="false"
                                                    />
                                                
                                                
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
                                    <fieldset>
                                        <legend>Challan View</legend>                                       
                                        <div style="text-align: center; width: 800px; overflow: scroll;" id="div1" runat="server">
                                       <%-- <table>
                                        <tr>
                                        <td><asp:Label ID="lblPropertyNo" runat="server"></asp:Label></td>
                                        <td><asp:Label ID="lblConsumerName" runat="server"></asp:Label></td>
                                        </tr>
                                        </table>--%>
                                        <asp:GridView ID="gvParentGrid" runat="server" AutoGenerateColumns="False" 
                                                        HeaderStyle-CssClass="gridHeader" HeaderStyle-Width="200px" 
                                                        OnRowDataBound="gvUserInfo_RowDataBound" RowStyle-CssClass="gridRows" 
                                                        Width="1024px">
                                                        <Columns>
                                                            <asp:BoundField DataField="RECEIPT_ID" HeaderText="Receipt Id" />
                                                            <asp:BoundField DataField="BL_PER_FR" DataFormatString="{0:dd-MMM-yyyy}" 
                                                                HeaderText="Bill From" />
                                                            <asp:BoundField DataField="BL_PER_TO" DataFormatString="{0:dd-MMM-yyyy}" 
                                                                HeaderText="Bill To" />
                                                            <asp:BoundField DataField="PAY_DATE" DataFormatString="{0:dd-MMM-yyyy}" 
                                                                HeaderText="Pay Date" />
                                                            <asp:BoundField DataField="BNK_CD" HeaderText="Bank Name" />
                                                            <asp:BoundField DataField="RECP_NO" HeaderText="Challan No." />
                                                            <asp:BoundField DataField="PAID_AMT" HeaderText="Water Bill" />
                                                            <asp:BoundField DataField="SURCHARGE" HeaderText="Sur Charge" />
                                                            <asp:BoundField DataField="ARREAR" HeaderText="Arrear" />
                                                            <asp:BoundField DataField="NOC" HeaderText="NDC" />
                                                            <asp:BoundField DataField="RMC" HeaderText="Maintenance" />
                                                            <asp:BoundField DataField="SECU" HeaderText="Security" />
                                                            <asp:BoundField DataField="T_FEE" HeaderText="Transfer" />
                                                            <asp:BoundField DataField="CSS" HeaderText="Cess" />
                                                            <asp:BoundField DataField="gst" HeaderText="GST" />
                                                            <asp:BoundField DataField="CONN_CHARGE" HeaderText="Con. Charge" />
                                                            <asp:BoundField DataField="PANALITY_CHARGES" 
                                                                HeaderText="Penalty Charges" />
                                                            <asp:BoundField DataField="R/T" HeaderText="TP" />
                                                            <asp:BoundField DataField="disconnection" HeaderText="Dis Conn" />
                                                            <asp:BoundField DataField="reconnection" HeaderText="Re Conn" />
                                                            <asp:TemplateField HeaderText="Edit">
                                                                <ItemTemplate>
                                                                    <asp:LinkButton ID="link_edit" runat="server" CausesValidation="false" 
                                                                        OnClick="link_edit_Click" OnCommand="link_edit_Command">Edit</asp:LinkButton>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                            <asp:TemplateField HeaderText="Delete">
                                                                <ItemTemplate>
                                                                    <asp:LinkButton ID="link_delete" runat="server" CausesValidation="false" 
                                                                        OnClick="link_delete_Click" OnCommand="link_delete_Command">Delete</asp:LinkButton>
                                                                </ItemTemplate>
                                                            </asp:TemplateField>
                                                        </Columns>
                                                        <EmptyDataTemplate>
                                                            <table align="center" border="1" bordercolor="black" 
                                                                style="border-collapse: collapse;" width="98%">
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
                                        </div>
                                    </fieldset>
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
