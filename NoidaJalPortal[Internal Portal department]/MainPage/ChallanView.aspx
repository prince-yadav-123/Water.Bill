<%@ Page Language="C#" AutoEventWireup="true" CodeFile="ChallanView.aspx.cs" Inherits="MainPage_ChallanView" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Water Challan</title>
<script
  src="https://code.jquery.com/jquery-3.6.1.js" integrity="sha256-3zlB5s2uwoUzrXK3BT7AX3FyvojsraNFxCc2vC/7pNI=" crossorigin="anonymous"></script>
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
<script type="text/javascript">   
   $(document).ready( function (){
   debugger;
        var ChallanNo = GetParameterValues('ChallanNo');  
         var ConsNo = GetParameterValues('ConsNo');  
         var BillNo = GetParameterValues('BillNo'); 
        
   function GetParameterValues(param) {  
            var url = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');  
            for (var i = 0; i < url.length; i++) {  
                var urlparam = url[i].split('=');  
                if (urlparam[0] == param) {  
                    return urlparam[1];  
                }  
            }  
        } 
    var ChallanContent = localStorage.getItem('pagedata');
   
   
   
    $.ajax({
            type: "POST",
            url: "ChallanView.aspx/Getdata",
            data:'{data:"'+ChallanContent+'",ConsNo:"'+ConsNo+'", ChallanNo:"'+ChallanNo+'", BillNo:"'+BillNo+'" }',
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (data) {
              if(data.d == true) 
              $("#div_show_data").show();
              else
              $("#div_show_data").hide();
                //alert("s");
            },
            failure: function (response) {
                alert(response.responseText);
            },
            error: function (response) {
                alert(response.responseText);
            }
            });
//    function getdata(){
    $("#div_show_data").html(localStorage["pagedata"])
//     debugger;
//     
//     var ChallanContent = localStorage.getItem('pagedata');
//     
//     $('input[id=hfChallanContent]').val(ChallanContent);
//     
    // alert($('input[id=hfChallanContent]').val());
     
    });
</script>
</head>
<body>
    <form id="form1" runat="server">
    <input type="button" value="Print" onclick="JavaScript:printPartOfPage('div_show_data');"/>
    <asp:Button ID="btnClose" runat="server" Text="Close" onclick="btnClose_Click"/>
   <%-- <asp:HiddenField ID="hfChallanContent" runat="server" />--%>
    <div id="div_show_data"></div>
    </form>
</body>
</html>
