using System;
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

public partial class _Default : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void Button1_Click(object sender, EventArgs e)
    {
       string g=up_load();
       // string connString = ConfigurationManager.ConnectionStrings["xls"].ConnectionString;
        // Create the connection object
       string connString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + g.ToString() + ";Extended Properties=Excel 8.0";
        OleDbConnection oledbConn = new OleDbConnection(connString);
        try
        {
            // Open connection
            oledbConn.Open();

            // Create OleDbCommand object and select data from worksheet Sheet1
            OleDbCommand cmd = new OleDbCommand("SELECT * FROM [PRINT$]", oledbConn);

            // Create new OleDbDataAdapter
            OleDbDataAdapter oleda = new OleDbDataAdapter();

            oleda.SelectCommand = cmd;

            // Create a DataSet which will hold the data extracted from the worksheet.
            DataSet ds = new DataSet();

            // Fill the DataSet from the data extracted from the worksheet.
            oleda.Fill(ds, "Employees");

            // Bind the data to the GridView
            //GridView1.DataSource = ds.Tables[0].DefaultView;
            //GridView1.DataBind();
        }
        catch
        {
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
                s = Server.MapPath("Upload").ToString();
                FileUpload1.SaveAs(s.ToString()+"/"+
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
