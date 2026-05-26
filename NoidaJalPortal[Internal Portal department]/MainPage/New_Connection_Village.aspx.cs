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

public partial class New_Connection : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        txt_form_no.Focus();

        if (!IsPostBack)
        {


            string[] colParam = { "Sector_id", "Sector_id as Sector_id1" };
            string[] condParam = { "sector_no is not null ", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };

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
                    r1.SelectedValue = "Y";
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
                    r1.SelectedValue = "N";
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
        if (rdb_Previous_Conn.SelectedValue == "Y")
        {
            txt_Other_Conn_Details.Enabled = true;
            txt_Other_Conn_Details.SelectedValue = "0";

            string[] colBank1 = { "count(*)" };
            string[] condBank1 = { "sector='" + ddl_Sector.SelectedValue + "' and BLK_NO='" + ddl_Block.SelectedValue + "' and FLAT_NO='" + txt_House_No.Text + "'", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };
            DataSet empadDs1 = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_MASTER", colBank1, condBank1, "CONSUMER_DETAILS_MASTER");
            if (empadDs1 != null)
            {
                if (empadDs1.Tables.Count > 0)
                {
                    if (empadDs1.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0)
                    {

                        txt_Other_Conn_Details.SelectedIndex = Convert.ToInt32(empadDs1.Tables["CONSUMER_DETAILS_MASTER"].Rows[0][0].ToString());
                        txt_Other_Conn_Details.Enabled = false;
                    }
                    else
                    {
                        txt_Other_Conn_Details.SelectedIndex = 0;
                    }
                }
                else
                {
                    txt_Other_Conn_Details.SelectedIndex = 0;
                }
            }
            else
            {
                txt_Other_Conn_Details.SelectedIndex = 0;
            }
        }
        else
        {
            txt_Other_Conn_Details.Enabled = false;
            txt_Other_Conn_Details.SelectedValue = "0";
        }
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

    protected void btnclose_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/MainPage/Welcome.aspx");
    }
    protected void btnReset_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/New_Connection_Village.aspx");
    }
    protected void btnSave_Click(object sender, ImageClickEventArgs e)
    {
        Literal lt = new Literal();
        foreach (string key in Request.Form.Keys)
        {
            if (key.Contains("txt_conn_date"))
            {
                txt_conn_date.Text = Request.Form[key];

            }
            if (key.Contains("txt_Allotment_Date"))
            {
                txt_Allotment_Date.Text = Request.Form[key];

            }


            if (key.Contains("txt_Compliance_Date"))
            {
                txt_Compliance_Date.Text = Request.Form[key];

            }
            if (key.Contains("txt_Possession_Date"))
            {
                txt_Possession_Date.Text = Request.Form[key];

            }
            if (key.Contains("txt_SSI_Date"))
            {
                txt_SSI_Date.Text = Request.Form[key];

            }
            if (key.Contains("txt_Est_date"))
            {
                txt_Est_date.Text = Request.Form[key];

            }


        }

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






            string[] valparam = {   "@P_CONS_NM1,"+txt_Name.Text, 
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
		                      "@P_REG_NO ,"+ txt_form_no.Text,
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
                              "@P_MAONTHY_CHARGES,"+txt_monthlyCharges.Text,
                              "@P_KHASRA_NO,"+txt_khasra_no.Text,
                              "@P_VILLGAE_NAME,"+ddl_village_name.SelectedItem.Text,
                              "@P_VILLGAE_ID,"+ddl_village_name.SelectedValue,
                              "@P_Rid,"+txtRid.Text,
			                   "@P_OPERATION_TYPE ,1" ,  
                            
                            };
            string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_CONSUMER_DETAILS_MASTER_FOR_VILLAGE", valparam);
            string[] returnval1 = returnval.Split('~');

            if (returnval1[0] == "0")
            {
                txt_con_no.Text = returnval1[1].ToString();
                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval1[0])) + "')</script>";
                this.Controls.Add(lt);
                btnSave.Enabled = false;
                //btnSave.ImageUrl = "~/images/save_d.png";


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

}
