using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Data;
using System.Web.Script.Serialization;
using System.Web.Services;


public partial class Update_Generate_bill : System.Web.UI.Page
{
    string date = System.DateTime.Now.ToShortDateString();
    double GSTAmount;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            BindBank();
            getCalenderDefaultdate();
        }

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

    public void getCalenderDefaultdate()
    {

        DateTime now = DateTime.Now;
        var startDate = new DateTime(now.Year, now.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);
        Bill_due_date.Text = endDate.ToString("dd-MMM-yyyy");
    }
    public void BindBank()
    {
        string[] colParamBANK = { "Bank_BranchCode", "bankName" };
        string[] condParamBANK = { "BranchType='debit' and BankType='debit' and IsActive=1" };
        UI.GeneralFunctions.CInst().DropDownFill(ddlBank, "BANK_MASTER", colParamBANK, condParamBANK, "Bank_BranchCode");
    }
    public void save()
    {
       DataSet dsChallanStatus = new DataSet();
       string[] logColParam = { "*" };
       string[] logCondParam = { "CONS_NO='" + txt_cons_no.Text.Trim() + "' and PAID_AMT='" + txt_paid_amt.Text.Trim() + "'" };
       dsChallanStatus = BAL.BAL.CInst().SelectTable("CHALLAN", logColParam, logCondParam, "CHALLAN", "CONS_NO");
       if (dsChallanStatus.Tables["CHALLAN"].Rows.Count > 0 || txt_paid_amt.Text.Trim().ToString() == "0")
       {

                   Literal lt = new Literal();
                   string[] valparam = {"@P_CONS_NO,"+txt_cons_no.Text,			                    
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
                                    "@P_OLD_RATE,"+txt_old_rate.Value,
                                    "@P_BILL_TYPE,"+bill_type.Value,
                                    "@P_LAST_PAID_AMT,"+txt_paid_amt.Text,
                                    "@P_BANK_CODE,"+ddlBank.SelectedValue,
                                    "@P_PAYMENT_TYPE,"+rblPaymentType.SelectedValue,
                                    "@USERID,"+Session["USERID"].ToString(),
                                    "@P_Rid,"+txtRid.Text,
				    "@P_Payment_Mode,2",
			            "@P_OPERATION_TYPE ,1"                                 
                                };
                   string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_JAL_PRINT_BILL_MASTER_WITH_NEW_RATE", valparam);
                   //string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_JAL_PRINT_BILL_MASTER_WITH_NEW_RATE_GST", valparam);
                   string[] returnVal1 = returnval.Split('~');
                   if (returnVal1[0].ToString() == "0" || returnVal1[0].ToString() == "1")
                   {
                       //lt.Text = "<Script>window.open('../Report/JAL_BILL_new.aspx?bill_no=" + returnVal1[1].ToString() + "&block=0&id=2', 'mywin','left=20,top=20,width=1024,height=675,toolbar=1,scrollbars=1,resizable=1')</Script>";
                       //this.Controls.Add(lt);
                       //txt_bill_no.Text = returnVal1[1].ToString();
                       //Bill_WeBupdate(returnVal1[1].ToString());
                       //btnPrint.Visible = true;

                       txt_bill_no.Text = returnVal1[1].ToString();
                       GetData(txt_bill_no.Text);

                   }
                   else
                   {
                       lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(Int32.Parse(returnval)) + "')</script>";
                       this.Controls.Add(lt);
                   }
    

       }

       else
       {
           Response.Write("<script>alert('Last Paid Amount "+txt_paid_amt.Text+" does not exists please challan entry first.');</script>");

           //Response.Write("Last Paid Amount does not exists please challan entry first.");
       }

    }

    public void GetData(string BILL_NO)
    {

        string[] col = new string[] { "*" };
        string[] cond = new string[] { "BILL_NO='" + BILL_NO + "' AND STATUS='1' AND BILL_COUNT='1'" };
        DataSet ds_Web = BAL.BAL.CInst().SelectTable("VIEW_JAL_BILL_MASTER_WEB", col, cond, "WEB_BILL_DETAIL");
        DataTable _dt = ds_Web.Tables[0];
        string sJSON = dataTableToJSON(_dt);
        ClientScript.RegisterStartupScript(this.GetType(), "Get", "Get('" + sJSON + "')", true);     
        //return sJSON;

    }
    //[WebMethod]
    //public static string GetChallanNo(string ChallanNo)
    //{
    //    return ChallanNo.ToString();
    //    //HttpContext.Current.Session["Name"] = name;
    //    //return "Hello " + HttpContext.Current.Session["Name"] + Environment.NewLine + "The Current Time is: " + DateTime.Now.ToString();
    //}
    public static string dataTableToJSON(DataTable table)
    {
        var list = new List<Dictionary<string, object>>();
        foreach (DataRow row in table.Rows)
        {
            var dict = new Dictionary<string, object>();
            foreach (DataColumn col in table.Columns)
            {
                dict[col.ColumnName] = (Convert.ToString(row[col]));
            }
            list.Add(dict);
        }
        JavaScriptSerializer serializer = new JavaScriptSerializer();
        return serializer.Serialize(list);
    }
    public void saveGST()
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
                                "@P_OLD_RATE,"+txt_old_rate.Value,
                                "@P_BILL_TYPE,"+bill_type.Value,                               
                                "@P_LAST_PAID_AMT,"+txt_paid_amt.Text,
                                 "@P_GST_AMT,"+hdGST.Value,                               
			                    "@P_OPERATION_TYPE ,1" 
                            
                            };
        //string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_JAL_PRINT_BILL_MASTER_WITH_NEW_RATE", valparam);
        string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_JAL_PRINT_BILL_MASTER_WITH_NEW_RATE_GST", valparam);



        string[] returnVal1 = returnval.Split('~');
        if (returnVal1[0].ToString() == "0" || returnVal1[0].ToString() == "1")
        {
            lt.Text = "<Script>window.open('../Report/JAL_BILL_new.aspx?bill_no=" + returnVal1[1].ToString() + "&block=0&id=2', 'mywin','left=20,top=20,width=1024,height=675,toolbar=1,scrollbars=1,resizable=1')</Script>";
            this.Controls.Add(lt);
            txt_bill_no.Text = returnVal1[1].ToString();
            Bill_WeBupdate(returnVal1[1].ToString());

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
        txtRid.Text = "";
        Bill_due_date.Text = "";
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
        Button2.Enabled = true;
       
      
    }

    public void Bill_WeBupdate(string BILL_NO)
    {
        string value;
        try
        {
            string[] col = new string[] { "*" };
            string[] cond = new string[] { "BILL_NO='" + BILL_NO.ToString() + "' AND STATUS='1' AND BILL_COUNT='1'" };
            DataSet ds_Web = BAL.BAL.CInst().SelectTable("VIEW_JAL_BILL_MASTER_WEB", col, cond, "WEB_BILL_DETAIL");
            if (ds_Web != null)
            {
                if (ds_Web.Tables.Count > 0)
                {
                    if (ds_Web.Tables["WEB_BILL_DETAIL"].Rows.Count > 0)
                    {
                        string BILL_NO_DB = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["BILL_NO"].ToString();
                        string CON_TP = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["CON_TP"].ToString();
                        string CONS_CTG = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["CONS_CTG"].ToString();
                        string BILL_DATE = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["BILL_DATE"].ToString();
                        string BILL_DUE_DATE = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["BILL_DUE_DATE"].ToString();
                        string BILL_DATE_FROM = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["BILL_DATE_FROM"].ToString();
                        string BILL_DATE_TO = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["BILL_DATE_TO"].ToString();
                        string MIN_RATE = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["MIN_RATE"].ToString();
                        string CESS_AMT = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["CESS_AMT"].ToString();
                        string AREAR = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["AREAR"].ToString();
                        string AREAR_TEXT = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["AREAR_TEXT"].ToString();
                        string AREAR_INT = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["AREAR_INT"].ToString();
                        string AREAR_INT_TEXT = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["AREAR_INT_TEXT"].ToString();
                        string LAST_BILL_EXTRA = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["LAST_BILL_EXTRA"].ToString();
                        string CONS_NO = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["CONS_NO"].ToString();
                        string CONS_NM1 = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["CONS_NM1"].ToString();
                        string CONS_NM2 = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["CONS_NM2"].ToString();
                        string FLAT_TYPE = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["FLAT_TYPE"].ToString();
                        string FLAT_NO = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["FLAT_NO"].ToString();
                        string BLK_NO = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["BLK_NO"].ToString();
                        string SECTOR = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["SECTOR"].ToString();
                        string property_no = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["property_no"].ToString();
                        string PLOT_SIZE = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["PLOT_SIZE"].ToString();
                        string PIPE_SIZE = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["PIPE_SIZE"].ToString();
                        string REG_NO = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["REG_NO"].ToString();
                        string CONN_DT = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["CONN_DT"].ToString();
                        string ESTI_NO = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["ESTI_NO"].ToString();
                        string ESTI_AMT = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["ESTI_AMT"].ToString();
                        string SECU = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["SECU"].ToString();
                        string ESTI_DT = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["ESTI_DT"].ToString();
                        string ESTI1_AMT = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["ESTI1_AMT"].ToString();
                        string DEV_TYPE = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["DEV_TYPE"].ToString();
                        string OLD_NEW_EN = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["OLD_NEW_EN"].ToString();
                        string OLD_NEW_UP = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["OLD_NEW_UP"].ToString();
                        string MOB_NO = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["MOB_NO"].ToString();
                        string EMAIL_ID = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["EMAIL_ID"].ToString();
                        string CONS_ADDRESS = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["CONS_ADDRESS"].ToString();
                        string STATUS = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["STATUS"].ToString();
                        string MIN_TOTAL_AMT = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["MIN_TOTAL_AMT"].ToString();
                        string BILL_REBATE_PER = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["BILL_REBATE_PER"].ToString();
                        string BILL_REBATE_AMT = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["BILL_REBATE_AMT"].ToString();
                        string TOTAL_BILL_AMT = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["TOTAL_BILL_AMT"].ToString();
                        string BEFORE_DATE = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["BEFORE_DATE"].ToString();
                        string AFTER_DATE = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["AFTER_DATE"].ToString();
                        string AFTER_DATE_AMT = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["AFTER_DATE_AMT"].ToString();
                        string Div_type = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["Div_type"].ToString();
                        string bill_after_sep_amt = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["bill_after_sep_amt"].ToString();
                        string adv_amt = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["adv_amt"].ToString();
                        string PRINT_STATUS = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["PRINT_STATUS"].ToString();
                        string B_MAY = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["B_MAY"].ToString();
                        string B_APR = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["B_APR"].ToString();
                        string B_SEP = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["B_SEP"].ToString();
                        string OLD_RATE = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["OLD_RATE"].ToString();
                        string LAST_PAID_AMT = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["LAST_PAID_AMT"].ToString();
                        string BILL_COUNT = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["BILL_COUNT"].ToString();
                        string KHASRA_NO = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["KHASRA_NO"].ToString();
                        string VILLGAE_NAME = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["VILLGAE_NAME"].ToString();
                        string SCHEME_ID = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["SCHEME_ID"].ToString();
                        string BILL_PERCENTAGE = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["BILL_PERCENTAGE"].ToString();
                        string TEL_NO = ds_Web.Tables["WEB_BILL_DETAIL"].Rows[0]["TEL_NO"].ToString();
                       
                        string WEBCODE = "noida@2017";
                        ds_Web.Reset();
                        ds_Web.Dispose();
                        com.noidajalonline.www.Bill_Details x = new com.noidajalonline.www.Bill_Details();
                        value = x.UpdateWaterBill(BILL_NO, CON_TP, CONS_CTG, BILL_DATE, BILL_DUE_DATE, BILL_DATE_FROM, BILL_DATE_TO, MIN_RATE, CESS_AMT, AREAR, AREAR_TEXT, AREAR_INT, AREAR_INT_TEXT, LAST_BILL_EXTRA, CONS_NO, CONS_NM1, CONS_NM2, FLAT_TYPE, FLAT_NO, BLK_NO, SECTOR, property_no, PLOT_SIZE, PIPE_SIZE, REG_NO, CONN_DT, ESTI_NO, ESTI_AMT, SECU, ESTI_DT, ESTI1_AMT, DEV_TYPE, OLD_NEW_EN, OLD_NEW_UP, MOB_NO, EMAIL_ID, CONS_ADDRESS, STATUS, MIN_TOTAL_AMT, BILL_REBATE_PER, BILL_REBATE_AMT, TOTAL_BILL_AMT, BEFORE_DATE, AFTER_DATE, AFTER_DATE_AMT, Div_type, bill_after_sep_amt, adv_amt, PRINT_STATUS, B_MAY, B_APR, B_SEP, OLD_RATE, LAST_PAID_AMT, BILL_COUNT, KHASRA_NO, VILLGAE_NAME, SCHEME_ID, BILL_PERCENTAGE, TEL_NO, WEBCODE);

                        //string value1 = x.BillDetails(BILL_NO_DB);
                        // x.BeginUpdateWaterBill(BILL_NO, CON_TP, CONS_CTG, BILL_DATE, BILL_DUE_DATE, BILL_DATE_FROM, BILL_DATE_TO, MIN_RATE, CESS_AMT, AREAR, AREAR_TEXT, AREAR_INT, AREAR_INT_TEXT, LAST_BILL_EXTRA, CONS_NO, CONS_NM1, CONS_NM2, FLAT_TYPE, FLAT_NO, BLK_NO, SECTOR, property_no, PLOT_SIZE, PIPE_SIZE, REG_NO, CONN_DT, ESTI_NO, ESTI_AMT, SECU, ESTI_DT, ESTI1_AMT, DEV_TYPE, OLD_NEW_EN, OLD_NEW_UP, MOB_NO, EMAIL_ID, CONS_ADDRESS, STATUS, MIN_TOTAL_AMT, BILL_REBATE_PER, BILL_REBATE_AMT, TOTAL_BILL_AMT, BEFORE_DATE, AFTER_DATE, AFTER_DATE_AMT, Div_type, bill_after_sep_amt, adv_amt, PRINT_STATUS, B_MAY, B_APR, B_SEP, OLD_RATE, LAST_PAID_AMT, BILL_COUNT, KHASRA_NO, VILLGAE_NAME, SCHEME_ID, BILL_PERCENTAGE, TEL_NO, WEBCODE);


                    }
                }
            }

        }
        catch
        {
            //ds.Tables[0].Rows[0][0] = 11;
            value = "1";
        }

    }


    public void GenerateBill(string cons_no)
    {

        string[] colParam2 = new string[] { "CONS_NO,NODUES_DT,CONS_NM1,FLAT_TYPE,SECTOR,LTRIM(RTRIM(CASE WHEN CONS_CTG= 'R' THEN 'Regular' when CONS_CTG= 'T' then  'Temporary' else 'Temporary' END)) AS CON_TP,BLK_NO,FLAT_NO,isnull(plot_size,0) as PLOT_SIZE,MOB_NO,Rid,cast(CAL_DATE as datetime) as CAL_DATE,PIPE_SIZE,CONS_CTG" };
        string[] condParam2 = new string[] { "CONS_NO='" + cons_no.ToString() + "'", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };
        DataSet ds_Update = BAL.BAL.CInst().SelectTable("VIEW_CONSUMER_DETAILS_MASTER", colParam2, condParam2, "CONSUMER_DETAILS_MASTER");
        //DataSet ds_Update1 = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_TRANS", colParam2, condParam2, "CONSUMER_DETAILS_TRANS");
        if (ds_Update != null)
        {
            if (ds_Update.Tables.Count > 0)
            {
                if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0 && (Convert.ToDateTime(ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CAL_DATE"]).ToString("dd-MMM-yyyy") == "31-Mar-2027" || Convert.ToDateTime(ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CAL_DATE"]).ToString("dd-MMM-yyyy") == "31-MAR-2027"))
                {                    
                    Response.Write("<script>alert('already paid');</script>");
                }
                else if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0 && ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_CTG"].ToString() == "S")
                {
                    Response.Write("<script>alert('staff');</script>");
                }
                else if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0 && ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_CTG"].ToString() == "M")
                {
                    Response.Write("<script>alert('maintenance');</script>");
                }
                else if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0 && ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_CTG"].ToString() == "D")
                {
                    Response.Write("<script>alert('disconnection');</script>");
                }
                else if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0 && ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_CTG"].ToString() == "CC")
                {
                    Response.Write("<script>alert('court case');</script>");
                }
                else if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0 && ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["PIPE_SIZE"].ToString() == "0")
                {
                    Response.Write("<script>alert('pipe size 0');</script>");
                }
                else if (ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows.Count > 0 && ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["MOB_NO"].ToString() == "0")
                {
                    Response.Write("<script>alert('mobile no. not exists');</script>");
                }
                else
                {                   
                    txt_cons_no.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_NO"].ToString();
                    txtRid.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["Rid"].ToString();
                    txt_due_date.Text = Bill_due_date.Text;
                    txt_Cons_nm.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CONS_NM1"].ToString();
                    txt_Flat_type.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["FLAT_TYPE"].ToString() + "(" + ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["plot_size"].ToString() + ")";
                    txt_sector.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["SECTOR"].ToString();
                    txt_category.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["CON_TP"].ToString();
                    txt_block_no.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["BLK_NO"].ToString();
                    txt_Flat_No.Text = ds_Update.Tables["CONSUMER_DETAILS_MASTER"].Rows[0]["FLAT_NO"].ToString();
                    int Year = Convert.ToDateTime(txt_due_date.Text).Year;
                    int month = Convert.ToDateTime(txt_due_date.Text).Month;
                    if (month >= 1 && month <= 3)
                    {
                        txt_bill_from.Text = "01-Apr-" + (Year - 1).ToString();
                        txt_bill_to.Text = "31-Mar-" + Year.ToString();
                    }
                    else
                    {

                        txt_bill_from.Text = "01-Apr-" + Year.ToString();
                        txt_bill_to.Text = "31-Mar-" + (Year + 1).ToString();
                    }
                    GetArrear();
                    txt_bill_date.Text = Convert.ToDateTime(date).ToString("dd-MMM-yyyy");
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
    public void GetCess()
    {
        double amtOld = 0;
        string valparamCess = txt_cons_no.Text;

        DataSet returnvalcess = BAL.BAL.CInst().FunctionCall("AREAR_CAL_YEAR_CESS", valparamCess);
        txt_cess_amt.Text = (Convert.ToDouble(returnvalcess.Tables[0].Rows[0][0].ToString())).ToString();
    }
    private string getxml(string st)
    {
        string st1 = "";
        string[] colParam2 = new string[] { "*" };
        string[] condParam2 = new string[] { "CONS_NO='" + st.ToString() + "'", "DEV_TYPE='" + Session["DEV_TYPE"] + "'" };
        DataSet ds_xml = BAL.BAL.CInst().SelectTable("CONSUMER_XML_DETAILS", colParam2, condParam2, "CONSUMER_XML_DETAILS");
        //DataSet ds_Update1 = BAL.BAL.CInst().SelectTable("CONSUMER_DETAILS_TRANS", colParam2, condParam2, "CONSUMER_DETAILS_TRANS");
        if (ds_xml != null)
        {
            if (ds_xml.Tables.Count > 0)
            {
                if (ds_xml.Tables["CONSUMER_XML_DETAILS"].Rows.Count > 0)
                {
                    for (int i = 0; i < ds_xml.Tables[0].Rows.Count; i++)
                    {

                        st1 = st1 + ds_xml.Tables["CONSUMER_XML_DETAILS"].Rows[i][1].ToString();
                    }


                    DataSet returnvalFun = BAL.BAL.CInst().FunctionCall_table("XML_TO_TABLE", st1.ToUpper());
                    gvParentGrid.DataSource = returnvalFun;
                    gvParentGrid.DataBind();
                }
            }
        }
        return st1;
    }
    public void OldRate()
    {
        double amtOld = 0;
        string valparamFun = txt_cons_no.Text + "','" + "30-Jun-2013";

        DataSet returnvalFun = BAL.BAL.CInst().FunctionCall("Find_Rate", valparamFun);
        txt_old_rate.Value = (Convert.ToDouble(returnvalFun.Tables[0].Rows[0][0].ToString())).ToString();

    }
    public void GetArrear()
    {
        double amt = 0;
        string returnval1 = "";
        
        //FOR GST ON INT ON ARREAR

        lblduedate.Text = Bill_due_date.Text;
        lblduedate30jun17.Text = "30-JUN-2017";
        string returnval2 = "";
        string[] valparam2 = {   "@A_CONSNO,"+txt_cons_no.Text,
                                 "@A_DATE,30-JUN-2017"
                                 };
        returnval2 = BAL.BAL.CInst().ProcedureCallSingleNew_string("AREAR_CALCULATION_FULL", valparam2);

        //FOR GST ON INT ON ARREAR


        string valparamFun = txt_cons_no.Text + "','" + DateTime.Now.Date.ToString("dd-MMM-yyyy");
        DataSet returnvalFun = BAL.BAL.CInst().FunctionCall("Find_Rate", valparamFun);
        amt = (Convert.ToDouble(returnvalFun.Tables[0].Rows[0][0].ToString()) * 12);

        string[] valparam1 = {   "@A_CONSNO,"+txt_cons_no.Text,
                                     "@A_DATE,"+Bill_due_date.Text
                                 };


        returnval1 = BAL.BAL.CInst().ProcedureCallSingleNew_string("AREAR_CALCULATION_FULL", valparam1);





        //////END//////

        OldRate();
        if (returnval1 != "")
        {
            string x = "";
            //for (int i = 0; i < returnval1.Tables[0].Rows.Count; i++)
            //{

            //    x = x + returnval1.Tables[0].Rows[i][0].ToString();
            //}
            x = getxml(txt_cons_no.Text);

            string[] returnVal1 = returnval1.ToString().Split('~');


            GetCess();
            double minrate = Convert.ToDouble(returnVal1[0].ToString());
            txt_t_min_charge.Text = minrate.ToString();
            txt_paid_amt.Text = returnVal1[1].ToString();

            double arear_amt = Convert.ToDouble(returnVal1[2].ToString());
            txt_arear_amt.Text = arear_amt.ToString();

            double intrest_amt = Convert.ToDouble(returnVal1[3].ToString());
            txt_intrest_amt.Text = intrest_amt.ToString();

            //FOR GST ON INT ON ARREAR
            string[] returnVal2 = returnval2.ToString().Split('~');
            double intrest_amt_30jun17 = Convert.ToDouble(returnVal2[3].ToString());
            txt_intrest_amt_30JUN17.Text = intrest_amt_30jun17.ToString();


            double diff_in_int = intrest_amt - intrest_amt_30jun17;

            lblIntUptoDate.Text = intrest_amt.ToString();
            lblIntUpto30Jun17.Text = intrest_amt_30jun17.ToString();
            lblIntUptoDate1.Text = intrest_amt.ToString();
            lblIntUpto30Jun171.Text = intrest_amt_30jun17.ToString();
            lblDiff1.Text = diff_in_int.ToString();
            lblDiff2.Text = diff_in_int.ToString();

            double cgst = diff_in_int * (.09);
            double sgst = diff_in_int * (.09);

            GSTAmount = cgst + sgst;

            hdGST.Value = Math.Round(GSTAmount).ToString();
            txtCGST_int.Text = Math.Round(cgst).ToString();
            txtSGST_int.Text = Math.Round(sgst).ToString();

            //////END//////


            txt_adv_amt.Text = returnVal1[4].ToString();
            bill_type.Value = UI.FilterText.Filter(x);

            txt_min_charges.Text = returnvalFun.Tables[0].Rows[0][0].ToString();
            OldRate();


            GetRebate(minrate);


        }

    }
    public void GetRebate(double min_rate)
    {
        string per = "";
      //  if (Convert.ToDateTime(Bill_due_date.Text).Month > 3 && Convert.ToDateTime(Bill_due_date.Text).Month <= 5 && Convert.ToInt32(min_rate) <= 15000)
        //if (Convert.ToDateTime(Bill_due_date.Text).Month > 3 && Convert.ToDateTime(Bill_due_date.Text).Month <= 5 && Convert.ToInt32(min_rate) <= 41000)
	 if (Convert.ToDateTime(Bill_due_date.Text).Month > 3 && Convert.ToDateTime(Bill_due_date.Text).Month <= 5)
       {
     	  per = "10";
	
       }
     	//  else if (Convert.ToDateTime(Bill_due_date.Text).Month > 5 && Convert.ToDateTime(Bill_due_date.Text).Month <= 9 && Convert.ToInt32(min_rate) <= 41000)
	else if (Convert.ToDateTime(Bill_due_date.Text).Month > 5 && Convert.ToDateTime(Bill_due_date.Text).Month <= 9 && Convert.ToInt32(min_rate) <= 16000)
       {
           per = "5";
       }
        else
        {
            per = "0";
        }
        txt_rebate_per.Text = per;
        txt_rebate_t_amt.Text = Math.Round(Convert.ToDouble((Convert.ToDouble(min_rate) * Convert.ToDouble(per) / 100)), 0).ToString();

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
        lt.Text = "<Script>window.open('../Report/JAL_BILL_new.aspx?bill_no=" + txt_bill_no.Text + "&block=0&id=2', 'mywin','left=20,top=20,width=1024,height=675,toolbar=1,scrollbars=1,resizable=1')</Script>";
        this.Controls.Add(lt);
    }
    protected void gvUserInfo_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.Header)
        {
            e.Row.Cells[5].Style.Add("width", "90px");
            e.Row.Cells[6].Style.Add("width", "90px");
            e.Row.Cells[7].Style.Add("width", "90px");
        }
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            e.Row.Cells[0].Style.Add("text-align", "center");
            e.Row.Cells[1].Style.Add("text-align", "center");
            e.Row.Cells[2].Style.Add("text-align", "right");
            e.Row.Cells[3].Style.Add("text-align", "right");
            e.Row.Cells[4].Style.Add("text-align", "right");
            e.Row.Cells[5].Style.Add("text-align", "right");
            e.Row.Cells[6].Style.Add("text-align", "right");
            e.Row.Cells[7].Style.Add("text-align", "center");
            e.Row.Cells[0].Style.Add("padding-left", "5px");
            e.Row.Cells[1].Style.Add("padding-left", "5px");
            e.Row.Cells[2].Style.Add("padding-right", "5px");
            e.Row.Cells[3].Style.Add("padding-right", "5px");
            e.Row.Cells[4].Style.Add("padding-right", "5px");
            e.Row.Cells[5].Style.Add("padding-right", "5px");
            e.Row.Cells[6].Style.Add("padding-right", "5px");
            e.Row.Cells[7].Style.Add("padding-left", "5px");
            e.Row.Cells[6].Style.Add("width", "90px");
            e.Row.Cells[5].Style.Add("width", "90px");
            e.Row.Cells[7].Style.Add("width", "90px");
            if (e.Row.Cells[5].Text == "31-Mar-1990")
            {
                e.Row.Cells[5].Text = "";
            }

        }
    }
    protected void link_cal_Click(object sender, EventArgs e)
    {
        Literal lt = new Literal();
        if (txt_cons_no.Text != "")
        {

            Session["cons_no"] = txt_cons_no.Text;
            lt.Text = "<Script>window.open('../MainPage/update_Cal_Date.aspx', 'mywin','left=20,top=20,width=500,height=200,toolbar=1,scrollbars=1,resizable=1')</Script>";
            //lt.Text = "<Script> window.showModalDialog('../MainPage/update_Cal_Date.aspx', 'newwindow','center:yes;dialogWidth:500px;dialogHeight:200px; resizable:no;')</Script>";
            this.Controls.Add(lt);
        }
        else
        {
            lt.Text = "<Script>alert('Error!Consumer No can not be blank!!!')</Script>";
            this.Controls.Add(lt);
            txt_cons_no.Focus();
        }
        //this.Controls.Remove(lt);
        //window.showModalDialog("../MainPage/update_Cal_Date.aspx", "newwindow", "center:yes;dialogWidth:500px;dialogHeight:200px; resizable:no;");
    }
    protected void btnSave_Click(object sender, ImageClickEventArgs e)
    {
        
    }
    protected void btnSave_Click(object sender, EventArgs e)
    {
        
    }
    protected void Button1_Click(object sender, EventArgs e)
    {
        saveGST();
    }
    protected void Button2_Click(object sender, EventArgs e)
    {
        save();

        Button2.Enabled = false;
    }
}
