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

public partial class MainPage_Manual_calculation : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }

    
    protected void ddl_bill_type_SelectedIndexChanged(object sender, EventArgs e)
    {
        Literal lt = new Literal();
        string[] valparam = {   "@P_AMOUNT,"+txt_amt.Text,			                    
                                "@P_cess_AMT,"+ txt_Cess.Text,
                                "@P_YEAR_from,"+ txt_from.Text,
                                "@P_YEAR_to,"+ txt_to.Text,                                                              
                                "@P_OPERATION_TYPE ,"+ddl_bill_type.SelectedValue

                            };
        string returnval = BAL.BAL.CInst().ProcedureCallSingleNew_string("PRCO_CALCULATOR", valparam);
        string[] returnval1 = returnval.Split('~');
        if (returnval.ToString() != "")
        {
            txt_arear.Text = returnval1[0].ToString();
            txt_arrear_int.Text = returnval1[1].ToString();
            txt_CessAmt.Text = returnval1[2].ToString();
            txt_cess_int.Text = returnval1[3].ToString();
        }
        else
        {
            txt_arear.Text = "";
            txt_arrear_int.Text = "";
            txt_CessAmt.Text = "";
            txt_cess_int.Text = "";
        }

    }
}
