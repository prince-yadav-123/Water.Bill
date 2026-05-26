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
            string[] colParamBANK = { "BANK_ID", "BANK_NAME+'('+ACCOUNT_NO+')' AS BANK_NAME" };
            string[] condParamBANK = { "BANK_ID is not null" };
            UI.GeneralFunctions.CInst().DropDownFill(ddl_bank, "JAL_BANK_MASTER", colParamBANK, condParamBANK, "BANK_ID");
            GetDetails();
          
        }
    }

    public void GetDetails()
    {
        string[] colBank1 = { "*" };
        string[] condBank1 = { "CONS_NO='" + Request.QueryString["cons_no"].ToString() + "'" };
        DataSet challanDs = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_MASTER", colBank1, condBank1, "CONSUMER_MASTER", "CONS_NO");
        if (challanDs != null)
        {
            if (challanDs.Tables.Count > 0)
            {
                if (challanDs.Tables["CONSUMER_MASTER"].Rows.Count > 0)
                {
                    fillData(challanDs);
                    GetChallanDetails();
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
    public void fillData(DataSet ds)
    {
        lblConName.Text = ds.Tables[0].Rows[0]["CONS_NM1"].ToString();
        lblEmailId.Text = ds.Tables[0].Rows[0]["EMAIL_ID"].ToString();
        lblConsNo.Text = ds.Tables[0].Rows[0]["CONS_NO"].ToString();
        txtFlatno.Value = ds.Tables[0].Rows[0]["FLAT_NO"].ToString();
        txtBlock.Value = ds.Tables[0].Rows[0]["BLK_NO"].ToString();
        txtSector.Value = ds.Tables[0].Rows[0]["SECTOR"].ToString();
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
    public void GetChallanDetails()
    {
        string[] colBank1 = { "*" };
        string[] condBank1 = { "RECEIPT_ID='" + Request.QueryString["R_id"].ToString() + "'" };
        DataSet challanDs = BAL.BAL.CInst().SelectTable("CHALLAN", colBank1, condBank1, "CHALLAN_MASTER_NEW", "CONS_NO");
        if (challanDs != null)
        {
            if (challanDs.Tables.Count > 0)
            {
                if (challanDs.Tables["CHALLAN_MASTER_NEW"].Rows.Count > 0)
                {
                    txtRecipt_no.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["RECEIPT_ID"].ToString();
                    ddl_bank.SelectedValue = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["BANK_ID"].ToString();
                    if (challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["BL_PER_FR"].ToString() == "")
                    {
                        txt_bill_from.Text = "";
                    }
                    else
                    {
                        txt_bill_from.Text = Convert.ToDateTime(challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["BL_PER_FR"].ToString()).ToString("dd-MMM-yyyy");
                    }
                    if (challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["BL_PER_TO"].ToString() == "")
                    {
                        txt_bill_to.Text = "";
                    }
                    else
                    {
                        txt_bill_to.Text = Convert.ToDateTime(challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["BL_PER_TO"].ToString()).ToString("dd-MMM-yyyy");
                    }
                    txt_billId.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["BILL_ID"].ToString();
                    txt_challan_no.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["RECP_NO"].ToString();
                    txtChallanAmt.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["BILL_AMT"].ToString();
                    if (challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["PAY_DATE"].ToString() == "")
                    {
                        txt_paid_date.Text = "";
                    }
                    else
                    {
                        txt_paid_date.Text = Convert.ToDateTime(challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["PAY_DATE"].ToString()).ToString("dd-MMM-yyyy");
                    }
                    if (challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["DUE_DT"].ToString() == "")
                    {
                        txt_due_date.Text = "";
                    }
                    else
                    {
                        txt_due_date.Text = Convert.ToDateTime(challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["DUE_DT"].ToString()).ToString("dd-MMM-yyyy");
                    }
                    TextBox t_w_c = (TextBox)tbltax.FindControl("txt0");
                    t_w_c.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["PAID_AMT"].ToString();
                    TextBox t_sur_c = (TextBox)tbltax.FindControl("txt1");
                    t_sur_c.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["SURCHARGE"].ToString();
                    TextBox t_sec_c = (TextBox)tbltax.FindControl("txt2");
                    t_sec_c.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["SECU"].ToString();

                    TextBox t_Pen_c = (TextBox)tbltax.FindControl("txt3");
                    t_Pen_c.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["PANALITY_CHARGES"].ToString();
                    TextBox t_Ndc_c = (TextBox)tbltax.FindControl("txt4");
                    t_Ndc_c.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["NOC"].ToString();
                    TextBox t_Trans_c = (TextBox)tbltax.FindControl("txt5");
                    t_Trans_c.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["T_FEE"].ToString();

                    TextBox t_Main_c = (TextBox)tbltax.FindControl("txt6");
                    t_Main_c.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["RMC"].ToString();
                    TextBox t_Lab_c = (TextBox)tbltax.FindControl("txt7");
                    t_Lab_c.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["gst"].ToString();
                    TextBox t_Conn_c = (TextBox)tbltax.FindControl("txt8");
                    t_Conn_c.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["CONN_CHARGE"].ToString();
                    TextBox t_Cess_c = (TextBox)tbltax.FindControl("txt9");
                    t_Cess_c.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["CSS"].ToString();
                    TextBox t_Arrear_c = (TextBox)tbltax.FindControl("txt10");
                    t_Arrear_c.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["ARREAR"].ToString();
                    TextBox t_tp_c = (TextBox)tbltax.FindControl("txt11");
                    t_tp_c.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["R/T"].ToString();
                    TextBox t_disconnection = (TextBox)tbltax.FindControl("txt12");
                    t_disconnection.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["disconnection"].ToString();
                    TextBox t_reconnection = (TextBox)tbltax.FindControl("txt13");
                    t_reconnection.Text = challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["reconnection"].ToString();

                    if (challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["ARREAR_FROM"].ToString() == "")
                    {
                        txt_due_date.Text = "";
                    }
                    else
                    {
                        txt_arrear_from.Text = Convert.ToDateTime(challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["ARREAR_FROM"].ToString()).ToString("dd-MMM-yyyy");
                    }
                    if (challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["ARREAR_TO"].ToString() == "")
                    {
                        txt_due_date.Text = "";
                    }
                    else
                    {
                        txt_arrear_to.Text = Convert.ToDateTime(challanDs.Tables["CHALLAN_MASTER_NEW"].Rows[0]["ARREAR_TO"].ToString()).ToString("dd-MMM-yyyy");
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
        else
        {
            Literal lt = new Literal();
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
            this.Controls.Add(lt);
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



    protected void btnSave_Click(object sender, ImageClickEventArgs e)
    {
        Literal lt = new Literal();
        int count = 0;
        string parma = "0";
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

                            parma = parma + "~" + t.Text;



                        }
                        else
                        {
                            parma = parma + "~0";
                        }


                    }

                }

                string[] val = parma.Split('~');
                string[] valparam = {    "@P_CONS_NO ,"+lblConsNo.Text,
                                  "@P_FLAT_NO ,"+txtFlatno.Value,
                                  "@P_BLK ,"+txtBlock.Value,
                                  "@P_SEC ,"+txtSector.Value,
                                  "@P_BL_PER_FR ,"+txt_bill_from.Text,
                                  "@P_BL_PER_TO ,"+txt_bill_to.Text,
                                  "@P_DUE_DT ,"+txt_due_date.Text,
                                  "@P_BILL_AMT ,"+txtChallanAmt.Text,
                                  "@P_SURCHARGE ,"+val[2].ToString(),
                                  "@P_PAID_AMT ,"+val[1].ToString(),
                                  "@P_PAY_DATE ,"+txt_paid_date.Text,
                                  "@P_ARREAR ,"+val[11].ToString(),
                                  "@P_CREDIT ,0",
                                  "@P_RECP_NO ,"+txt_challan_no.Text,
                                  "@P_DIS_CD ,0",
                                  "@P_NOC  ,"+val[5].ToString(),
                                  "@P_RMC ,"+val[7].ToString(),
                                  "@P_SECU ,"+val[3].ToString(),
                                  "@P_T_FEE ,"+val[6].ToString(),
                                  "@P_CSS ,"+val[10].ToString(),
                                  "@P_LAB_CHARGE ,"+val[8].ToString(),
                                  "@P_CONN_CHARGE ,"+val[9].ToString(),
                                  "@P_PANALITY_CHARGES ,"+val[4].ToString(),
                                  "@P_BNK_CD ,"+ddl_bank.SelectedItem.Text,
                                  "@P_BR_NM ,Noida",
                                  "@P_DEV_TYPE ,"+Session["DEV_TYPE"].ToString(),
                                  "@P_ACOUNT_NO ,0",
                                  "@P_BANK_ID ,"+ddl_bank.SelectedValue,
                                  "@P_DEPOSETER_NAME ,"+lblConName.Text,
                                  "@P_BILL_ID ,"+txt_billId.Text,
                                  "@P_ARREAR_FROM ,"+ txt_arrear_from.Text,
                                  "@P_ARREAR_TO ,"+ txt_arrear_to.Text,	
	                              "@PRECIPT_NO1,"+ txtRecipt_no.Text,
                                  "@P_USERID ,"+ Session["USERID"].ToString(),
                                  "@R_T ,"+ val[12].ToString(),
                                  "@disconnection ,"+ val[13].ToString(),
                                  "@reconnection ,"+ val[14].ToString(),
                                  "@P_OPERATION_TYPE ,2" 

                            };
                string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_JAL_CHALLAN_MASTER_SINGLE", valparam);
               
                if (returnval == "1")
                {
                    lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
                    this.Controls.Add(lt);
                    //txtRecipt_no.Text = retVal[1].ToString();

                }
                else
                {
                    lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
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
