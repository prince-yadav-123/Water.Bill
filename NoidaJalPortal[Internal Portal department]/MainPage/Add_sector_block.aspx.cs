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

public partial class MainPage_Add_sector_block : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void btnSave_Click(object sender, ImageClickEventArgs e)
    {
        if (rb_property.SelectedValue == "S")
        {
            string[] valparam = {   
                                "@P_SECTOR_1,"+txt_Sector.Text,
                                "@P_SECTOR,"+ddl_sector.SelectedValue, 
			                    "@P_BLOCK,"+ txt_block.Text,
                                "@P_USERID ,"+ Session["USERID"].ToString(),
                                "@P_DEV_TYPE ,"+Session["DEV_TYPE"],
                                "@P_OPERATION_TYPE ,1" ,  
                            
                            };
            string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_SECTOR_BLOCK", valparam);

            Literal lt = new Literal();
            if (returnval == "0")
            {
                // txt_con_no.Text = returnval1[1].ToString();
                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
                this.Controls.Add(lt);
                btnSave.Enabled = false;
            }
            else
            {
                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
                this.Controls.Add(lt);
            }
        }
        else
        {
            string[] valparam = {   
                                "@P_SECTOR_1,"+txt_Sector.Text,
                                "@P_SECTOR,"+ddl_sector.SelectedValue, 
			                    "@P_BLOCK,"+ txt_block.Text,
                                "@P_USERID ,"+ Session["USERID"].ToString(),
                                "@P_DEV_TYPE ,"+Session["DEV_TYPE"],
                                "@P_OPERATION_TYPE ,2" ,  
                            
                            };
            string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_SECTOR_BLOCK", valparam);

            Literal lt = new Literal();
            if (returnval == "0")
            {
                // txt_con_no.Text = returnval1[1].ToString();
                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
                this.Controls.Add(lt);
                btnSave.Enabled = false;
            }
            else
            {
                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
                this.Controls.Add(lt);
            }
        }
    }
    protected void btnReset_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Add_sector_block.aspx");

    }
    protected void btnclose_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Welcome.aspx");
    }

    protected void rb_property_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (rb_property.SelectedValue == "S")
        {
            tr1.Visible = true;
            tr4.Visible = true;
            tr2.Visible = false;
            tr3.Visible = false;
            txt_Sector.Text = "";
            txt_block.Text = "";
            ddl_sector.SelectedValue = "0";
            btnSave.Enabled = true;
        }
        else
        {
            string[] colParam = { "Sector_id", "Sector_id as Sector_id1" };
            string[] condParam = { "sector_no is not null", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };

            UI.GeneralFunctions.CInst().DropDownFill(ddl_sector, "Sector_Detail", colParam, condParam, "sector_no");
            tr1.Visible = false;
            tr4.Visible = true;
            tr2.Visible = true;
            tr3.Visible = true;
            txt_Sector.Text = "";
            txt_block.Text = "";
            ddl_sector.SelectedValue = "0";
            btnSave.Enabled = true;
        }
    }
}
