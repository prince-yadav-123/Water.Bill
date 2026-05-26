using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Data.OleDb;
using System.Data.SqlClient;
namespace DAL
{
    /// <summary>
    /// Summary description for DbFunctions
    /// </summary>
    public partial class DbFunctions
    {
        //string conType = "Oracle";

        string conType = "SQL";
        string conn;
        private DbFunctions()
        {
            
            //
            // TODO: Add constructor logic here
            //
        }
/*######################################################################################################### */               
        public static DbFunctions CInst()
        {
            return new DbFunctions();
        }

/*######################################################################################################### */       
       
        
        private string ConString()
        {
            string ab = System.Environment.MachineName;
            ab=ab+"\\SQLEXPRESS";
            string s1 = "Data Source="+ab+";Initial Catalog=IRM_Salary;Integrated Security=True";           

            if (conType == "Oracle")
            {
                conn = ConfigurationManager.ConnectionStrings["ConStringOracle"].ConnectionString;
            }
            else if (conType == "SQL")
            {
             //conn = ConfigurationManager.ConnectionStrings["ConStringSQL"].ConnectionString;
             //conn = s1.ToString(); 
               string Password = DecodeFrom64("amFsQDQ2MTE=");
               conn = @"Data Source=103.145.36.151;Initial Catalog=JAL;Persist Security Info=True;User ID=jallogin;Password=" + Password + "";          
            }
            return conn;
        }
        private string ConString_database()
        {
            string ab = System.Environment.MachineName;
            string s1 = "Data Source=" + ab + ";Initial Catalog=master;Integrated Security=True";
          


            if (conType == "Oracle")
            {
                conn = ConfigurationManager.ConnectionStrings["ConStringOracle"].ConnectionString;
            }
            else if (conType == "SQL")
            {
                //conn = ConfigurationManager.ConnectionStrings["ConStringSQL"].ConnectionString;
                conn = s1.ToString();

            }
            return conn;
        }
/*######################################################################################################### */       

        
        /// <summary>
        /// Method returns SqlDataAdapter  Object.
        /// </summary>
        /// <param name="Qry">Sql Query to run.</param>
        /// <returns></returns>
       
        
    }
    /*######################################################################################################### */       
   
    
    public partial class DbFunctions
    {
        /// <summary>
        /// Method is used to select Table Records. 
        /// </summary>
        /// <param name="TableName">Name of Database Table</param>
        /// <param name="dsfillTableName">Table Alias Name for DataSet filling</param>
        /// <param name="OrderByColumn">Column Name for Order by clause.</param>
        /// <returns></returns>
        public DataSet SelectTable(String TableName, string dsfillTableName ,string OrderByColumn)
        {
            SqlDataAdapter  da = new SqlDataAdapter ("select * from " + TableName + " where status=1 order by " + OrderByColumn + "", ConString());
            DataSet ds = new DataSet();
            try
            {
                da.Fill(ds, dsfillTableName);
                return ds;
                
            }
            catch 
            {
                //ds.Tables[0].Rows[0][0] = 11;
                return null;
            }
            
            
        }

        public string DecodeFrom64(string encodedData)
        {
            System.Text.UTF8Encoding encoder = new System.Text.UTF8Encoding();
            System.Text.Decoder utf8Decode = encoder.GetDecoder();
            byte[] todecode_byte = Convert.FromBase64String(encodedData);
            int charCount = utf8Decode.GetCharCount(todecode_byte, 0, todecode_byte.Length);
            char[] decoded_char = new char[charCount];
            utf8Decode.GetChars(todecode_byte, 0, todecode_byte.Length, decoded_char, 0);
            string result = new String(decoded_char);
            return result;
        }

/*############################################################################################################*/
        /// <summary>
        /// Method is used to select Table Records. 
        /// </summary>
        /// <param name="TableName">Name of Database Table</param>
        /// <param name="dsfillTableName">Table Alias Name for DataSet filling</param>
        /// <param name="OrderByColumn">Column Name for Order by clause.</param>
        /// <returns></returns>
        public DataSet SelectTable_fordatabase(String databaseName, string dsfillTableName)
        {
            string as1;
            as1 = "select * from " + databaseName + " where name ='" + "IRM_Salary" + "'";


            SqlDataAdapter da = new SqlDataAdapter("select * from " + databaseName + " where name ='" + "test2" + "'", ConString_database());
            DataSet ds = new DataSet();
            try
            {
                da.Fill(ds, dsfillTableName);
                return ds;

            }
            catch
            {
                //ds.Tables[0].Rows[0][0] = 11;
                return null;
            }


        }
/*############################################################################################################*/

