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

public partial class MainPage_Challan_Entry_With_PropertyNo : System.Web.UI.Page
{
    static double totaltax = 0;
    int totaltaxno = 0;
    DataSet objDs = new DataSet();
    DataRow dr;   
    protected void Page_Load(object sender, EventArgs e)
    {
        AddTextbox();
        if (!IsPostBack)
        {

            //fill_Grid();
            //

            pnl_main.Visible = false;


            string[] colParamBANK = { "BANK_ID", "BANK_NAME+'('+ACCOUNT_NO+')' AS BANK_NAME" };
            string[] condParamBANK = { "BANK_ID is not null" };

            UI.GeneralFunctions.CInst().DropDownFill(ddl_bank, "JAL_BANK_MASTER", colParamBANK, condParamBANK, "BANK_ID");

        }

        //if (Session["cons_no"] != "" && Session["cons_no"] != null)
        //{
        //    txt_cons_no.Text = Session["cons_no"].ToString();
        //    Session["cons_no"] = "";
        //}
        //else
        //{
        //    txt_cons_no.Text = txt_cons_no.Text;
        //}


    }
    protected void btnView_Click(object sender, ImageClickEventArgs e)
    {
        Reset();
        GetChallanByProperty();
        //string[] colBank1 = { "*" };
        //string[] condBank1 = { "SECTOR='" + txtSec.Text + "' and BLK_NO='" + txtBlk.Text + "' and FLAT_NO='" + txtFlat.Text + "'" };
        //DataSet challanDs = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_MASTER", colBank1, condBank1, "CONSUMER_MASTER", "CONS_NO");
        //if (challanDs != null)
        //{
        //    if (challanDs.Tables.Count > 0)
        //    {
        //        if (challanDs.Tables["CONSUMER_MASTER"].Rows.Count > 0)
        //        {
        //            fillData(challanDs);
        //            pnl_main.Enabled = true;
        //        }
        //        else
        //        {
        //            Literal lt = new Literal();
        //            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
        //            this.Controls.Add(lt);
        //        }
        //    }
        //    else
        //    {
        //        Literal lt = new Literal();
        //        lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
        //        this.Controls.Add(lt);
        //    }
        //}
        //else
        //{
        //    Literal lt = new Literal();
        //    lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
        //    this.Controls.Add(lt);
        //}
    }
    public void fillData(DataSet ds)
    {
        lblConName.Text = ds.Tables[0].Rows[0]["CONS_NM1"].ToString().Trim();
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
    protected void btnReset_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Challan_Entry_single.aspx");
    }
    protected void btnclose_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Welcome.aspx");
    }
    //protected void btnSearch_Click(object sender, ImageClickEventArgs e)
    //{
    //    Reset();
    //    GetChallan();
    //    // AddTextbox();
    //    string[] colBank1 = { "*" };
    //    string[] condBank1 = { "CONS_NO='" + txt_cons_no.Text + "'" };
    //    DataSet challanDs = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_MASTER", colBank1, condBank1, "CONSUMER_MASTER", "CONS_NO");
    //    if (challanDs != null)
    //    {
    //        if (challanDs.Tables.Count > 0)
    //        {
    //            if (challanDs.Tables["CONSUMER_MASTER"].Rows.Count > 0)
    //            {
    //                fillData(challanDs);
    //                pnl_main.Enabled = true;
    //            }
    //            else
    //            {
    //                Literal lt = new Literal();
    //                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
    //                this.Controls.Add(lt);
    //            }
    //        }
    //        else
    //        {
    //            Literal lt = new Literal();
    //            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
    //            this.Controls.Add(lt);
    //        }
    //    }
    //    else
    //    {
    //        Literal lt = new Literal();
    //        lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("3")) + "')</script>";
    //        this.Controls.Add(lt);
    //    }
    //}
    public void GetChallanByProperty()
    {
        string[] colBank1 = { "RECEIPT_ID,BL_PER_FR,BL_PER_TO,DEPOSETER_NAME,PAID_AMT,NOC,RMC,RECP_NO,SECU,PAY_DATE,BNK_CD,SEC,BLK,FLAT_NO,CONS_NO" };
        string[] condBank1 = { "SEC='" + txtSec.Text + "' and BLK='" + txtBlk.Text + "' and FLAT_NO='" + txtFlat.Text + "'" };
        DataSet challanDs = BAL.BAL.CInst().SelectTable("CHALLAN", colBank1, condBank1, "CONSUMER_MASTER", "PAY_DATE desc");
        if (challanDs != null)
        {
            if (challanDs.Tables.Count > 0)
            {
                if (challanDs.Tables["CONSUMER_MASTER"].Rows.Count > 0)
                {
                    gvParentGrid.DataSource = challanDs;
                    gvParentGrid.DataBind();
                    pnl_main.Enabled = true;
                }
                else
                {
                    gvParentGrid.DataSource = null;
                    gvParentGrid.DataBind();
                }
            }
            else
            {
                gvParentGrid.DataSource = null;
                gvParentGrid.DataBind();

            }
        }
        else
        {
            gvParentGrid.DataSource = null;
            gvParentGrid.DataBind();

        }
    }
    //public void GetChallan()
    //{
    //    string[] colBank1 = { "RECEIPT_ID,BL_PER_FR,BL_PER_TO,DEPOSETER_NAME,PAID_AMT,SURCHARGE,ARREAR,NOC,RMC,SECU,T_FEE,CSS,gst,CONN_CHARGE,PANALITY_CHARGES,PAY_DATE,BNK_CD,RECP_NO,(SEC+'/'+BLK+'-'+FLAT_NO) as Property_no" };
    //    string[] condBank1 = { "CONS_NO='" + txt_cons_no.Text + "'" };
    //    DataSet challanDs = BAL.BAL.CInst().SelectTable("CHALLAN", colBank1, condBank1, "CONSUMER_MASTER", "PAY_DATE desc");
    //    if (challanDs != null)
    //    {
    //        if (challanDs.Tables.Count > 0)
    //        {
    //            if (challanDs.Tables["CONSUMER_MASTER"].Rows.Count > 0)
    //            {
    //                gvParentGrid.DataSource = challanDs;
    //                gvParentGrid.DataBind();
    //                pnl_main.Enabled = true;
    //            }
    //            else
    //            {
    //                gvParentGrid.DataSource = null;
    //                gvParentGrid.DataBind();
    //            }
    //        }
    //        else
    //        {
    //            gvParentGrid.DataSource = null;
    //            gvParentGrid.DataBind();

    //        }
    //    }
    //    else
    //    {
    //        gvParentGrid.DataSource = null;
    //        gvParentGrid.DataBind();

    //    }
    //}
    protected void link_find_Click(object sender, EventArgs e)
    {
        //Literal lt = new Literal();
        //lt.Text = "<script>window.showModalDialog('../Search/Search.aspx', 'newwindow','center:yes;dialogWidth:800px;dialogHeight:600px; resizable:no;');</script>";
        ////lt.Text = "<script>window.showModelDialog('../MainPage/NEW_USER.aspx',my,center:yes;dialogHeight:300px; resizable:no;)</script>";
        //this.Controls.Add(lt);
    }
    protected void txt_billId_TextChanged(object sender, EventArgs e)
    {

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
                string[] valparam = {    
                                  "@P_CONS_NO ,"+lblConsNo.Text,
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
                                  "@P_USERID ,"+ Session["USERID"].ToString(),
                                  "@P_OPERATION_TYPE ,1" 
                            
                            };
                string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_JAL_CHALLAN_MASTER_SINGLE", valparam);
                string[] retVal = returnval.Split('~');
                if (retVal[0].ToString() == "0")
                {
                    lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse("0")) + "')</script>";
                    this.Controls.Add(lt);
                    txtRecipt_no.Text = retVal[1].ToString();
                    //GetChallan();
                    GetChallanByProperty();


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
    //protected void gvUserInfo_RowDataBound(object sender, GridViewRowEventArgs e)
    //{

    //    if (e.Row.RowType == DataControlRowType.Header)
    //    {
    //        e.Row.Cells[1].Style.Add("text-align", "center");
    //        e.Row.Cells[1].Style.Add("Width", "90px");
    //        e.Row.Cells[2].Style.Add("text-align", "center");
    //        e.Row.Cells[2].Style.Add("Width", "90px");
    //        e.Row.Cells[3].Style.Add("text-align", "center");
    //        e.Row.Cells[3].Style.Add("Width", "90px");
       

    //    }
    //    if (e.Row.RowType == DataControlRowType.DataRow)
    //    {
    //        LinkButton lnk = (LinkButton)e.Row.Cells[7].FindControl("link_edit");
    //        lnk.CommandArgument = e.Row.Cells[0].Text;
    //        LinkButton lnk1 = (LinkButton)e.Row.Cells[8].FindControl("link_delete");
    //        lnk1.CommandArgument = e.Row.Cells[0].Text;
    //        string[] a = e.Row.Cells[4].Text.Split(' ');
    //        e.Row.Cells[4].Text = a[0].ToString();
    //        e.Row.Cells[0].Style.Add("text-align", "center");
    //        e.Row.Cells[0].Style.Add("padding", "5px");
    //        e.Row.Cells[1].Style.Add("text-align", "center");
    //        e.Row.Cells[1].Style.Add("Width", "90px");
    //        e.Row.Cells[2].Style.Add("text-align", "center");
    //        e.Row.Cells[2].Style.Add("Width", "90px");
    //        e.Row.Cells[3].Style.Add("text-align", "center");
    //        e.Row.Cells[3].Style.Add("Width", "90px");
    //        e.Row.Cells[4].Style.Add("text-align", "center");
    //        e.Row.Cells[4].Style.Add("padding", "5px");
    //        e.Row.Cells[5].Style.Add("text-align", "center");
    //        e.Row.Cells[5].Style.Add("padding", "5px");
    //        e.Row.Cells[6].Style.Add("text-align", "right");
    //        e.Row.Cells[6].Style.Add("padding", "5px");
    //        e.Row.Cells[7].Style.Add("text-align", "right");
    //        e.Row.Cells[7].Style.Add("padding", "5px");
    //        e.Row.Cells[8].Style.Add("text-align", "right");
    //        e.Row.Cells[8].Style.Add("padding", "5px");
    //        e.Row.Cells[9].Style.Add("text-align", "right");
    //        e.Row.Cells[9].Style.Add("padding", "5px");
    //        e.Row.Cells[10].Style.Add("text-align", "right");
    //        e.Row.Cells[10].Style.Add("padding", "5px");
    //        e.Row.Cells[11].Style.Add("text-align", "right");
    //        e.Row.Cells[11].Style.Add("padding", "5px");
    //        e.Row.Cells[12].Style.Add("text-align", "right");
    //        e.Row.Cells[12].Style.Add("padding", "5px");
    //        e.Row.Cells[13].Style.Add("text-align", "right");
    //        e.Row.Cells[13].Style.Add("padding", "5px");
    //        e.Row.Cells[14].Style.Add("text-align", "right");
    //        e.Row.Cells[14].Style.Add("padding", "5px");
    //        e.Row.Cells[15].Style.Add("text-align", "right");
    //        e.Row.Cells[15].Style.Add("padding", "5px");
    //        e.Row.Cells[16].Style.Add("text-align", "right");
    //        e.Row.Cells[16].Style.Add("padding", "5px");
    //        e.Row.Cells[17].Style.Add("text-align", "right");
    //        e.Row.Cells[17].Style.Add("padding", "5px");
    //    }

    //}
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
    //protected void link_edit_Command(object sender, CommandEventArgs e)
    //{
    //    Literal lt = new Literal();
    //    lt.Text = "<Script>window.open('../Update/Challan_Update_PropertyNo.aspx?R_id=" + e.CommandArgument.ToString() + "&Sector=" + txtSec.Text + "&Block=" + txtBlk.Text+ "&Flat=" +txtFlat.Text +"', 'mywin','left=20,top=20,width=1024,height=675,toolbar=1,scrollbars=1,resizable=1')</Script>";
    //    this.Controls.Add(lt);
    //    //Response.Redirect("~/Update/Challan_Entry_SINGLE_UPDATE.aspx?R_id='" + e.CommandArgument.ToString() + "'");
    //}
    //protected void link_edit_Click(object sender, EventArgs e)
    //{

    //}
    //protected void link_delete_Click(object sender, EventArgs e)
    //{

    //}
    //protected void link_delete_Command(object sender, CommandEventArgs e)
    //{
    //    Literal lt = new Literal();
    //    //string del = e.CommandArgument.ToString();

    //    var closeLink = (Control)sender;
    //    GridViewRow row = (GridViewRow)closeLink.NamingContainer;
    //    string paydate = row.Cells[3].Text.Trim();
    //    string challanno = row.Cells[5].Text.Trim();
    //    string amt = row.Cells[6].Text.Trim();

    //    string[] valparam = {    
                                 
    //                              "@P_challanno,"+challanno.ToString(),
    //                              "@P_amt,"+amt.ToString(),
    //                              "@P_OPERATION_TYPE ,1" 
                            
    //                        };
    //    string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_DELETE_CHALLAN", valparam);

    //    if (returnval == "2")
    //    {
    //        lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
    //        this.Controls.Add(lt);           
    //        //GetChallan();
    //        GetChallanByProperty();
    //    }
    //    else
    //    {
    //        lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
    //        this.Controls.Add(lt);
    //    }

    //}

}
