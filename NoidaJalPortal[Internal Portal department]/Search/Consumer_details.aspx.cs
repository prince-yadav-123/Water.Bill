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

public partial class Consumer_details : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {




    }

    public void GetChallan(string cuns_no)
    {
        string[] colBank = { "ROW_NUMBER() Over (Order by PAY_DATE desc) As S_no,isnull(BL_PER_FR,'01-JAN-1900') as BL_PER_FR,isnull(BL_PER_TO,'01-JAN-1900') as BL_PER_TO,isnull(DUE_DT,'01-JAN-1900') as DUE_DT,BILL_AMT,isnull(SURCHARGE,0) as SURCHARGE,isnull(PAID_AMT,0) as PAID_AMT,isnull(PAY_DATE,'01-JAN-1900') as PAY_DATE,isnull(RECP_NO,0) as RECP_NO,isnull(T_FEE,0) as T_FEE,BNK_CD,BR_NM" };
        string[] condBank = { "cons_no='" + cuns_no + "' and BILL_AMT is not null" };
        DataSet empDs = BAL.BAL.CInst().SelectTable("CHALAN_DETAILS", colBank, condBank, "EmpData", "PAY_DATE desc");
        if (empDs != null)
        {
            if (empDs.Tables.Count > 0)
            {
                if (empDs.Tables["EmpData"].Rows.Count > 0)
                {
                    gvChallan.DataSource = empDs;
                    gvChallan.DataBind();
                }
            }
        }
    }
    protected void linkPayment_Click(object sender, EventArgs e)
    {
        Response.Redirect("~/Disclaimer.aspx");
    }

    public void getBill()
    {
        string[] para ={
                            "@P_CONS_NO,"+txt_cons_no.Text,                            
                            "@P_OPERATION_TYPE,2" 
                            };
        DataSet objDs = BAL.BAL.CInst().ProcedureCallMulti_datset("PRCO_BILL_SEARCH", para);
        if (objDs != null)
        {
            if (objDs.Tables.Count > 0)
            {
                if (objDs.Tables[0].Rows.Count > 0)
                {
                    gvChildGrid.DataSource = objDs;
                    gvChildGrid.DataBind();
                }
            }
        }
    }
    protected void gvChildGrid_RowDataBound(object sender, GridViewRowEventArgs e)
    {
        if (e.Row.RowType == DataControlRowType.DataRow)
        {
            LinkButton lnk = (LinkButton)e.Row.Cells[7].FindControl("linkPrint");
            lnk.CommandArgument = e.Row.Cells[2].Text;
           // cons_no.Value = e.Row.Cells[1].Text;
            LinkButton lnk1 = (LinkButton)e.Row.Cells[8].FindControl("linkEdit");
            lnk1.CommandArgument = e.Row.Cells[1].Text;
            LinkButton lnk2 = (LinkButton)e.Row.Cells[9].FindControl("linkNew");
            lnk2.CommandArgument = e.Row.Cells[1].Text;

        }
    }
    protected void lbRefund_Command(object sender, CommandEventArgs e)
    {

        string x = e.CommandArgument.ToString();
        Literal lt = new Literal();
        lt.Text = "<Script>window.open('../Report/JAL_BILL_new.aspx?bill_no=" + x + "&id=2', 'mywin','left=20,top=20,width=1024,height=675,toolbar=1,scrollbars=1,resizable=1')</Script>";
        this.Controls.Add(lt);
        // Response.Redirect("~/Report/JAL_BILL_new.aspx?cons_no="+x+"");
    }
    protected void lbRefund1_Command(object sender, CommandEventArgs e)
    {
        Session["cons_no"] = e.CommandArgument.ToString();
        Literal lt = new Literal();
        lt.Text = "<Script>window.open('../Update/Generate_bill_update.aspx','_blank')</Script>";
        this.Controls.Add(lt);
        //Response.Redirect("~/Update/Generate_bill_update.aspx");
    }
    protected void lbRefund2_Command(object sender, CommandEventArgs e)
    {
        Session["cons_no"] = e.CommandArgument.ToString();
        Literal lt = new Literal();
        lt.Text = "<Script>window.open('../MainPage/Generate_bill.aspx','_blank')</Script>";
        this.Controls.Add(lt);
        //Response.Redirect("~/MainPage/Generate_bill.aspx");
    }
    protected void btnView_Click(object sender, ImageClickEventArgs e)
    {
        string[] colBank = { "CONS_NO,case CONS_CTG when 'R' then 'Regular' when 'T' then 'Temporary' else '-' end as CONS_CTG,CONS_NM1 as CONS_NM1 ,FLAT_TYPE,(SECTOR+'/'+isnull(BLK_NO,'NA')+'-'+FLAT_NO) as SECTOR,PLOT_SIZE,case CON_TP when 'R' then 'Residential'when 'C' then 'Commercial' when 'I' then 'Industrial' when 'T' then 'Industrial' when 'S' then 'Staff' else '-' end as CON_TP,PIPE_SIZE,REG_NO,isnull(ESTI_DT,'01-JAN-1900') as ESTI_DT,isnull(CONN_DT,'01-JAN-1900') as CONN_DT,0 as NODUE_AMT,ESTI_NO,isnull(NODUES_DT,'01-JAN-1900') as NODUES_DT,isnull(ESTI1_AMT,0) as ESTI1_AMT,isnull(NODUES_DT,'01-JAN-1900') as NODUE_DT" };
        string[] condBank = { "cons_no='" + txt_cons_no.Text + "'" };
        DataSet empDs = BAL.BAL.CInst().SelectTable("VIEW_CONSUMER_DETAILS_MASTER", colBank, condBank, "EmpData");
        if (empDs != null)
        {
            if (empDs.Tables.Count > 0)
            {
                if (empDs.Tables["EmpData"].Rows.Count > 0)
                {
                    Session["Name"] = empDs.Tables["EmpData"].Rows[0]["CONS_NM1"].ToString();
                    Session["Property"] = empDs.Tables["EmpData"].Rows[0]["SECTOR"].ToString();
                    lblconsumerNo.Text = empDs.Tables["EmpData"].Rows[0]["CONS_NO"].ToString();

                    lblconnCategory.Text = empDs.Tables["EmpData"].Rows[0]["CONS_CTG"].ToString();

                    lblconsumerName.Text = empDs.Tables["EmpData"].Rows[0]["CONS_NM1"].ToString();
                    lblflattype.Text = empDs.Tables["EmpData"].Rows[0]["FLAT_TYPE"].ToString();
                    lbladdress.Text = empDs.Tables["EmpData"].Rows[0]["SECTOR"].ToString();
                    lblPlotsize.Text = empDs.Tables["EmpData"].Rows[0]["PLOT_SIZE"].ToString() + " Sq Meter";

                    lblconnType.Text = empDs.Tables["EmpData"].Rows[0]["CON_TP"].ToString();

                    lblpipesize.Text = empDs.Tables["EmpData"].Rows[0]["PIPE_SIZE"].ToString() + " mm";
                    lblRegNo.Text = empDs.Tables["EmpData"].Rows[0]["REG_NO"].ToString();
                    if (Convert.ToDateTime(empDs.Tables["EmpData"].Rows[0]["ESTI_DT"].ToString()).ToString("dd-MMM-yyyy") != "01-Jan-1900")
                    {

                        lblEstDate.Text = Convert.ToDateTime(empDs.Tables["EmpData"].Rows[0]["ESTI_DT"].ToString()).ToString("dd-MMM-yyyy");
                    }
                    else
                    {
                        lblEstDate.Text = "-";
                    }
                    if (Convert.ToDateTime(empDs.Tables["EmpData"].Rows[0]["CONN_DT"].ToString()).ToString("dd-MMM-yyyy") != "01-Jan-1900")
                    {
                        lblConnDate.Text = Convert.ToDateTime(empDs.Tables["EmpData"].Rows[0]["CONN_DT"].ToString()).ToString("dd-MMM-yyyy");
                    }
                    else
                    {
                        lblConnDate.Text = "-";
                    }
                    lblNoDues.Text = empDs.Tables["EmpData"].Rows[0]["NODUE_AMT"].ToString();
                    lblEstNo.Text = empDs.Tables["EmpData"].Rows[0]["ESTI_NO"].ToString();
                    if (Convert.ToDateTime(empDs.Tables["EmpData"].Rows[0]["NODUE_DT"].ToString()).ToString("dd-MMM-yyyy") != "01-Jan-1900")
                    {

                        lblNoduesUpto.Text = Convert.ToDateTime(empDs.Tables["EmpData"].Rows[0]["NODUE_DT"].ToString()).ToString("dd-MMM-yyyy");
                    }
                    else
                    {
                        lblNoduesUpto.Text = "-";
                    }
                    lblEstAmt.Text = "-";//empDs.Tables["EmpData"].Rows[0]["ESTI1_AMT"].ToString();
                    lblDueAmt.Text = "-";

                    lblDuesDate.Text = "-";
                    GetChallan(txt_cons_no.Text);
                    getBill();
                    pnl_main.Visible = true;

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
}
