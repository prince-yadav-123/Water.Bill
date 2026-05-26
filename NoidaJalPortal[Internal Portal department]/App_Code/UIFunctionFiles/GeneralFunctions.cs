using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using BAL;
namespace UI
{
    /// <summary>
    /// Summary description for GeneralFunctions
    /// </summary>
    public partial class GeneralFunctions
    {

        private GeneralFunctions()
        {

            //
            // TODO: Add constructor logic here
            //
        }
        /*######################################################################################################### */
        public static GeneralFunctions CInst()
        {

            return new GeneralFunctions(); ;
        }

    }
    /*######################################################################################################### */
    public partial class GeneralFunctions
    {

        /*######################################################################################################### */

        /// <summary>
        /// Method that fills DropDownList and returns DropDownList Object.
        /// </summary>
        /// <param name="ddList">Name of DropDownList to be filled.</param>
        /// <param name="TableName">Table Name of selecting records.</param>
        /// <param name="TextFieldColumn">Name of column for Text field.</param>
        /// <param name="ValueFieldColumn">Name of column for value field.</param>
        /// <param name="OrderByColumn">Column Name for Order by clause.</param>
        /// <returns></returns>
        public void DropDownFill(DropDownList ddList, string TableName, string TextFieldColumn, string ValueFieldColumn, string OrderByColumn)
        {

            ListItem lItem;
            ddList.Items.Clear();
            DataSet ds = BAL.BAL.CInst().SelectTable(TableName, "ddlDS", OrderByColumn);
            lItem = new ListItem();
            lItem.Text = "---Select-----";
            lItem.Value = "0";
            ddList.Items.Add(lItem);
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    lItem = new ListItem();
                    lItem.Text = ds.Tables[0].Rows[i][1].ToString();
                    lItem.Value = ds.Tables[0].Rows[i][0].ToString();
                    ddList.Items.Add(lItem);
                }
                ddList.SelectedValue = "0";
            }
            //ListItem lItem;
            //ddList.Items.Clear();
            //DataSet ds = 

            //try
            //{ lItem = new ListItem();
            //    lItem.Text = "---Select-----";
            //    lItem.Value = "0";

            //    ddList.DataSource = ds;
            //    ddList.DataTextField = TextFieldColumn;
            //    ddList.DataValueField = ValueFieldColumn;
            //    ddList.DataBind();
            //    ddList.Items.Add(lItem);
            //    //ddList.Items.Add(   
            //    //ddList.Items.Insert(0, "---Select-----");

