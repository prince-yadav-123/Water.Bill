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

public partial class NoidaJal_login : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        btn_close.Attributes.Add("onclick", "window.close()");
    }
    private string GetUserIP()
    {
        string ipList = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

        if (!string.IsNullOrEmpty(ipList))
        {
            return ipList.Split(',')[0];
        }

        return Request.ServerVariables["REMOTE_ADDR"];
    }
    protected void btn_login_Click(object sender, ImageClickEventArgs e)
    {
        lblerror.Text = "asjkhfdkjhaklshdfjkhaskl";
        if (txt_uid.Text.ToString().Trim() == "")
        {
            lblerror.Visible = false;
            lblerror.Visible = true;
            lblerror.Text = "Please Enter User Id";
        }
        else if (txt_pass.Text.Trim() == "")
        {
            lblerror.Visible = false;
            lblerror.Visible = true;
            lblerror.Text = "Please Enter Password";
        }


        else if (txt_uid.Text.ToString() != "" && txt_pass.Text.Trim() != "")
        {
          

            DataSet ds = new DataSet();
            string[] logColParam = { "*" };
            string[] logCondParam = { "USERID='" + txt_uid.Text.ToString() + "' and USER_PASS='" + txt_pass.Text.Trim() + "'" };
            ds = BAL.BAL.CInst().SelectTable("USER_MASTER", logColParam, logCondParam, "USER_MASTER", "USERID");
            //ds = BAL.BAL.CInst().SelectTable("Login_info", "LoginInfo");
            if (ds != null)
            {
                if (ds.Tables.Count > 0)
                {
                    if (ds.Tables["USER_MASTER"].Rows.Count > 0)
                    {

                        Session["USERID"] = ds.Tables["USER_MASTER"].Rows[0]["USERID"].ToString();
                        Session["NAME"] = ds.Tables["USER_MASTER"].Rows[0]["USER_FNAME"].ToString();
                        Session["DEV_TYPE"] = ds.Tables["USER_MASTER"].Rows[0]["DEV_TYPE"].ToString();

                        //string[] valparam = {   "@P_USER_ID,"+ Session["USERID"].ToString(),
                        //        "@P_USER_IP,"+System.Net.Dns.GetHostName(),
                        //        "@P_USER_LOGIN_TIME,"+ DateTime.Now.ToString("HH:mm:ss tt"),
                        //        "@P_USER_LOGIN_DATE,"+DateTime.Now.Date,                                
                        //        "@P_OPERATION_TYPE,1"
                            
                        //    };
                        //string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_USER_LOGIN_INFO", valparam);
                        //string[] returnval1 = returnval.Split('~');
                        //if (returnval1[0] == "0")
                        //{
                        //    Session["ID"] = returnval1[1].ToString();
                            FormsAuthentication.RedirectFromLoginPage(ds.Tables["USER_MASTER"].Rows[0]["USERID"].ToString(), false);
                        //}




                    }
                    else
                    {
                        lblerror.Visible = false;
                        lblerror.Visible = true;
                        lblerror.Text = "Invalid User ID or Password.";
                        txt_pass.Text = "";
                    }

                }
                else
                {
                    lblerror.Visible = false;
                    lblerror.Visible = true;
                    lblerror.Text = "Invalid User ID or Password.";
                    txt_pass.Text = "";
                }

            }
            else
            {
                lblerror.Visible = false;
                lblerror.Visible = true;
                lblerror.Text = "Invalid User ID or Password.";
                txt_pass.Text = "";
            }

        }
    }
}
