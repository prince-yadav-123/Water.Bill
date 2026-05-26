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

public partial class Report_Print_bill : System.Web.UI.Page
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
    protected void btnView_Click(object sender, ImageClickEventArgs e)
    {
        Literal lt = new Literal();
        if (rbView.SelectedValue == "S")
        {
            lt.Text = "<Script>window.open('../Report/JAL_BILL_SUMM.aspx?sector=" + ddl_Sector.SelectedValue + "&block=" + ddl_block.SelectedValue + "&ID=1&bill_no=0', 'mywin','left=20,top=20,width=1024,height=675,toolbar=1,scrollbars=1,resizable=1')</Script>";
            this.Controls.Add(lt);
        }
        else if (rbView.SelectedValue == "P")
        {
            lt.Text = "<Script>window.open('../Report/JAL_BILL_SUMM.aspx?sector=" + ddl_Sector.SelectedValue + "&block=" + ddl_block.SelectedValue + "&ID=2&bill_no=0', 'mywin','left=20,top=20,width=1024,height=675,toolbar=1,scrollbars=1,resizable=1')</Script>";
            this.Controls.Add(lt);
        }
        else if (rbView.SelectedValue == "D")
        {
            lt.Text = "<Script>window.open('../Report/Defalulter.aspx?sector=" + ddl_Sector.SelectedValue + "&block=0&id=3&bill_no=0', 'mywin','left=20,top=20,width=1024,height=675,toolbar=1,scrollbars=1,resizable=1')</Script>";
            this.Controls.Add(lt);
        }
        else if (rbView.SelectedValue == "BB")
        {
            lt.Text = "<Script>window.open('../Report/JAL_BILL_new.aspx?sector=" + ddl_Sector.SelectedValue + "&block=" + ddl_block.SelectedValue + "&id=1&bill_no=0', 'mywin','left=20,top=20,width=1024,height=675,toolbar=1,scrollbars=1,resizable=1')</Script>";
            this.Controls.Add(lt);
        }
        else
        {
            lt.Text = "<Script>window.open('../Report/JAL_BILL_new.aspx?sector=" + ddl_Sector.SelectedValue + "&block=" + ddl_block.SelectedValue + "&id=1&bill_no=0', 'mywin','left=20,top=20,width=1024,height=675,toolbar=1,scrollbars=1,resizable=1')</Script>";
            this.Controls.Add(lt);
        }
    }
    protected void ddl_Sector_SelectedIndexChanged(object sender, EventArgs e)
    {
        string[] colParam = { "block", "block as block1" };
        string[] condParam = { "sector_id='" + ddl_Sector.SelectedValue.ToString() + "'" };
        UI.GeneralFunctions.CInst().DropDownFill(ddl_block, "block_detail", colParam, condParam, "block");
    }
}
