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

public partial class MainPage_New_Entry : System.Web.UI.Page
{
    //int total = 0;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ddlSector.Focus();
            string[] colParam = { "Sector_id", "Sector_id as Sector_id1" };
            string[] condParam = { "sector_no is not null" };

            UI.GeneralFunctions.CInst().DropDownFill(ddlSector, "Sector_Detail", colParam, condParam, "sector_no");
        }
    }

    protected void btnSave_Click(object sender, ImageClickEventArgs e)
    {

        string[] valparam = {   
                                "@P_CONS_NO,"+txt_cons_no.Text, 
		                        "@P_CONS_NAME,"+txt_Name.Text, 
		                        "@P_SECTOR ,"+ ddlSector.SelectedValue,
                                "@P_BLK_NO ,"+ txt_block.Text,
		                        "@P_FLAT_NO,"+ txt_flat_No.Text,
		                        "@P_DUE_DT,"+ txt_dueDate.Text, 
		                        "@P_PAID_AMT ,"+ txt_Paidamt.Text ,
		                        "@P_DEV_TYPE ,"+ ddl_divType.SelectedValue,
		                        "@P_DISCOUNT ,"+ ddl_diascont.SelectedValue,                                
                                "@P_USERID ,"+ Session["USERID"].ToString(),
			                    "@P_OPERATION_TYPE ,2" ,  
                            
                            };
        string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_CONSUMER_DETAILS_MASTER_temp", valparam);

        Literal lt = new Literal();
        if (returnval == "0")
        {

            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
            this.Controls.Add(lt);
            Reset();
        }
        else
        {
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
            this.Controls.Add(lt);
        }


    }
    public void Reset()
    {
        txt_cons_no.Text = "";
        txt_Name.Text = "";
        txt_block.Text = "";
        txt_flat_No.Text = "";
        txt_Paidamt.Text = "";
        btnSave.Enabled = true;
        txt_cons_no.Focus();
    }
    protected void btnReset_Click(object sender, ImageClickEventArgs e)
    {
        Reset();
    }
    protected void btnclose_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Welcome.aspx");
    }
    protected void btnSearch_Click(object sender, ImageClickEventArgs e)
    {
        string[] colParam2 = new string[] { "*" };
        string[] condParam2 = new string[] { "CONS_NO='" + txt_cons_no.Text + "'" };
        DataSet ds_Update = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_MASTER_temp", colParam2, condParam2, "CONSUMER_DETAILS_MASTER");
        //DataSet ds_Update1 = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_TRANS", colParam2, condParam2, "CONSUMER_DETAILS_TRANS");
        if (ds_Update != null)
        {
            if (ds_Update.Tables.Count > 0)
            {
                if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0)
                {
                    ddlSector.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["SECTOR"].ToString();
                    txt_dueDate.Text = Convert.ToDateTime(ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["DUE_DT"].ToString()).ToString("dd-MMM-yyyy");
                    ddl_diascont.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["DISCOUNT"].ToString();
                    ddl_divType.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["DEV_TYPE"].ToString();
                    txt_Name.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_NAME"].ToString();
                    txt_block.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["BLK_NO"].ToString();
                    txt_flat_No.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["FLAT_NO"].ToString();
                    txt_Paidamt.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["PAID_AMT"].ToString();
                    pnlMain.Enabled = true;

                }
                else
                {
                    Literal lt = new Literal();
                    lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
                    this.Controls.Add(lt);
                    pnlMain.Enabled = false;
                }
            }
            else
            {
                Literal lt = new Literal();
                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
                this.Controls.Add(lt);
                pnlMain.Enabled = false;
            }
        }
        else
        {
            Literal lt = new Literal();
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
            this.Controls.Add(lt);
            pnlMain.Enabled = false;
        }
    }
}
