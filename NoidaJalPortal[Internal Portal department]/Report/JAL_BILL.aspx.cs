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
using System.Collections.Generic;

public partial class Report_JAL_BILL : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string[] logColParam1 = { "*" };
        //string[] logCondParam1 = { "BILL_NO=100002" };
        string[] logCondParam1 = { "BILL_NO='" + Request.QueryString["cons_no"].ToString() + "'" };
        DataSet dszoneName1 = BAL.BAL.CInst().SelectTable("VIEW_JAL_BILL_MASTER", logColParam1, logCondParam1, "VIEW_JAL_BILL_MASTER", "CONS_NO");
        ReportDataSource rds = new ReportDataSource("JALONLINEDataSet_VIEW_JAL_BILL_MASTER", dszoneName1.Tables[0]);
        ReportViewer1.LocalReport.DataSources.Clear();
        ReportViewer1.LocalReport.DataSources.Add(rds);
        ReportViewer1.ProcessingMode = ProcessingMode.Local;
        ReportViewer1.LocalReport.ReportPath = Server.MapPath("JAL_BILL_Report.rdlc");
        ReportViewer1.LocalReport.Refresh();
    }

    protected void print()
    {
        byte[] bytes = null;
        string strDeviceInfo = "";
        string strMimeType = "";
        string strEncoding = "";
        string strExtension = "";
        string[] strStreams = null;
        Warning[] warnings = null;
        FileStream oFileStream = default(FileStream);
        List<Stream> _stream = new List<Stream>();
        try
        {
            bytes = ReportViewer1.LocalReport.Render("PDF", strDeviceInfo,out strMimeType, out strEncoding, out strExtension, out strStreams,out  warnings);

            oFileStream = new FileStream("D:\\Print.pdf", FileMode.Create);
            oFileStream.Write(bytes, 0, bytes.Length);
            _stream.Add(oFileStream);
        }
        finally
        {
            if ((oFileStream != null))
            {
                oFileStream.Close();
                oFileStream.Dispose();
            }
        }
    }
    protected void btnPrint_Click(object sender, EventArgs e)
    {
        print();
    }
}
