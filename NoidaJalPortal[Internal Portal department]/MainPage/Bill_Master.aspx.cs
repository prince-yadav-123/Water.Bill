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

public partial class MainPage_Bill_Master : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {


            string[] colParam = { "Sector_id", "Sector_id as Sector_id1" };
            string[] condParam = { "sector_no is not null" };

            UI.GeneralFunctions.CInst().DropDownFill(ddl_Sector, "Sector_Detail", colParam, condParam, "sector_no");
        }

    }
    protected void btnView_Click(object sender, EventArgs e)
    {

    }
}
