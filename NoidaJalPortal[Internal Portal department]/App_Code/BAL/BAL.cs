using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using DAL;
namespace BAL
{
    /// <summary>
    /// Summary description for BAL
    /// </summary>
    public partial class BAL
    {
        private BAL()
        {
            //
            // TODO: Add constructor logic here
            //
        }
/*######################################################################################################### */       
        public static BAL CInst()
        {
            return new BAL();
        }
        
/*##########################################################################################################*/       
       // Procedure and Function Calling Methods.//

        /// <summary>
        /// This Method is used to call Procedure.
        /// </summary>
        /// <param name="procName">Name of Procedure to call.</param>
        /// <param name="valparam">Array of Parameter values.</param>
        /// <returns></returns>
        public string ProcedureCallSingle(string procName, string[] valparam)
        {
            return DAL.DbFunctions.CInst().ProcedureCallSingle(procName, valparam);
        }

        public string ProcedureCallSingleNew_string(string procName, string[] valparam)
        {
            return DAL.DbFunctions.CInst().ProcedureCallSingleNew_string(procName, valparam);
        }

        public string ProcedureCallForDivDataUpdate(string procName, string data, string ConsNo, string ChallanNo, string BillNo, int OPERATION_TYPE)
        {
            return DAL.DbFunctions.CInst().ProcedureCallForDivDataUpdate(procName, data, ConsNo, ChallanNo, BillNo, OPERATION_TYPE);
        }
        
        /// <summary>
        /// This Method is used to call Procedure which returns array of string.
        /// </summary>
        /// <param name="procName">Name of Procedure to call.</param>
        /// <param name="valparam">Array of Parameter values.</param>
        /// <returns></returns>
        public string[] ProcedureCallMulti(string procName, string[] valparam)
        {
            return DAL.DbFunctions.CInst().ProcedureCallMulti(procName, valparam);
        }
        public DataSet ProcedureCallMulti_datset(string procName, string[] param)
        {
            return DAL.DbFunctions.CInst().ProcedureCallMulti_datset(procName, param);
        }
        public DataSet ProcedureCallMulti_datsetVar(string procName, string[] param)
        {
            return DAL.DbFunctions.CInst().ProcedureCallMulti_datsetVar(procName, param);
        }
        /// <summary>
        /// This Method is used to call Sql Function.
        /// </summary>
        /// <param name="FuncName">Name of Function to be called.</param>
        /// <param name="valParam">String of Parameter values.</param>
        /// <returns></returns>
        public DataSet FunctionCall(string FuncName, string valParam)
        {

            return DAL.DbFunctions.CInst().FunctionCall(FuncName, valParam);
        }
        public DataSet FunctionCall_table(string FuncName, string valParam)
        {

            return DAL.DbFunctions.CInst().FunctionCall_table(FuncName, valParam);
        }
/*######################################################################################################### */       
      // Record Selection Mehtods //
        
        /// <summary>
        /// This Method is used to select records of all columns without condition, of a Database table.
        /// </summary>
        /// <param name="TableName">Name of Database Table.</param>
        /// <param name="dsfillTableName">Table Alias name for filling Dataset.</param>
        /// <param name="OrderByColumn">Column Name for Order by clause.</param>
        /// <returns></returns>
        public DataSet SelectTable(String TableName, string dsfillTableName, string OrderByColumn)
        {
            return DAL.DbFunctions.CInst().SelectTable(TableName, dsfillTableName, OrderByColumn);
        }
        public DataSet SelectTable_fordatabase(String databaseName, string dsfillTableName)
        {
            return DAL.DbFunctions.CInst().SelectTable_fordatabase(databaseName, dsfillTableName);
        }


        /// <summary>
        /// This Method is used to select records of all columns without condition, of a Database table.
        /// </summary>
        /// <param name="TableName">Name of Database Table.</param>
        /// <param name="dsfillTableName">Table Alias name for filling Dataset.</param>
       /// <returns></returns>
        public DataSet SelectTable(String TableName, string dsfillTableName)
        {
            return DAL.DbFunctions.CInst().SelectTable(TableName, dsfillTableName);
        }



