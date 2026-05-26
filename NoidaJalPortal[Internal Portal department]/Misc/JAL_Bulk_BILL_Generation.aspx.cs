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

public partial class Report_Print_bill : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string[] colParam = { "Sector_id", "Sector_id as Sector_id1" };
            string[] condParam = { "sector_no is not null" };

            UI.GeneralFunctions.CInst().DropDownFill(ddl_Sector, "Sector_Detail", colParam, condParam, "sector_no");
        }
    }
    protected void btnView_Click(object sender, ImageClickEventArgs e)
    {
        Literal lt = new Literal();
        trShow.Visible = true;

        string[] valparam = {   "@P_SECTOR,"+ddl_Sector.SelectedValue,			                    
                                "@P_BLK_NO,"+ ddl_block.SelectedValue,
                                "@BILL_DUE_DATE,"+ txt_due_date.Text,
                                "@P_DEV_TYPE ,"+Session["DEV_TYPE"].ToString(),
                                "@P_USERID ,"+ Session["USERID"].ToString(),
                                "@P_OPERATION_TYPE ,1" 
                            
                            };
        string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_BULK_BILL_GENERATION", valparam);
        if (returnval == "1")
        {
            ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), Guid.NewGuid().ToString(), "alert('Row Save Sucessfully!!!');", true);
            trShow.Visible = false;
        }
        else
        {
            ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), Guid.NewGuid().ToString(), "alert('Row does not Saved!!! Try Again');", true);
            trShow.Visible = false;
        }
    }
    protected void ddl_Sector_SelectedIndexChanged(object sender, EventArgs e)
    {
        string[] colParam = { "block", "block as block1" };
        string[] condParam = { "sector_id='" + ddl_Sector.SelectedValue.ToString() + "'" };
        UI.GeneralFunctions.CInst().DropDownFill(ddl_block, "block_detail", colParam, condParam, "block");
    }
}
