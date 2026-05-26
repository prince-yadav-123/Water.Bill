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

public partial class Search_complete_search : System.Web.UI.Page
{
    DataSet objDs = new DataSet();
    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {

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
            GridView1.DataSource = objDs.Tables[1];
            GridView1.DataBind();            
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



    protected void btnView_Click(object sender, ImageClickEventArgs e)
    {

        GetRecords();

    }
    protected void ddlSector_SelectedIndexChanged(object sender, EventArgs e)
    {

    }
}