        /// <summary>
        /// Method is used to select Table Records. 
        /// </summary>
        /// <param name="TableName">Name of Database Table</param>
        /// <param name="dsfillTableName">Table Alias Name for DataSet filling</param>
        /// <returns></returns>
        public DataSet SelectTable(String TableName, string dsfillTableName)
        {
            SqlDataAdapter  da = new SqlDataAdapter ("select * from " + TableName + " where status=1", ConString());
            DataSet ds = new DataSet();
            try
            {
                da.Fill(ds, dsfillTableName);
                return ds;

            }
            catch
            {
                //ds.Tables[0].Rows[0][0] = 11;
                return null;
            }


        }


/*######################################################################################################### */       
       
        /// <summary>
        /// Method is used to select Table Records of specific columns on the basis of condition. 
        /// </summary>
        /// <param name="TableName">Name of Database Table</param>
        /// <param name="colParam">String of column name to be selected in query.</param>
        /// <param name="condParam">String of condition for query.</param>
        /// <param name="dsfillTableName">Table Alias Name for DataSet filling</param>
       /// <param name="OrderByColumn">Column Name for Order by clause.</param>
        /// <returns></returns>
        public DataSet SelectTable(String TableName,string colParam ,string condParam,string dsfillTableName,string OrderByColumn)
        {
            SqlDataAdapter  da = new SqlDataAdapter ("select " + colParam + " from " + TableName + " where status=1 and " + condParam + " order by " + OrderByColumn + "", ConString());
          
            DataSet ds = new DataSet();
            try
            {
                da.Fill(ds, dsfillTableName);
                return ds;

            }
            catch
            {
                //ds.Tables[0].Rows[0][0] = 11;
                return null;
            }
            

        }

        /*######################################################################################################### */

        /// <summary>
        /// Method is used to select Table Records of specific columns on the basis of condition. 
        /// </summary>
        /// <param name="TableName">Name of Database Table</param>
        /// <param name="colParam">String of column name to be selected in query.</param>
        /// <param name="condParam">String of condition for query.</param>
        /// <param name="dsfillTableName">Table Alias Name for DataSet filling</param>
        /// <param name="OrderByColumn">Column Name for Order by clause.</param>
        /// <returns></returns>
        //public DataSet SelectTable(String TableName, string colParam, string dsfillTableName, string OrderByColumn)
        //{
        //    SqlDataAdapter da = new SqlDataAdapter("select " + colParam + " from " + TableName + " where status=1  order by " + OrderByColumn + "", ConString());

        //    DataSet ds = new DataSet();
        //    try
        //    {
        //        da.Fill(ds, dsfillTableName);
        //        return ds;

        //    }
        //    catch
        //    {
        //        //ds.Tables[0].Rows[0][0] = 11;
        //        return null;
        //    }


        //}
       //create by krishan

        public DataSet SelectTable(String TableName, string colParam, string condParam, string dsfillTableName)
        {
            SqlDataAdapter da = new SqlDataAdapter("select " + colParam + " from " + TableName + " where status=1 and " + condParam + "", ConString());

            DataSet ds = new DataSet();
            try
            {
                da.Fill(ds, dsfillTableName);
                return ds;

            }
            catch
            {
                //ds.Tables[0].Rows[0][0] = 11;
                return null;
            }


        }



/*######################################################################################################### */       


