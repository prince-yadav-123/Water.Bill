<%@ Page Language="C#" MasterPageFile="~/noidajal.master" AutoEventWireup="true"
    CodeFile="Generate_bill_With_dues.aspx.cs" Inherits="Update_Generate_bill" Title="NoidaJal::Generate Bill" %>

<%@ Register Assembly="AjaxControlToolkit" Namespace="AjaxControlToolkit" TagPrefix="cc1" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="Server">

    <script type="text/javascript" language="javascript">
        function f1() {
            window.showModalDialog("../Search/Search.aspx", "newwindow", "center:yes;dialogWidth:800px;dialogHeight:750px; resizable:no;");
        }
        function f2() {
            window.showModalDialog("../MainPage/update_Cal_Date.aspx", "newwindow", "center:yes;dialogWidth:500px;dialogHeight:200px; resizable:no;");
        }

        function GetAmount(x) {
            var a1 = x;

            var a = document.getElementById(a1).value;
            var b = document.getElementById(b1).value;
            var c = a * b;
            document.getElementById(c1).value = c;

        }
    </script>
   
    <script src="../js/jquery.min.js" type="text/javascript"></script>
    
        <script type="text/javascript">
        function ValidateDropDownList() {
        //debugger;
            var isSelected = false;
            var ddlBank = document.getElementById("<%=ddlBank.ClientID%>").value;
            if (ddlBank == "0") {
                //Selected option from dropdownlist
                alert("Please select bank name");            
            } else {
                //Not selected so alert user to select any option
                isSelected = true;
            }
            return isSelected;
            }
        </script>

        <script type="text/javascript">
                var ChallanNo="";
                var bearer=""; 
                var Department_Id="";
                var Allottee="";
                var Address="";
                var Mobile_No="";
                var BankCode="";
                var ChallanTypeId="";
                var TotalAmount="";
                var PaymentModeId="";
                var FormNumber="";
                var HeadId="";
                var SubheadId="";                                         
                var Amount="";
                var Email="";
                var Rid="";
                var Plot_No="";
                var PAN="";
                var GST_No="";
                var SectorName="" ;
                var BlockName="";
                var User= "";
                var CategoryTypeId=""; 
                var ConsumerNo="";
                var OldChallanId="";
                var PropertyType="";
                var DueDate="";
                var BillPeriod=""; 
                var CustomerType="";
                var ConnectionType ="";               
                jsonObj = [];

           function Get(d) {
           //debugger;
//                    alert(d);                   
                   var result = JSON.parse(d);
                   
                     $.each(result, function (index, obj) {
//                                      Department_Id= 1;
//                                      Allottee=obj.CONS_NM1;                                      
//                                      Address=obj.property_no;
//                                      Mobile_No="7503674611";
//                                      BankCode= "HDFC18C";
//                                      ChallanTypeId=1;
//                                      TotalAmount=obj.TOTAL_BILL_AMT;
//                                      PaymentModeId=1;
//                                      FormNumber= obj.BILL_NO;
//                                      Rid=obj.CONS_NO;
//                                      HeadId= 1;
//                                      SubheadId=1;                                         
//                                      Amount= obj.TOTAL_BILL_AMT;
//                                      Email=obj.EMAIL_ID;
//                                      Plot_No=obj.FLAT_NO;
//                                      PAN="";
//                                      GST_No= "";
//                                      SectorName=obj.SECTOR ;
//                                      BlockName= obj.BLK_NO;
//                                      User="admin";
//                                      CategoryTypeId= 7;
                                      Department_Id= 94;
                                      Rid = obj.Rid;                                     
                                      Allottee=obj.CONS_NM1;                                      
                                      Address=obj.property_no;
                                      Mobile_No=obj.MOB_NO;                                     
                                      BankCode= obj.BANK_CODE;  
                                      //BankCode= "HDFC18C";                                 
                                      ChallanTypeId= 2;                                   
                                      ///TotalAmount=obj.TOTAL_BILL_AMT;
                                      //TotalAmount=300;
                                      //PaymentModeId=1
                                      PaymentModeId=obj.PAYMENT_TYPE;                                     
                                      FormNumber= obj.CONS_NO;
                                      ConsumerNo =  obj.CONS_NO;
                                      OldChallanId = obj.BILL_NO;
                                      CustomerType = obj.CONS_CTG;
                                      ConnectionType = obj.PIPE_SIZE_WITH_MM;                                      
                                      var date= obj.DueDate;
                                      var d=new Date(date.split("/").reverse().join("-"));                                       
                                      var dd=d.getDate();
                                      var mm=d.getMonth()+1;
                                      var yy=d.getFullYear();                                      
                                      var newdate=yy+"-"+mm+"-"+dd;                                      
                                      DueDate = newdate;                                     
                                      BillPeriod = obj.BillPeriod;
                                      
                                      if (obj.CON_TP == "Housing") {
                                      TotalAmount = parseInt(obj.MIN_TOTAL_AMT)- parseInt(obj.LAST_PAID_AMT)- parseInt(obj.BILL_REBATE_AMT) + parseInt(obj.CESS_AMT) + parseInt(obj.AREAR) + parseInt(obj.AREAR_INT) - parseInt(obj.adv_amt);                                                                           
                                      }
                                      if (obj.CON_TP == "Residential") {
                                      TotalAmount = parseInt(obj.MIN_TOTAL_AMT)- parseInt(obj.LAST_PAID_AMT)- parseInt(obj.BILL_REBATE_AMT) + parseInt(obj.CESS_AMT) + parseInt(obj.AREAR) + parseInt(obj.AREAR_INT) - parseInt(obj.adv_amt);                                                                             
                                      }
                                      if (obj.CON_TP == "Industrial") {
                                      TotalAmount = parseInt(obj.MIN_TOTAL_AMT)- parseInt(obj.LAST_PAID_AMT)- parseInt(obj.BILL_REBATE_AMT) + parseInt(obj.CESS_AMT) + parseInt(obj.AREAR) + parseInt(obj.AREAR_INT) - parseInt(obj.adv_amt);                                                                            
                                      }
                                      if (obj.CON_TP == "Commercial") {
                                      TotalAmount = parseInt(obj.MIN_TOTAL_AMT)- parseInt(obj.LAST_PAID_AMT)- parseInt(obj.BILL_REBATE_AMT) + parseInt(obj.CESS_AMT) + parseInt(obj.AREAR) + parseInt(obj.AREAR_INT) - parseInt(obj.adv_amt);                                                                            
                                      }
                                      if (obj.CON_TP == "Institutional") {
                                      TotalAmount = parseInt(obj.MIN_TOTAL_AMT)- parseInt(obj.LAST_PAID_AMT)- parseInt(obj.BILL_REBATE_AMT) + parseInt(obj.CESS_AMT) + parseInt(obj.AREAR) + parseInt(obj.AREAR_INT) - parseInt(obj.adv_amt);                                                                           
                                      }
                                      if (obj.CON_TP == "Group Housing") {
                                      TotalAmount = parseInt(obj.MIN_TOTAL_AMT)- parseInt(obj.LAST_PAID_AMT)- parseInt(obj.BILL_REBATE_AMT) + parseInt(obj.CESS_AMT) + parseInt(obj.AREAR) + parseInt(obj.AREAR_INT) - parseInt(obj.adv_amt);                                                                           
                                      }
                                      if (obj.CON_TP == "Village") {
                                      TotalAmount = parseInt(obj.MIN_TOTAL_AMT)- parseInt(obj.LAST_PAID_AMT)- parseInt(obj.BILL_REBATE_AMT) + parseInt(obj.CESS_AMT) + parseInt(obj.AREAR) + parseInt(obj.AREAR_INT) - parseInt(obj.adv_amt);                                                                           
                                      }
                                      
                                      if (obj.CON_TP == "Housing") {                                                                    
                                        item = {}
                                        item["Rid"] = obj.Rid;
                                        item["HeadId"] = "10";
                                        item["SubheadId"] = "634";
                                        item["Amount"] = obj.MIN_TOTAL_AMT;
                                        jsonObj.push(item); 
                                        item1 = {}
                                        item1["Rid"] = obj.Rid;
                                        item1["HeadId"] = "10";
                                        item1["SubheadId"] = "635";
                                        item1["Amount"] = obj.LAST_PAID_AMT;
                                        jsonObj.push(item1);
                                        item2 = {}
                                        item2["Rid"] = obj.Rid;
                                        item2["HeadId"] = "10";
                                        item2["SubheadId"] = "633";
                                        item2["Amount"] = obj.BILL_REBATE_AMT;
                                        jsonObj.push(item2);                                                                
                                        item3 = {}
                                        item3["Rid"] = obj.Rid;
                                        item3["HeadId"] = "10";
                                        item3["SubheadId"] = "57";
                                        item3["Amount"] = obj.CESS_AMT;
                                        jsonObj.push(item3);
                                        item4 = {}
                                        item4["Rid"] = obj.Rid;
                                        item4["HeadId"] = "10";
                                        item4["SubheadId"] = "89";
                                        item4["Amount"] = obj.AREAR;
                                        jsonObj.push(item4); 
                                        item5 = {}
                                        item5["Rid"] = obj.Rid;
                                        item5["HeadId"] = "10";
                                        item5["SubheadId"] = "621";
                                        item5["Amount"] = obj.AREAR_INT;
                                        jsonObj.push(item5);  
                                        item6 = {}
                                        item6["Rid"] = obj.Rid;
                                        item6["HeadId"] = "10";
                                        item6["SubheadId"] = "622";
                                        item6["Amount"] = obj.adv_amt;
                                        jsonObj.push(item6);                                                                                                                
                                      }
                                      
                                      if (obj.CON_TP == "Residential") {                                                                    
                                        item = {}
                                        item["Rid"] = obj.Rid;
                                        item["HeadId"] = "10";
                                        item["SubheadId"] = "634";
                                        item["Amount"] = obj.MIN_TOTAL_AMT;
                                        jsonObj.push(item); 
                                        item1 = {}
                                        item1["Rid"] = obj.Rid;
                                        item1["HeadId"] = "10";
                                        item1["SubheadId"] = "635";
                                        item1["Amount"] = obj.LAST_PAID_AMT;
                                        jsonObj.push(item1);
                                        item2 = {}
                                        item2["Rid"] = obj.Rid;
                                        item2["HeadId"] = "10";
                                        item2["SubheadId"] = "633";
                                        item2["Amount"] = obj.BILL_REBATE_AMT;
                                        jsonObj.push(item2);                                                                
                                        item3 = {}
                                        item3["Rid"] = obj.Rid;
                                        item3["HeadId"] = "10";
                                        item3["SubheadId"] = "57";
                                        item3["Amount"] = obj.CESS_AMT;
                                        jsonObj.push(item3);
                                        item4 = {}
                                        item4["Rid"] = obj.Rid;
                                        item4["HeadId"] = "10";
                                        item4["SubheadId"] = "89";
                                        item4["Amount"] = obj.AREAR;
                                        jsonObj.push(item4); 
                                        item5 = {}
                                        item5["Rid"] = obj.Rid;
                                        item5["HeadId"] = "10";
                                        item5["SubheadId"] = "621";
                                        item5["Amount"] = obj.AREAR_INT;
                                        jsonObj.push(item5);  
                                        item6 = {}
                                        item6["Rid"] = obj.Rid;
                                        item6["HeadId"] = "10";
                                        item6["SubheadId"] = "622";
                                        item6["Amount"] = obj.adv_amt;
                                        jsonObj.push(item6);                                                                                                                       
                                      }
                                      
                                       if (obj.CON_TP == "Industrial") {                                                                    
                                        item = {}
                                        item["Rid"] = obj.Rid;
                                        item["HeadId"] = "10";
                                        item["SubheadId"] = "634";
                                        item["Amount"] = obj.MIN_TOTAL_AMT;
                                        jsonObj.push(item); 
                                        item1 = {}
                                        item1["Rid"] = obj.Rid;
                                        item1["HeadId"] = "10";
                                        item1["SubheadId"] = "635";
                                        item1["Amount"] = obj.LAST_PAID_AMT;
                                        jsonObj.push(item1);
                                        item2 = {}
                                        item2["Rid"] = obj.Rid;
                                        item2["HeadId"] = "10";
                                        item2["SubheadId"] = "633";
                                        item2["Amount"] = obj.BILL_REBATE_AMT;
                                        jsonObj.push(item2);                                                                
                                        item3 = {}
                                        item3["Rid"] = obj.Rid;
                                        item3["HeadId"] = "10";
                                        item3["SubheadId"] = "57";
                                        item3["Amount"] = obj.CESS_AMT;
                                        jsonObj.push(item3);
                                        item4 = {}
                                        item4["Rid"] = obj.Rid;
                                        item4["HeadId"] = "10";
                                        item4["SubheadId"] = "89";
                                        item4["Amount"] = obj.AREAR;
                                        jsonObj.push(item4); 
                                        item5 = {}
                                        item5["Rid"] = obj.Rid;
                                        item5["HeadId"] = "10";
                                        item5["SubheadId"] = "621";
                                        item5["Amount"] = obj.AREAR_INT;
                                        jsonObj.push(item5);  
                                        item6 = {}
                                        item6["Rid"] = obj.Rid;
                                        item6["HeadId"] = "10";
                                        item6["SubheadId"] = "622";
                                        item6["Amount"] = obj.adv_amt;
                                        jsonObj.push(item6);                                                                                                                         
                                      }
                                      
                                       if (obj.CON_TP == "Commercial") {                                                                    
                                        item = {}
                                        item["Rid"] = obj.Rid;
                                        item["HeadId"] = "10";
                                        item["SubheadId"] = "634";
                                        item["Amount"] = obj.MIN_TOTAL_AMT;
                                        jsonObj.push(item); 
                                        item1 = {}
                                        item1["Rid"] = obj.Rid;
                                        item1["HeadId"] = "10";
                                        item1["SubheadId"] = "635";
                                        item1["Amount"] = obj.LAST_PAID_AMT;
                                        jsonObj.push(item1);
                                        item2 = {}
                                        item2["Rid"] = obj.Rid;
                                        item2["HeadId"] = "10";
                                        item2["SubheadId"] = "633";
                                        item2["Amount"] = obj.BILL_REBATE_AMT;
                                        jsonObj.push(item2);                                                                
                                        item3 = {}
                                        item3["Rid"] = obj.Rid;
                                        item3["HeadId"] = "10";
                                        item3["SubheadId"] = "57";
                                        item3["Amount"] = obj.CESS_AMT;
                                        jsonObj.push(item3);
                                        item4 = {}
                                        item4["Rid"] = obj.Rid;
                                        item4["HeadId"] = "10";
                                        item4["SubheadId"] = "89";
                                        item4["Amount"] = obj.AREAR;
                                        jsonObj.push(item4); 
                                        item5 = {}
                                        item5["Rid"] = obj.Rid;
                                        item5["HeadId"] = "10";
                                        item5["SubheadId"] = "621";
                                        item5["Amount"] = obj.AREAR_INT;
                                        jsonObj.push(item5);  
                                        item6 = {}
                                        item6["Rid"] = obj.Rid;
                                        item6["HeadId"] = "10";
                                        item6["SubheadId"] = "622";
                                        item6["Amount"] = obj.adv_amt;
                                        jsonObj.push(item6);                                                                                                                         
                                      }
                                      
                                      if (obj.CON_TP == "Institutional") {                                                                    
                                        item = {}
                                        item["Rid"] = obj.Rid;
                                        item["HeadId"] = "10";
                                        item["SubheadId"] = "634";
                                        item["Amount"] = obj.MIN_TOTAL_AMT;
                                        jsonObj.push(item); 
                                        item1 = {}
                                        item1["Rid"] = obj.Rid;
                                        item1["HeadId"] = "10";
                                        item1["SubheadId"] = "635";
                                        item1["Amount"] = obj.LAST_PAID_AMT;
                                        jsonObj.push(item1);
                                        item2 = {}
                                        item2["Rid"] = obj.Rid;
                                        item2["HeadId"] = "10";
                                        item2["SubheadId"] = "633";
                                        item2["Amount"] = obj.BILL_REBATE_AMT;
                                        jsonObj.push(item2);                                                                
                                        item3 = {}
                                        item3["Rid"] = obj.Rid;
                                        item3["HeadId"] = "10";
                                        item3["SubheadId"] = "57";
                                        item3["Amount"] = obj.CESS_AMT;
                                        jsonObj.push(item3);
                                        item4 = {}
                                        item4["Rid"] = obj.Rid;
                                        item4["HeadId"] = "10";
                                        item4["SubheadId"] = "89";
                                        item4["Amount"] = obj.AREAR;
                                        jsonObj.push(item4); 
                                        item5 = {}
                                        item5["Rid"] = obj.Rid;
                                        item5["HeadId"] = "10";
                                        item5["SubheadId"] = "621";
                                        item5["Amount"] = obj.AREAR_INT;
                                        jsonObj.push(item5);  
                                        item6 = {}
                                        item6["Rid"] = obj.Rid;
                                        item6["HeadId"] = "10";
                                        item6["SubheadId"] = "622";
                                        item6["Amount"] = obj.adv_amt;
                                        jsonObj.push(item6);                                                                                                                        
                                      }
                                      
                                       if (obj.CON_TP == "Group Housing") {                                                                    
                                        item = {}
                                        item["Rid"] = obj.Rid;
                                        item["HeadId"] = "10";
                                        item["SubheadId"] = "634";
                                        item["Amount"] = obj.MIN_TOTAL_AMT;
                                        jsonObj.push(item); 
                                        item1 = {}
                                        item1["Rid"] = obj.Rid;
                                        item1["HeadId"] = "10";
                                        item1["SubheadId"] = "635";
                                        item1["Amount"] = obj.LAST_PAID_AMT;
                                        jsonObj.push(item1);
                                        item2 = {}
                                        item2["Rid"] = obj.Rid;
                                        item2["HeadId"] = "10";
                                        item2["SubheadId"] = "633";
                                        item2["Amount"] = obj.BILL_REBATE_AMT;
                                        jsonObj.push(item2);                                                                
                                        item3 = {}
                                        item3["Rid"] = obj.Rid;
                                        item3["HeadId"] = "10";
                                        item3["SubheadId"] = "57";
                                        item3["Amount"] = obj.CESS_AMT;
                                        jsonObj.push(item3);
                                        item4 = {}
                                        item4["Rid"] = obj.Rid;
                                        item4["HeadId"] = "10";
                                        item4["SubheadId"] = "89";
                                        item4["Amount"] = obj.AREAR;
                                        jsonObj.push(item4); 
                                        item5 = {}
                                        item5["Rid"] = obj.Rid;
                                        item5["HeadId"] = "10";
                                        item5["SubheadId"] = "621";
                                        item5["Amount"] = obj.AREAR_INT;
                                        jsonObj.push(item5);  
                                        item6 = {}
                                        item6["Rid"] = obj.Rid;
                                        item6["HeadId"] = "10";
                                        item6["SubheadId"] = "622";
                                        item6["Amount"] = obj.adv_amt;
                                        jsonObj.push(item6);                                                                                                                          
                                      }

                                      if (obj.CON_TP == "Village") {                                                                    
                                        item = {}
                                        item["Rid"] = obj.Rid;
                                        item["HeadId"] = "10";
                                        item["SubheadId"] = "634";
                                        item["Amount"] = obj.MIN_TOTAL_AMT;
                                        jsonObj.push(item); 
                                        item1 = {}
                                        item1["Rid"] = obj.Rid;
                                        item1["HeadId"] = "10";
                                        item1["SubheadId"] = "635";
                                        item1["Amount"] = obj.LAST_PAID_AMT;
                                        jsonObj.push(item1);
                                        item2 = {}
                                        item2["Rid"] = obj.Rid;
                                        item2["HeadId"] = "10";
                                        item2["SubheadId"] = "633";
                                        item2["Amount"] = obj.BILL_REBATE_AMT;
                                        jsonObj.push(item2);                                                                
                                        item3 = {}
                                        item3["Rid"] = obj.Rid;
                                        item3["HeadId"] = "10";
                                        item3["SubheadId"] = "57";
                                        item3["Amount"] = obj.CESS_AMT;
                                        jsonObj.push(item3);
                                        item4 = {}
                                        item4["Rid"] = obj.Rid;
                                        item4["HeadId"] = "10";
                                        item4["SubheadId"] = "89";
                                        item4["Amount"] = obj.AREAR;
                                        jsonObj.push(item4); 
                                        item5 = {}
                                        item5["Rid"] = obj.Rid;
                                        item5["HeadId"] = "10";
                                        item5["SubheadId"] = "621";
                                        item5["Amount"] = obj.AREAR_INT;
                                        jsonObj.push(item5);  
                                        item6 = {}
                                        item6["Rid"] = obj.Rid;
                                        item6["HeadId"] = "10";
                                        item6["SubheadId"] = "622";
                                        item6["Amount"] = obj.adv_amt;
                                        jsonObj.push(item6);                                                                                                                          
                                      }

                                   
                                      Email=obj.EMAIL_ID;
                                      Plot_No=obj.FLAT_NO;
                                      PAN="";
                                      GST_No= "";
                                      SectorName=obj.SECTOR ;
                                      BlockName= obj.BLK_NO;
                                      User="admin";
                                      //CategoryTypeId= 7; 
                                      if (obj.DEV_TYPE == "1") { 
                                      CategoryTypeId = "1034";
                                      }
                                      if (obj.DEV_TYPE == "2") { 
                                      CategoryTypeId = "1035";
                                      }
                                      if (obj.DEV_TYPE == "3") { 
                                      CategoryTypeId = "1039";
                                      }
                                      //Department Id
                                      if (obj.CON_TP == "Institutional") { 
                                      PropertyType = "Institutional";
                                      }
                                      if (obj.CON_TP == "Commercial") { 
                                      PropertyType = "Commercial";
                                      }
                                      if (obj.CON_TP == "Residential") { 
                                      PropertyType = "Residential";
                                      }
                                      if (obj.CON_TP == "Industrial") { 
                                      PropertyType = "Industrial";
                                      }
                                      if (obj.CON_TP == "Housing") { 
                                      PropertyType = "Housing";
                                      }
                                      if (obj.CON_TP == "Group Housing") { 
                                      PropertyType = "Group Housing";
                                      }
                                      if (obj.CON_TP == "Village") { 
                                      PropertyType = "Village";
                                      }
                                  GetToken();
                    });
//                },
//                error: function (XMLHttpRequest, textStatus, errorThrown) {
//                    alert(errorThrown);
//                }
//            })
           
        
              }
        
    
    
