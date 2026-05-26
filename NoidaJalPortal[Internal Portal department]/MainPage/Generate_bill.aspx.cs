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

public partial class Update_Generate_bill : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

        if (Session["cons_no"] != "" && Session["cons_no"] != null)
        {
            txt_cons_no.Text = Session["cons_no"].ToString();
            GenerateBill(Session["cons_no"].ToString());
            Session["cons_no"] = "";
        }
        else
        {
            txt_cons_no.Text = txt_cons_no.Text;
        }
    }
    protected void btnSave_Click(object sender, ImageClickEventArgs e)
    {
        Literal lt = new Literal();
        string[] valparam = {   "@P_CONS_NO,"+txt_cons_no.Text,			                    
		                        "@P_BILL_DATE,"+ txt_bill_date.Text,
		                        "@P_BILL_DUE_DATE,"+ txt_due_date.Text,
                                "@P_BILL_DATE_FROM,"+ txt_bill_from.Text,
                                "@P_BILL_DATE_TO,"+ txt_bill_to.Text,
                                "@P_MIN_RATE,"+ txt_min_charges.Text,
                                "@P_MIN_TOTAL_AMT,"+txt_t_min_charge.Text,
                                "@P_BILL_REBATE_PER,"+txt_rebate_per.Text,
                                
                                "@P_BILL_REBATE_AMT,"+ txt_rebate_t_amt.Text,
                                "@P_CESS_AMT,"+ txt_cess_amt.Text,
                                "@P_AREAR,"+ txt_arear_amt.Text,
                                "@P_AREAR_INT,"+txt_intrest_amt.Text,
                                "@P_CONS_NAME,"+txt_Cons_nm.Text,
                                "@P_Adv_amt,"+txt_adv_amt.Text,
			                    "@P_OPERATION_TYPE ,1" 
                            
                            };
        string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_JAL_PRINT_BILL_MASTER", valparam);
        string[] returnVal1 = returnval.Split('~');

        if (returnVal1[0].ToString() == "0")
        {
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnVal1[0])) + "')</script>";
            this.Controls.Add(lt);
            txt_bill_no.Text = returnVal1[1].ToString();
            btnPrint.Visible = true;

        }
        else
        {
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
            this.Controls.Add(lt);
        }

    }

    public void reset()
    {
        txt_cons_no.Text = "";
        txt_bill_no.Text = "";
        txt_due_date.Text = "";
        txt_Cons_nm.Text = "";
        txt_Flat_type.Text = "";
        txt_sector.Text = "";
        txt_category.Text = "";
        txt_block_no.Text = "";
        txt_bill_from.Text = "";
        txt_Flat_No.Text = "";
        txt_bill_to.Text = "";
        txt_min_charges.Text = "";
        txt_t_min_charge.Text = "";
        txt_rebate_per.Text = "";
        txt_rebate_t_amt.Text = "";
        txt_cess_amt.Text = "";
        txt_arear_amt.Text = "";
        txt_intrest_amt.Text = "";
    }
    protected void btnReset_Click(object sender, ImageClickEventArgs e)
    {
        reset();
    }
    protected void btnclose_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Welcome.aspx");
    }
    protected void btnSearch_Click(object sender, ImageClickEventArgs e)
    {
        GenerateBill(txt_cons_no.Text);
    }

    public void GenerateBill(string cons_no)
    {
        string[] colParam2 = new string[] { "CONS_NO,NODUES_DT,CONS_NM1,FLAT_TYPE,SECTOR,LTRIM(RTRIM(CASE WHEN CONS_CTG= 'R' THEN 'Regular' when CONS_CTG= 'T' then  'Temporary' else 'Temporary' END)) AS CON_TP,BLK_NO,FLAT_NO,isnull(plot_size,0) as plot_size" };
        string[] condParam2 = new string[] { "CONS_NO='" + cons_no.ToString() + "'" };
        DataSet ds_Update = BAL.BAL.CInst().SelectTable("VIEW_CONSUMER_DETAILS_MASTER", colParam2, condParam2, "CONSUMER_DETAILS_MASTER");
        //DataSet ds_Update1 = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_TRANS", colParam2, condParam2, "CONSUMER_DETAILS_TRANS");
        if (ds_Update != null)
        {
            if (ds_Update.Tables.Count > 0)
            {
                if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0)
                {
                    //txt_cons_nm.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_NM1"].ToString();
                    txt_cons_no.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_NO"].ToString();
                    txt_due_date.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["NODUES_DT"].ToString();
                    txt_Cons_nm.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_NM1"].ToString();
                    txt_Flat_type.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["FLAT_TYPE"].ToString() + "(" + ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["plot_size"].ToString() + ")";
                    txt_sector.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["SECTOR"].ToString();
                    txt_category.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CON_TP"].ToString();
                    txt_block_no.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["BLK_NO"].ToString();
                    txt_Flat_No.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["FLAT_NO"].ToString();
                    int Year = DateTime.Now.Year;
                    txt_bill_from.Text = "01-Apr-" + Year.ToString();
                    txt_bill_to.Text = "31-Mar-" + (Year + 1).ToString();
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
    protected void txt_min_charges_TextChanged(object sender, EventArgs e)
    {
        txt_t_min_charge.Text = (12 * Convert.ToInt16(txt_min_charges.Text)).ToString();
        txt_rebate_per.Focus();
    }
    protected void txt_rebate_per_TextChanged(object sender, EventArgs e)
    {
        txt_rebate_t_amt.Text = (Convert.ToInt16(txt_t_min_charge.Text) * Convert.ToInt16(txt_rebate_per.Text) / 100).ToString();
        txt_bill_date.Focus();
    }

    protected void btnPrint_Click(object sender, EventArgs e)
    {
        //Response.Redirect("~/Report/JAL_BILL_NEW.aspx?bill_no=" + txt_bill_no.Text + "&id=2");
        Literal lt = new Literal();
        lt.Text = "<Script>window.open('../Report/JAL_BILL_new.aspx?bill_no=" + txt_bill_no.Text + "&id=2', 'mywin','left=20,top=20,width=1024,height=675,toolbar=1,scrollbars=1,resizable=1')</Script>";
        this.Controls.Add(lt);
    }
}
