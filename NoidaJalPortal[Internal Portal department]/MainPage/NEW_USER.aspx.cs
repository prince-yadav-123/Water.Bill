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

public partial class MainPage_NEW_USER : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {


            string[] colParam = { "USER_ROLE", "ROLE_DIS" };
            string[] condParam = { "ROLE_ID is not null" };

            UI.GeneralFunctions.CInst().DropDownFill(ddlrole, "MASTER_USER_ROLE", colParam, condParam, "USER_ROLE");
            string[] colParam1 = { "DEPT_NAME", "DEPT_NAME AS DEPT_NAME1 " };
            string[] condParam1 = { "DEPT_ID is not null" };

            UI.GeneralFunctions.CInst().DropDownFill(ddldepartment, "MASTER_DEPT_DETAILS", colParam1, condParam1, "DEPT_ID");
        }
    }
    protected void btnSave_Click(object sender, EventArgs e)
    {
        Literal lt = new Literal();
        //@P_USER_ROLE+ cast(max(isnull(cast(substring(USERID,2,len(USERID)) as int),0)+1)as varchar(10))FROM USER_MASTER
        string[] colBank1 = { "cast(max(isnull(cast(substring(USERID,2,len(USERID)) as int),0)+1)as varchar(10))" };
        string[] condBank1 = { "sl_no is not null" };
        DataSet ds = BAL.BAL.CInst().SelectTable("USER_MASTER", colBank1, condBank1, "USER_MASTER");
        if (ds != null)
        {
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {

                    string userid = ddlrole.SelectedValue + ds.Tables[0].Rows[0][0].ToString();

                    string[] valparam = {   "@P_USER_FNAME,"+ txt_name.Text,
                                "@P_USER_depart,"+ddldepartment.SelectedValue,
	                            "@P_USER_EMAIL,"+ txt_email.Text,
                                "@P_USERID,"+userid.ToString(),
                                "@P_USER_PASS,"+txt_Cpass.Text,
                                "@P_USER_ROLE,"+ddlrole.SelectedValue,
                                "@P_USER_MOB,"+txt_mobNo.Text,
                                "@P_DEV_TYPE,"+dll_devision.SelectedValue,
                                "@P_OPERATION_TYPE,1"
                            
                            };
                    string returnval = BAL.BAL.CInst().ProcedureCallSingle("PROC_USER_MASTER", valparam);

                    if (returnval == "0")
                    {
                        lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
                        this.Controls.Add(lt);
                        btnSave.Enabled = false;
                        txt_userid.Text = userid.ToString();

                    }
                    else
                    {
                        lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
                        this.Controls.Add(lt);
                    }
                }
                else
                {
                    lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(5) + "')</script>";
                    this.Controls.Add(lt);
                }
            }
            else
            {
                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(5) + "')</script>";
                this.Controls.Add(lt);
            }
        }
        else
        {
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(5) + "')</script>";
            this.Controls.Add(lt);
        }
    }
    protected void btnReset_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/NEW_USER.aspx");
    }
    protected void btnclose_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Welcome.aspx");
    }
}
