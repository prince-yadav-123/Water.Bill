<%@ Page Language="C#" AutoEventWireup="true" CodeFile="BlockWise.aspx.cs" Inherits="Report_BlockWise" %>

<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>NoidaJal::New Connection Report</title>
</head>
<body>
    <form id="form1" runat="server">
    <div style="text-align:center;">
        <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" 
            Font-Size="8pt" Height="685px" Width="750px" >
            <LocalReport ReportPath="BlockWiseReport.rdlc">
                <DataSources>
                    <rsweb:ReportDataSource DataSourceId="ObjectDataSource1" 
                        Name="JALONLINEDataSet_View_CONSUMER_DETAILS_MASTER_TEMP" />
                </DataSources>
            </LocalReport>
        </rsweb:ReportViewer>
        <asp:ObjectDataSource ID="ObjectDataSource1" runat="server" 
            SelectMethod="GetData" 
            TypeName="JALONLINEDataSetTableAdapters.View_CONSUMER_DETAILS_MASTER_TEMPTableAdapter">
        </asp:ObjectDataSource>
    </div>
    </form>
</body>
</html>
