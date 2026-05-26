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

public partial class Report_BlockWiseTotal : System.Web.UI.Page
{
    int total = 0;
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            ddlSector.Focus();
            string[] colParam = { "Sector_id", "Sector_id as Sector_id1" };
            string[] condParam = { "sector_no is not null" };

            UI.GeneralFunctions.CInst().DropDownFill(ddlSector, "Sector_Detail", colParam, condParam, "sector_no");

        }
    }
    protected void btnSearch_Click(object sender, ImageClickEventArgs e)
    {
        Literal lt = new Literal();
        lt.Text = "<script>window.open('../Report/BlockWise.aspx?Sector=" + ddlSector.SelectedValue + "','newwindow','width=800 ,height=600,scrollbars=yes');</script>";
        this.Controls.Add(lt);
    }
}