        /// <summary>
        /// Method to Insert Records.
        /// </summary>
        /// <param name="procName">Procedure Name to Insert Records.</param>
        /// <param name="param">Array Of Parameter values.</param>
        /// <returns></returns>
        public string[] ProcedureCallMulti(string procName, string[] param)
        {
            string[] arr = new string[2];
            SqlConnection OleCon = new SqlConnection(ConString());
            SqlCommand OleCmd = new SqlCommand();
            OleCmd.Connection = OleCon;
            OleCmd.CommandType = CommandType.StoredProcedure;
            foreach (string val in param)
            {
                SqlParameter oledbPara = new SqlParameter();
                oledbPara.Value = val;
                OleCmd.Parameters.Add(oledbPara);
                
            }
            OleCmd.Parameters.Add(new SqlParameter("ErrVal", SqlDbType.Int));
            OleCmd.Parameters["ErrVal"].Direction = ParameterDirection.Output;
            OleCmd.Parameters.Add(new SqlParameter("OutId", SqlDbType.VarChar,15));
            OleCmd.Parameters["OutId"].Direction = ParameterDirection.Output;
           
            OleCmd.CommandText = procName;
            OleCon.Open();
            try
            {
                OleCmd.ExecuteNonQuery();
                arr[0] = OleCmd.Parameters["ErrVal"].Value.ToString();
                arr[1] = OleCmd.Parameters["OutId"].Value.ToString();
                return arr;
            }
            catch 
            {
               arr[0] = "9";
               arr[1] = "0";
            return arr;
           // HttpContext.Current.Response.Write("<script>alert('Database Error !!! "+ ex.Message + "')</script>");
           // HttpContext.Current.Response.End();

             
            }
            finally
            {

                OleCon.Close();
                OleCmd.Dispose();
                OleCon.Dispose();


            }

             
        }
        /*#######################################################################*/
        //procedurerecord dataset
        //5/3/2012
        public DataSet ProcedureCallMulti_datset(string procName, string[] param)
        {

            string arr= "9";
            string[] strValue = new string[2];
            DataSet ds = new DataSet();
            SqlConnection OleCon = new SqlConnection(ConString());
            SqlCommand OleCmd = new SqlCommand(procName, OleCon);
           // OleCmd.Connection = OleCon;
            OleCmd.CommandType = CommandType.StoredProcedure;
            foreach (string val in param)
            {
                strValue = val.Split(',');
                SqlParameter oledbPara = new SqlParameter();
                oledbPara.ParameterName = strValue[0].ToString();
                oledbPara.Value = strValue[1].ToString();
                OleCmd.Parameters.Add(oledbPara);

            }
            OleCmd.Parameters.Add(new SqlParameter("@totalRows", SqlDbType.Int));
            OleCmd.Parameters["@totalRows"].Direction = ParameterDirection.Output;
           // OleCmd.CommandText = procName;
            OleCon.Open();
           
            try
            {
                SqlDataAdapter ad = new SqlDataAdapter(OleCmd);
               
                ad.Fill(ds,"Table1");
                arr = OleCmd.Parameters["@totalRows"].Value.ToString();
                DataTable dt = new DataTable("Table2");
                //DataColumn dc = new DataColumn();
                //dc.ColumnName = "Row_Count";
                //dc.DataType = "";
                dt.Columns.Add("Row_Count");
                DataRow dr=dt.NewRow();
                dr["Row_Count"] = arr.ToString();
                dt.Rows.Add(dr);
                ds.Tables.Add(dt);
                //DataTable dt = new DataTable("rocount");
                //DataColumn dc = new DataColumn("Row_Count");
                
                return ds;
            }
            catch
            {
                arr = "9";
                return ds;
                // HttpContext.Current.Response.Write("<script>alert('Database Error !!! "+ ex.Message + "')</script>");
                // HttpContext.Current.Response.End();


            }
            finally
            {

                OleCon.Close();
                OleCmd.Dispose();
                OleCon.Dispose();


            }


        }
/*##########################################################################################################*/
      //added by krihan 19/07/2013
        public DataSet ProcedureCallMulti_datsetVar(string procName, string[] param)
        {

            string arr = "9";
            string[] strValue = new string[2];
            DataSet ds = new DataSet();
            SqlConnection OleCon = new SqlConnection(ConString());
            SqlCommand OleCmd = new SqlCommand(procName, OleCon);
            // OleCmd.Connection = OleCon;
            OleCmd.CommandType = CommandType.StoredProcedure;
            foreach (string val in param)
            {
                strValue = val.Split(',');
                SqlParameter oledbPara = new SqlParameter();
                oledbPara.ParameterName = strValue[0].ToString();
                oledbPara.Value = strValue[1].ToString();
                OleCmd.Parameters.Add(oledbPara);

            }
            OleCmd.Parameters.Add(new SqlParameter("@OUTPARAM", SqlDbType.VarChar, 250));
            OleCmd.Parameters["@OUTPARAM"].Direction = ParameterDirection.Output;
            // OleCmd.CommandText = procName;
            OleCon.Open();

            try
            {
                SqlDataAdapter ad = new SqlDataAdapter(OleCmd);

                ad.Fill(ds, "Table1");
                arr = OleCmd.Parameters["@OUTPARAM"].Value.ToString();
                DataTable dt = new DataTable("Table2");
                //DataColumn dc = new DataColumn();
                //dc.ColumnName = "Row_Count";
                //dc.DataType = "";
                dt.Columns.Add("Row_Count");
                DataRow dr = dt.NewRow();
                dr["Row_Count"] = arr.ToString();
                dt.Rows.Add(dr);
                ds.Tables.Add(dt);
                //DataTable dt = new DataTable("rocount");
                //DataColumn dc = new DataColumn("Row_Count");

                return ds;
            }
            catch
            {
                arr = "9";
                return ds;
                // HttpContext.Current.Response.Write("<script>alert('Database Error !!! "+ ex.Message + "')</script>");
                // HttpContext.Current.Response.End();


            }
            finally
            {

                OleCon.Close();
                OleCmd.Dispose();
                OleCon.Dispose();


            }


        }
 /*######################################################################################################### */

