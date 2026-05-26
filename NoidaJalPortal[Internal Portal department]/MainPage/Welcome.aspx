<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Welcome.aspx.cs" Inherits="MainPage_Welcome" Title="NoidaJal::Welcome" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <table style="height: 450px">
        <tr>
            <td>
                <table class="tbl">
                    <tr>
                        <td align="center">
                            <div style="text-align: center;" id="div_main" runat="server">
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</asp:Content>
