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

public partial class Search_Search1 : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {



            string[] colParam = { "Sector_id", "Sector_id as Sector_id1" };
            string[] condParam = { "sector_no is not null", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };

            UI.GeneralFunctions.CInst().DropDownFill(ddlSector, "Sector_Detail", colParam, condParam, "CAST(ORDER_BY AS INT)");
        }
    }
    private void GetRecords()
    {
        Literal lt = new Literal();
        string[] para ={
                            "@p_cons_nm1,"+txtConsName.Text,
                            "@p_Sector,"+ddlSector.SelectedValue,
                            "@p_blk_no,"+ddlBlock.SelectedValue,
                            "@p_flat_no,"+ txtFlatNo.Text,
                            "@P_DEV_TYPE ,"+Session["DEV_TYPE"].ToString() ,
                            "@P_OPERATION_TYPE,1" 
                            };
        DataSet objDs = BAL.BAL.CInst().ProcedureCallMulti_datset("PRCO_CONSUMER_SEARCH", para);
        if (objDs != null)
        {
            if (objDs.Tables.Count > 0)
            {
                if (objDs.Tables[0].Rows.Count > 0)
                {

                    gridSearch.DataSource = objDs;
                    gridSearch.DataBind();
                    objDs.Reset();
                    trSearch.Visible = true;
                }
                else
                {
                    gridSearch.DataSource = null;
                    gridSearch.DataBind();
                    //trSearch.Visible = false;

                }
            }
            else
            {
                gridSearch.DataSource = null;
                gridSearch.DataBind();
                // trSearch.Visible = false;

            }
        }
        else
        {
            gridSearch.DataSource = null;
            gridSearch.DataBind();
            //trSearch.Visible = false;

        }
    }
    protected void btnView_Click(object sender, EventArgs e)
    {
        GetRecords();
    }
    protected void gridSearch_PageIndexChanging(object sender, GridViewPageEventArgs e)
    {
        gridSearch.PageIndex = e.NewPageIndex;
        GetRecords();
    }
    protected void linkCons_Click(object sender, EventArgs e)
    {
        LinkButton link = sender as LinkButton;
        Session["cons_no"] = link.Text;
        //Response.Redirect("~/MainPage/Challan_Entry.aspx" +  + "&id=1");
    }
    protected void gridSearch_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        
    }
    protected void ddlSector_SelectedIndexChanged(object sender, EventArgs e)
    {
        string[] colParam = { "block", "block as block1" };
        string[] condParam = { "sector_id='" + ddlSector.SelectedValue.ToString() + "'", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };
        UI.GeneralFunctions.CInst().DropDownFill(ddlBlock, "block_detail", colParam, condParam, "block");
        ddlSector.Focus();
    }
}
