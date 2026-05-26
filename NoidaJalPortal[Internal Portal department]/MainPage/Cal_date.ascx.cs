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

public partial class PageButton : System.Web.UI.UserControl
{

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            txt_cal_date.Text = "";
            txt_cons_no.Text = "";
            GetRecords();
        }
        //txt_cal_date.Text = "";
        //txt_cons_no.Text = "";
        //GetRecords();
    }
    private void GetRecords()
    {
        string[] colParam2 = new string[] { "cast(CAL_DATE as datetime) as CAL_DATE" };
        string[] condParam2 = new string[] { "CONS_NO='" + Session["cons_no"].ToString() + "'" };
        DataSet ds_Update = BAL.BAL.CInst().SelectTable("VIEW_CONSUMER_DETAILS_MASTER", colParam2, condParam2, "CONSUMER_DETAILS_MASTER");
        //DataSet ds_Update1 = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_TRANS", colParam2, condParam2, "CONSUMER_DETAILS_TRANS");
        if (ds_Update != null)
        {
            if (ds_Update.Tables.Count > 0)
            {
                if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0)
                {
                    if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CAL_DATE"].ToString() == "")
                    {
                        txt_cal_date.Text = "";
                    }
                    else
                    {
                        txt_cons_no.Text = Session["cons_no"].ToString();
                        txt_cal_date.Text = Convert.ToDateTime(ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CAL_DATE"].ToString()).ToString("dd-MMM-yyyy");
                    }
                }
            }
        }
    }


    protected void btnReset_Click(object sender, ImageClickEventArgs e)
    {
        Literal lt = new Literal();
        string[] valparam = {   "@P_CONS_NO,"+txt_cons_no.Text,
		                        "@P_CAL_DATE ,"+ txt_cal_date.Text,
                                "@P_USERID ,"+ Session["USERID"].ToString(),
			                    "@P_OPERATION_TYPE ,1" 
                            
                            };
        string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_CONSUMER_CAL_DATE_UPDATE", valparam);
        if (returnval.ToString() == "0")
        {
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
            this.Controls.Add(lt);
            txt_cal_date.Text = "";
            txt_cons_no.Text = "";
            Session.Remove("cons_no");
        }
        else
        {
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
            this.Controls.Add(lt);
        }
    }
    
}
