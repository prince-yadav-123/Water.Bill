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
            dszoneName1 = BAL.BAL.CInst().SelectTable("VIEW_JAL_BILL_MASTER_FULL", logColParam1, logCondParam1, "VIEW_JAL_BILL_MASTER", "SECTOR,BLK_NO,FLAT_NO");
            rds = new ReportDataSource("JALONLINEDataSet_VIEW_JAL_BILL_MASTER", dszoneName1.Tables[0]);
            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(rds);
            ReportViewer1.ProcessingMode = ProcessingMode.Local;
            if(Request.QueryString["sector"].ToString().Equals("12"))
            {
                ReportViewer1.LocalReport.ReportPath = Server.MapPath("JAL_BILL_Report_NEW_for_half_12_gst.rdlc");
            }
            else
            {
                ReportViewer1.LocalReport.ReportPath = Server.MapPath("JAL_BILL_Report_NEW_for_half_gst.rdlc");
            }
            ReportViewer1.LocalReport.Refresh();
        }
        else
        {
            string[] logColParam1 = { "*" };
            ReportDataSource rds = new ReportDataSource();
            string[] logCondParam1 = { "BILL_NO='" + Request.QueryString["bill_no"].ToString() + "'" };
            dszoneName1 = BAL.BAL.CInst().SelectTable("VIEW_JAL_BILL_MASTER_FULL", logColParam1, logCondParam1, "VIEW_JAL_BILL_MASTER", "SECTOR,BLK_NO,FLAT_NO");
            rds = new ReportDataSource("JALONLINEDataSet_VIEW_JAL_BILL_MASTER", dszoneName1.Tables[0]);
            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(rds);
            ReportViewer1.ProcessingMode = ProcessingMode.Local;
            ReportViewer1.LocalReport.ReportPath = Server.MapPath("JAL_BILL_Report_NEW_for_half_gst_new.rdlc");
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
    /*Code Start for Print RDLC*/
    private Stream CreateStream(string name, string fileNameExtension, Encoding encoding, string mimeType, bool willSeek)
    {
        Stream stream = new MemoryStream();
        m_streams.Add(stream);
        return stream;
    }
    // Export the given report as an EMF (Enhanced Metafile) file.
    private void Export(LocalReport report)
    {//EMF
        string deviceInfo =
          @"<DeviceInfo>
                <OutputFormat>EMF</OutputFormat>
                <PageWidth>8.5in</PageWidth>
                <PageHeight>11in</PageHeight>
                <MarginTop>0.02in</MarginTop>
                <MarginLeft>0.02in</MarginLeft>
                <MarginRight>0.02in</MarginRight>
                <MarginBottom>0.02in</MarginBottom>
            </DeviceInfo>";
        Warning[] warnings;
        m_streams = new List<Stream>();
        report.Render("Image", deviceInfo, CreateStream, out warnings);
        foreach (Stream stream in m_streams)
            stream.Position = 0;
    }
    // Handler for PrintPageEvents
    private void PrintPage(object sender, PrintPageEventArgs ev)
    {
        Metafile pageImage = new Metafile(m_streams[m_currentPageIndex]);
        // Adjust rectangular area with printer margins.
        Rectangle adjustedRect = new Rectangle(
            ev.PageBounds.Left - (int)ev.PageSettings.HardMarginX,
            ev.PageBounds.Top - (int)ev.PageSettings.HardMarginY,
            ev.PageBounds.Width,
            ev.PageBounds.Height);
        // Draw a white background for the report
        ev.Graphics.FillRectangle(Brushes.White, adjustedRect);
        // Draw the report content
        ev.Graphics.DrawImage(pageImage, adjustedRect);
        // Prepare for the next page. Make sure we haven't hit the end.
        m_currentPageIndex++;
        ev.HasMorePages = (m_currentPageIndex < m_streams.Count);
    }
    private void Print()
    {
        if (m_streams == null || m_streams.Count == 0)
            throw new Exception("Error: no stream to print.");
        PrintDocument printDoc = new PrintDocument();
        if (!printDoc.PrinterSettings.IsValid)
        {
            throw new Exception("Error: cannot find the default printer.");
        }
        else
        {
            printDoc.PrintPage += new PrintPageEventHandler(PrintPage);
            m_currentPageIndex = 0;
            printDoc.Print();
        }
    }
    // Create a local report for Report.rdlc, load the data,
    //    export the report to an .emf file, and print it.
    protected void btnPrint_Click(object sender, EventArgs e)
    {
        int count = 0;
        string confirmValue = Request.Form["confirm_value"];
        if (confirmValue == "Yes")
        {
            for (int i = 0; i < dszoneName1.Tables[0].Rows.Count; i++)
            {
                string[] valparam = {   
                                
                                "@CONS_NO,"+dszoneName1.Tables[0].Rows[i]["CONS_NO"].ToString(),
                                "@P_OPERATION_TYPE,1"
                            
                            };
                string returnval = BAL.BAL.CInst().ProcedureCallSingle("PRCO_BILL_PRINTED", valparam);

                if (returnval == "1")
                {
                    count = count + 1;
                }

            }
            if (count == dszoneName1.Tables[0].Rows.Count)
            {
                Export(ReportViewer1.LocalReport);
                Print();
            }
            //this.Page.ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('You clicked YES!')", true);
        }
        else
        {
            for (int i = 0; i < dszoneName1.Tables[0].Rows.Count; i++)
            {
                string[] valparam = {   
                               
                                "@CONS_NO,"+dszoneName1.Tables[0].Rows[i]["CONS_NO"].ToString(),
                                "@P_OPERATION_TYPE,1"
                            
                            };
                string returnval = BAL.BAL.CInst().ProcedureCallSingle("PRCO_BILL_PRINTED", valparam);

                if (returnval == "1")
                {
                    count = count + 1;
                }

            }
            if (count == dszoneName1.Tables[0].Rows.Count)
            {
                Literal lt = new Literal();
                lt.Text = "<script>alert('" + count + " Print Waiting for Printing')</script>";
                this.Controls.Add(lt);
            }
        }


    }
    /*Code End for Print RDLC*/
}
