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
        string sector_no = "";
        string sector_no_gp = "";
        if (Request.QueryString["sector"].ToString() == "A")
        {
            sector_no = "sector is not null";
            sector_no_gp = "sector is not null group by conn_type";
        }
        else
        {
            sector_no = "sector='" + Request.QueryString["sector"].ToString() + "'";
            sector_no_gp = "sector='" + Request.QueryString["sector"].ToString() + "' group by conn_type";
        }

        string[] colParam2 = new string[] { "ROW_NUMBER () OVER(ORDER BY total_bill_amt desc) AS 'SNO',cons_no,property_no,cons_nm1,total_bill_amt,upper(conn_type)" };
        string[] condParam2 = new string[] { sector_no };
        DataSet ds_Update = BAL.BAL.CInst().SelectTable("default_lacs_main", colParam2, condParam2, "CONSUMER_DETAILS_MASTER", "TOTAL_BILL_AMT desc");
        //DataSet ds_Update1 = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_TRANS", colParam2, condParam2, "CONSUMER_DETAILS_TRANS");
        if (ds_Update != null)
        {
            if (ds_Update.Tables.Count > 0)
            {
                if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0)
                {

                    string bill = "  <link href='../App_Themes/JalTheme/jalbutton.css' rel='stylesheet' type='text/css' /><center><table  class='td_center_reoprt' cellpadding='2' cellspacing='0'><tr><td colspan='6'  style='font-family:Arial;font-size:16px;text-align:center;'>New Okhla Industrial Development Authority(Jal-" + Session["DEV_TYPE"].ToString()+ ")<br>Office of The Project Engineer <br>Noida, Gautam Budh Nagar, U.P.</td></tr>";
                    bill = bill + "<tr><td colspan='6' height='12px'></td></tr><tr ><td class='td_row2'>S#</td><td class='td_row2'>Consumer No</td><td class='td_row2'>Property No</td><td class='td_row2'>Name</td><td class='td_row2'>Amount (Rs.)</td><td class='td_row1'>Property Type</td></tr>";

                    for (int i = 0; i < ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count; i++)
                    {
                        bill = bill + "<tr><td class='td_row_center'>" + ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[i][0] + "</td><td class='td_row_left'>" + ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[i][1] + "</td ><td class='td_row_left'>" + ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[i][2] + "</td><td class='td_row_left'>" + ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[i][3] + "</td><td class='td_row_center'>" + ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[i][4] + "</td>";

                        bill = bill + "<td class='td_row_right'>" + ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[i][5] + "</td></tr>";
                    }
                    string[] colParamtotal = new string[] { "sum(total_bill_amt)" };
                    string[] condParamtotal = new string[] { sector_no };
                    DataSet ds_Total_amt = BAL.BAL.CInst().SelectTable("default_lacs_main", colParamtotal, condParamtotal, "CONSUMER_DETAILS_MASTER");
                    bill = bill + "</tr><tr ><td class='td_row_center' colspan='4'> <b>Total Amount</b> </td><td class='td_row_center'>" + ds_Total_amt.Tables["CONSUMER_DETAILS_MASTER"].Rows[0][0] + "</td><td class='td_row_right'>&nbsp;</td></tr>";

                    string[] colParamcount = new string[] { "upper(conn_type) as conn_type ,count(*) as count, sum(total_bill_amt) as amount" };
                    string[] condParamcount = new string[] { sector_no_gp };
                    DataSet ds_count_amt = BAL.BAL.CInst().SelectTable("default_lacs_main", colParamcount, condParamcount, "default_lacs_main");
                    bill = bill + "</tr><tr ><td  colspan='6'>&nbsp;</td></tr><tr ><td class='tbl_summery' colspan='6'> <b>Summary</b></> </td></tr>";
                    bill = bill + "</tr><tr ><td  colspan='6'> <table style='width:600px;' class='td_row3' cellpadding='0' cellspacing='0'><tr ><td class='tbl_sum' >  &nbsp; </td>";

                    for (int i = 0; i < ds_count_amt.Tables["default_lacs_main"].Rows.Count; i++)
                    {
                        bill = bill + "<td class='tbl_sum'>" + ds_count_amt.Tables["default_lacs_main"].Rows[i]["conn_type"].ToString() + "</td>";
                    }

                    bill = bill + "</tr><tr ><td  class='tbl_sum'> No Of Property	</td>";
                    for (int i = 0; i < ds_count_amt.Tables["default_lacs_main"].Rows.Count; i++)
                    {
                        bill = bill + "<td class='tbl_sum'>" + ds_count_amt.Tables["default_lacs_main"].Rows[i]["count"].ToString() + "</td>";
                    }

                    bill = bill + "</tr><tr ><td  class='tbl_sum1'> Amount	</td>";
                    for (int i = 0; i < ds_count_amt.Tables["default_lacs_main"].Rows.Count; i++)
                    {
                        bill = bill + "<td class='tbl_sum1'>" + ds_count_amt.Tables["default_lacs_main"].Rows[i]["amount"].ToString() + "</td>";
                    }
                    bill = bill + "</tr></table></tr></table></center>";
                    div1.InnerHtml = bill;
                }
            }
        }
    }
}
