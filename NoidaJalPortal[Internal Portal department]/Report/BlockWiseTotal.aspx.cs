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

public partial class Report_BlockWiseTotal : System.Web.UI.Page
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
            gvChildGrid.DataSource = null;
            gvChildGrid.DataBind();
        }
    }
    protected void btnSearch_Click(object sender, ImageClickEventArgs e)
    {
        FillGrid();
    }
    public void FillGrid()
    {
        string[] colParam2 = new string[] { "ROW_NUMBER ( ) OVER(ORDER BY CONS_NO asc) AS 'SNo',*" };
        string[] condParam2 = new string[] { "SECTOR='" + ddlSector.SelectedValue + "' and DEV_TYPE=" + ddl_divType.SelectedValue + " and DUE_DT='" + txt_dueDate.Text + "' and blk_no='" + ddlBlock.SelectedValue + "'" };
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
                else
                {
                    gvChildGrid.DataSource = null;
                    gvChildGrid.DataBind();
                }
            }
            else
            {
                gvChildGrid.DataSource = null;
                gvChildGrid.DataBind();
            }
        }
        else
        {
            gvChildGrid.DataSource = null;
            gvChildGrid.DataBind();
        }
    }
    protected void ddlSector_SelectedIndexChanged(object sender, EventArgs e)
    {
        string[] colParam = { "blk_no", "blk_no as blk_no1" };
        string[] condParam = { "sector='" + ddlSector.SelectedValue + "' group by blk_no" };

        UI.GeneralFunctions.CInst().DropDownFill(ddlBlock, "view_CONSUMER_DETAILS_MASTER_TEMP", colParam, condParam, "blk_no");
        ddlBlock.Focus();
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
            //lblGrosstotal.Text = total.ToString();
        }
    }
}
