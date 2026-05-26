using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Web.Services;

public partial class MainPage_ChallanView : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
    }
    [WebMethod]
    public static bool Getdata(string data, string ConsNo, string ChallanNo, string BillNo)
    {
        string returnval = BAL.BAL.CInst().ProcedureCallForDivDataUpdate("PRCO_JAL_PRINT_BILL_MASTER_WITH_NEW_RATE", data, ConsNo, ChallanNo, BillNo, 2);
        if (returnval == "0")
        {
            return  true;
        }
        return false;
    }
    protected void btnClose_Click(object sender, EventArgs e)
    {
        Response.Redirect("Generate_bill_With_dues_Scheme.aspx");
    }
}
