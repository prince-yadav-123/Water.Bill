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
        AddTextbox();
        if (!IsPostBack)
        {

            //fill_Grid();
            //

            string[] colParamBANK = { "BANK_ID", "BANK_NAME+'('+ACCOUNT_NO+')' AS BANK_NAME" };
            string[] condParamBANK = { "BANK_ID is not null" };

            UI.GeneralFunctions.CInst().DropDownFill(ddl_bank, "JAL_BANK_MASTER", colParamBANK, condParamBANK, "BANK_ID");

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
    public void fillData(DataSet ds)
    {
        lblConName.Text = ds.Tables[0].Rows[0]["CONS_NM1"].ToString();
        lblEmailId.Text = ds.Tables[0].Rows[0]["EMAIL_ID"].ToString();
        lblConsNo.Text = ds.Tables[0].Rows[0]["CONS_NO"].ToString();
        lblSector.Text = ds.Tables[0].Rows[0]["SECTOR"].ToString() + "/" + ds.Tables[0].Rows[0]["BLK_NO"].ToString() + "-" + ds.Tables[0].Rows[0]["FLAT_NO"].ToString();
        lblMobNo.Text = ds.Tables[0].Rows[0]["MOB_NO"].ToString();
        if (ds.Tables[0].Rows[0]["CON_TP"].ToString() == "R")
        {

            lblPayType.Text = "Residential";
        }
        else if (ds.Tables[0].Rows[0]["CON_TP"].ToString() == "T")
        {
            lblPayType.Text = "Industrial";
        }
        else if (ds.Tables[0].Rows[0]["CON_TP"].ToString() == "C")
        {
            lblPayType.Text = "Commercial";
        }
        else if (ds.Tables[0].Rows[0]["CON_TP"].ToString() == "I")
        {
            lblPayType.Text = "Institutional";
        }
        else if (ds.Tables[0].Rows[0]["CON_TP"].ToString() == "V")
        {
            lblPayType.Text = "Village";
        }
        else
        {
            lblPayType.Text = "";
        }

    }





    private void GetRecords()
    {
        string[] para ={
                           
                            
                            "@P_cons_NO,"+ txt_cons_no.Text,                            
                            "@P_OPERATION_TYPE,1" 
                            };
        objDs = BAL.BAL.CInst().ProcedureCallMulti_datset("PRCO_COMPLETE_CHALLAN_SEARCH", para);
        if (objDs != null)
        {
            //if (objDs.Tables.Count > 0)
            //{
            //    if (objDs.Tables[0].Rows.Count > 0)
            //    {

            gvParentGrid.DataSource = objDs.Tables[0];
            gvParentGrid.DataBind();
            GridView3.DataSource = objDs.Tables[1];
            GridView3.DataBind();
            GridView2.DataSource = objDs.Tables[2];
            GridView2.DataBind();
            objDs.Reset();

            //    }
            //    else
            //    {
            //        gvParentGrid.DataSource = null;
            //        gvParentGrid.DataBind();
            //    }
            //}
            //else
            //{
            //    gvParentGrid.DataSource = null;
            //    gvParentGrid.DataBind();
            //}
        }
        else
        {
            gvParentGrid.DataSource = null;
            gvParentGrid.DataBind();
        }
    }



    protected void btnReset_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Challan_Entry.aspx");
    }
    protected void btnclose_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Welcome.aspx");
    }
    protected void btnSearch_Click(object sender, ImageClickEventArgs e)
    {
        Reset();
        GetRecords();
        // AddTextbox();
        string[] colBank1 = { "*" };
        string[] condBank1 = { "CONS_NO='" + txt_cons_no.Text + "'" };
        DataSet challanDs = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_MASTER", colBank1, condBank1, "CONSUMER_MASTER", "CONS_NO");
        if (challanDs != null)
        {
            if (challanDs.Tables.Count > 0)
            {
                if (challanDs.Tables["CONSUMER_MASTER"].Rows.Count > 0)
                {
                    fillData(challanDs);
                    pnl_main.Enabled = true;
                }
                else
                {
                    Literal lt = new Literal();
                    lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
                    this.Controls.Add(lt);
                }
            }
            else
            {
                Literal lt = new Literal();
                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
                this.Controls.Add(lt);
            }
        }
        else
        {
            Literal lt = new Literal();
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
            this.Controls.Add(lt);
        }
    }

    protected void link_find_Click(object sender, EventArgs e)
    {
        //Literal lt = new Literal();
        //lt.Text = "<script>window.showModalDialog('../Search/Search.aspx', 'newwindow','center:yes;dialogWidth:800px;dialogHeight:600px; resizable:no;');</script>";
        ////lt.Text = "<script>window.showModelDialog('../MainPage/NEW_USER.aspx',my,center:yes;dialogHeight:300px; resizable:no;)</script>";
        //this.Controls.Add(lt);
    }

    protected void txt_billId_TextChanged(object sender, EventArgs e)
    {
        string[] colBank1 = { "*" };
        string[] condBank1 = { "CONS_NO='" + txt_cons_no.Text + "'" };
        DataSet challanDs = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_MASTER", colBank1, condBank1, "CONSUMER_MASTER", "CONS_NO");
        if (challanDs != null)
        {
            if (challanDs.Tables.Count > 0)
            {
                if (challanDs.Tables["CONSUMER_MASTER"].Rows.Count > 0)
                {
                }
            }
        }
    }
    protected void btnSave_Click(object sender, ImageClickEventArgs e)
    {
        Literal lt = new Literal();
        int count = 0;
        string returnval1 = "";
        string dateFrom = "";
        string dateTo = "";
        string dateVal = "0";
        double totalchequeamount = 0.0;
        string[] colBank2 = { "HEAD_ID", "dbo.Proper(head_name) as HEAD_NAME" };
        string[] condBank2 = { "HEAD_ID is not null" };
        DataSet ds1 = BAL.BAL.CInst().SelectTable("RECEIPT_HEAD_MASTER", colBank2, condBank2, "RECEIPT_HEAD_MASTER", "HEAD_ID");
        if (ds1.Tables[0].Rows.Count > 0)
        {
            for (int i = 0; i < ds1.Tables[0].Rows.Count; i++)
            {
                TextBox t = (TextBox)tbltax.FindControl("txt" + i.ToString());
                HiddenField h = (HiddenField)tbltax.FindControl("hf" + i.ToString());
                //totaltax = totaltax + Convert.ToDouble(t.Text);
                totalchequeamount = totalchequeamount + double.Parse(t.Text);
                if (h.Value == "11" && t.Text != "0")
                {
                    dateVal = "1";
                    dateFrom = txt_arrear_from.Text;
                    dateTo = txt_arrear_to.Text;
                }


            }
            if (dateVal == "1")
            {
                if (txt_arrear_from.Text == "" || txt_arrear_to.Text == "")
                {
                    dateVal = "1";

                }
                else
                {
                    dateVal = "0";
                }
            }

        }
        if (dateVal == "0")
        {
            if (txtChallanAmt.Text == totalchequeamount.ToString())
            {

                string[] valparam = {   "@P_CONS_NO,"+lblConsNo.Text, 
                                "@P_CHALLAN_NO,"+ txt_challan_no.Text,  
                                "@P_BANK_ID,"+ ddl_bank.SelectedValue,
                                "@P_BILL_ID,"+txt_billId.Text,
                                "@P_CHALLAN_AMT,"+ txtChallanAmt.Text,
                                "@P_BILL_FROM,"+ txt_bill_from.Text,
                                "@P_BILL_TO,"+ txt_bill_to.Text,
                                "@P_DUE_DATE,"+txt_due_date.Text,
                                "@P_PAID_DATE,"+txt_paid_date.Text,
                                "@P_USERID ,"+ Session["USERID"].ToString(),
                                "@P_OPERATION_TYPE ,1" 
                            
                            };
                string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_JAL_CHALLAN_MASTER", valparam);
                string[] retVal = returnval.Split('~');
                if (retVal[0].ToString() == "0")
                {
                    string[] colBank1 = { "HEAD_ID", "dbo.Proper(head_name) as HEAD_NAME" };
                    string[] condBank1 = { "HEAD_ID is not null" };
                    DataSet ds = BAL.BAL.CInst().SelectTable("RECEIPT_HEAD_MASTER", colBank1, condBank1, "RECEIPT_HEAD_MASTER", "HEAD_ID");
                    //DataSet ds = new DataSet();
                    //ds = db.ReadAllFromDb("select * from TAX_DEDUCT_MASTER where type_ded='G' or type_ded='GS'", "TAX_DEDUCT_MASTER");
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        {
                            TextBox t = (TextBox)tbltax.FindControl("txt" + i.ToString());
                            HiddenField h = (HiddenField)tbltax.FindControl("hf" + i.ToString());
                            totaltax = totaltax + Convert.ToDouble(t.Text);
                            if (t.Text != "0")
                            {
                                string[] valparam1 = {"@P_RECIPT_NO,"+retVal[1].ToString(),
                                        "@P_CONS_NO,"+lblConsNo.Text, 
                                        "@P_CHALLAN_NO,"+ txt_challan_no.Text,  
                                        "@P_BANK_ID,"+ ddl_bank.SelectedValue, 
                                        "@P_HEAD_ID,"+ h.Value,
                                        "@P_HEAD_AMT,"+ t.Text,
                                        "@P_ARREAR_FROM,"+dateFrom,
                                        "@P_ARREAR_TO,"+dateTo,
                                        "@P_USERID ,"+ Session["USERID"].ToString(),
                                        "@P_OPERATION_TYPE ,2"   
                                       };
                                returnval1 = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_JAL_CHALLAN_MASTER", valparam1);
                                if (returnval1.ToString().Equals("0"))
                                {
                                    totaltaxno = totaltaxno + 1;
                                }
                            }
                            else
                            {
                                totaltaxno = totaltaxno + 1;
                            }

                        }
                        if (totaltaxno == ds.Tables[0].Rows.Count)
                        {

                            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("0")) + "')</script>";
                            this.Controls.Add(lt);
                            txtRecipt_no.Text = retVal[1].ToString();
                            GetRecords();
                        }
                        else
                        {
                            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("5")) + "')</script>";
                            this.Controls.Add(lt);
                        }
                    }
                }
                else
                {
                    lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(retVal[0])) + "')</script>";
                    this.Controls.Add(lt);
                }
            }
            else
            {
                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("16")) + "')</script>";
                this.Controls.Add(lt);
            }
        }
        else
        {

            lt.Text = "<script>alert('Arrear Date To & From can not blank!!!!')</script>";
            this.Controls.Add(lt);
        }





    }

    public void AddTextbox()
    {


        string[] colBank1 = { "HEAD_ID", "dbo.Proper(left(head_name, PATINDEX('%%[ ]%%', head_name) - 1)) as HEAD_NAME" };
        string[] condBank1 = { "HEAD_ID is not null" };
        DataSet ds = BAL.BAL.CInst().SelectTable("RECEIPT_HEAD_MASTER", colBank1, condBank1, "RECEIPT_HEAD_MASTER", "HEAD_ID");
        //ds = db.ReadAllFromDb("select * from TAX_DEDUCT_MASTER where type_ded='G' or type_ded='GS'", "TAX_DEDUCT_MASTER");
        if (ds != null)
        {
            if (ds.Tables.Count > 0)
            {
                if (ds.Tables["RECEIPT_HEAD_MASTER"].Rows.Count > 0)
                {

                    Table t = new Table();
                    int k = 0;
                    for (int i = 0; i < ds.Tables[0].Rows.Count; )
                    {
                        TableRow tr = new TableRow();

                        for (int j = 0; j < 4; j++)
                        {
                            if (i < ds.Tables[0].Rows.Count)
                            {
                                int count = ds.Tables[0].Rows.Count;
                                htax.Value = count.ToString();
                                string taxname = ds.Tables[0].Rows[i]["HEAD_NAME"].ToString();
                                string taxid = ds.Tables[0].Rows[i]["HEAD_ID"].ToString();
                                HiddenField h = new HiddenField();
                                h.ID = "hf" + i.ToString();
                                h.Value = taxid;
                                tbltax.Controls.Add(h);
                                TableCell tc = new TableCell();
                                TableCell tc2 = new TableCell();
                                tc.CssClass = "pageLabel";
                                tc.Width = 350;
                                tc.Style["text-align"] = "left";
                                tc2.Style["text-align"] = "left";
                                TextBox txt = new TextBox();
                                txt.MaxLength = 13;
                                txt.Style["width"] = "60px";
                                txt.ID = "txt" + i.ToString();
                                txt.Attributes.Add("onkeypress", "return amount(event.keyCode)");
                                if (txt.Text == string.Empty)
                                {
                                    txt.Text = "0";
                                }
                                if ((txt.Text != " "))
                                {
                                    txt.Attributes.Add("onblur", "return calculate()");

                                }

                                Label l = new Label();
                                l.Width = 100;
                                l.ID = "lbl" + i.ToString();
                                l.Text = taxname;
                                tc.Controls.Add(l);
                                tc2.Controls.Add(txt);
                                tr.Cells.Add(tc);
                                tr.Cells.Add(tc2);

                                t.Rows.Add(tr);
                                i++;
                            }
                            else
                            {
                                break;
                            }
                        }

                    }
                    tbltax.Controls.Add(t);
                }
            }
        }
    }
    protected void gvUserInfo_RowDataBound(object sender, GridViewRowEventArgs e)
    {

    }

    public void Reset()
    {
        txtRecipt_no.Text = "";
        lblConsNo.Text = "";
        lblConName.Text = "";
        lblSector.Text = "";
        lblEmailId.Text = "";
        lblMobNo.Text = "";
        lblPayType.Text = "";
        ddl_bank.SelectedValue = "0";
        txt_billId.Text = "0";
        txt_bill_from.Text = "";
        txt_bill_to.Text = "";
        txt_challan_no.Text = "";
        txtChallanAmt.Text = "";
        txt_due_date.Text = "";
        txt_paid_date.Text = "";
        txt_arrear_from.Text = "";
        txt_arrear_to.Text = "";
    }
}
