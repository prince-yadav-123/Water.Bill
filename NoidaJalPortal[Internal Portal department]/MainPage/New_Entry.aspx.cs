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
    int total = 0;
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
                                "@P_NEWOLD,"+HiddenNew.Value,
                                "@P_USERID ,"+ Session["USERID"].ToString(),
			                    "@P_OPERATION_TYPE ,1" ,  
                            
                            };
        string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_CONSUMER_DETAILS_MASTER_temp", valparam);

        Literal lt = new Literal();
        if (returnval == "0")
        {
            lblMsg.Text = UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval));
            Reset();
            FillGrid();

        }
        else
        {
            lblMsg.Text = UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval));
            FillGrid();
        }


    }
    public void Reset()
    {
        txt_cons_no.Text = "";
        txt_Name.Text = "";
        txt_block.Text = "0";
        txt_flat_No.Text = "";
        txt_Paidamt.Text = "";
        btnSave.Enabled = true;
        txt_cons_no.Focus();

    }

    protected void btnReset_Click(object sender, ImageClickEventArgs e)
    {
        Reset();
        lblMsg.Text = "";
    }
    protected void btnclose_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Welcome.aspx");
    }
    protected void txt_cons_no_TextChanged(object sender, EventArgs e)
    {
        string[] colParam2 = new string[] { "*" };
        string[] condParam2 = new string[] { "CONS_NO='" + txt_cons_no.Text + "' and DEV_TYPE=" + ddl_divType.SelectedValue + "" };
        DataSet ds_Update = BAL.BAL.CInst().SelectTable("CONSUMER_MASTER", colParam2, condParam2, "CONSUMER_DETAILS_MASTER");
        //DataSet ds_Update1 = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_TRANS", colParam2, condParam2, "CONSUMER_DETAILS_TRANS");
        if (ds_Update != null)
        {
            if (ds_Update.Tables.Count > 0)
            {
                if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0)
                {
                    txt_Name.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["cons_nm1"].ToString();
                    txt_block.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["blk_no"].ToString();
                    txt_flat_No.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["Flat_no"].ToString();
                    //txt_Paidamt.Text = Convert.ToDateTime(ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONN_DT"].ToString()).ToString("dd-MMM-yyyy");
                    HiddenNew.Value = "O";
                    FillGrid();
                    txt_Paidamt.Focus();
                }
                else
                {
                    HiddenNew.Value = "N";
                }

            }
            else
            {
                HiddenNew.Value = "N";
            }
        }
        else
        {
            HiddenNew.Value = "N";
        }
    }

    public void FillGrid()
    {
        string[] colParam2 = new string[] { "ROW_NUMBER ( ) OVER(ORDER BY CONS_NO asc) AS 'SNo',*" };
        string[] condParam2 = new string[] { "SECTOR='" + ddlSector.SelectedValue + "' and DEV_TYPE=" + ddl_divType.SelectedValue + " and DUE_DT='" + txt_dueDate.Text + "'" };
        DataSet ds_Update = BAL.BAL.CInst().SelectTable("View_CONSUMER_DETAILS_MASTER_TEMP", colParam2, condParam2, "CONSUMER_DETAILS_MASTER");
        //DataSet ds_Update1 = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_TRANS", colParam2, condParam2, "CONSUMER_DETAILS_TRANS");
        if (ds_Update != null)
        {
            if (ds_Update.Tables.Count > 0)
            {
                if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0)
                {
                    gvChildGrid.DataSource = ds_Update;
                    gvChildGrid.DataBind();
                }
            }
        }
    }
    protected void gvChildGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            total += Convert.ToInt32(DataBinder.Eval(e.Row.DataItem, "PAID_AMT"));
        }
        if (e.Row.RowType == DataControlRowType.Footer)
        {
            Label lblamount = (Label)e.Row.FindControl("lblTotal");
            lblamount.Text = total.ToString();
            lblGrosstotal.Text = total.ToString();
        }
    }
    
}
