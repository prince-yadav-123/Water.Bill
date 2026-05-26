<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true" CodeFile="user_attendance1.aspx.cs" Inherits="MainPage_user_attendance" Title="Untitled Page" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" Runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" Runat="Server">
<%--<div style="height:400px; width:400px ; background-color: Black" >--%>
<br />
<div style="height:390px; width:400px; background-color:white" >
   <asp:Label ID="timeviewlbl" runat="server" Text="TIME IN/OUT" 
        Font-Bold="True"></asp:Label> 
        <br />
        <br />
      <div>
     <asp:Timer ID="Timer1" runat="server" ontick="Timer1_Tick" Interval="1000">
     </asp:Timer>
     <asp:UpdatePanel ID="UpdatePanel1" runat="server">
           <ContentTemplate>
               <asp:Label ID="Label1" runat="server" Text="Label" Font-Size="Larger"></asp:Label>
           </ContentTemplate>
               <Triggers>
                   <asp:AsyncPostBackTrigger ControlID="Timer1" EventName="Tick" />
               </Triggers>
     </asp:UpdatePanel>
 </div>
        <br />
        <table>
        <tr><td>
            <asp:Label ID="timeinlabel" runat="server" Text="TIME IN" Width="150px"></asp:Label></td></tr>
            
            <tr><td>
            <asp:Button ID="Button1" runat="server" Text="Time In" BackColor="gray" 
                Width="150px" onclick="timeinbtn_Click" Height="32px" /></td></tr>
          
            <tr><td style="height:50px"></td></tr>
            
        
        
         <tr><td> <asp:Label ID="timeoutlabel" runat="server" Text="TIME OUT" Width="150px"></asp:Label></td></tr>
                <tr><td>
                <asp:Button ID="timeoutbtn" runat="server" Text="Time Out" BackColor="orange" 
                    Width="150px" onclick="timeoutbtn_Click" Height="32px" />  </td></tr>
                    
        </table>
        </div>
    <%--    </div>--%>
</asp:Content>