            //    ddList.SelectedValue = "0";
            //}
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("<script>alert('" + ex.Message + "')</script>");
                HttpContext.Current.Response.End();
            }



        }


        /*######################################################################################################### */

        /// <summary>
        /// Method that fills DropDownList and returns DropDownList Object.
        /// </summary>
        /// <param name="ddList">Name of DropDownList to be filled.</param>
        /// <param name="TableName">Table Name of selecting records.</param>
        /// <param name="TextFieldColumn">Name of column for Text field.</param>
        /// <param name="ValueFieldColumn">Name of column for value field.</param>
        /// <param name="OrderByColumn">Column Name for Order by clause.</param>
        /// <returns></returns>
        public void DropDownFill(DropDownList ddList, DataSet dsValue)
        {
            ListItem lItem;
            ddList.Items.Clear();
            DataSet ds = dsValue;

            lItem = new ListItem();
            lItem.Text = "---Select-----";
            lItem.Value = "0";
            ddList.Items.Add(lItem);
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    lItem = new ListItem();
                    lItem.Text = ds.Tables[0].Rows[i][1].ToString();
                    lItem.Value = ds.Tables[0].Rows[i][0].ToString();
                    ddList.Items.Add(lItem);
                }
                ddList.SelectedValue = "0";
            }

            //try
            //{
            //    ddList.DataSource = ds;
            //   ddList.DataTextField = ds.
            //   ddList.DataValueField = ValueFieldColumn;
            //    ddList.DataBind();
            //    lItem = new ListItem();
            //    //ddList.Items.Add(   
            //    //ddList.Items.Insert(0, "---Select-----");
            //    lItem.Text = "---Select-----";
            //    lItem.Value = "0";
            //    ddList.Items.Add(lItem);
            //    ddList.SelectedValue = "0";
            //}
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("<script>alert('" + ex.Message + "')</script>");
                HttpContext.Current.Response.End();
            }



        }
        /*######################################################################################################### */

        /// <summary>
        /// Method that fills DropDownList and returns DropDownList Object.
        /// </summary>
        /// <param name="ddList">Name of DropDownList to be filled.</param>
        /// <param name="TableName">Table Name of selecting records.</param>
        /// <param name="TextFieldColumn">Name of column for Text field.</param>
        /// <param name="ValueFieldColumn">Name of column for value field.</param>
        /// <param name="OrderByColumn">Column Name for Order by clause.</param>
        /// <returns></returns>
        public void DropDownFill(DropDownList ddList, DataSet dsValue, string filter, int indexOfValue, int indexOfText)
        {
            ListItem lItem;
            int len;
            ddList.Items.Clear();
            DataSet ds = dsValue;
            len = ds.Tables[0].Select(filter).Length;
            lItem = new ListItem();
            lItem.Text = "---Select-----";
            lItem.Value = "0";
            ddList.Items.Add(lItem);
            try
            {
                for (int i = 0; i < len; i++)
                {
                    lItem = new ListItem();
                    lItem.Text = ds.Tables[0].Select(filter)[i].ItemArray[indexOfText].ToString();
                    lItem.Value = ds.Tables[0].Select(filter)[i].ItemArray[indexOfValue].ToString();
                    ddList.Items.Add(lItem);
                }
                ddList.SelectedValue = "0";
            }

            //try
            //{
            //    ddList.DataSource = ds;
            //   ddList.DataTextField = ds.
            //   ddList.DataValueField = ValueFieldColumn;
            //    ddList.DataBind();
            //    lItem = new ListItem();
            //    //ddList.Items.Add(   
            //    //ddList.Items.Insert(0, "---Select-----");
            //    lItem.Text = "---Select-----";
            //    lItem.Value = "0";
            //    ddList.Items.Add(lItem);
            //    ddList.SelectedValue = "0";
            //}
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("<script>alert('" + ex.Message + "')</script>");
                HttpContext.Current.Response.End();
            }



        }

        public void DropDownFillSingleColumn(DropDownList ddList, DataSet dsValue)
        {
            ListItem lItem;
            ddList.Items.Clear();
            DataSet ds = dsValue;

            lItem = new ListItem();
            lItem.Text = "---Select-----";
            lItem.Value = "0";
            ddList.Items.Add(lItem);
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    lItem = new ListItem();
                    lItem.Text = ds.Tables[0].Rows[i][0].ToString();
                    lItem.Value = ds.Tables[0].Rows[i][0].ToString();
                    ddList.Items.Add(lItem);
                }
                ddList.SelectedValue = "0";
            }

            //try
            //{
            //    ddList.DataSource = ds;
            //   ddList.DataTextField = ds.
            //   ddList.DataValueField = ValueFieldColumn;
            //    ddList.DataBind();
            //    lItem = new ListItem();
            //    //ddList.Items.Add(   
            //    //ddList.Items.Insert(0, "---Select-----");
            //    lItem.Text = "---Select-----";
            //    lItem.Value = "0";
            //    ddList.Items.Add(lItem);
            //    ddList.SelectedValue = "0";
            //}
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("<script>alert('" + ex.Message + "')</script>");
                HttpContext.Current.Response.End();
            }



        }
        /*######################################################################################################### */

        /// <summary>
        /// Method that fills DropDownList and returns DropDownList Object.
        /// </summary>
        /// <param name="ddList">Name of DropDownList to be filled.</param>
        /// <param name="TableName">Table Name of selecting records.</param>
        /// <param name="colParam">Array of selecting column names.</param>
        /// <param name="condParam">Array of condition.</param>
        /// <param name="OrderByColumn">Column Name for Order by clause.</param>
        /// <returns></returns>
        /// 

        public void DropDownFill(DropDownList ddList, string TableName, string[] colParam, string[] condParam, string OrderByColumn)
        {
            ListItem lItem;
            ddList.Items.Clear();
            DataSet ds = BAL.BAL.CInst().SelectTable(TableName, colParam, condParam, "ddlDS", OrderByColumn);
            lItem = new ListItem();
            lItem.Text = "---Select-----";
            lItem.Value = "0";
            ddList.Items.Add(lItem);
            //lItem = new ListItem();
            //lItem.Text = "All";
            //lItem.Value = "A";
            //ddList.Items.Add(lItem);
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    lItem = new ListItem();
                    lItem.Text = ds.Tables[0].Rows[i][1].ToString();
                    lItem.Value = ds.Tables[0].Rows[i][0].ToString();
                    ddList.Items.Add(lItem);
                }
                ddList.SelectedValue = "0";
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("<script>alert('" + ex.Message + "')</script>");
                HttpContext.Current.Response.End();
            }



        }

        public void DropDownFill(DropDownList ddList, string TableName, string[] colParam, string[] condParam)
        {
            ListItem lItem;
            ddList.Items.Clear();
            DataSet ds = BAL.BAL.CInst().SelectTable(TableName, colParam, condParam, "ddlDS");
            lItem = new ListItem();
            lItem.Text = "---Select-----";
            lItem.Value = "0";
            ddList.Items.Add(lItem);
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    lItem = new ListItem();
                    lItem.Text = ds.Tables[0].Rows[i][1].ToString();
                    lItem.Value = ds.Tables[0].Rows[i][0].ToString();
                    ddList.Items.Add(lItem);
                }
                ddList.SelectedValue = "0";
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("<script>alert('" + ex.Message + "')</script>");
                HttpContext.Current.Response.End();
            }



        }

        /*###################################################################################################*/
        public void DropDownFill(DropDownList ddList, string TableName, string[] colParam)
        {
            ListItem lItem;
            ddList.Items.Clear();
            DataSet ds = BAL.BAL.CInst().ProcedureCallMulti_datset(TableName, colParam);
            lItem = new ListItem();
            lItem.Text = "---Select-----";
            lItem.Value = "0";
            ddList.Items.Add(lItem);
            try
            {
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    lItem = new ListItem();
                    lItem.Text = ds.Tables[0].Rows[i][1].ToString();
                    lItem.Value = ds.Tables[0].Rows[i][0].ToString();
                    ddList.Items.Add(lItem);
                }
                ddList.SelectedValue = "0";
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("<script>alert('" + ex.Message + "')</script>");
                HttpContext.Current.Response.End();
            }



        }
        /*###################################################################################################*/


        public void DropDownFillWithAll(DropDownList ddList, string TableName, string TextFieldColumn, string ValueFieldColumn, string OrderByColumn)
        {
            ListItem lItem;
            ddList.Items.Clear();
            DataSet ds = BAL.BAL.CInst().SelectTable(TableName, "ddlDS", OrderByColumn);

            try
            {
                ddList.DataSource = ds;
                ddList.DataTextField = TextFieldColumn;
                ddList.DataValueField = ValueFieldColumn;
                ddList.DataBind();
                ddList.Items.Insert(0, "---Select-----");
                ddList.Items.Insert(1, new ListItem("ALL", "A"));
                //ddList.Items.Insert(-1, "All");
                // lItem = new ListItem();

                //lItem.Text = "ALL";
                //lItem.Value = "999";
                // lItem.Text = "---Select-----";
                // lItem.Value = "0";
                //  ddList.Items.Add(lItem);
                ddList.SelectedValue = "0";
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("<script>alert('" + ex.Message + "')</script>");
                HttpContext.Current.Response.End();
            }



        }
        /*###########################################################################################################*/


        /// <summary>
        /// Method that fills DataGrid and returns DataGrid Object.
        /// </summary>
        /// <param name="Dgrid">Name of DataGrid to be filled.</param>
        /// <param name="TableName">Table Name of selecting records.</param>
        /// <param name="dsTableName">Table Alias Name for DataSet filling.</param>
        /// <returns></returns>
        public void DatagridFill(DataGrid Dgrid, string TableName, string dsFillTableName)
        {
            DataSet ds = BAL.BAL.CInst().SelectTable(TableName, dsFillTableName);
            try
            {
                Dgrid.DataSource = ds;
                Dgrid.DataBind();
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("<script>alert('" + ex.Message + "')</script>");
                HttpContext.Current.Response.End();
            }
            finally
            {
                ds.Dispose();
            }

        }

        /*######################################################################################################### */
        /// <summary>
        /// Method that fills GridView and returns GridView Object.
        /// </summary>
        /// <param name="grid">Name of GridView to be filled.</param>
        /// <param name="TableName">Table Name of selecting records.</param>
        /// <param name="dsTableName">Table Alias Name for DataSet filling.</param>
        /// <returns></returns>
        public void GridViewFill(GridView grid, string TableName, string dsFillTableName)
        {
            DataSet ds = BAL.BAL.CInst().SelectTable(TableName, dsFillTableName);
            try
            {
                grid.DataSource = ds;

                grid.DataBind();
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("<script>alert('" + ex.Message + "')</script>");
                HttpContext.Current.Response.End();
            }
            finally
            {
                ds.Dispose();
            }


        }





        /// <summary>
        /// Method that fills GridView and returns GridView Object.
        /// </summary>
        /// <param name="grid">Name of GridView to be filled.</param>
        /// <param name="TableName">Table Name of selecting records.</param>
        /// <param name="dsTableName">Table Alias Name for DataSet filling.</param>
        /// <returns></returns>
        public void GridViewFill(GridView grid, string TableName, string[] colParam, string[] condParam, string dsFillTableName, string orderByColumn)
        {
            DataSet ds = BAL.BAL.CInst().SelectTable(TableName, colParam, condParam, dsFillTableName, orderByColumn);
            try
            {
                grid.DataSource = ds;

                grid.DataBind();
            }
            catch (Exception ex)
            {
                HttpContext.Current.Response.Write("<script>alert('" + ex.Message + "')</script>");
                HttpContext.Current.Response.End();
            }
            finally
            {
                ds.Dispose();
            }


        }

        /*######################################################################################################### */
        /// <summary>
        /// Method creates DataTable.
        /// </summary>
        /// <param name="columnParam">Name of Columns for data table.</param>

        public void CreateDataTable(DataTable dt, string[] columnParam)
        {

            foreach (string val in columnParam)
            {
                DataColumn dc = new DataColumn();
                dc.ColumnName = val;
                //dc.DataType = "";
                dt.Columns.Add(dc);
            }

        }


        public DataTable CreateDataTable1(string[] columnParam)
        {

            DataTable dt = new DataTable();
            foreach (string val in columnParam)
            {
                DataColumn dc = new DataColumn();
                dc.ColumnName = val;
                //dc.DataType = "";
                dt.Columns.Add(dc);
            }
            return dt;

        }

        public void CreateDataRow(DataTable dt, string[] columnValueParam)
        {
            DataRow dr;
            dr = dt.NewRow();
            dr.ItemArray = columnValueParam;
            dt.Rows.Add(dr);
        }


        /*######################################################################################################### */

        /// <summary>
        /// Method fills GridView With DataTable Object.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="valParam"></param>
        /// <returns></returns>
        public void GridViewFill(GridView grid, DataTable dt)
        {
            grid.DataSource = dt;
            grid.DataBind();
        }

        /*######################################################################################################### */
        /// <summary>
        /// Method shows Error message .
        /// </summary>
        /// <param name="errno">Index value of Message Array.</param>
        /// <returns></returns>
        public Literal msgShow(int errno) //for integer value//
        {

            Literal lt = new Literal();
            lt.Text = "<script>alert('" + UI.ErrorMessage.CInst().CheckError(errno) + "')</script>";
            return lt;
        }


        /// <summary>
        /// Method shows Error message .
        /// </summary>
        /// <param name="msg">String Message to be displayed.</param>
        /// <returns></returns>
        public Literal msgShow(string msg) //for string message//
        {

            Literal lt = new Literal();
            lt.Text = "<script>alert('" + msg + "')</script>";
            return lt;
        }
        /*######################################################################################################### */

        /// <summary>
        /// Method converts String into Consumable form for sql.
        /// </summary>
        /// <param name="txt">String to convert.</param>
        /// <returns></returns>

        public string Filter(string txt)
        {
            string OutPut = txt;

            while (OutPut.Contains("<"))
            {
                OutPut = OutPut.Replace("<", "&lt;");
            }
            while (OutPut.Contains(">"))
            {
                OutPut = OutPut.Replace(">", "&gt;");
            }
            while (OutPut.Contains(",,"))
            {
                OutPut = OutPut.Replace(",,", ",");
            }
            while (OutPut.Contains(";;"))
            {
                OutPut = OutPut.Replace(";;", ";");
            }
            while (OutPut.Contains("''"))
            {
                OutPut = OutPut.Replace("''", "'");
            }
            OutPut = OutPut.Replace("'", "''");
            return OutPut;
        }

        /*######################################################################################################### */
        public void ddlYear(DropDownList ddl)
        {
            ddl.Items.Clear();
            for (int i = DateTime.Now.Year; i <= DateTime.Now.Year + 1; i++)
            {
                ddl.Items.Add(i.ToString());
            }
            ddl.Items.Insert(0, "---Select-----");
        }
        public void ddlYear(DropDownList ddl, int P_START_YEAR, int P_END_YEAR)
        {
            ddl.Items.Clear();
            for (int i = P_START_YEAR; i <= P_END_YEAR; i++)
            {
                ddl.Items.Add(i.ToString());
            }
            ddl.Items.Insert(0, "---Select-----");
        }
        public void ddlpost(DropDownList ddl, int P_START_YEAR, int P_END_YEAR)
        {
            ddl.Items.Clear();
            for (int i = P_START_YEAR; i <= P_END_YEAR; i++)
            {
                ddl.Items.Add(i.ToString());
            }
            //ddl.Items.Insert(0, "---Select-----");
        }
        public void ddlYear(DropDownList ddl, int LessYear)
        {
            ddl.Items.Clear();
            for (int i = DateTime.Now.Year - LessYear; i <= DateTime.Now.Year; i++)
            {
                ddl.Items.Add(i.ToString());
            }
            ddl.Items.Insert(0, "---Select-----");
        }

        public void erroLogEntry(string[] value)
        {
            try
            {
                string errVal;
                errVal = BAL.BAL.CInst().ProcedureCallSingle("sp_error_log_maintain", value);
            }
            catch { }
        }


        public void Roll_Check(DropDownList dllzone,DropDownList dllDistict,DropDownList dllschool,string rolltype)
        {
            try
            {
                if (rolltype.ToString().Equals("Z"))
                {
                    dllzone.Enabled = false;
                    dllDistict.Enabled = true;
                    dllschool.Enabled = true;
                }
                else if (rolltype.ToString().Equals("D"))
                {
                    dllzone.Enabled = false;
                    dllDistict.Enabled = false;
                    dllschool.Enabled = true;
                }
                else if (rolltype.ToString().Equals("S"))
                {
                    dllzone.Enabled = false;
                    dllDistict.Enabled = false;
                    dllschool.Enabled = false;
                }
                else
                {
                    dllzone.Enabled = true;
                    dllDistict.Enabled = true;
                    dllschool.Enabled = true;
                }
            }
            catch { }
        }

    }
}