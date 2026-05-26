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

    protected void chk_Doc_Provided_SelectedIndexChanged(object sender, EventArgs e)
    {
        for (int i = 0; i < chk_Doc_Provided.Items.Count; i++)
        {

            bool value_check = chk_Doc_Provided.Items[i].Selected;
            string checktext = chk_Doc_Provided.Items[i].Value;
            Control c1 = new Control();
            c1 = div_main.FindControl(checktext);
            Type type1 = c1.GetType();
            TextBox t1 = new TextBox();
            RadioButtonList r1 = new RadioButtonList();

            if (type1.Name == "TextBox")
            {
                t1 = (TextBox)c1;
            }
            if (type1.Name == "RadioButtonList")
            {
                r1 = (RadioButtonList)c1;
            }


            if (value_check == true)
            {
                if (type1.Name == "TextBox")
                {
                    t1.Enabled = true;

                }
                else
                {
                    r1.Enabled = true;
                }
            }
            else
            {
                if (type1.Name == "TextBox")
                {
                    t1.Enabled = false;
                    t1.Text = "";
                }
                else
                {
                    r1.Enabled = false;
                }
            }
            //if (chk_Doc_Provided.SelectedValue == "1")
            //{
            //    txt_Compliance_Date.Enabled = true;
            //}
            //else
            //{
            //    txt_Compliance_Date.Enabled = false;
            //}
            //if (chk_Doc_Provided.SelectedValue == "2")
            //{
            //    txt_Possession_Date.Enabled = true;
            //}
            //else
            //{
            //    txt_Possession_Date.Enabled = false;
            //}
            //if (chk_Doc_Provided.SelectedValue == "3")
            //{
            //    txt_SSI_Date.Enabled = true;
            //}
            //else
            //{
            //    txt_SSI_Date.Enabled = false;
            //}
        }


    }
    protected void rdb_Previous_Conn_SelectedIndexChanged(object sender, EventArgs e)
    {
        //if (rdb_Previous_Conn.SelectedValue == "Y")
        //{
        //    txt_Other_Conn_Details.Enabled = true;
        //    txt_Other_Conn_Details.Text = "";
        //}
        //else
        //{
        //    txt_Other_Conn_Details.Enabled = false;
        //    txt_Other_Conn_Details.Text = "NONE";
        //}
    }
    protected void btnFind_Click(object sender, EventArgs e)
    {
        // txt_con_no.Text = txt_cons_no.Text;

        string[] colBank1 = { "*" };
        string[] condBank1 = { "CONS_NO='23423'" };
        DataSet empadDs1 = BAL.BAL.CInst().SelectTable("CONSUMER_MASTER", colBank1, condBank1, "CONSUMER_MASTER", "CONS_NO");

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
    protected void btnSearch_Click(object sender, EventArgs e)
    {



    }
    protected void btnSave_Click(object sender, ImageClickEventArgs e)
    {//@P_CONS_NO
        Literal lt = new Literal();
        try
        {
            if (txt_Possession_Date.Text != "")
            {
                DateTime dt = DateTime.Parse(txt_Possession_Date.Text);
            }
            if (txt_Allotment_Date.Text != "")
            {
                DateTime dt1 = DateTime.Parse(txt_Allotment_Date.Text);
            }
            if (txt_Est_date.Text != "")
            {
                DateTime dt2 = DateTime.Parse(txt_Est_date.Text);
            }
            if (txt_SSI_Date.Text != "")
            {
                DateTime dt3 = DateTime.Parse(txt_SSI_Date.Text);
            }
            if (txt_Compliance_Date.Text != "")
            {
                DateTime dt4 = DateTime.Parse(txt_Compliance_Date.Text);
            }
            if (txt_conn_date.Text != "")
            {
                DateTime dt5 = DateTime.Parse(txt_conn_date.Text);
            }
            
            string[] valparam = { 
                            "@P_CONS_NO,"+txt_con_no.Text,
                            "@P_CONS_NM1,"+txt_Name.Text, 
                            "@P_MOB_NO,"+ txt_Mobile_No.Text,  
                            "@P_EMAIL_ID,"+ txt_email.Text, 
                            "@P_CONS_ADDRESS,"+ txt_Address.Text,
                            "@P_CONS_NM2 ,"+ txt_Fname.Text,
                            "@P_CON_TP ,"+ ddl_Con_cat.SelectedValue,
                            "@P_CONS_CTG ,"+ ddl_con_type.SelectedValue,
                            "@P_FLAT_TYPE ,"+ ddl_flat_type.SelectedValue,
                            "@P_FLAT_NO ,"+ txt_House_No.Text,
                            "@P_BLK_NO ,"+ ddl_Block.SelectedValue,
                            "@P_SECTOR ,"+ ddl_Sector.SelectedValue,
                            "@P_PLOT_SIZE,"+ txtArea.Text, 
                            "@P_PIPE_SIZE ,"+ ddl_pipe_size.SelectedValue ,
                            "@P_REG_NO ,"+ lbl_form_no.Text,
                            "@P_CONN_DT ,"+ txt_conn_date.Text,
                            "@P_ESTI_NO ,"+txt_Est_No.Text,
			                "@P_ESTI_AMT ,"+txt_Est_Amt.Text,
			                "@P_SECU ,"+txt_security.Text,
			                "@P_ESTI_DT," +txt_Est_date.Text,
                            "@P_OTHER_CON ,"+ txt_Other_Conn_Details.Text ,
                            "@P_ISSUE_OFFICER ,"+ txt_Officer_Detail.Text,
                            "@P_PURPOSE_CON ,"+ txt_Purpose.Text,
                            "@P_CAL_DATE ,"+ txt_conn_date.Text,
                            "@P_ALLOT_DATE ,"+ txt_Allotment_Date.Text ,
                            "@P_POS_DATE ,"+ txt_Possession_Date.Text,
                            "@P_COMP_DATE ,"+ txt_Compliance_Date.Text,
                            "@P_SSI_DATE ,"+ txt_SSI_Date.Text,
                            "@P_AFFIDAVIT_YN ,"+ rbaffyn.SelectedValue,
                            "@P_USERID ,"+ Session["USERID"].ToString(),
                            "@P_DEV_TYPE ,"+Session["DEV_TYPE"].ToString() ,
                            "@P_CESS_AMT,"+txt_CessAmt.Text,
                            "@P_Rid,"+txtrid.Text,
                            "@P_MAONTHY_CHARGES,"+txt_monthlyCharges.Text,
                            "@P_OPERATION_TYPE ,2" ,  

                };
            string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_CONSUMER_DETAILS_MASTER", valparam);
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
        catch
        {
            lt.Text = "<script>alert('Date is not in valid Format!!!!')</script>";
            this.Controls.Add(lt);
        }
    }
    protected void btnReset_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/Update/Update_Connection.aspx");
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
                    lbl_form_no.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["REG_NO"].ToString();
                    txtArea.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["PLOT_SIZE"].ToString();
                    try
                    {
                        ddl_pipe_size.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["PIPE_SIZE"].ToString();
                    }
                    catch (Exception ex)
                    {
                        ddl_pipe_size.SelectedValue = "0";

                    }
                    //ddl_pipe_size.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["PIPE_SIZE"].ToString();
                    txt_conn_date.Text = Convert.ToDateTime(ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONN_DT"].ToString()).ToString("dd-MMM-yyyy");

                    txtrid.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["Rid"].ToString();
                    txt_Est_No.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["ESTI_NO"].ToString();
                    txt_Est_Amt.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["ESTI_AMT"].ToString();
                    txt_Est_date.Text = Convert.ToDateTime(ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["ESTI_DT"].ToString()).ToString("dd-MMM-yyyy");
                    txt_security.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["SECU"].ToString();

                    if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["OTHER_CON"].ToString() == "NONE")
                    {
                        rdb_Previous_Conn.SelectedValue = "N";
                        txt_Other_Conn_Details.Enabled = false;
                    }
                    else
                    {
                        rdb_Previous_Conn.SelectedValue = "Y";
                        txt_Other_Conn_Details.Enabled = true;
                    }
                    txt_Other_Conn_Details.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["OTHER_CON"].ToString();
                    txt_Allotment_Date.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["ALLOT_DATE"].ToString();
                    txt_Compliance_Date.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["COMP_DATE"].ToString();
                    txt_Possession_Date.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["POS_DATE"].ToString();
                    txt_SSI_Date.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["SSI_DATE"].ToString();
                    rbaffyn.SelectedValue = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["AFFIDAVIT_YN"].ToString();
                    txt_Officer_Detail.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["ISSUE_OFFICER"].ToString();
                    txt_Purpose.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["PURPOSE_CON"].ToString();
                    txt_Name.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_NM1"].ToString().Trim();
                    txt_Fname.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_NM2"].ToString().Trim();
                    txt_Mobile_No.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["MOB_NO"].ToString().Trim();
                    txt_Address.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_ADDRESS"].ToString().Trim();
                    txt_email.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["EMAIL_ID"].ToString().Trim();
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
                    if (txt_Other_Conn_Details.Text == "NONE")
                    {
                        rdb_Previous_Conn.SelectedValue = "N";
                    }
                    else
                    {
                        rdb_Previous_Conn.SelectedValue = "Y";
                    }
                    if (txt_Allotment_Date.Text == "")
                    {
                        txt_Allotment_Date.Enabled = false;
                        chk_Doc_Provided.Items[0].Selected = false;
                    }
                    else
                    {
                        txt_Allotment_Date.Enabled = true;
                        chk_Doc_Provided.Items[0].Selected = true;
                    }
                    if (txt_Compliance_Date.Text == "")
                    {
                        txt_Compliance_Date.Enabled = false;
                        chk_Doc_Provided.Items[1].Selected = false;
                    }
                    else
                    {
                        txt_Compliance_Date.Enabled = true;
                        chk_Doc_Provided.Items[1].Selected = true;
                    }
                    if (txt_Possession_Date.Text == "")
                    {
                        txt_Possession_Date.Enabled = false;
                        chk_Doc_Provided.Items[2].Selected = false;
                    }
                    else
                    {
                        txt_Possession_Date.Enabled = true;
                        chk_Doc_Provided.Items[2].Selected = true;
                    }
                    if (txt_SSI_Date.Text == "")
                    {
                        txt_SSI_Date.Enabled = false;
                        chk_Doc_Provided.Items[3].Selected = false;
                    }
                    else
                    {
                        txt_SSI_Date.Enabled = true;
                        chk_Doc_Provided.Items[3].Selected = true;
                    }
                    if (txt_SSI_Date.Text == "")
                    {
                        txt_SSI_Date.Enabled = false;
                        chk_Doc_Provided.Items[3].Selected = false;
                    }
                    else
                    {
                        txt_SSI_Date.Enabled = true;
                        chk_Doc_Provided.Items[3].Selected = true;
                    }
                    if (rbaffyn.SelectedValue == "1")
                    {
                        chk_Doc_Provided.Items[4].Selected = true;
                    }
                    else
                    {
                        chk_Doc_Provided.Items[4].Selected = false;
                    }
                    pnl_details.Enabled = true;
                    btnSave.Enabled = true;

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
