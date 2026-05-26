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

public partial class MainPage_New_Department : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    protected void btnSave_Click(object sender, EventArgs e)
    {
        string returnval = "";
        if (rb_property.SelectedValue == "D")
        {
            string[] valparam = {   "@P_DEPT_ID,1",
                                    "@P_DEPT_NAME,"+txt_property_type.Text,
	                            "@P_SUB_DEPT_NAME,NA", 
                                "@P_USERID,0101",
                                "@P_OPERATION_TYPE,1"
                            
                            };
            returnval = BAL.BAL.CInst().ProcedureCallSingle("PRCO_MASTER_DEPT_DETAILS", valparam);

        }
        else
        {
            string[] valparam = {   "@P_DEPT_ID,"+ddl_department.SelectedValue,
                                    "@P_DEPT_NAME,NA",
	                            "@P_SUB_DEPT_NAME,"+txt_subpro_typr.Text, 
                                "@P_USERID,0101",
                                "@P_OPERATION_TYPE,2"
                            
                            };
            returnval = BAL.BAL.CInst().ProcedureCallSingle("PRCO_MASTER_DEPT_DETAILS", valparam);
        }

        Literal lt = new Literal();
        if (returnval == "0")
        {
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
            this.Controls.Add(lt);

        }
        else
        {
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
            this.Controls.Add(lt);
        }
    }
    protected void rb_property_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (rb_property.SelectedValue == "D")
        {
            tr1.Visible = true;
            tr4.Visible = true;
            tr2.Visible = false;
            tr3.Visible = false;
            txt_subpro_typr.Text = "";
            txt_property_type.Text = "";
            ddl_department.SelectedValue = "0";
        }
        else
        {
            string[] colParam = { "CON_ID", "CON_NAME " };
            string[] condParam = { "CON_ID is not null" };

            UI.GeneralFunctions.CInst().DropDownFill(ddl_department, "MASTER_CONNECTION_TYPE_DETAILS", colParam, condParam, "CON_ID");
            tr1.Visible = false;
            tr4.Visible = true;
            tr2.Visible = true;
            tr3.Visible = true;
            txt_subpro_typr.Text = "";
            txt_property_type.Text = "";
            ddl_department.SelectedValue = "0";
        }
    }
    protected void btn_Cancel_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/New_Department.aspx");
       
    }
    protected void btnclose_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Welcome.aspx");
    }
}
