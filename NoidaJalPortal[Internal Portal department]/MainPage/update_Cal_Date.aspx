<%@ Page Language="C#" AutoEventWireup="true" CodeFile="update_Cal_Date.aspx.cs" Inherits="Search_Search" %>

<%@ Register Src="~/MainPage/Cal_date.ascx" TagPrefix="uc2" TagName="UserControl" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <base target="_self" />
    <title>NoidaJal::Calculation Date Update</title>
    <link href="../App_Themes/JalTheme/jalbutton.css" rel="stylesheet" type="text/css" />
</head>
<body>
    <form id="form1" runat="server">
    <div>
        <asp:ScriptManager ID="ScriptManager1" runat="server">
        </asp:ScriptManager>
        <center>
            <table width="90%" style="vertical-align: top; text-align: center;">
                <tr>
                    <td align="center">
                        <uc2:UserControl ID="uc1" runat="server" />
                    </td>
                </tr>
            </table>
        </center>
    </div>
    </form>
</body>
</html>
