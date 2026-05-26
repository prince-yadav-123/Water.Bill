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
    DataSet objDs = new DataSet();
    DataRow dr;
    double totalchequeamount = 0;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {

            fill_Grid();

            string[] colParam1 = { "HEAD_ID", "HEAD_NAME" };
            string[] condParam1 = { "HEAD_ID is not null" };

            UI.GeneralFunctions.CInst().DropDownFill(ddlSubHead, "RECEIPT_HEAD_MASTER", colParam1, condParam1, "HEAD_ID");
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

    private void fill_Grid()
    {
        Session.Remove("d");
        DataTable dt = new DataTable();
        DataColumn c1 = new DataColumn();

        c1.ColumnName = "Deposit_Id";
        dt.Columns.Add(c1);

        DataColumn c2 = new DataColumn();
        c2.ColumnName = "Deposit_head";
        dt.Columns.Add(c2);

        DataColumn c3 = new DataColumn();
        c3.ColumnName = "Deopsit_amt";
        dt.Columns.Add(c3);

        DataColumn c4 = new DataColumn();
        c4.ColumnName = "Date_From";
        dt.Columns.Add(c4);

        DataColumn c5 = new DataColumn();
        c5.ColumnName = "Date_To";
        dt.Columns.Add(c5);

        Session["d"] = dt;
        GridView1.DataSource = ((DataTable)Session["d"]);
        GridView1.DataBind();
    }
    protected void GridView1_RowCommand(object sender, GridViewCommandEventArgs e)
    {
        if (e.CommandName == "Del")
        {
            string i = e.CommandArgument.ToString();

            int a = int.Parse(i);

            ((DataTable)Session["d"]).Rows[a - 1].Delete();
            ((DataTable)Session["d"]).AcceptChanges();
            GridView1.DataSource = ((DataTable)Session["d"]);
            GridView1.DataBind();
            btnAdd.Visible = true;

        }

    }
    protected void btnAdd_Click(object sender, EventArgs e)
    {

        bool chequenoAdded = false;
        double ttlchkAmount = 0.00;
        // ddldepartmentid.Enabled = false;
        Literal lt = new Literal();

        for (int i = 0; i < GridView1.Rows.Count; i++)
        {

            if (ddlSubHead.SelectedValue == GridView1.Rows[i].Cells[1].Text)
            {
                chequenoAdded = true;
            }
            totalchequeamount = totalchequeamount + double.Parse(GridView1.Rows[i].Cells[3].Text);
        }
        if (chequenoAdded)
        {
            lt.Text = "<script>alert('This Sub deposit Head Is Already Added ')</script>";
            this.Controls.Add(lt);
            //   txtChallanAmt.Focus();
            //db.msg1(Master, "This Sub deposit Head Is Already Added ");
        }
        else
        {
            totalchequeamount = totalchequeamount + double.Parse(txtAmt.Text);
            if (totalchequeamount == 0 || double.Parse(txtAmt.Text) == 0)
            {
                lt.Text = "<script>alert('Amount cannnot be Zero ')</script>";
                this.Controls.Add(lt);
                txtAmt.Focus();

            }
            else if (totalchequeamount > double.Parse(txtChallanAmt.Text))
            {
                lt.Text = "<script>alert('Total Amount Should be less Than Challan Amount')</script>";
                this.Controls.Add(lt);
                txtAmt.Focus();
            }
            else
            {
                dr = ((DataTable)Session["d"]).NewRow();

                dr["Deposit_Id"] = ddlSubHead.SelectedValue;
                dr["Deposit_head"] = ddlSubHead.SelectedItem.Text;
                dr["Deopsit_amt"] = txtAmt.Text;
                dr["Date_From"] = txtArrearFrom.Text;
                dr["Date_To"] = txtArrearTo.Text;


                ((DataTable)Session["d"]).Rows.Add(dr);
                GridView1.DataSource = ((DataTable)Session["d"]);
                GridView1.DataBind();
                GridView1.HeaderRow.Cells[1].Visible = false;
                if (GridView1.Rows.Count == 0)
                {
                    btnAdd.Visible = true;
                }
                for (int i = 0; i < GridView1.Rows.Count; i++)
                {
                    GridView1.Rows[i].Cells[1].Visible = false;
                    //GridView1.Rows[i].Cells[5].Visible = false;
                    ttlchkAmount = ttlchkAmount + double.Parse(GridView1.Rows[i].Cells[3].Text);
                    if (ttlchkAmount == double.Parse(txtChallanAmt.Text))
                    {
                        btnAdd.Visible = false;
                        btnSave.Visible = true;
                        btnReset.Visible = true;
                        btnclose.Visible = true;
                    }
                    else
                    {
                        ddlSubHead.Focus();
                        btnAdd.Visible = true;
                    }
                }

            }
        }
        txtAmt.Text = "";
        txtArrearFrom.Text = "";
        txtArrearTo.Text = "";





    }
    protected void ddlSubHead_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ddlSubHead.SelectedValue == "11")
        {
            txtArrearFrom.Enabled = true;
            txtArrearTo.Enabled = true;
            txtArrearFrom.CssClass = "td_text_ddl";
            txtArrearTo.CssClass = "td_text_ddl";
            //td1.Visible = true;
            //td2.Visible = true;
        }
        else
        {
            txtArrearFrom.Enabled = false;
            txtArrearTo.Enabled = false;
            txtArrearFrom.CssClass = "readonlytxt";
            txtArrearTo.CssClass = "readonlytxt";
            //td1.Visible = false;
            //td2.Visible = false;
        }
    }
    protected void GridView1_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[3].Style.Add("text-align", "right");
            e.Row.Cells[3].Style.Add("padding-right", "10px");
            e.Row.Cells[0].Style.Add("text-align", "center");
            e.Row.Cells[1].Style.Add("padding-right", "10px");
            e.Row.Cells[2].Style.Add("padding-right", "10px");
            e.Row.Cells[1].Style.Add("padding-left", "10px");
            e.Row.Cells[2].Style.Add("padding-left", "10px");
            e.Row.Cells[3].Style.Add("padding-right", "10px");
            e.Row.Cells[4].Style.Add("padding-right", "10px");
            e.Row.Cells[3].Style.Add("padding-left", "10px");
            e.Row.Cells[4].Style.Add("padding-left", "10px");
            e.Row.Cells[1].Style.Add("padding-right", "10px");
            e.Row.Cells[2].Style.Add("padding-right", "10px");
            e.Row.Cells[5].Style.Add("padding-left", "10px");
            e.Row.Cells[5].Style.Add("padding-right", "10px");
            //e.Row.Cells[0].Style.Add("padding-right", "10px");
            if (e.Row.Cells[4].Text == "&nbsp;")
            {
                e.Row.Cells[4].Text = "";
            }
            if (e.Row.Cells[5].Text == "&nbsp;")
            {
                e.Row.Cells[5].Text = "";
            }
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
    protected void gvUserInfo_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[0].Style.Add("text-align", "center");
            e.Row.Cells[1].Style.Add("padding-left", "5px");
            //e.Row.Cells[0].Style.Add("Width", "100px");
            //e.Row.Cells[1].Style.Add("text-align", "center");
            //e.Row.Cells[2].Style.Add("text-align", "center");
            e.Row.Cells[2].Style.Add("padding-left", "5px");
            e.Row.Cells[3].Style.Add("padding-left", "5px");
            e.Row.Cells[4].Style.Add("padding-left", "5px");
            e.Row.Cells[2].Style.Add("width", "200px");
            e.Row.Cells[3].Style.Add("width", "200px");
            //e.Row.Cells[4].Style.Add("text-align", "center");
            e.Row.Cells[5].Style.Add("text-align", "Right");
            e.Row.Cells[6].Style.Add("text-align", "center");
            e.Row.Cells[6].Style.Add("width", "100px");
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
        GetRecords();
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
        double totalchequeamount = 0.0;
        for (int i = 0; i < GridView1.Rows.Count; i++)
        {
            totalchequeamount = totalchequeamount + double.Parse(GridView1.Rows[i].Cells[3].Text);
        }

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
                for (int i = 0; i < GridView1.Rows.Count; i++)
                {
                    string[] valparam1 = {   "@P_RECIPT_NO,"+retVal[1].ToString(),
                                        "@P_CONS_NO,"+lblConsNo.Text, 
			                            "@P_CHALLAN_NO,"+ txt_challan_no.Text,  
			                            "@P_BANK_ID,"+ ddl_bank.SelectedValue, 
			                            "@P_HEAD_ID,"+ GridView1.Rows[i].Cells[1].Text,
		                                "@P_HEAD_AMT,"+ GridView1.Rows[i].Cells[3].Text,
		                                "@P_ARREAR_FROM,"+ GridView1.Rows[i].Cells[4].Text,
                                        "@P_ARREAR_TO,"+ GridView1.Rows[i].Cells[5].Text,
		                                "@P_USERID ,"+ Session["USERID"].ToString(),
			                            "@P_OPERATION_TYPE ,2"   
                                       };
                    returnval1 = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_JAL_CHALLAN_MASTER", valparam1);
                    if (returnval1 == "0")
                    {
                        count++;
                    }
                }
                if (count == GridView1.Rows.Count)
                {
                    lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval1)) + "')</script>";
                    this.Controls.Add(lt);
                    txtRecipt_no.Text = retVal[1].ToString();
                    btnSave.Enabled = true;
                    GetRecords();

                }
                else
                {
                    string[] valparam1 = {   
                                        "@P_CONS_NO,"+lblConsNo.Text, 
			                            "@P_CHALLAN_NO,"+ txt_challan_no.Text,		                            
			                            "@P_OPERATION_TYPE ,3"   
                                       };
                    returnval1 = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_JAL_CHALLAN_MASTER", valparam1);
                    if (returnval1.ToString() == "5")
                    {
                        lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval1)) + "')</script>";
                        this.Controls.Add(lt);
                    }
                    else
                    {
                        lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval1)) + "')</script>";
                        this.Controls.Add(lt);
                    }
                }
            }
            else
            {
                lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(retVal[0].ToString())) + "')</script>";
                this.Controls.Add(lt);
            }
        }
        else
        {
            lt.Text = "<script>alert('Total Amount Should be less Than Challan Amount')</script>";
            this.Controls.Add(lt);
        }
    }
}
