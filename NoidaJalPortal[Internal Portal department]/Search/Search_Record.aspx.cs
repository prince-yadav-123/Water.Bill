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

public partial class MainPage_Search_Record : System.Web.UI.Page
{
    DataSet objDs = new DataSet();
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {



            string[] colParam = { "Sector_id", "Sector_id as Sector_id1" };
            string[] condParam = { "sector_no is not null", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };

            UI.GeneralFunctions.CInst().DropDownFill(ddlSector, "Sector_Detail", colParam, condParam, "sector_no");
        }
    }

    //protected void rbSearch_SelectedIndexChanged(object sender, EventArgs e)
    //{
    //    if (rbSearch.SelectedValue == "P")
    //    {
    //        tr1.Visible = true;
    //        tr2.Visible = false;
    //        tr3.Visible = true;
    //    }
    //    else
    //    {
    //        tr1.Visible = false;
    //        tr2.Visible = true;
    //        tr3.Visible = true;
    //    }

    //}


    private void GetRecords()
    {
        string[] para ={
                            "@P_CONS_NO,"+txt_cons_no.Text,
                            "@p_Sector,"+ ddlSector.SelectedValue,
                            "@p_blk_no,"+ ddlBlock.SelectedValue,
                            "@p_flat_no,"+ txtFlatNo.Text,
                            "@P_DEV_TYPE ,"+Session["DEV_TYPE"].ToString() ,
                            "@P_OPERATION_TYPE,1" 
                            };
        objDs = BAL.BAL.CInst().ProcedureCallMulti_datset("PRCO_BILL_SEARCH", para);
        if (objDs != null)
        {
            if (objDs.Tables.Count > 0)
            {
                if (objDs.Tables[0].Rows.Count > 0)
                {

                    gvParentGrid.DataSource = objDs;
                    gvParentGrid.DataBind();
                    objDs.Reset();
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
        else
        {
            gvParentGrid.DataSource = null;
            gvParentGrid.DataBind();
        }
    }
    protected void gvUserInfo_RowDataBound(object sender, GridViewRowEventArgs e)
    {

        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[0].Style.Add("text-align", "center");
            //e.Row.Cells[0].Style.Add("Width", "100px");
            //e.Row.Cells[1].Style.Add("text-align", "center");
            //e.Row.Cells[2].Style.Add("text-align", "center");
            //e.Row.Cells[3].Style.Add("text-align", "center");
            //e.Row.Cells[4].Style.Add("text-align", "center");
            e.Row.Cells[6].Style.Add("text-align", "center");

            GridView gv = (GridView)e.Row.FindControl("gvChildGrid");
            string CountryId = e.Row.Cells[1].Text;
            string[] para ={
                            "@P_CONS_NO,"+CountryId,                            
                            "@P_OPERATION_TYPE,2" 
                            };
            DataSet Ds = BAL.BAL.CInst().ProcedureCallMulti_datset("PRCO_BILL_SEARCH", para);
            if (objDs != null)
            {
                if (objDs.Tables.Count > 0)
                {
                    if (objDs.Tables[0].Rows.Count > 0)
                    {
                        gv.DataSource = Ds;
                        gv.DataBind();
                    }
                }
            }

        }
    }

    protected void gvChildGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lnk = (LinkButton)e.Row.Cells[7].FindControl("linkPrint");
            lnk.CommandArgument = e.Row.Cells[2].Text;
            cons_no.Value = e.Row.Cells[1].Text;
            LinkButton lnk1 = (LinkButton)e.Row.Cells[8].FindControl("linkEdit");
            lnk1.CommandArgument = e.Row.Cells[1].Text;
            LinkButton lnk2 = (LinkButton)e.Row.Cells[9].FindControl("linkNew");
            lnk2.CommandArgument = e.Row.Cells[2].Text;

        }
    }
    protected void lbRefund_Command(object sender, CommandEventArgs e)
    {

        string x = e.CommandArgument.ToString();
        Literal lt = new Literal();
        lt.Text = "<Script>window.open('../Search/Challan.aspx?cons_no=" + cons_no.Value + "', 'mywin','left=20,top=20,width=1024,height=675,toolbar=1,scrollbars=1,resizable=1')</Script>";
        this.Controls.Add(lt);
        // Response.Redirect("~/Report/JAL_BILL_new.aspx?cons_no="+x+"");
    }
    protected void lbRefund1_Command(object sender, CommandEventArgs e)
    {
        Session["cons_no"] = e.CommandArgument.ToString();
        Literal lt = new Literal();
        lt.Text = "<Script>window.open('../Update/Generate_bill_update.aspx','_blank')</Script>";
        this.Controls.Add(lt);
        //Response.Redirect("~/Update/Generate_bill_update.aspx");
    }
    protected void lbRefund2_Command(object sender, CommandEventArgs e)
    {
        Session["bill_no"] = e.CommandArgument.ToString();
        Literal lt = new Literal();
        lt.Text = "<Script>window.open('../Report/Calculation.aspx','mywin','left=20,top=20,width=800,height=600,toolbar=1,scrollbars=1,resizable=1')</Script>";
        this.Controls.Add(lt);
        //Response.Redirect("~/MainPage/Generate_bill.aspx");
    }
    protected void btnView_Click(object sender, ImageClickEventArgs e)
    {

        GetRecords();

    }
    protected void ddlSector_SelectedIndexChanged(object sender, EventArgs e)
    {
        string[] colParam = { "block", "block as block1" };
        string[] condParam = { "sector_id='" + ddlSector.SelectedValue.ToString() + "'" ,"DEV_TYPE='" + Session["DEV_TYPE"] + "'" };
        UI.GeneralFunctions.CInst().DropDownFill(ddlBlock, "block_detail", colParam, condParam, "block");
        ddlSector.Focus();
    }
    protected void btnReset_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/Search/Search_Record.aspx");
    }
}