function GetToken() {
        //var bearer ="";
        var user ={
            grant_type:'password',
            username:'admin',
            password:'Admin'
        };
        $.ajax({
            type: "POST",
            url: "https://bankapi.mynoida.in/token",
            data:user,
            contentType: "application/x-www-form-urlencoded",
            dataType: "json",
            success: function (data) {
                bearer = JSON.parse(JSON.stringify(data));
                bearer = bearer.access_token;
                //alert(bearer);
                Authorization();
            },
           error: function (request, status, error) {
                   alert(request.responseText);
               }
            });

         function Authorization() {

           var f = JSON.stringify({
             Department_Id:Department_Id, Allottee:  Allottee + '' , Address: Address +'', Mobile_No: Mobile_No + '', BankCode: BankCode +'',ChallanTypeId: ChallanTypeId,TotalAmount: TotalAmount,PaymentModeId: PaymentModeId,FormNumber: FormNumber+'',"ChallanChargesVM":jsonObj, Email: Email +'',Rid: Rid, Plot_No: Plot_No +'',  PAN: PAN +'',GST_No: GST_No+'',SectorName: SectorName+'',BlockName: BlockName+'', User:User+'',CategoryTypeId:CategoryTypeId,ConsumerNo:ConsumerNo,OldChallanId:OldChallanId,PropertyType:PropertyType,DueDate:DueDate,BillPeriod:BillPeriod,CustomerType:CustomerType,ConnectionType:ConnectionType
           });
           
           debugger
            $.ajax({
                type: "POST",
                url: "https://bankapi.mynoida.in/api/Receipt/CreateJalChallan",       
                headers:  {
               "Authorization": 'Bearer '  + bearer },
                data:f,
                contentType: "application/json",             
                success: function (data) { 
                //debugger  
                
                      var Challan = data.ChallanNo;
//                     alert(Challan);
                     localStorage["pagedata"]=data.ChallanContent;
                      //alert(localStorage["pagedata"]);
                      //window.open("ChallanView.aspx?ChallanNo="+Challan+"&ConsNo="+ConsumerNo+"&BillNo="+OldChallanId+"", "new Window", "left=20,top=20,width=1024,height=675,toolbar=1,scrollbars=1,resizable=1");

                      //window.open("ChallanView.aspx?ChallanNo="+Challan+"&ConsNo="+ConsumerNo+"&BillNo="+OldChallanId+"", "_blank");

                     window.location.assign("ChallanView.aspx?ChallanNo="+Challan+"&ConsNo="+ConsumerNo+"&BillNo="+OldChallanId+""); 
                                     
                },                
                error: function (request, status, error) {
                   alert(request.responseText);
               }
            });
        }
 }
 