        /// <summary>
        /// Method to procedure Calling and return error message as string.
        /// </summary>
        /// <param name="procName">Procedure Name to Insert Records.</param>
        /// <param name="param">Array Of Parameter values.</param>
        /// <returns></returns>
        public string ProcedureCallSingle(string procName, string[] param)
        {
            string arr="9";
            string[] strValue;
            SqlConnection OleCon = new SqlConnection(ConString());
            SqlCommand OleCmd = new SqlCommand();
            OleCmd.Connection = OleCon;
            OleCmd.CommandType = CommandType.StoredProcedure;
            foreach (string val in param)
            {
                //SqlParameter  oledbPara = new SqlParameter();
                //oledbPara.Value = val;
                //OleCmd.Parameters.Add(oledbPara);
                strValue = val.Split(',');
                SqlParameter oledbPara = new SqlParameter();
                oledbPara.ParameterName = strValue[0].ToString();
                oledbPara.Value = strValue[1].ToString();
                OleCmd.Parameters.Add(oledbPara);

            }
            OleCmd.Parameters.Add(new SqlParameter("@outparam", SqlDbType.Int));
            OleCmd.Parameters["@outparam"].Direction = ParameterDirection.Output;
            

            OleCmd.CommandText = procName;
            OleCon.Open();
            try
            {
                OleCmd.ExecuteNonQuery();
                arr = OleCmd.Parameters["@outparam"].Value.ToString();
                return arr;
            }
            catch
            {
                
                
                return arr;
                // HttpContext.Current.Response.Write("<script>alert('Database Error !!! "+ ex.Message + "')</script>");
                // HttpContext.Current.Response.End();


            }
            finally
            {

                OleCon.Close();
                OleCmd.Dispose();
                OleCon.Dispose();


            }


        }


       //New Function Vishal

        public string ProcedureCallSingleNew(string procName, string[] param)
        {
            string arr = "9";
            string[] strValue;
            SqlConnection OleCon = new SqlConnection(ConString());
            SqlCommand OleCmd = new SqlCommand();
            OleCmd.Connection = OleCon;
            OleCmd.CommandType = CommandType.StoredProcedure;
            foreach (string val in param)
            {
                strValue = val.Split(',');
                SqlParameter oledbPara = new SqlParameter();
                oledbPara.ParameterName = strValue[0].ToString();
                oledbPara.Value = strValue[1].ToString();
                OleCmd.Parameters.Add(oledbPara);
                

            }
            OleCmd.Parameters.Add(new SqlParameter("@outparam", SqlDbType.Int));
            OleCmd.Parameters["@outparam"].Direction = ParameterDirection.Output;


            OleCmd.CommandText = procName;
            OleCon.Open();
            try
            {
                OleCmd.ExecuteNonQuery();
                arr = OleCmd.Parameters["@outparam"].Value.ToString();
                return arr;
            }
            catch
            {


                return arr;
                // HttpContext.Current.Response.Write("<script>alert('Database Error !!! "+ ex.Message + "')</script>");
                // HttpContext.Current.Response.End();


            }
            finally
            {

                OleCon.Close();
                OleCmd.Dispose();
                OleCon.Dispose();


            }


        }

