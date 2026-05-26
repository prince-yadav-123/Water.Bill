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

public partial class Report_Daily_Report : System.Web.UI.Page
{
    DataSet objDs;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string[] colParam = { "SL_NO,USERID" };
            string[] condParam = { "STATUS=1", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };
            UI.GeneralFunctions.CInst().DropDownFill(ddlUserID, "USER_MASTER", colParam, condParam, "USERID");
        }

    }
    protected void btnGetReport_Click(object sender, EventArgs e)
    {
        string[] para ={
                           
                            
                            "@userid,"+ ddlUserID.SelectedItem.Text.Trim(),                            
                            "@datefrom,"+txtFromDate.Text.Trim(),
                            "@dateto,"+txtToDate.Text.Trim(),
                            "@P_DEV_TYPE ,"+Session["DEV_TYPE"].ToString(),
                            "@P_OPERATION_TYPE,1"
                            };
        objDs = BAL.BAL.CInst().ProcedureCallMulti_datset("sp_DailyWorkReport", para);
        if (objDs != null)
        {
            maintable.Visible = true;
            lblChallan.Text=objDs.Tables[0].Rows[0][0].ToString();
            lblChallanUpdate.Text = objDs.Tables[1].Rows[0][0].ToString();
            lblBilling.Text = objDs.Tables[2].Rows[0][0].ToString();
            lblNewFile.Text = objDs.Tables[3].Rows[0][0].ToString();
            lblNameTransfer.Text = objDs.Tables[4].Rows[0][0].ToString();
            lblNOC.Text = objDs.Tables[5].Rows[0][0].ToString();

            if (Convert.ToString(hdFileScanning.Value) == "")
                lblFileScanning.Text = "0";
            else
                lblFileScanning.Text = Convert.ToString(hdFileScanning.Value);

            if (Convert.ToString(hdEmailQuery.Value) == "")
                lblEmailQuery.Text = "0";
            else
                lblEmailQuery.Text = Convert.ToString(hdEmailQuery.Value);

            
        }
    }

    //protected void ExportToExcel(object sender, EventArgs e)
    //{
    //    Response.Clear();
    //    Response.Buffer = true;
    //    Response.AddHeader("content-disposition", "attachment;filename=HTML.xls");
    //    Response.Charset = "";
    //    Response.ContentType = "application/vnd.ms-excel";
    //    Response.Output.Write(Request.Form[hfGridHtml.UniqueID]);
    //    Response.Flush();
    //    Response.End();
    //}

    public override void VerifyRenderingInServerForm(Control control)
    {
        /* Confirms that an HtmlForm control is rendered for the specified ASP.NET
      server control at run time. */
    }

    protected void btnExport_Click(object sender, EventArgs e)
    {
        btnExport.Visible = false;
        Response.ContentType = "application/x-msexcel"; 
        Response.AddHeader("Content-Disposition", "attachment;filename=Work Report From "+txtFromDate.Text+" To "+txtToDate.Text+" .xls");
        Response.ContentEncoding = Encoding.UTF8; 
        StringWriter tw = new StringWriter();
        HtmlTextWriter hw = new HtmlTextWriter(tw);
        maintable.RenderControl(hw);
        Response.Write(tw.ToString());
        Response.End();
    }
    protected void txtSave_Click(object sender, EventArgs e)
    {
        hdFileScanning.Value = txtFileScanning.Text;
        hdEmailQuery.Value = txtEmailQuery.Text;
    }
}
