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

public partial class MainPage_Connection_transfer : System.Web.UI.Page
{
    string dev_type = "";
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {



            string[] colParam1 = { "CON_MAIN_ID", "CON_NAME" };
            string[] condParam1 = { "CON_MAIN_ID is not null", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };
            UI.GeneralFunctions.CInst().DropDownFill(ddl_Con_cat, "MASTER_CONNECTION_TYPE_DETAILS", colParam1, condParam1, "CON_ID");
            string[] colParamBANK = { "BANK_ID", "BANK_NAME+'('+ACCOUNT_NO+')' AS BANK_NAME" };
            string[] condParamBANK = { "BANK_ID is not null" };

            UI.GeneralFunctions.CInst().DropDownFill(ddl_bank_name, "JAL_BANK_MASTER", colParamBANK, condParamBANK, "BANK_ID");

        }
        if (Session["cons_no"] != "" && Session["cons_no"] != null)
        {
            txt_cons_no.Text = Session["cons_no"].ToString();
            Session["cons_no"] = "";
        }
        else
        {
            txt_cons_no.Text = txt_cons_no.Text;
        }
    }
    protected void ddl_Con_cat_SelectedIndexChanged(object sender, EventArgs e)
    {
        string[] colParam = { "SUB_CON_NAME", "SUB_CON_NAME as SUB_CON_NAME1" };
        string[] condParam = { "CON_ID='" + ddl_Con_cat.SelectedValue + "'", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };

        UI.GeneralFunctions.CInst().DropDownFill(ddl_flat_type, "MASTER_CONNECTION_TYPE_DETAILS_TRANS", colParam, condParam, "CON_ID");
    }
    protected void btnSearch_Click(object sender, ImageClickEventArgs e)
    {
        if (Session["DEV_TYPE"].ToString().Equals("4"))
        {
            dev_type = "";
        }
        else
        {
            dev_type ="";
        }

        string[] colParam2 = new string[] { "*" };
        string[] condParam2 = new string[] { "CONS_NO='" + txt_cons_no.Text + "'" + dev_type + "" };
        DataSet ds_Update = BAL.BAL.CInst().SelectTable("VIEW_CONSUMER_DETAILS_MASTER", colParam2, condParam2, "CONSUMER_DETAILS_MASTER");
        //DataSet ds_Update1 = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_TRANS", colParam2, condParam2, "CONSUMER_DETAILS_TRANS");
        if (ds_Update != null)
        {
            if (ds_Update.Tables.Count > 0)
            {
                if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0)
                {
                    txt_cons_nm.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_NM1"].ToString();
                    txt_cons_fnm.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_NM2"].ToString();
                    try
                    {
                        ddl_Con_cat.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CON_TP"].ToString();
                    }
                    catch (Exception ex)
                    {
                        ddl_Con_cat.SelectedValue = "0";

                    }
                    string[] colParam3 = { "SUB_CON_NAME", "SUB_CON_NAME as SUB_CON_NAME1" };
                    string[] condParam3 = { "CON_ID='" + ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CON_TP"].ToString() + "'", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };
                    UI.GeneralFunctions.CInst().DropDownFill(ddl_flat_type, "MASTER_CONNECTION_TYPE_DETAILS_TRANS", colParam3, condParam3, "CON_ID");
                    try
                    {
                        ddl_flat_type.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["FLAT_TYPE"].ToString();
                    }
                    catch (Exception ex)
                    {
                        ddl_flat_type.SelectedValue = "0";

                    }
                    try
                    {
                        ddl_con_type.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_CTG"].ToString();
                    }
                    catch (Exception ex)
                    {
                        ddl_con_type.SelectedValue = "0";

                    }

                    txt_phone_no.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["MOB_NO"].ToString();
                    txt_email.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["EMAIL_ID"].ToString();
                    pnl_New.Enabled = true;
                    pnl_old.Enabled = true;
                    btnclose.Visible = true;
                    btnReset.Visible = true;
                    btnSave.Visible = true;
                    txt_cons_nm_new.Focus();
                }
                else
                {
                }
            }
        }


    }
    protected void btnSave_Click(object sender, ImageClickEventArgs e)
    {
        Literal lt = new Literal();
        string[] valparam = {   "@P_CONS_NO,"+txt_cons_no.Text, 
			                    "@P_CONS_NM_NEW,"+ txt_cons_nm_new.Text,  
			                    "@P_CONS_FNM_NEW,"+ txt_cons_fnm_new.Text,
                                "@P_CONS_NM,"+txt_cons_nm.Text,
			                    "@P_CONS_FNM,"+ txt_cons_fnm.Text,
		                        "@P_TRANS_DATE,"+ txt_trans_date.Text,
		                        "@P_TRANS_AMT,"+ txt_trans_amt.Text,
                                "@P_CHALLAN_NO,"+ txt_challan_no.Text,
                                "@P_BANK_NM,"+ ddl_bank_name.SelectedValue,
                                "@P_CHALLAN_DATE,"+ txt_challan_date.Text,
                                "@P_MOB_NO,"+txt_phone_no.Text,
                                "@P_EMAIL_ID,"+txt_email.Text,
                                "@P_SECU,"+txt_seuc.Text,
		                        "@P_USERID ,"+ Session["USERID"].ToString(),
                                "@P_DEV_TYPE ,"+Session["DEV_TYPE"],
			                    "@P_OPERATION_TYPE ,1" 
                            
                            };
        string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_CONSUMER_TRANSFER", valparam);
        if (returnval.ToString() == "0")
        {
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
            this.Controls.Add(lt);
            TransferConsumerName(txt_cons_no.Text, txt_cons_nm_new.Text, txt_cons_fnm_new.Text, txt_cons_nm.Text, txt_cons_fnm.Text, Convert.ToDateTime(txt_trans_date.Text), Int32.Parse(txt_trans_amt.Text), txt_challan_no.Text, ddl_bank_name.SelectedValue, Convert.ToDateTime(txt_challan_date.Text), txt_phone_no.Text, txt_email.Text, Int32.Parse(txt_seuc.Text), Session["USERID"].ToString());
            reset();
        }
        else
        {
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
            this.Controls.Add(lt);
        }
    }
    protected void TransferConsumerName(string consno, string cnamenew, string cfnamenew, string cname, string cfname, DateTime tdate, float tamt, string challanNo, string bankname, DateTime challanDate, string phoneNo, string email, float seuc, string userid)
    {
        string value;
        try
        {
            com.noidajalonline.www.Bill_Details x = new com.noidajalonline.www.Bill_Details();
            value = x.TransferConsumerName(consno, cnamenew, cfnamenew, cname, cfname, tdate, tamt, challanNo, bankname, challanDate, phoneNo, email, seuc, userid.ToString());
        }
        catch
        {
            value = "1";
        }

    }
    protected void btnReset_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Connection_transfer.aspx");
    }
    protected void btnclose_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Welcome.aspx");
    }

    public void reset()
    {
        txt_cons_no.Text = "";
        txt_cons_nm.Text = "";
        txt_cons_fnm.Text = "";
        ddl_Con_cat.SelectedValue = "0";
        ddl_flat_type.SelectedValue = "0";
        txt_phone_no.Text = "";
        txt_email.Text = "";
        txt_cons_nm_new.Text = "";
        txt_cons_fnm_new.Text = "";
        txt_trans_date.Text = "";
        txt_trans_amt.Text = "";
        txt_challan_no.Text = "";
        ddl_bank_name.SelectedValue = "0";
        txt_challan_date.Text = "";
        txt_seuc.Text = "";
    }
}
