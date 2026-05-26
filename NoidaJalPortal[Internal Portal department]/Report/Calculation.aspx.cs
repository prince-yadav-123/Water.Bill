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

public partial class Report_Calculation : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        GetReport();
    }

    public void GetReport()
    {
        string bill_no = Session["bill_no"].ToString();
        string xml = "";
        string[] colParam2 = new string[] { "cons_no,property_no,BILL_TYPE,CONS_NM1" };
        string[] condParam2 = new string[] { "BILL_NO='" + bill_no.ToString() + "'" };
        DataSet ds_Update = BAL.BAL.CInst().SelectTable("VIEW_JAL_BILL_MASTER", colParam2, condParam2, "CONSUMER_DETAILS_MASTER");
        //DataSet ds_Update1 = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_TRANS", colParam2, condParam2, "CONSUMER_DETAILS_TRANS");
        if (ds_Update != null)
        {
            if (ds_Update.Tables.Count > 0)
            {
                if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0)
                {

                    string bill = "  <link href='../App_Themes/JalTheme/jalbutton.css' rel='stylesheet' type='text/css' /><center><table  class='td_center_reoprt' cellpadding='2' cellspacing='0'><tr><td colspan='10'><b>NEW OKHLA INDUSTRIAL DEVELOPMENT AUTHORITY<br>LEDGER FOR CONSUMER NUMBER-" + ds_Update.Tables[0].Rows[0][0].ToString() + "</td></tr>";
                    bill = bill + "<tr><td colspan='10' height='12px'></td></tr><tr><td colspan='3'>Consumer No.:" + ds_Update.Tables[0].Rows[0][0].ToString() + "</td><td colspan='4'>Consumer Name:" + ds_Update.Tables[0].Rows[0][3].ToString() + "</td><td colspan='3'>Address:" + ds_Update.Tables[0].Rows[0][1].ToString() + "</td></tr>";
                    bill = bill + "<tr><td colspan='10' height='12px'></td></tr><tr ><td class='td_row'>S#</td><td class='td_row'>Year</td><td class='td_row'>Bill Period</td><td class='td_row'>Rate/Month</td><td class='td_row'>Paid Amt.</td><td class='td_row'>Pay Date</td><td class='td_row'>Credit</td><td class='td_row'>Principal</td><td class='td_row'>Intrest</td><td class='td_row'>Arrear</td></tr>";
                    
                    string valparamFun = UI.FilterText.XFilter(ds_Update.Tables[0].Rows[0][2].ToString().ToUpper());
                    DataSet returnvalFun = BAL.BAL.CInst().FunctionCall_table("XML_TO_TABLE", valparamFun);
                    for (int i = 0; i < returnvalFun.Tables[0].Rows.Count; i++)
                    {
                        bill = bill + "<tr><td>" + returnvalFun.Tables[0].Rows[i][0] + "</td><td>" + returnvalFun.Tables[0].Rows[i][1] + "</td><td>" + returnvalFun.Tables[0].Rows[i][2] + "</td><td>" + returnvalFun.Tables[0].Rows[i][3] + "</td><td>" + returnvalFun.Tables[0].Rows[i][4] + "</td>";
                        string x = returnvalFun.Tables[0].Rows[i][5].ToString();
                        if (Convert.ToDateTime(returnvalFun.Tables[0].Rows[i][5]).ToString("dd-MMM-yyyy") == "31-Mar-1990")
                        {
                            bill = bill + "<td>--</td>";
                        }
                        else
                        {
                        bill = bill + "<td>" + Convert.ToDateTime(returnvalFun.Tables[0].Rows[i][5]).ToString("dd-MMM-yyyy") + "</td>";
                        }
                        bill = bill + "<td>" + returnvalFun.Tables[0].Rows[i][8] + "</td><td>" + returnvalFun.Tables[0].Rows[i][6] + "</td><td>" + returnvalFun.Tables[0].Rows[i][7] + "</td><td>" + returnvalFun.Tables[0].Rows[i][9] + "</td></tr>";
                    }
                    bill = bill + "</table></center>";
                    div1.InnerHtml = bill;
                }
            }
        }
    }
}
