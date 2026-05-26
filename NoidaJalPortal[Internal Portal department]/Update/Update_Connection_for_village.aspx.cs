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

public partial class Update_connection : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {


            string[] colParam = { "Sector_id", "Sector_id as Sector_id1" };
            string[] condParam = { "sector_no is not null", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };

            UI.GeneralFunctions.CInst().DropDownFill(ddl_Sector, "Sector_Detail", colParam, condParam, "sector_no");
            string[] colParam1 = { "CON_MAIN_ID", "CON_NAME" };
            string[] condParam1 = { "CON_MAIN_ID is not null", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };
            UI.GeneralFunctions.CInst().DropDownFill(ddl_Con_cat, "MASTER_CONNECTION_TYPE_DETAILS", colParam1, condParam1, "CON_ID");

            string[] colParam2 = { "PIPE_SIZE", "PIPE_SIZE AS PIPE_SIZE1" };
            string[] condParam2 = { "PIPE_SIZE is not null", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };
            UI.GeneralFunctions.CInst().DropDownFill(ddl_pipe_size, "PIPE_SIZE_MASTER", colParam2, condParam2, "PIPE_SIZE");

            string[] colParam21 = { "Village_id", "Village_Name " };
            string[] condParam21 = { "Village_id is not null", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };
            UI.GeneralFunctions.CInst().DropDownFill(ddl_village_name, "Village_Detail", colParam21, condParam21, "Village_Name");
        }
        if (Session["cons_no"] != "" && Session["cons_no"] != null)
        {
            txt_con_no.Text = Session["cons_no"].ToString();
            Session["cons_no"] = "";
        }
        else
        {
            txt_con_no.Text = txt_con_no.Text;
        }

    }


    protected void ddl_Sector_SelectedIndexChanged(object sender, EventArgs e)
    {
        string[] colParam = { "block", "block as block1" };
        string[] condParam = { "sector_id='" + ddl_Sector.SelectedValue.ToString() + "'", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };
        UI.GeneralFunctions.CInst().DropDownFill(ddl_Block, "block_detail", colParam, condParam, "block");
        ddl_Sector.Focus();
    }
    protected void ddl_Con_cat_SelectedIndexChanged(object sender, EventArgs e)
    {
        string[] colParam = { "SUB_CON_NAME", "SUB_CON_NAME as SUB_CON_NAME1" };
        string[] condParam = { "CON_ID='" + ddl_Con_cat.SelectedValue + "'", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };

        UI.GeneralFunctions.CInst().DropDownFill(ddl_flat_type, "MASTER_CONNECTION_TYPE_DETAILS_TRANS", colParam, condParam, "CON_ID");
        ddl_Con_cat.Focus();
    }

    protected void btnSave_Click(object sender, ImageClickEventArgs e)
    {//@P_CONS_NO
        Literal lt = new Literal();
        string[] valparam = { 
                            "@P_CONS_NO,"+txt_con_no.Text,
                            "@P_CON_TP ,"+ ddl_Con_cat.SelectedValue,
                            "@P_CONS_CTG ,"+ ddl_con_type.SelectedValue,
                            "@P_FLAT_TYPE ,"+ ddl_flat_type.SelectedValue,
                            "@P_FLAT_NO ,"+ txt_House_No.Text,
                            "@P_BLK_NO ,"+ ddl_Block.SelectedValue,
                            "@P_SECTOR ,"+ ddl_Sector.SelectedValue,
                            "@P_PLOT_SIZE,"+ txtArea.Text, 
                            "@P_PIPE_SIZE ,"+ ddl_pipe_size.SelectedValue ,
                            "@P_REG_NO ,"+ lbl_form_no.Value,
                            "@P_CONN_DT ,"+ txt_conn_date.Text,
                            "@P_ESTI_NO ,"+txt_Est_No.Text,
			                "@P_ESTI_AMT ,"+txt_Est_Amt.Text,
			                "@P_SECU ,"+txt_security.Text,
			                "@P_ESTI_DT," +txt_Est_date.Text,                                                      
                            "@P_USERID ,"+ Session["USERID"].ToString(),
                            "@P_DEV_TYPE ,"+Session["DEV_TYPE"].ToString() ,
                            "@P_CESS_AMT,"+txt_CessAmt.Text,
                            "@P_MAONTHY_CHARGES,"+txt_monthlyCharges.Text,
                            "@P_CAL_DATE,"+txt_cal_date.Text,
                            "@P_NODUES_DT,"+txt_ndc_date.Text,
                            "@P_TYPE_CHANGE_DATE,"+txt_type_change_date.Text,
                            "@P_LEDGER_DATE,"+txt_ledger_date.Text,
                            "@P_CONS_NM1,"+txt_name.Text,
                            "@P_KHASRA_NO,"+txt_khasra_no.Text,
                            "@P_VILLGAE_NAME,"+ddl_village_name.SelectedItem.Text,
                            "@P_VILLGAE_ID,"+ddl_village_name.SelectedValue,
                            "@P_OPERATION_TYPE ,1" ,  

                };
        string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_CONSUMER_DETAILS_MASTER_UPDATE_FOR_VILLAGE", valparam);
        if (returnval == "1")
        {

            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
            this.Controls.Add(lt);
            btnSave.Enabled = false;
            btnSave.ImageUrl = "~/images/Update.png";


        }
        else
        {
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
            this.Controls.Add(lt);
        }
    }
    protected void btnReset_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/Update/Update_Connection_NEW.aspx");
    }
    protected void btnclose_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Welcome.aspx");
    }
    protected void btnSearch_Click(object sender, ImageClickEventArgs e)
    {

        pnl_details.Enabled = true;
        string[] colParam2 = new string[] { "*" };
        string[] condParam2 = new string[] { "CONS_NO='" + txt_con_no.Text + "'" };
        DataSet ds_Update = BAL.BAL.CInst().SelectTable("VIEW_CONSUMER_DETAILS_MASTER", colParam2, condParam2, "CONSUMER_DETAILS_MASTER");
        //DataSet ds_Update1 = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_TRANS", colParam2, condParam2, "CONSUMER_DETAILS_TRANS");
        if (ds_Update != null)
        {
            if (ds_Update.Tables.Count > 0)
            {
                if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0)
                {
                    txt_House_No.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["FLAT_NO"].ToString();
                    lbl_form_no.Value = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["REG_NO"].ToString();
                    txt_name.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_NM1"].ToString();
                    txtArea.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["PLOT_SIZE"].ToString();
                    try
                    {
                        ddl_pipe_size.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["PIPE_SIZE"].ToString();
                    }
                    catch (Exception ex)
                    {
                        ddl_pipe_size.SelectedValue = "0";

                    }
                    txt_conn_date.Text = Convert.ToDateTime(ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONN_DT"].ToString()).ToString("dd-MMM-yyyy");


                    txt_Est_No.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["ESTI_NO"].ToString();
                    txt_Est_Amt.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["ESTI_AMT"].ToString();
                    txt_Est_date.Text = Convert.ToDateTime(ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["ESTI_DT"].ToString()).ToString("dd-MMM-yyyy");
                    txt_security.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["SECU"].ToString();



                    ddl_Sector.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["SECTOR"].ToString();
                    string[] colParam = { "block", "block as block1" };
                    string[] condParam = { "sector_id='" + ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["SECTOR"].ToString() + "'" };
                    UI.GeneralFunctions.CInst().DropDownFill(ddl_Block, "block_detail", colParam, condParam, "block");
                    ddl_Block.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["BLK_NO"].ToString();
                    ddl_con_type.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_CTG"].ToString();
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
                        ddl_flat_type.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["FLAT_TYPE"].ToString();
                    }
                    catch (Exception ex)
                    {
                        ddl_flat_type.SelectedValue = "0";

                    }
                    //ddl_flat_type.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["FLAT_TYPE"].ToString();
                    txt_CessAmt.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CESS_AMT"].ToString();
                    txt_monthlyCharges.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["MAONTHY_CHARGES"].ToString();
                    if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CAL_DATE"].ToString() == "")
                    {
                        txt_cal_date.Text = "";
                    }
                    else
                    {
                       txt_cal_date.Text = Convert.ToDateTime(ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CAL_DATE"].ToString()).ToString("dd-MMM-yyyy");
                    }
                    if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["NODUES_DT"].ToString() == "")
                    {
                        txt_ndc_date.Text = "";
                    }
                    else
                    {
                        txt_ndc_date.Text = Convert.ToDateTime(ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["NODUES_DT"].ToString()).ToString("dd-MMM-yyyy");
                    }
                    if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["LEDGER_DATE"].ToString() == "")
                    {
                        txt_ledger_date.Text = "";
                    }
                    else
                    {
                        txt_ledger_date.Text = Convert.ToDateTime(ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["LEDGER_DATE"].ToString()).ToString("dd-MMM-yyyy");
                    }
                    if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["TYPE_CHANGE_DATE"].ToString() == "")
                    {
                        txt_type_change_date.Text = "";
                    }
                    else
                    {
                        txt_type_change_date.Text = Convert.ToDateTime(ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["TYPE_CHANGE_DATE"].ToString()).ToString("dd-MMM-yyyy");
                    }
                    pnl_details.Enabled = true;
                    btnSave.Enabled = true;
                    txt_khasra_no.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["KHASRA_NO"].ToString();
                    ddl_village_name.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["VILLGAE_ID"].ToString();
                }
                else
                {
                    Literal lt = new Literal();
                    lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
                    this.Controls.Add(lt);
                    pnl_details.Enabled = false;


                }
            }
            else
            {
                Literal lt = new Literal();
                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
                this.Controls.Add(lt);
                pnl_details.Enabled = false;



            }
        }
        else
        {
            Literal lt = new Literal();
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
            this.Controls.Add(lt);
            pnl_details.Enabled = false;



        }

    }
}
