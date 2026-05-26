<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Daily_Report.aspx.cs" Inherits="Report_Daily_Report" 
MasterPageFile="~/noidajal.master" Title="NoidaJal::Bill" EnableEventValidation = "false"%>
<%@ Register Assembly="Anthem" Namespace="Anthem" TagPrefix="anthem" %>
<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
                   
    <table style="height: 450px; width: 80%">        
          <tr>
                   <td valign="top">
                   <fieldset>
                   <legend><b>Daily Report</b></legend>
                   
                                    
                    <table>
                       <tr>
                           <td class="pageLabel">
                                 File Scanning
                           </td>      
                           <td class="pageControl">
                                  <asp:TextBox ID="txtFileScanning" runat="server"></asp:TextBox>
                                  <asp:HiddenField ID="hdFileScanning" runat="server" />
                           </td>
                           <td></td>
                      
                           <td class="pageLabel">
                                 Email/Query 
                           </td>      
                           <td class="pageControl">
                                  <asp:TextBox ID="txtEmailQuery" runat="server"></asp:TextBox>
                                  <asp:HiddenField ID="hdEmailQuery" runat="server" />
                           </td>
                           <td>
                           </td>
                           <td class="pageControl">
                               <asp:Button ID="txtSave" runat="server" 
                                 Text="Save" onclick="txtSave_Click"/></td>                        
                         </tr>
                    
                    </table>
                    
                    <table>
                        <tr>
                         <td class="pageLabel">UserID : </td>                    
                         <td class="pageControl">
                         <asp:DropDownList ID="ddlUserID" runat="server" Style="height: 22px"></asp:DropDownList>
                         </td>                      
                         <td></td>                         
                         <td class="pageLabel">Date From : </td>                         
                         <td class="pageControl">
                         <asp:TextBox ID="txtFromDate" runat="server" MaxLength="12"></asp:TextBox>
                         <cc1:CalendarExtender ID="CalendarExtender1" runat="server" Format="yyyy-MM-dd" TargetControlID="txtFromDate"></cc1:CalendarExtender>                                                        
                         </td> 
                         <td class="pageLabel">Date To : </td>                         
                         <td class="pageControl">
                         <asp:TextBox ID="txtToDate" runat="server" MaxLength="12"></asp:TextBox>
                         <cc1:CalendarExtender ID="CalendarExtender2" runat="server" Format="yyyy-MM-dd" TargetControlID="txtToDate"></cc1:CalendarExtender>                                                        
                         </td> 
                         <td></td>
                         <td class="pageControl"><asp:Button ID="btnGetReport" runat="server" 
                                 Text="Get Report" onclick="btnGetReport_Click"/></td>                        
                         </tr>
                        
                    </table>
                    
                    <table id="maintable" runat="server" visible="false"   border="1" style="width:100%; font-weight:bold; border:solid; border-color:black;">
                    <thead>
                    <tr>
                    <th>Challan</th>
                    <th>Update</th>
                    <th>Billing</th>
                    <th>New_File</th>
                    <th>Name_Transfer</th>
                    <th>NOC/NDC</th>
                    <th>File Scanning</th>
                    <th>Email/Query</th>
                    </tr>
                    </thead> 
                    <tbody>  
                    <tr>                  
                    <td><asp:Label ID="lblChallan" runat="server"></asp:Label></td>
                    <td><asp:Label ID="lblChallanUpdate" runat="server"></asp:Label></td>
                    <td><asp:Label ID="lblBilling" runat="server"></asp:Label></td>
                    <td><asp:Label ID="lblNewFile" runat="server"></asp:Label></td>
                    <td><asp:Label ID="lblNameTransfer" runat="server"></asp:Label></td>
                    <td><asp:Label ID="lblNOC" runat="server"></asp:Label></td>
                    <td><asp:Label ID="lblFileScanning" runat="server"></asp:Label></td>
                    <td><asp:Label ID="lblEmailQuery" runat="server"></asp:Label></td>
                    </tr>
                    </tbody>
                    <tr>
                    <td><asp:Button ID="btnExport" runat="server" Text="Export To Excel" 
                            onclick="btnExport_Click" /></td> 
                    </tr>
                    </table>
                                 
                    </fieldset>
                   </td>
             
        </tr>
    </table>
</asp:Content>