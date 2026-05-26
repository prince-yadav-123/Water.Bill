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

public partial class MainPage_user_attendance : System.Web.UI.Page
{
     DataSet objDs = new DataSet();
    protected void Page_Load(object sender, EventArgs e)
     {
        if (!Page.IsPostBack)
        {
            Label1.Text = DateTime.Now.ToString();
           
        }
    }

    protected void timeinbtn_Click(object sender, EventArgs e)
    {
                  string userid= Session["USERID"].ToString();
                  string username = Session["NAME"].ToString();
                  string type = "P";
                  string timein = DateTime.Now.ToString("hh:mm tt");
                  string date = DateTime.Now.ToString("yyyy/MM/dd");
                  string day = DateTime.Now.ToString("dddddddd");

                  string[] valparam = {   "@USER_ID,"+userid,
                                "@USER_NAME,"+username, 
                                "@A_TYPE,"+type,
                                "@IN_TIME,"+timein,
                                "@DATE,"+date,
                                "@DAY,"+day,
                                "@P_OPERATION_TYPE,1"
                            };
                  string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PROC_USER_ATTENDANCE", valparam);
                  if (Convert.ToInt32(returnval) == 4)
                  {
                      Response.Write("<script>alert('In Time Save Successfully.');</script>");
                  }
                  else
                  {
                      Response.Write("<script>alert(' Today In Time Allready Exist.');</script>");
                  }

    }

    protected void Timer1_Tick(object sender, EventArgs e)
    {
        Label1.Text = DateTime.Now.ToString();
    }
    protected void timeoutbtn_Click(object sender, EventArgs e)
    {
        string userid = Session["USERID"].ToString();
        string date = DateTime.Now.ToString("yyyy/MM/dd");
        string timeout= DateTime.Now.ToString("hh:mm tt");
        string[] valparam = {   "@USER_ID,"+userid, 
                                "@OUT_TIME,"+timeout,
                                "@DATE,"+date,
                                "@P_OPERATION_TYPE,2"
                            };
        string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PROC_USER_ATTENDANCE", valparam);
        if (Convert.ToInt32(returnval) == 4)
        {
            Response.Write("<script>alert('Out Time Save Successfully.');</script>");
        }
        else
        {
            Response.Write("<script>alert(' Out Time Allready Exist.');</script>");
        }
    }


    protected void viewlinkbtn_Click(object sender, EventArgs e)
    {
         Response.Redirect("view_user_attendance.aspx");
        
    }
}

