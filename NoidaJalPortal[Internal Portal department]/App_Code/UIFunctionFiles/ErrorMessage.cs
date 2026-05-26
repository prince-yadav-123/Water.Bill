using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Collections;
using System.Collections.Specialized;
namespace UI
{
/// <summary>
/// Summary description for ErrorMessage
/// </summary>
/// 
    public class ErrorMessage
    {
/*######################################################################################################### */                
        Hashtable ht;
        private ErrorMessage()
        {
            ht = new Hashtable();
            ht.Add(0, "Record Saved Successfully");
            ht.Add(1, "Record Updated Successfully");
            ht.Add(2, "Record Deleted Successfully");
            ht.Add(3, "Error !!! Record not Found");
            ht.Add(4, "Error !!! Record already exists");
            ht.Add(5, "Database Error !!! Record not saved");
            ht.Add(6, "Database Error !!! Record not updated");
            ht.Add(7, "Database Error !!! Record not deleted");
            ht.Add(8, "Database Error !!! Operation Type mismatch");
            ht.Add(9, "RunTime Query Error");// fires when error is generated in Query
            ht.Add(10, "SQL Error !!! Function Calling");
            ht.Add(11, "DataAdapter Error !!! Query not executed");
            ht.Add(12, "Error !!! File not Found");
            ht.Add(13, "Login Error !!! User Authentication Failed \\n Try Again");
            ht.Add(14, "Operation Successfull");
            ht.Add(15, "Already Exists");
            ht.Add(16, "Error!!!Head Amount is not equal to Challan Amount!!!!");
            ht.Add(17, "Connection Already Regular!!!!");
            ht.Add(18, "Record already added for this Date");
            ht.Add(19, "Error !!! Post not more then Total Approve Post");
            ht.Add(20, "Error !!! Approve Post are not avilable for this school");
            ht.Add(21, "Error !!! Approve Post not less then Total Post");
            ht.Add(22, "Error !!! Scheme is not Avialable");
            //
            // TODO: Add constructor logic here
            //
        }
/*######################################################################################################### */       

        public static ErrorMessage CInst()
        {
            return new ErrorMessage();
        }
/*######################################################################################################### */       
        public string CheckError(int ErrNo)
        {
            return ht[ErrNo].ToString();
        }
    }
 /*######################################################################################################### */       
}
