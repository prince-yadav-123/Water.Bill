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
public partial class Challan : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        string[] logColParam1 = { "Challan_Content" };       
        string[] logCondParam1 = { "CONS_NO='" + Request.QueryString["cons_no"].ToString() + "'" };
        DataSet dszoneName1 = BAL.BAL.CInst().SelectTable("VIEW_JAL_BILL_MASTER", logColParam1, logCondParam1, "VIEW_JAL_BILL_MASTER", "CONS_NO");
        dv_challan_content.InnerHtml = dszoneName1.Tables[0].Rows[0][0].ToString();
    }
}
