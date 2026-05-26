<%@ Page Language="C#" AutoEventWireup="true" CodeFile="JAL_BILL_NEW.aspx.cs" Inherits="Report_JAL_BILL_NEW" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<%@ Register Assembly="Microsoft.ReportViewer.WebForms, Version=9.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    Namespace="Microsoft.Reporting.WebForms" TagPrefix="rsweb" %>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>NoidaJal::Bill</title>

    <script language="javascript" type="text/javascript">
    
    function Confirm() {
            var confirm_value = document.createElement("INPUT");
            confirm_value.type = "hidden";
            confirm_value.name = "confirm_value";
            if (confirm("Do you want to save data?")) {
                confirm_value.value = "Yes";
            } else {
                confirm_value.value = "No";
            }
            document.forms[0].appendChild(confirm_value);
        }
    </script>

</head>
<body>
    <form id="form1" runat="server">
    <div id="r1" runat="server" style="height: 100%; width: 100%;">
        <table style="height: 480px; width: 770px;">
            <tr>
                <td align="left">
                    <asp:Button ID="btnPrint" runat="server" Text="Print" OnClientClick="Confirm()" OnClick="btnPrint_Click" />
                </td>
            </tr>
            <tr>
                <td style="vertical-align: top;">
                    <rsweb:ReportViewer ID="ReportViewer1" runat="server" Font-Names="Verdana" Font-Size="8pt"
                        Height="871px" Width="889px">
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
    </div>
    </form>
</body>
</html>