        //End Function 
        //new function vishal shukla
        

        //End vishal shukla

        //New Function krishan dutt

        public string ProcedureCallSingleNew_string(string procName, string[] param)
        {
            string arr = "9";
            string[] strValue;
            SqlConnection OleCon = new SqlConnection(ConString());
            SqlCommand OleCmd = new SqlCommand();
            OleCmd.Connection = OleCon;
            OleCmd.CommandTimeout = 3600;
            OleCmd.CommandType = CommandType.StoredProcedure;
            foreach (string val in param)
            {
                strValue = val.Split(',');
                SqlParameter oledbPara = new SqlParameter();
                oledbPara.ParameterName = strValue[0].ToString();
                oledbPara.Value = strValue[1].ToString();
                OleCmd.Parameters.Add(oledbPara);


            }
            OleCmd.Parameters.Add(new SqlParameter("@outparam", SqlDbType.VarChar,100));
            OleCmd.Parameters["@outparam"].Direction = ParameterDirection.Output;
            OleCmd.CommandText = procName;
            OleCon.Open();
            try
            {
                OleCmd.ExecuteNonQuery();
                arr = OleCmd.Parameters["@outparam"].Value.ToString();
                return arr;
            }
            catch
            {


                return arr;
                // HttpContext.Current.Response.Write("<script>alert('Database Error !!! "+ ex.Message + "')</script>");
                // HttpContext.Current.Response.End();


            }
            finally
            {

                OleCon.Close();
                OleCmd.Dispose();
                OleCon.Dispose();


            }


        }

        //End Function 
        //new function krishan dutt


        //End krishan dutt

        public string ProcedureCallForDivDataUpdate(string procName, string data, string ConsNo, string ChallanNo, string BillNo, int OPERATION_TYPE)
        {
            string arr = "9";
            string[] strValue;
            SqlConnection OleCon = new SqlConnection(ConString());
            SqlCommand OleCmd = new SqlCommand();
            OleCmd.Connection = OleCon;
            OleCmd.CommandTimeout = 3600;
            OleCmd.CommandType = CommandType.StoredProcedure;
            OleCmd.Parameters.Add("@P_ChallanContent", data);
            OleCmd.Parameters.Add("@P_CONS_NO", ConsNo);
            OleCmd.Parameters.Add("@P_CHALLAN_NO", ChallanNo);
            OleCmd.Parameters.Add("@P_PRECIPT_NO", BillNo);
            OleCmd.Parameters.Add("@P_OPERATION_TYPE", OPERATION_TYPE);
            //foreach (string val in param)
            //{
            //    strValue = val.Split(',');
            //    SqlParameter oledbPara = new SqlParameter();
            //    oledbPara.ParameterName = strValue[0].ToString();
            //    oledbPara.Value = strValue[1].ToString();
            //    OleCmd.Parameters.Add(oledbPara);


            //}
            OleCmd.Parameters.Add(new SqlParameter("@outparam", SqlDbType.VarChar, 100));
            OleCmd.Parameters["@outparam"].Direction = ParameterDirection.Output;
            OleCmd.CommandText = procName;
            OleCon.Open();
            try
            {
                OleCmd.ExecuteNonQuery();
                arr = OleCmd.Parameters["@outparam"].Value.ToString();
                return arr;
            }
            catch
            {


                return arr;
                // HttpContext.Current.Response.Write("<script>alert('Database Error !!! "+ ex.Message + "')</script>");
                // HttpContext.Current.Response.End();


            }
            finally
            {

                OleCon.Close();
                OleCmd.Dispose();
                OleCon.Dispose();


            }


        }

/*###########################################################################################################*/
        /// <summary>
        /// This Method is used to call Sql Function.
        /// </summary>
        /// <param name="funcName">Name of Function to be called.</param>
        /// <param name="valParam">String of Parameter values.</param>
        /// <returns></returns>
        public DataSet FunctionCall(string funcName,string valParam)
        {
            SqlDataAdapter  oleda;
            string FuncStr;
            FuncStr = "select dbo." + funcName + "('" + valParam + "')";
            oleda = new SqlDataAdapter  (FuncStr,ConString());
            DataSet ds = new DataSet();
            try
            {
                oleda.Fill(ds);
                return ds;
            }
            catch 
            {
                ds.Tables[0].Rows[0][0] = 10;
                return ds;
            }
            finally 
            {
                oleda.Dispose();
                
            }
        }
/*########################################################################################################*/
//added by krishan for calling table function.
        public DataSet FunctionCall_table(string funcName, string valParam)
        {
            SqlDataAdapter oleda;
            string FuncStr;
            FuncStr = "select * from dbo." + funcName + "('" + valParam + "')";
            oleda = new SqlDataAdapter(FuncStr, ConString());
            DataSet ds = new DataSet();
            try
            {
                oleda.Fill(ds);
                return ds;
            }
            catch
            {
                ds.Tables[0].Rows[0][0] = 10;
                return ds;
            }
            finally
            {
                oleda.Dispose();

            }
        }
/*######################################################################################################### */

