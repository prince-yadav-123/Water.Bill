using System;
using System.Web.UI.WebControls;
using System.Data;
using System.Web.UI;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using System.Web;


public partial class MainPage_Defaulter : System.Web.UI.Page
{
    DataSet objDs;
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void Button1_Click(object sender, EventArgs e)
    {
        Literal lt = new Literal();
        string[] valparam = {   "@P_FROM_AMT,"+txt_From_amt.Text,
		                        "@P_TO_AMT ,"+ txt_To_amt.Text
                            
                            };
        objDs = BAL.BAL.CInst().ProcedureCallMulti_datset("USP_GET_DEFAULTER", valparam);
        if (objDs != null)
        {
            if (objDs.Tables.Count > 0)
            {
                    gvParentGrid.DataSource = objDs;
                    gvParentGrid.DataBind();
            }
            else
            {
                gvParentGrid.DataSource = null;
                gvParentGrid.DataBind();

            }
        }
        else
        {
            gvParentGrid.DataSource = null;
            gvParentGrid.DataBind();

        }
    }

    protected void btnExport_Click(object sender, ImageClickEventArgs e)
    {

        using (StringWriter sw = new StringWriter())
        {
            using (HtmlTextWriter hw = new HtmlTextWriter(sw))
            {
                for (int i = 0; i < gvParentGrid.Rows.Count; i++)
                {
                    GridViewRow row = gvParentGrid.Rows[i];
                }
                gvParentGrid.RenderControl(hw);
                StringReader sr = new StringReader(sw.ToString());
                Document pdfDoc = new Document(PageSize.A1, 10f, 10f, 50f, 0f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                pdfDoc.Open();

                Paragraph details = new Paragraph("Defaulter Details : - ");
                pdfDoc.Add(details);

                writer.CloseStream = false;
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);

                pdfDoc.Close();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment;filename=" + "Defaulter_List_" + DateTime.Now.ToString("dd-MMM-yyyy") + ".pdf");
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Write(pdfDoc);
                Response.End();
            }
        }

    }
    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Confirms that an HtmlForm control is rendered for the specified ASP.NET
           server control at run time. */
    }
}