//            function GetChallanNo()
//            {
//            //alert(ChallanNo);
//                 $.ajax({                
//                    type: "POST",
//                    url: "Generate_bill_With_dues.aspx/GetChallanNo",
//                    data: ChallanNo,
//                    contentType: "application/json; charset=utf-8",
//                    dataType: "json",
//                    success: function (response) {
//                    //debugger
//                        alert(response.d);
//                    },
//                    failure: function (response) {
//                    //debugger
//                        alert(response.responseText);
//                    },
//                    error: function (response) {
//                    //debugger
//                        alert(response.responseText);
//                    }
//                });
//            }

</script>

</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="Server">
    <center>
    
       
       <div id="div_show_data"></div>
       
        <table cellpadding="1" cellspacing="0" width="850px">
            <tr>
                <td style="height: 30px;">
                </td>
            </tr>
            <tr>
                <td>
                    <fieldset>
                        <legend><b>Connection Details</b></legend>
                        <table style="width: 100%;">
                            <tr>
                                <td class="pageLabel">
                                    Consumer No.
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txt_cons_no" runat="server" CssClass="td_text" MaxLength="10" Width="120px"></asp:TextBox>
                                </td>
                                <td class="pageLabel">
                                    Bill Due Date
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="Bill_due_date" runat="server" CssClass="td_text" Width="120px" MaxLength="12"></asp:TextBox>
                                    <cc1:CalendarExtender ID="CalendarExtender2" runat="server" Format="dd-MMM-yyyy"
                                        TargetControlID="Bill_due_date">
                                    </cc1:CalendarExtender>
                                </td>
                                  <td class="pageLabel">
                                    Rid
                                </td>
                                <td class="pageControl">
                                    <asp:TextBox ID="txtRid" runat="server" CssClass="td_text" MaxLength="15" Width="120px" ReadOnly="true" Required="required"></asp:TextBox>
                                </td>
                                <td class="pageControl">
                                    <asp:ImageButton ID="btnSearch" runat="server" ImageUrl="~/images/View.png" OnClick="btnSearch_Click"
                                        ToolTip="Click Search Record" AccessKey="s" CausesValidation="false" Style="height: 22px" />
                                </td>
                                
                            </tr>
                            <tr>
                                <td>
                                </td>
                                <td colspan="3">
                                    <asp:LinkButton ID="link_find" CausesValidation="false" runat="server" OnClientClick="javascript:f1();">Find Consumer No ?</asp:LinkButton>
                                    <br />
                                    <br />
                                    <asp:LinkButton ID="link_cal" CausesValidation="false" runat="server" OnClick="link_cal_Click">Update Calculation Date</asp:LinkButton>
                                </td>
                                <td>
                                </td>
                            </tr>
                        </table>
                    </fieldset>
                </td>
            </tr>
            <tr>
                <td>
                    <fieldset>
                        <legend><b>Connection Details</b></legend>
                        <asp:UpdatePanel ID="UPanel1" runat="server">
                            <ContentTemplate>
                                <table style="width: 100%;">
                                    <tr>
                                        <td class="pageLabel">
                                            Bill No.
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_bill_no" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                        <td class="pageLabel">
                                            Due Date
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_due_date" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator1" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_due_date"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Consumer Name:
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_Cons_nm" runat="server" TextMode="MultiLine" CssClass="td_text"></asp:TextBox>
                                        </td>
                                        <td class="pageLabel">
                                            Flat Type:
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_Flat_type" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Sector:
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_sector" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                        <td class="pageLabel">
                                            Category
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_category" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Block No.:
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_block_no" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                        <td class="pageLabel">
                                            Bill Period From :
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_bill_from" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator12" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_bill_from"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Flat No.:
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_Flat_No" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                        <td class="pageLabel">
                                            Bill Period Upto:
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_bill_to" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator2" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_bill_to"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                   
                                </table>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </fieldset>
                </td>
            </tr>
            <tr>
                <td>
                    <fieldset>
                        <legend><b>Bill Details</b></legend>
                        <asp:UpdatePanel ID="UpdatePanel1" runat="server">
                            <ContentTemplate>
                                <table style="width: 100%;">
                                    <tr>
                                        <td class="pageLabel">
                                            Min Charge per Month(Rs.)
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_min_charges" runat="server" CssClass="td_text" AutoPostBack="True"
                                                OnTextChanged="txt_min_charges_TextChanged"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator3" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_min_charges"></asp:RequiredFieldValidator>
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_t_min_charge" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Rebate(%)
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_rebate_per" runat="server" CssClass="td_text" AutoPostBack="True"
                                                OnTextChanged="txt_rebate_per_TextChanged"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator4" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_rebate_per"></asp:RequiredFieldValidator>
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_rebate_t_amt" runat="server" CssClass="readonlytxt" ReadOnly="true"></asp:TextBox>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Bill Date:
                                        </td>
                                        <td class="pageControl">
                                            &nbsp;
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_bill_date" runat="server" CssClass="td_text"></asp:TextBox>
                                            <cc1:CalendarExtender ID="CalendarExtender1" runat="server" Format="dd-MMM-yyyy"
                                                TargetControlID="txt_bill_date">
                                            </cc1:CalendarExtender>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator5" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_bill_date"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Cess Amt.:
                                        </td>
                                        <td class="pageControl">
                                            &nbsp;
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_cess_amt" runat="server" CssClass="td_text"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator6" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_cess_amt"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Arrear Amt. Upto:
                                        </td>
                                        <td class="pageControl">
                                            &nbsp;
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_arear_amt" runat="server" CssClass="td_text"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator7" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_arear_amt"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Interest Amt. Upto:
                                        </td>
                                        <td class="pageLabel">
                                            <asp:Label ID="lblduedate30jun17" runat="server" Text=""></asp:Label>
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_intrest_amt_30JUN17" runat="server" CssClass="td_text"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator11" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_intrest_amt_30JUN17"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Interest Amt. Upto:
                                        </td>
                                        <td class="pageLabel">
                                            &nbsp;<asp:Label ID="lblduedate" runat="server" Text=""></asp:Label>
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_intrest_amt" runat="server" CssClass="td_text"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator8" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_intrest_amt"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            CGST (9%) ON :
                                        </td>
                                        <td class="pageLabel">
                                            (
                                            <asp:Label ID="lblIntUptoDate1" runat="server" Text=""></asp:Label>-
                                            <asp:Label ID="lblIntUpto30Jun171" runat="server" Text=""></asp:Label>)=
                                            <asp:Label ID="lblDiff1" runat="server" Text=""></asp:Label>
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txtCGST_int" runat="server" CssClass="td_text"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator13" runat="server" ErrorMessage="*"
                                                ControlToValidate="txtCGST_int"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            SGST (9%) ON :
                                        </td>
                                        <td class="pageLabel">
                                            (
                                            <asp:Label ID="lblIntUptoDate" runat="server" Text=""></asp:Label>-
                                            <asp:Label ID="lblIntUpto30Jun17" runat="server" Text=""></asp:Label>)=
                                            <asp:Label ID="lblDiff2" runat="server" Text=""></asp:Label>
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txtSGST_int" runat="server" CssClass="td_text"></asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator14" runat="server" ErrorMessage="*"
                                                ControlToValidate="txtSGST_int"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Other Credit (-):
                                        </td>
                                        <td class="pageControl">
                                            &nbsp;
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_adv_amt" runat="server" CssClass="td_text">0</asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator9" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_adv_amt"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                    <tr>
                                        <td class="pageLabel">
                                            Last Paid Amt:
                                        </td>
                                        <td class="pageControl">
                                            &nbsp;
                                        </td>
                                        <td class="pageControl">
                                            <asp:TextBox ID="txt_paid_amt" runat="server" CssClass="td_text">0</asp:TextBox>
                                            <asp:RequiredFieldValidator ID="RequiredFieldValidator10" runat="server" ErrorMessage="*"
                                                ControlToValidate="txt_adv_amt"></asp:RequiredFieldValidator>
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </asp:UpdatePanel></fieldset>
                </td>
            </tr>
             <tr>
                <td>
                    <fieldset>
                        <legend><b>Bank & Payment Type Details</b></legend>
                        <asp:UpdatePanel ID="UpdatePanel2" runat="server">
                            <ContentTemplate>
                                <table>
                                    <tr>
                                        <td class="pageLabel">
                                            Bank :
                                        </td>
                                        <td>
                                            <%--<asp:RadioButtonList ID="rblBank" runat="server" AutoPostBack="True">
                                                <asp:ListItem Value="HDFC18C" Selected="True">HDFC</asp:ListItem>
                                                <asp:ListItem Value="ICICI18B">ICICI</asp:ListItem>
                                                <asp:ListItem Value="KTMB18">KOTAK</asp:ListItem>
                                            </asp:RadioButtonList>--%>
                                            <asp:DropDownList ID="ddlBank" runat="server" Width="180px" Height="25px"></asp:DropDownList>
                                        </td>
                                        <td width="100">
                                        </td>
                                        <td class="pageLabel">
                                            Payment Type :
                                        </td>
                                        <td>
                                            <asp:RadioButtonList ID="rblPaymentType" runat="server" AutoPostBack="True">
                                                <asp:ListItem Value="1" Selected="True">RTGS/NEFT</asp:ListItem>
                                                <asp:ListItem Value="2">DD/Cash/Fund Transfer</asp:ListItem>
                                               <%-- <asp:ListItem Value="3">Online</asp:ListItem>--%>
                                            </asp:RadioButtonList>
                                        </td>
                                    </tr>
                                </table>
                            </ContentTemplate>
                        </asp:UpdatePanel>
                    </fieldset>
                </td>
            </tr>
            <tr>
                <td colspan="4" align="center">
                    <%--<asp:ImageButton ID="btnSave" runat="server" OnClick="btnSave_Click" Text="SAVE"/>--%>
                   <%-- <asp:ImageButton ID="ImageButton1" runat="server" Text="SAVE (GST)"/>--%>
                    <asp:Button ID="Button2" runat="server" Text="SAVE" onclick="Button2_Click" OnClientClick="return ValidateDropDownList();"/>
                     <asp:Button ID="btnSave" runat="server" Text="SAVE" OnClick="btnSave_Click"  Visible="false"/>
                     <asp:Button ID="btnSaveGST" runat="server" Text="SAVE (WITH GST)" OnClick="btnSave_Click" Visible="false"  />
                    <asp:Button ID="Button1" runat="server" Text="SAVE (GST)" onclick="Button1_Click" Visible="false" />
                    <asp:ImageButton ID="btnReset" runat="server" ImageUrl="~/images/Reset.png" CausesValidation="false"
                        OnClick="btnReset_Click" ToolTip="Click Reset Page" AccessKey="r" />
                    <asp:ImageButton ID="btnclose" runat="server" ImageUrl="~/images/Cancel.png" CausesValidation="false"
                        OnClick="btnclose_Click" ToolTip="Click Cancel" AccessKey="c" />
                    <asp:Button ID="btnPrint" runat="server" Text="Print" CausesValidation="false" ToolTip="Click Print"
                        AccessKey="p" OnClick="btnPrint_Click" Visible="false" />
                    <asp:HiddenField ID="txt_old_rate" runat="server" />
                    <asp:HiddenField ID="bill_type" runat="server" />
                    
                   <asp:HiddenField ID="hdGST" runat="server" />
                </td>
            </tr>
            <tr>
                <td>
                    <div style="text-align: center; width: 100%; height: 300px; overflow: scroll;" id="div_main"
                        runat="server">
                        <fieldset>
                            <legend>Challan View</legend>
                            <asp:GridView ID="gvParentGrid" runat="server" Width="100%" HeaderStyle-CssClass="gridHeader"
                                RowStyle-CssClass="gridRows" AutoGenerateColumns="false" DataBound="gvUserInfo_RowDataBound"
                                OnRowDataBound="gvUserInfo_RowDataBound">
                                <Columns>
                                    <asp:BoundField DataField="SNO" HeaderText="S.No." />
                                    <asp:BoundField DataField="f_year" HeaderText="Year" />
                                    <asp:BoundField DataField="BILL_PERIOD" HeaderText="Bill Period" />
                                    <asp:BoundField DataField="BillAmount" HeaderText="Rate/Month" />
                                    <asp:BoundField DataField="PaidAmount" HeaderText="Paid Amt." />
                                    <asp:BoundField DataField="PayDate" HeaderText="Pay Date" DataFormatString="{0:dd-MMM-yyyy}" />
                                    <asp:BoundField DataField="Credit" HeaderText="Credit." />
                                    <asp:BoundField DataField="Principal" HeaderText="Principal " />
                                    <asp:BoundField DataField="Intrest" HeaderText="Intrest" />
                                    <asp:BoundField DataField="ARREAR" HeaderText="Arrear" />
                                </Columns>
                                <EmptyDataTemplate>
                                    <table border="1" bordercolor="black" style="border-collapse: collapse;" align="center"
                                        width="98%">
                                        <tr class="gridHeader">
                                            <td>
                                                Year
                                            </td>
                                            <td>
                                                Bill Amt.
                                            </td>
                                            <td>
                                                Paid Amt.
                                            </td>
                                            <td>
                                                Pay Date
                                            </td>
                                            <td>
                                                Credit.
                                            </td>
                                            <td>
                                                Principal
                                            </td>
                                            <td>
                                                Intrest
                                            </td>
                                        </tr>
                                        <tr>
                                            <td colspan="8" cssclass="gridRows">
                                                Records not available
                                            </td>
                                        </tr>
                                    </table>
                                </EmptyDataTemplate>
                                <HeaderStyle CssClass="gridHeader" />
                                <PagerSettings Mode="NumericFirstLast" />
                                <RowStyle CssClass="gridRows" />
                            </asp:GridView>
                        </fieldset>
                    </div>
                </td>
            </tr>
        </table>
        
    </center>
</asp:Content>
