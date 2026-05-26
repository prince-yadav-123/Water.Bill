<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true" CodeFile="SendMailMessage.aspx.cs" Inherits="MainPage_SendMailMessage" Title="Untitled Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
  <div style=" height:560px; background-color:#ccccff">
    <div style="margin-left:50px; padding-top:100px;">
    <div style=" height:300px; width:500px; background-color:gray; padding-left:150px;padding-top:40px">
        <table width="500">
        <caption> <h3 style="margin-left:-80px">Send Current Year Jal Bill To Consumer By Mail</h3></caption>
      <tr><td style="width:150px"> Consumer Name: </td><td style="width:300px"> <asp:TextBox ID="txtname" runat="server"></asp:TextBox>
      <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*" ControlToValidate="txtname">
        </asp:RequiredFieldValidator> 
      </td></tr>
       <tr><td> Mail Id : </td><td><asp:TextBox ID="txtemail" runat="server"></asp:TextBox> 
       <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="*" ControlToValidate="txtemail">
        </asp:RequiredFieldValidator> </br>
       <asp:RegularExpressionValidator ID="RegularExpressionValidator1" runat="server"    
        ErrorMessage="Mail id should be right formate." ControlToValidate="txtemail"   
        ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*"></asp:RegularExpressionValidator>
       </td></tr>
      <tr><td>  Attached file  </td><td><asp:FileUpload ID="fuAttachment" runat="server" /></td></tr>
         <tr><td><asp:Button ID="sendbtn" runat="server" Text="Send Mail" onclick="sendbtn_Click" /></td></tr>
            </table>
            </div>
            </div>
            </div>
</asp:Content>

