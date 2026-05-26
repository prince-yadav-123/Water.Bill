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
using System.IO;
using System.Text;
using System.Data.OleDb;

public partial class Misc_Upload_Excel : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void Button1_Click(object sender, EventArgs e)
    {
        string g = up_load();
        string ext=Path.GetExtension(g);
        int count = 0;
        StringBuilder cons = new StringBuilder();
       string connString ="";
        // string connString = ConfigurationManager.ConnectionStrings["xls"].ConnectionString;
        // Create the connection object
        if (ext.ToString().Equals(".xls"))
        {
             //connString = "Provider=Search.CollatorDSO.1;Data Source=" + g + ";Extended Properties=\"Excel 12.0;IMEX=1;HDR=YES;TypeGuessRows=0;ImportMixedTypes=Text\"";
           connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + g.ToString() + ";Extended Properties=Excel 8.0";
        }
        else
        {
            // connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + g + ";Extended Properties=\"Excel 12.0;IMEX=1;HDR=YES;TypeGuessRows=0;ImportMixedTypes=Text\"";
           connString = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + g.ToString() + ";Extended Properties=Excel 12.0";
        }
        OleDbConnection oledbConn = new OleDbConnection(connString);
        try
        {
            // Open connection
            oledbConn.Open();

            // Create OleDbCommand object and select data from worksheet Sheet1
            OleDbCommand cmd = new OleDbCommand("SELECT * FROM [Sheet1$]", oledbConn);

            // Create new OleDbDataAdapter
            OleDbDataAdapter oleda = new OleDbDataAdapter();

            oleda.SelectCommand = cmd;

            // Create a DataSet which will hold the data extracted from the worksheet.
            DataSet ds = new DataSet();

            // Fill the DataSet from the data extracted from the worksheet.
            oleda.Fill(ds, "Employees");
            for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
            {
                string[] valparam = {
                                "@CONS_NO,"+ds.Tables[0].Rows[i][0].ToString(),
                                "@P_OPERATION_TYPE,1"
                            
                            };
                string returnval = BAL.BAL.CInst().ProcedureCallSingle("PRCO_BILL_PRINTED", valparam);

                if (returnval == "1")
                {
                    count = count + 1;
                }
                else
                {
                    cons.Append(ds.Tables[0].Rows[i][0].ToString()+",");
                }

            }
            if (count == ds.Tables[0].Rows.Count)
            {
                Literal lt = new Literal();
                lt.Text = "<script>alert('" + count + " File Sucessfully Uploaded')</script>";
                this.Controls.Add(lt);
            }
            else
            {
                Literal lt = new Literal();
                lt.Text = "<script>alert('" + count + " File Sucessfully Uploaded and "+cons.ToString()+" ')</script>";
                this.Controls.Add(lt);
            }
          
        }
        catch
        {
            Literal lt = new Literal();
            lt.Text = "<script>alert('Connection Not Open')</script>";
            this.Controls.Add(lt);

        }
        finally
        {
            // Close connection
            oledbConn.Close();
        }

    }
    public string up_load()
    {
        StringBuilder sb = new StringBuilder();
        string s = "";
        if (FileUpload1.HasFile)
        {
            try
            {
                sb.AppendFormat(" Uploading file: {0}",
                                            FileUpload1.FileName);
                //saving the file
                s = Server.MapPath("../Upload").ToString();
                FileUpload1.SaveAs(s.ToString() + "/" +
                                            FileUpload1.FileName);
                //Showing the file information
                //sb.AppendFormat("<br/> Save As: {0}",
                //                   FileUpload1.PostedFile.FileName);
                //sb.AppendFormat("<br/> File type: {0}",
                //                   FileUpload1.PostedFile.ContentType);
                //sb.AppendFormat("<br/> File length: {0}",
                //                   FileUpload1.PostedFile.ContentLength);
                //sb.AppendFormat("<br/> File name: {0}",
                //                   FileUpload1.PostedFile.FileName);
                s = s.ToString() + "/" + FileUpload1.FileName;


            }
            catch (Exception ex)
            {
                sb.Append("<br/> Error <br/>");
                sb.AppendFormat("Unable to save file <br/> {0}",
                                   ex.Message);
            }
        }
        else
        {
            //  lblmessage.Text = sb.ToString();
        }
        return s.ToString();
    }
}
