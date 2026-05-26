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

public partial class MainPage_Challan : System.Web.UI.Page
{
    static double totaltax = 0;
    int totaltaxno = 0;
    DataSet objDs = new DataSet();
    DataRow dr;
    //double totalchequeamount = 0;
    protected void Page_Load(object sender, EventArgs e)
    {

        if (!IsPostBack)
        {
            string[] colParamBANK = { "bnk_cd", "bnk_cd AS BANK_NAME" };
            string[] condParamBANK = { "bnk_cd is not null group by bnk_cd" };
            UI.GeneralFunctions.CInst().DropDownFill(BNK_CD, "CHALLAN", colParamBANK, condParamBANK);

        }




    }

    public void Fill_grid()
    {
        DataSet ds_rate = new DataSet();
        string query = "";
        for (int i = 0; i < Checlst1.Items.Count; i++)
        {
            bool value_check = Checlst1.Items[i].Selected;
            string checktext = Checlst1.Items[i].Value;
            Control c1 = new Control();
            c1 = div_find.FindControl(checktext);
            Type type1 = c1.GetType();
            TextBox t1 = new TextBox();
            Type type2 = c1.GetType();
            DropDownList t2 = new DropDownList();
            RadioButtonList r1 = new RadioButtonList();

            if (type1.Name == "TextBox")
            {
                t1 = (TextBox)c1;
            }
            if (type2.Name == "DropDownList")
            {
                t2 = (DropDownList)c1;
            }
            if (value_check == true)
            {
                if (checktext == "BNK_CD")
                {
                    query = query + Checlst1.Items[i].Value + " like '" + t2.SelectedItem.Text + "%' and";
                }
                else if (checktext == "Pay_date")
                {
                    query = query + Checlst1.Items[i].Value + " between '" + t1.Text + "'AND '" + Pay_date1.Text + "' and";
                }
                else if (checktext == "RECP_NO")
                {
                    query = query + Checlst1.Items[i].Value + " like '%" + t1.Text + "%'  and ";
                }
                else
                {
                    query = query + Checlst1.Items[i].Value + "='" + t1.Text + "'  and ";
                }
            }

        }
        if (query.Length <= 0)
        {

            query = query + "paid_amt IS NOT NULL";

        }
        else
        {

            query = query.Substring(0, query.ToString().Length - 5);
        }

        string[] colParam2 = new string[] { "receipt_id,CONS_NO,sec+'/'+blk+'-'+flat_no as property_no,BL_PER_FR,BL_PER_TO,pay_date,paid_amt,RECP_NO,BNK_CD,deposeter_name" };
        string[] condParam2 = new string[] { query };
        ds_rate = BAL.BAL.CInst().SelectTable("CHALLAN", colParam2, condParam2, "CHALLAN_MISC", "pay_date desc");

        //DataSet ds_Update1 = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_TRANS", colParam2, condParam2, "CONSUMER_DETAILS_TRANS");
        if (ds_rate != null)
        {
            if (ds_rate.Tables.Count > 0)
            {
                if (ds_rate.Tables["CHALLAN_MISC"].Rows.Count > 0)
                {
                    gvParentGrid.DataSource = ds_rate;
                    gvParentGrid.DataBind();
                    div1.Visible = true;
                }
                else
                {
                    gvParentGrid.DataSource = "";
                    gvParentGrid.DataBind();
                    div1.Visible = true;
                }
            }
            else
            {
                gvParentGrid.DataSource = "";
                gvParentGrid.DataBind();
                div1.Visible = true;
            }
        }
        else
        {
            gvParentGrid.DataSource = "";
            gvParentGrid.DataBind();
            div1.Visible = true;
        }
    }
    protected void gvParentGrid_RowEditing(object sender, GridViewEditEventArgs e)
    {
        //  DropDownList bind_dropdownlist1;
        gvParentGrid.EditIndex = e.NewEditIndex;
        //bind_dropdownlist1 = (DropDownList)e.Row.FindControl("dd_sector");
        //bind_dropdownlist1.SelectedValue= gvParentGrid.Rows[e.NewEditIndex].Cells[4].Text.ToString();
        Fill_grid();
    }

