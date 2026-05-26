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
using Microsoft.Reporting.WebForms;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Text;
using System.Collections.Generic;


public partial class Report_JAL_BILL_NEW : System.Web.UI.Page
{

    private int m_currentPageIndex;
    private IList<Stream> m_streams;
    string block = "";
    DataSet dszoneName1 = new DataSet();
    protected void Page_Load(object sender, EventArgs e)
    {
        string bill_type = "";
        if (Request.QueryString["block"].ToString() == "0")
        {
            block = "%";
        }
        else
        {
            block = Request.QueryString["block"].ToString();
        }
        if (Request.QueryString["bill_no"].ToString().Equals("0"))
        {
            string[] logColParam1 = { "*" };
            ReportDataSource rds = new ReportDataSource();
            string[] logCondParam1 = { "sector='" + Request.QueryString["sector"].ToString() + "'", "blk_no like'" + block + "'" };
            dszoneName1 = BAL.BAL.CInst().SelectTable("VIEW_JAL_BILL_MASTER_FULL", logColParam1, logCondParam1, "VIEW_JAL_BILL_MASTER", "CONS_NO,BLK_NO,FLAT_NO");
            rds = new ReportDataSource("JALONLINEDataSet_VIEW_JAL_BILL_MASTER", dszoneName1.Tables[0]);
            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(rds);
            ReportViewer1.ProcessingMode = ProcessingMode.Local;
            ReportViewer1.LocalReport.ReportPath = Server.MapPath("JAL_BILL_Report_NEW_for_Scheme.rdlc");
            ReportViewer1.LocalReport.Refresh();
        }
        else
        {
            string[] logColParam1 = { "*" };
            ReportDataSource rds = new ReportDataSource();
            string[] logCondParam1 = { "BILL_NO='" + Request.QueryString["bill_no"].ToString() + "'" };
            dszoneName1 = BAL.BAL.CInst().SelectTable("VIEW_JAL_BILL_MASTER_FULL", logColParam1, logCondParam1, "VIEW_JAL_BILL_MASTER", "CONS_NO,BLK_NO,FLAT_NO");
            rds = new ReportDataSource("JALONLINEDataSet_VIEW_JAL_BILL_MASTER", dszoneName1.Tables[0]);
            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(rds);
            ReportViewer1.ProcessingMode = ProcessingMode.Local;
            ReportViewer1.LocalReport.ReportPath = Server.MapPath("JAL_BILL_Report_NEW_for_Scheme.rdlc");
            ReportViewer1.LocalReport.Refresh();
        }
    }
    public string get_BillType()
    {
        string cons_no = "";
        string[] colParam2 = new string[] { "BILL_TYPE" };
        string[] condParam2 = new string[] { "BILL_NO='" + Request.QueryString["bill_no"].ToString() + "'" };
        DataSet ds_Update = BAL.BAL.CInst().SelectTable("JAL_PRINT_BILL_MASTER", colParam2, condParam2, "CONSUMER_DETAILS_MASTER");
        if (ds_Update != null)
        {
            if (ds_Update.Tables.Count > 0)
            {
                if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0)
                {
                    cons_no = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0][0].ToString();
                }
            }
        }

        return cons_no;
    }
   
}
