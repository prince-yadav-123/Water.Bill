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

public partial class MainPage_Challan : System.Web.UI.Page
{
    static double totaltax = 0;
    int totaltaxno = 0;
    DataSet objDs = new DataSet();
    DataRow dr;
    //double totalchequeamount = 0;
    protected void Page_Load(object sender, EventArgs e)
    {

        if (!IsPostBack)
        {

            //fill_Grid();
            //

            string[] colParamBANK = { "ID", "PROPERTY_TYPE" };
            string[] condParamBANK = { "ID is not null" };

            UI.GeneralFunctions.CInst().DropDownFill(ddl_Con_cat, "JAL_RATE_MASTER", colParamBANK, condParamBANK, "PROPERTY_TYPE");
            string[] colParam2 = { "PIPE_SIZE", "PIPE_SIZE AS PIPE_SIZE1" };
            string[] condParam2 = { "PIPE_SIZE is not null", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };
            UI.GeneralFunctions.CInst().DropDownFill(ddl_pipe_size, "PIPE_SIZE_MASTER", colParam2, condParam2, "PIPE_SIZE");
        }




    }
    protected void btnReset_Click(object sender, ImageClickEventArgs e)
    {
        reset();
    }
    protected void btnclose_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Welcome.aspx");
    }
    protected void btnSave_Click(object sender, ImageClickEventArgs e)
    {
        Literal lt = new Literal();

        string[] valparam = {   "@P_ID,"+ddl_Con_cat.SelectedValue,			                    
                                "@P_AREA_START,"+ txt_areastart.Text,
                                "@P_AREA_END,"+ txt_areaend.Text,
                                "@P_REGULAR,"+ txt_r_rate.Text,
                                "@P_TEMPORARY,"+ txt_t_rate.Text,
                                "@P_PIPE_SIZE,"+ ddl_pipe_size.SelectedValue,
                                "@P_CESS_RATE,"+txt_cess_rate.Text,
                                "@P_EFF_FROM,"+txt_eff_date_fr.Text,
                                "@P_EFF_TO ,"+ txt_eff_date_to.Text,
                                "@P_DEV_TYPE ,"+Session["DEV_TYPE"],
                                "@P_OPERATION_TYPE ,1" 
                            
                            };
        string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_JAL_RATE_MASTER", valparam);
        if (returnval == "0")
        {
            ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), Guid.NewGuid().ToString(), "alert('Row Save Sucessfully!!!');", true);
            Fill_grid();
            reset();
        }
        else
        {
            ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), Guid.NewGuid().ToString(), "alert('Row does not Saved!!! Try Again');", true);


        }





    }
    public void reset()
    {
        txt_areaend.Text = "";
        txt_areastart.Text = "";
        txt_cess_rate.Text = "";
        txt_eff_date_fr.Text = "";
        txt_eff_date_to.Text = "";
        txt_r_rate.Text = "";
        txt_t_rate.Text = "";
    }
    protected void ddl_pipe_size_SelectedIndexChanged(object sender, EventArgs e)
    {
        Fill_grid();
        lblCtg.Text = ddl_Con_cat.SelectedItem.Text;
    }
    public void Fill_grid()
    {
        string[] colParam2 = new string[] { "*" };
        string[] condParam2 = new string[] { "ID='" + ddl_Con_cat.SelectedValue + "'", "PIPE_SIZE='" + ddl_pipe_size.SelectedValue + "'", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };
        DataSet ds_rate = BAL.BAL.CInst().SelectTable("JAL_RATE_TRANS", colParam2, condParam2, "JAL_RATE_TRANS");
        //DataSet ds_Update1 = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_TRANS", colParam2, condParam2, "CONSUMER_DETAILS_TRANS");
        if (ds_rate != null)
        {
            if (ds_rate.Tables.Count > 0)
            {
                if (ds_rate.Tables["JAL_RATE_TRANS"].Rows.Count > 0)
                {
                    gvParentGrid.DataSource = ds_rate;
                    gvParentGrid.DataBind();
                }
                else
                {
                    gvParentGrid.DataSource = "";
                    gvParentGrid.DataBind();
                }
            }
            else
            {
                gvParentGrid.DataSource = "";
                gvParentGrid.DataBind();
            }
        }
        else
        {
            gvParentGrid.DataSource = "";
            gvParentGrid.DataBind();
        }
    }
    protected void gvParentGrid_RowEditing(object sender, GridViewEditEventArgs e)
    {
        gvParentGrid.EditIndex = e.NewEditIndex;
        Fill_grid();
    }
    protected void gvParentGrid_RowDeleting(object sender, GridViewDeleteEventArgs e)
    {
        try
        {
            int sid = Convert.ToInt32(gvParentGrid.Rows[e.RowIndex].Cells[0].Text);
            int id = Convert.ToInt32(gvParentGrid.Rows[e.RowIndex].Cells[1].Text);
            string AREA_START = gvParentGrid.Rows[e.RowIndex].Cells[2].Text;
            string AREA_END = gvParentGrid.Rows[e.RowIndex].Cells[3].Text;
            string REGULAR = gvParentGrid.Rows[e.RowIndex].Cells[4].Text;
            string TEMPORARY = gvParentGrid.Rows[e.RowIndex].Cells[5].Text;
            string PIPE_SIZE = gvParentGrid.Rows[e.RowIndex].Cells[6].Text;
            string CESS_RATE = gvParentGrid.Rows[e.RowIndex].Cells[7].Text;
            string EFF_FROM = gvParentGrid.Rows[e.RowIndex].Cells[8].Text;
            string EFF_TO = "";
            if (gvParentGrid.Rows[e.RowIndex].Cells[9].Text == "&nbsp;")
            {
                EFF_TO = "";
            }
            else
            {
                EFF_TO = gvParentGrid.Rows[e.RowIndex].Cells[9].Text;
            }
            string[] valparam = {   "@P_ID,"+id,			                    
                                "@P_AREA_START,"+ AREA_START.ToString(),
                                "@P_AREA_END,"+ AREA_END.ToString(),
                                "@P_REGULAR,"+ REGULAR.ToString(),
                                "@P_TEMPORARY,"+ TEMPORARY.ToString(),
                                "@P_PIPE_SIZE,"+ PIPE_SIZE.ToString(),
                                "@P_CESS_RATE,"+CESS_RATE.ToString(),
                                "@P_EFF_FROM,"+EFF_FROM.ToString(),
                                "@P_EFF_TO ,"+ EFF_TO.ToString(),
                                "@P_SID ,"+sid,
                                "@P_DEV_TYPE ,"+Session["DEV_TYPE"],
                                "@P_OPERATION_TYPE ,3" 
                            
                            };
            string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_JAL_RATE_MASTER", valparam);
            if (returnval == "2")
            {
                ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), Guid.NewGuid().ToString(), "alert('Row Deleted Sucessfully!!!');", true);
                gvParentGrid.EditIndex = -1;
                Fill_grid();
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), Guid.NewGuid().ToString(), "alert('Row does not Delete!!! Try Again');", true);
                gvParentGrid.EditIndex = -1;
                Fill_grid();
            }
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), Guid.NewGuid().ToString(), "alert('" + ex.Message.ToString() + "');", true);
        }
        finally
        {

        }
    }
    protected void gvParentGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        try
        {
            int sid = Convert.ToInt32(gvParentGrid.Rows[e.RowIndex].Cells[0].Text);
            int id = Convert.ToInt32(gvParentGrid.Rows[e.RowIndex].Cells[1].Text);
            TextBox AREA_START = (TextBox)gvParentGrid.Rows[e.RowIndex].Cells[2].Controls[0];
            TextBox AREA_END = (TextBox)gvParentGrid.Rows[e.RowIndex].Cells[3].Controls[0];
            TextBox REGULAR = (TextBox)gvParentGrid.Rows[e.RowIndex].Cells[4].Controls[0];
            TextBox TEMPORARY = (TextBox)gvParentGrid.Rows[e.RowIndex].Cells[5].Controls[0];
            TextBox PIPE_SIZE = (TextBox)gvParentGrid.Rows[e.RowIndex].Cells[6].Controls[0];
            TextBox CESS_RATE = (TextBox)gvParentGrid.Rows[e.RowIndex].Cells[7].Controls[0];
            TextBox EFF_FROM = (TextBox)gvParentGrid.Rows[e.RowIndex].Cells[8].Controls[0];
            TextBox EFF_TO = (TextBox)gvParentGrid.Rows[e.RowIndex].Cells[9].Controls[0];
            string EFF_TO1 = "";
            if (EFF_TO.Text == "&nbsp;" || EFF_TO.Text == "")
            {
                EFF_TO1 = "";
            }
            else
            {
                EFF_TO1 = Convert.ToDateTime(EFF_TO.Text).ToString("MM/dd/yyyy");
            }

            string[] valparam = {   "@P_ID,"+id,			                    
                                "@P_AREA_START,"+ AREA_START.Text,
                                "@P_AREA_END,"+ AREA_END.Text,
                                "@P_REGULAR,"+ REGULAR.Text,
                                "@P_TEMPORARY,"+ TEMPORARY.Text,
                                "@P_PIPE_SIZE,"+ PIPE_SIZE.Text,
                                "@P_CESS_RATE,"+CESS_RATE.Text,
                                "@P_EFF_FROM,"+ Convert.ToDateTime(EFF_FROM.Text).ToString("MM/dd/yyyy"),
                                "@P_EFF_TO ,"+ EFF_TO1.ToString(),
                                "@P_SID ,"+sid,
                                "@P_DEV_TYPE ,"+Session["DEV_TYPE"],
                                "@P_OPERATION_TYPE ,2" 
                            
                            };
            string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_JAL_RATE_MASTER", valparam);
            if (returnval == "1")
            {
                ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), Guid.NewGuid().ToString(), "alert('Row updated Sucessfully!!!');", true);
                gvParentGrid.EditIndex = -1;
                Fill_grid();
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), Guid.NewGuid().ToString(), "alert('Row does not update!!! Try Again');", true);
                gvParentGrid.EditIndex = -1;
                Fill_grid();
            }
        }
        catch (Exception ex)
        {
            ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), Guid.NewGuid().ToString(), "alert('" + ex.Message.ToString() + "');", true);
        }
        finally
        {

        }
    }
    protected void gvParentGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {

        gvParentGrid.EditIndex = -1;
        Fill_grid();

    }
    protected void ddl_Con_cat_SelectedIndexChanged(object sender, EventArgs e)
    {
        ddl_pipe_size.SelectedValue = "0";
        Fill_grid();
    }
}