        /// <summary>
        /// This Method is used to select records of specific columns for given condition of a Database Table.
        /// </summary>
        /// <param name="TableName">Name of Database Table.</param>
        /// <param name="colParam">Array of column names.</param>
        /// <param name="condParam">Array of condition values.</param>
        /// <param name="dsfillTableName">Table Alias Name for filling DataSet.</param>
        /// <param name="OrderByColumn">Column Name for Order by clause.</param>
        /// <returns></returns>
        public DataSet SelectTable(String TableName, string[] colParam, string[] condParam, string dsfillTableName, string OrderByColumn)
        {
            string colStr=null;
            string condStr=null;
            for (int i = 0; i < colParam.Length; i++)
            {
                colStr = colStr + colParam[i].ToString() + ",";
            }
            colStr = colStr.Substring(0, colStr.Length - 1).ToString();
            for (int i = 0; i < condParam.Length; i++)
            {
                condStr = condStr + condParam[i].ToString() + " and ";
            }
            condStr = condStr.Substring(0, condStr.Length - 5).ToString();
            return DAL.DbFunctions.CInst().SelectTable(TableName, colStr, condStr, dsfillTableName, OrderByColumn);
        }
        // create by krishan
        public DataSet SelectTable(String TableName, string[] colParam, string[] condParam, string dsfillTableName)
        {
            string colStr = null;
            string condStr = null;
            for (int i = 0; i < colParam.Length; i++)
            {
                colStr = colStr + colParam[i].ToString() + ",";
            }
            colStr = colStr.Substring(0, colStr.Length - 1).ToString();
            for (int i = 0; i < condParam.Length; i++)
            {
                condStr = condStr + condParam[i].ToString() + " and ";
            }
            condStr = condStr.Substring(0, condStr.Length - 5).ToString();
            return DAL.DbFunctions.CInst().SelectTable(TableName, colStr, condStr, dsfillTableName);
        }

        public DataSet SelectTable(String TableName, string[] colParam, string dsfillTableName, string OrderByColumn)
        {
            string colStr = null;
            //string condStr = null;
            for (int i = 0; i < colParam.Length; i++)
            {
                colStr = colStr + colParam[i].ToString() + ",";
            }
            colStr = colStr.Substring(0, colStr.Length - 1).ToString();
            //for (int i = 0; i < condParam.Length; i++)
            //{
            //    condStr = condStr + condParam[i].ToString() + " and ";
            //}
            //condStr = condStr.Substring(0, condStr.Length - 5).ToString();
            return DAL.DbFunctions.CInst().SelectTable(TableName, colStr,  dsfillTableName, OrderByColumn);
        }


/*######################################################################################################### */

        /*##########################################################################################################*/
        // Tables Calling Methods.//

        /// <summary>
        /// This Method is used to call Procedure.
        /// </summary>
        /// <param name="procName">Name of Procedure to call.</param>
        /// <param name="valparam">Array of Parameter values.</param>
        /// <returns></returns>
        /// 
        public string ReadSingleStringFromDb(String TableName, string[] colParam, string[] condParam, string OrderByColumn)
        {
            string colStr = null;
            string condStr = null;
            for (int i = 0; i < colParam.Length; i++)
            {
                colStr = colStr + colParam[i].ToString() + ",";
            }
            colStr = colStr.Substring(0, colStr.Length - 1).ToString();
            for (int i = 0; i < condParam.Length; i++)
            {
                condStr = condStr + condParam[i].ToString() + " and ";
            }
            condStr = condStr.Substring(0, condStr.Length - 5).ToString();
            return DAL.DbFunctions.CInst().ReadSingleStringFromDb(TableName, colStr, condStr, OrderByColumn);
        }
        
    }
}