    protected void gvParentGrid_RowUpdating(object sender, GridViewUpdateEventArgs e)
    {
        try
        {
            string challanNO = gvParentGrid.Rows[e.RowIndex].Cells[7].Text;

            string RECEIPT_ID = gvParentGrid.Rows[e.RowIndex].Cells[0].Text;
            TextBox CONS_NO = (TextBox)gvParentGrid.Rows[e.RowIndex].Cells[1].Controls[1];



            string[] valparam = {   "@P_RECEIPT_ID,"+RECEIPT_ID,			                    
                                "@P_CONS_NO,"+ CONS_NO.Text,
                                "@P_RECP_NO,"+ challanNO,
                                "@P_USER_ID,"+Session["USERID"],
                                "@P_OPERATION_TYPE ,1" 
                            
                            };
            string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_CHALLAN_MASTER_CONS", valparam);
            if (returnval == "1")
            {
                ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), Guid.NewGuid().ToString(), "alert('Row Updated Sucessfully!!!');", true);
                gvParentGrid.EditIndex = -1;
                Fill_grid();
            }
            else
            {
                ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), Guid.NewGuid().ToString(), "alert('Row does not update!!! Try Again');", true);
                gvParentGrid.EditIndex = -1;
                Fill_grid();
            }


            //ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), Guid.NewGuid().ToString(), "alert('Row updated Sucessfully!!!');", true);
            gvParentGrid.EditIndex = -1;
            Fill_grid();

        }
        catch (Exception ex)
        {
            ScriptManager.RegisterClientScriptBlock(Page, Page.GetType(), Guid.NewGuid().ToString(), "alert('" + ex.Message.ToString() + "');", true);
        }
        finally
        {

        }
    }
    protected void gvParentGrid_RowCancelingEdit(object sender, GridViewCancelEditEventArgs e)
    {

        gvParentGrid.EditIndex = -1;
        Fill_grid();

    }
    protected void gvParentGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        DropDownList bind_dropdownlist;
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[1].Style.Add("width", "100px");
            e.Row.Cells[1].Style.Add("text-align", "center");
            e.Row.Cells[2].Style.Add("text-align", "left");
            e.Row.Cells[2].Style.Add("font-weight", "bold");
            e.Row.Cells[3].Style.Add("text-align", "left");
            e.Row.Cells[7].Style.Add("font-weight", "bold");
            //e.Row.Cells[0].Style.Add("background-color", "#E5E4E2");
            //e.Row.Cells[2].Style.Add("background-color", "#E5E4E2");
            //e.Row.Cells[4].Style.Add("background-color", "#E5E4E2");
            //e.Row.Cells[6].Style.Add("background-color", "#E5E4E2");
            //e.Row.Cells[8].Style.Add("background-color", "#E5E4E2");
            if (e.Row.Cells[8].Text == "HDFC BANK (15921450000074)")
            {
                e.Row.Cells[8].Text = "HDFC BANK";
            }
        }
    }
    protected void btnView_Click(object sender, ImageClickEventArgs e)
    {
        lbl_lgnd.Text = "ALL CHALLAN VIEW";
        Fill_grid();
    }
    protected void txt_cons_no_TextChanged(object sender, EventArgs e)
    {
        TextBox tb1 = ((TextBox)(sender));
        GridViewRow gv1 = ((GridViewRow)(tb1.NamingContainer));
        int rownumber = gv1.RowIndex;
        string[] colBank1 = { "isnull(sector,'NA')+'/'+isnull(blk_no,'NA')+'-'+isnull(flat_no,'NA') as property_no" };
        string[] condBank1 = { "CONS_NO='" + tb1.Text + "'" };
        DataSet consDs = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_MASTER", colBank1, condBank1, "CONSUMER_MASTER", "CONS_NO");
        if (consDs != null)
        {
            if (consDs.Tables.Count > 0)
            {
                if (consDs.Tables["CONSUMER_MASTER"].Rows.Count > 0)
                {
                    gvParentGrid.Rows[rownumber].Cells[2].Text = consDs.Tables[0].Rows[0]["property_no"].ToString().Trim();
                }
            }
        }

    }
    protected void CheckBoxList1_SelectedIndexChanged(object sender, EventArgs e)
    {

        for (int i = 0; i < Checlst1.Items.Count; i++)
        {

            bool value_check = Checlst1.Items[i].Selected;
            string checktext = Checlst1.Items[i].Value;
            Control c1 = new Control();
            c1 = div_find.FindControl(checktext);
            Type type1 = c1.GetType();
            TextBox t1 = new TextBox();
            Type type2 = c1.GetType();
            DropDownList t2 = new DropDownList();
            if (type1.Name == "TextBox")
            {
                t1 = (TextBox)c1;
            }
            if (type2.Name == "DropDownList")
            {
                t2 = (DropDownList)c1;
            }
            if (type2.Name == "DropDownList")
            {
                if (value_check == true)
                {
                    t2.Visible = true;
                }
                else
                {
                    t2.Visible = false;
                    t2.SelectedValue = "0";
                }
            }
            else
            {
                if (value_check == true)
                {
                    t1.Visible = true;
                    
                    if (t1.ID == "Pay_date")
                    {
                        Pay_date1.Visible = true;
                        Pay_date1.Text = "";
                    }
                }
                else
                {
                    t1.Visible = false;
                    t1.Text = "";
                    if (t1.ID == "Pay_date")
                    {
                        Pay_date1.Visible = false;
                        Pay_date1.Text = "";
                    }
                }
            }

        }
    }

}
