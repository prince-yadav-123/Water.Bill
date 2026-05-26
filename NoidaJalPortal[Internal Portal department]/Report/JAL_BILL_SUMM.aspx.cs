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

public partial class Report_JAL_BILL_SUMM : System.Web.UI.Page
{
    DataSet dszoneName1 = new DataSet();
    ReportDataSource rds = new ReportDataSource();
    string block = "";
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Request.QueryString["block"].ToString() == "0")
        {
            block = "%";
        }
        else
        {
            block = Request.QueryString["block"].ToString();
        }
        if (Request.QueryString["ID"].ToString() == "1")
        {
            string[] logColParam1 = { "*" };

            string[] logCondParam1 = { "sector='" + Request.QueryString["sector"].ToString() + "' AND (CAST(BLK_NO AS VARCHAR) LIKE '"+block+"' OR CAST(BLK_NO AS VARCHAR) IS NULL)" };
            dszoneName1 = BAL.BAL.CInst().SelectTable("VIEW_JAL_BILL_MASTER_BULK", logColParam1, logCondParam1, "VIEW_JAL_BILL_MASTER", "SECTOR,BLK_NO,FLAT_NO");
        }
        else
        {
            string[] logColParam1 = { "*" };
            string[] logCondParam1 = { "sector='" + Request.QueryString["sector"].ToString() + "' AND (CAST(BLK_NO AS VARCHAR) LIKE '" + block + "' OR CAST(BLK_NO AS VARCHAR) IS NULL) and PRINT_STATUS=1" };
            //string[] logCondParam1 = { "PRINT_STATUS=1" };
            dszoneName1 = BAL.BAL.CInst().SelectTable("VIEW_JAL_BILL_MASTER_FULL", logColParam1, logCondParam1, "VIEW_JAL_BILL_MASTER", "SECTOR,BLK_NO,FLAT_NO");
        }

        rds = new ReportDataSource("JALONLINEDataSet_VIEW_JAL_BILL_MASTER", dszoneName1.Tables[0]);
        ReportViewer1.LocalReport.DataSources.Clear();
        ReportViewer1.LocalReport.DataSources.Add(rds);
        ReportViewer1.ProcessingMode = ProcessingMode.Local;
        ReportViewer1.LocalReport.ReportPath = Server.MapPath("JAL_BILL_SUMMERY.rdlc");
        ReportViewer1.LocalReport.Refresh();
    }
}
