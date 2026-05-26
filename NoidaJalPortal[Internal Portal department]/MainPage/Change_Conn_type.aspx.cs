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

public partial class MainPage_Change_Conn_type : System.Web.UI.Page
{
    string dev_type = "";
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            string[] colParam1 = { "CON_MAIN_ID", "CON_NAME" };
            string[] condParam1 = { "CON_MAIN_ID is not null" };
            UI.GeneralFunctions.CInst().DropDownFill(ddl_Con_cat, "MASTER_CONNECTION_TYPE_DETAILS", colParam1, condParam1, "CON_ID");
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
        string[] condParam = { "CON_ID='" + ddl_Con_cat.SelectedValue + "'" };

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
            dev_type = "";
        }
        string[] colParam1 = new string[] { "count(*)" };
        string[] condParam1 = new string[] { "CONS_NO='" + txt_cons_no.Text + "'" + dev_type + "" };
        DataSet ds_Update1 = BAL.BAL.CInst().SelectTable("VIEW_CONSUMER_DETAILS_MASTER", colParam1, condParam1, "CONSUMER_DETAILS_MASTER");
        if (ds_Update1.Tables[0].Rows[0][0].ToString() != "0")
        {

            string[] colParam2 = new string[] { "*" };
            string[] condParam2 = new string[] { "CONS_NO='" + txt_cons_no.Text + "'" + dev_type + " AND CONS_CTG='T'" };
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
                        //ddl_Con_cat.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CON_TP"].ToString();
                        string[] colParam3 = { "SUB_CON_NAME", "SUB_CON_NAME as SUB_CON_NAME1" };
                        string[] condParam3 = { "CON_ID='" + ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CON_TP"].ToString() + "'" };

                        UI.GeneralFunctions.CInst().DropDownFill(ddl_flat_type, "MASTER_CONNECTION_TYPE_DETAILS_TRANS", colParam3, condParam3, "CON_ID");
                        try
                        {
                            ddl_flat_type.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["flat_type"].ToString();
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
                        // ddl_flat_type.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["FLAT_TYPE"].ToString();

                        txt_secu_amt.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["SECU"].ToString();
                        txt_est_no.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["ESTI_NO"].ToString();
                        txt_Est_amt.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["ESTI_AMT"].ToString();
                        txt_monthly_rate.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["MONTHLY_RATE"].ToString();

                        pnl_New.Enabled = true;
                        pnl_old.Enabled = true;
                        btnclose.Visible = true;
                        btnReset.Visible = true;
                        btnSave.Visible = true;

                    }
                    else
                    {
                        reset();
                        Literal lt = new Literal();
                        lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(17) + "')</script>";
                        this.Controls.Add(lt);
                    }
                }
                else
                {
                    reset();
                    Literal lt = new Literal();
                    lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(17) + "')</script>";
                    this.Controls.Add(lt);
                }
            }
            else
            {
                reset();
                Literal lt = new Literal();
                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(17) + "')</script>";
                this.Controls.Add(lt);
            }

        }
        else
        {
            reset();
            Literal lt = new Literal();
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(3) + "')</script>";
            this.Controls.Add(lt);
        }
    }
    protected void btnSave_Click(object sender, ImageClickEventArgs e)
    {
        Literal lt = new Literal();
        string[] valparam = {   "@P_CONS_NO,"+txt_cons_no.Text,			                    
                                "@P_CONS_CTG,"+ ddl_con_type.SelectedValue,
                                "@P_TYPE_CHANGE_DATE,"+ txt_change_date.Text,
                                "@P_ESTI_NO,"+ txt_est_no.Text,
                                "@P_ESTI_AMT,"+ txt_Est_amt.Text,
                                "@P_SECU,"+ txt_secu_amt.Text,
                                "@P_MONTHLY_RATE,"+txt_monthly_rate.Text,                               
                                "@P_OPERATION_TYPE ,1" 

                            };
        string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_CONSUMER_CONNECTION_TYPE_CHANGE", valparam);
        if (returnval.ToString() == "0")
        {
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
            this.Controls.Add(lt);
            reset();
        }
        else
        {
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
            this.Controls.Add(lt);
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
        ddl_con_type.SelectedValue = "R";
        txt_change_date.Text = "";
        txt_est_no.Text = "";
        txt_Est_amt.Text = "";
        txt_secu_amt.Text = "";
        txt_monthly_rate.Text = "";
    }
}
