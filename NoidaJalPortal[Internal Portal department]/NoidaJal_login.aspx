<%@ Page Language="C#" MasterPageFile="~/noidajallogin.master" AutoEventWireup="true"
    CodeFile="NoidaJal_login.aspx.cs" Inherits="NoidaJal_login" Title="NoidaJal::Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
<script type="text/javascript">
     var specialKeys = new Array();
     specialKeys.push(8);  //Backspace
     specialKeys.push(9);  //Tab
     specialKeys.push(46); //Delete
     specialKeys.push(36); //Home
     specialKeys.push(35); //End
     specialKeys.push(37); //Left
     specialKeys.push(39); //Right
 
     function IsAlphaNumeric(e) {
         var keyCode = e.keyCode == 0 ? e.charCode : e.keyCode;
         var ret = ((keyCode >= 48 && keyCode <= 57) || (keyCode >= 65 && keyCode <= 90) || keyCode == 32 || (keyCode >= 97 && keyCode <= 122) || (specialKeys.indexOf(e.keyCode) != -1 && e.charCode != e.keyCode));
         document.getElementById("error").style.display = ret ? "none" : "inline";
         return ret;
     }
</script>
    <center>
        <table style="vertical-align: middle;" border="0" cellpadding="0" cellspacing="0">
            <tr>
                <td rowspan="8" style="vertical-align: top; background: url(images/Login_01.png) no-repeat center top;"
                    width="221px" height="254px">
                </td>
                <td style="vertical-align: top; background: url(images/Login_02.png) no-repeat center top;"
                    width="197" height="45">
                </td>
                <td colspan="5" style="vertical-align: top; background: url(images/Login_03.png) no-repeat center top;"
                    width="382" height="45">
                </td>
            </tr>
            <tr>
                <td style="vertical-align: top; background: url(images/Login_04.png) no-repeat center top;"
                    width="197" height="30">
                </td>
                <td colspan="5" style="vertical-align: top; text-align: left; background: url(images/Login_05.png) no-repeat center top;"
                    width="382" height="30">
                    <asp:Label ID="lblerror" runat="server" Font-Names="Verdana" Font-Size="Small" ForeColor="#FF3300"
                        Visible="false"></asp:Label>
                </td>
            </tr>
            <tr>
                <td style="vertical-align: top; background: url(images/Login_06.png) no-repeat center top;"
                    width="197" height="38">
                </td>
                <td colspan="4" style="vertical-align: middle; text-align: left; background: url(images/Login_07.png) no-repeat center top;"
                    width="270" height="38">
                    <asp:TextBox ID="txt_uid" CssClass="td_text" runat="server" MaxLength="10" onkeypress="return IsAlphaNumeric(event);" ondrop="return false;" onpaste="return false;"></asp:TextBox>
                    <span id="error" style="color: Red; display: none">* Special Characters not allowed.</span>
                </td>
                <td rowspan="6" style="vertical-align: top; background: url(images/Login_08.png) no-repeat center top;"
                    width="112" height="179">
                    &nbsp;
                </td>
            </tr>
            <tr>
                <td style="vertical-align: middle; background: url(images/Login_09.png) no-repeat center top;"
                    width="197" height="42">
                </td>
                <td colspan="4" style="vertical-align: middle; text-align: left; background: url(images/Login_10.png) no-repeat center top;"
                    width="270" height="42">
                    <asp:TextBox ID="txt_pass" CssClass="td_text" runat="server" MaxLength="10" TextMode="Password"></asp:TextBox>
                </td>
            </tr>
            <tr>
                <td rowspan="4" style="vertical-align: top; background: url(images/Login_11.png) no-repeat center top;"
                    width="197" height="99">
                </td>
                <td colspan="4" style="vertical-align: top; background: url(images/Login_12.png) no-repeat center top;"
                    width="270" height="15">
                </td>
            </tr>
            <tr>
                <td style="vertical-align: top; background: url(images/Login_16.png) center top;">
                    <asp:ImageButton ID="btn_login" runat="server" ImageUrl="~/images/Login_14.png" Width="104"
                        Height="35" OnClick="btn_login_Click" />
                </td>
                <td style="vertical-align: top; background: url(images/Login_16.png) center top;">
                    <asp:ImageButton ID="btn_close" runat="server" ImageUrl="~/images/Login_15.png" Width="100"
                        Height="35" />
                </td>
                <td style="vertical-align: top; background: url(images/Login_16.png) center top;"
                    width="33" height="35">
                    &nbsp;
                </td>
                <td rowspan="2" style="vertical-align: top; background: url(images/Login_16.png)  center top;"
                    width="33" height="76">
                    &nbsp;
                </td>
            </tr>
            <tr>
                <td colspan="3" style="vertical-align: top; background: url(images/Login_17.png) no-repeat center top;"
                    width="237" height="37">
                </td>
            </tr>
            <tr>
                <td colspan="4" style="vertical-align: top; background: url(images/Login_18.png)  center top;"
                    width="237" height="8">
                </td>
            </tr>
        </table>
    </center>
</asp:Content>
