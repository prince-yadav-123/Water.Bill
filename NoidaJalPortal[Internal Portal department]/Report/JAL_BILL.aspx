<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="JAL_BILL.aspx.cs" Inherits="Report_JAL_BILL" Title="NoidaJal::Bill" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <table style="height: 480px; width: 770px;">
        <tr>
            <td align="left">
                <asp:Button ID="btnPrint" runat="server" Text="Print" 
                    onclick="btnPrint_Click" />
            </td>
        </tr>
        <tr>
            <td style="vertical-align: top;">
                <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt"
                    Height="871px" Width="745px">
                    <LocalReport ReportPath="Report\JAL_BILL_Report.rdlc">
                        <DataSources>
                            <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" Name="JALONLINEDataSet_VIEW_JAL_BILL_MASTER" />
                        </DataSources>
                    </LocalReport>
                </rsweb:ReportViewer>
                <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" OldValuesParameterFormatString="original_{0}"
                    SelectMethod="GetData" TypeName="JALONLINEDataSetTableAdapters.VIEW_JAL_BILL_MASTERTableAdapter">
                </asp:ObjectDataSource>
            </td>
        </tr>
    </table>
</asp:Content>
