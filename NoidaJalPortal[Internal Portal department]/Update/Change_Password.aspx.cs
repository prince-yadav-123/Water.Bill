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

public partial class MainPage_change_password : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void btn_Save_Click(object sender, EventArgs e)
    {
        string current_password = txt_Current_Password.Text;
        string new_password = txt_New_Password.Text;
        if (current_password==new_password)
        {
            Literal lt = new Literal();
            lt.Text = "<script>alert('Current and New Password is same. Enter a different Password')</script>";
            this.Controls.Add(lt);
        }
        else
        {
            string[] test;
            string[] valparam = new string[] {"@userid,"+Session["USERID"].ToString(),
                           "@current_password,"+txt_Current_Password.Text,
                           "@new_password,"+txt_New_Password.Text};

            string val = BAL.BAL.CInst().ProcedureCallSingleNew_string("PROC_UPDATE_PASSWORD", valparam);
            test = val.Split('~');
            if (test[0].ToString() == "1")
            {

                Literal lt = new Literal();
                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Convert.ToInt32(test[0].ToString())) + "')</script>";
                this.Controls.Add(lt);


            }
            else
            {
                Literal lt = new Literal();
                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Convert.ToInt32(test[0].ToString())) + "')</script>";
                this.Controls.Add(lt);
            }
        }
        
    }
    
    protected void btn_Cancel_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/Update/Change_Password.aspx");
    }
    protected void btnclose_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Welcome.aspx");
    }
}
