<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Challan.aspx.cs" Inherits="Challan" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Water Challan</title>
    <script type="text/javascript">  
    <!--  
    function printPartOfPage(elementId) {  
    var printContent = document.getElementById(elementId);  
    var windowUrl = 'about:blank';  
    var uniqueName = new Date();  
    var windowName = 'Print' + uniqueName.getTime();  
    var printWindow = window.open(windowUrl, windowName, 'left=50000,top=50000,width=0,height=0');  
    printWindow.document.write(printContent.innerHTML);  
    printWindow.document.close();  
    printWindow.focus();  
    printWindow.print();  
    printWindow.close();  
    }  
    // -->  
    </script> 
</head>
<body>
    <form id="form1" runat="server">
    <input type="button" value="Print" onclick="JavaScript:printPartOfPage('dv_challan_content');"/>
    <div id="dv_challan_content" runat="server">
   
    </div>
    </form>
</body>
</html>
