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

public partial class Report_BlockWise : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string[] logColParam1 = { "*" };
        string[] logCondParam1 = { "sector='"+Request.QueryString["Sector"].ToString()+"'" };
        DataSet dszoneName1 = BAL.BAL.CInst().SelectTable("view_CONSUMER_DETAILS_MASTER_TEMP", logColParam1, logCondParam1, "VIEW_JAL_BILL_MASTER", "CONS_NO");
        ReportDataSource rds = new ReportDataSource("JALONLINEDataSet_View_CONSUMER_DETAILS_MASTER_TEMP", dszoneName1.Tables[0]);
        ReportViewer1.LocalReport.DataSources.Clear();
        ReportViewer1.LocalReport.DataSources.Add(rds);
        ReportViewer1.ProcessingMode = ProcessingMode.Local;
        ReportViewer1.LocalReport.ReportPath = Server.MapPath("BlockWiseReport.rdlc");
        ReportViewer1.LocalReport.Refresh();
    }
}