        /// <summary>
        /// This Method is used to return string value on selecting a table with columns, conditons & order by values as signature.
        /// </summary>
        /// <param name="TableName">Name of Table to be called.</param>
        /// <param name="colParam">String of Columns of tables to be selected values.</param>
        /// <param name="condParam">String of Condition.</param>
        /// <param name="OrderByColumn">String of Order by clause in table.</param>
        /// <returns>String Value str</returns>
        public string ReadSingleStringFromDb(String TableName, string colParam, string condParam, string OrderByColumn)
        {
            string str = null;
            SqlConnection con = new SqlConnection(ConString());
              
            SqlCommand com = new SqlCommand("select " + colParam + " from " + TableName + " where " + condParam + " order by " + OrderByColumn + "", con);
            try
            { 
                con.Open();
                SqlDataReader dR = com.ExecuteReader();
                if (dR.Read())
                {
                    str = dR[0].ToString();
                }
                con.Close();
                com.Dispose();
                con.Dispose();
            }
            catch (Exception ex)
            {
                con.Close();
                com.Dispose();
                con.Dispose();
                HttpContext.Current.Response.Write("<script>alert('" + ex.Message + "')</script>");
                HttpContext.Current.Response.End();
                return null;


            }
            return str;
        }
    /*###########################################################################################################*/
        /// <summary>
        /// This Method is used to return string value on executing procedure.
        /// </summary>
        /// <param name="MSTR">Name of Master page</param>
        /// <param name="procedureName">Procedure Name</param>
        /// <param name="param">Parameters for procedue</param>       
        /// <returns>String Value str</returns>
        //public string ProcedureToDbret_(MasterPage MSTR, string procedureName, string[] param)
        //{
        //    string arr = "9";
        //    SqlConnection con = new SqlConnection(ConString);
        //    SqlCommand com = new SqlCommand();
        //    com.Connection = con;
        //    com.CommandType = CommandType.StoredProcedure;
        //    com.CommandText = procedureName;

        //    for (int i = 0; i < param.Length; i++)
        //    {
        //        SqlParameter oldb = new SqlParameter();
        //        oldb.Value = param[i];
        //        com.Parameters.Add(oldb);
        //    }
        //    com.Parameters.Add(new SqlParameter("OutParam", SqlDbType.VarChar, 100)).Direction = ParameterDirection.Output;

        //    try
        //    {
        //        con.Open();
        //        com.ExecuteNonQuery();
        //        arr = com.Parameters["OutParam"].Value.ToString();
        //        msg1(MSTR, arr);
        //        con.Close();
        //        com.Dispose();
        //        con.Dispose();
        //    }
        //    catch (Exception ex)
        //    {
        //        //HttpContext.Current.Response.Write(ex.Message);
        //        //HttpContext.Current.Response.End();
        //        //rowAffected = -1;
        //    }
        //    return arr;
        //}
    }
}
