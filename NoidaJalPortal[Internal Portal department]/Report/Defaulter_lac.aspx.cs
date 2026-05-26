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

        string[] logColParam1 = { "*" };

        string[] logCondParam1 = { "TOTAL_BILL_AMT is not null" };
        dszoneName1 = BAL.BAL.CInst().SelectTable("default_lacs_main", logColParam1, logCondParam1, "default_lacs_main", "TOTAL_BILL_AMT desc");


        rds = new ReportDataSource("JALONLINEDataSet_default_lacs_main", dszoneName1.Tables[0]);
        ReportViewer1.LocalReport.DataSources.Clear();
        ReportViewer1.LocalReport.DataSources.Add(rds);
        ReportViewer1.ProcessingMode = ProcessingMode.Local;
        ReportViewer1.LocalReport.ReportPath = Server.MapPath("Defaulter.rdlc");
        ReportViewer1.LocalReport.Refresh();
    }
}
