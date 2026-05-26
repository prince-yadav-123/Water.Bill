using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.IO;

namespace DAL
{
    /// <summary>
    /// Summary description for xmlFunctions
    /// </summary>
    public class xmlFunctions
    {
        int Errval;
//###########################################################################################################//
        private xmlFunctions()
        {
            //
            // TODO: Add constructor logic here
            //
        }
        public static xmlFunctions CInst()
        {
            return new xmlFunctions();
        }
//###########################################################################################################//
        
        
        /// <summary>
        /// Method is used to Fill Data from XML file.
        /// </summary>
        /// <param name="FileName">Full Path of Xml file .</param>
        /// <returns></returns>
        public DataSet loadData(string FileName)
        {
            
            DataSet ds = new DataSet();
            ds = null;
            if (File.Exists(FileName))
            {
                ds.ReadXml(FileName);
            }
            if (ds != null)
            {
                return ds;
            }
            else
            {
                ds.Tables[0].Rows[0][0] = "12";
                return ds;
            }


        }


//###########################################################################################################//
       
        
        /// <summary>
        /// Mehtod is used to update Records in Xml file.
        /// </summary>
        /// <param name="FileName">Full Path of Xml file.</param>
        /// <param name="valParam">Array of column values.</param>
        /// <param name="condParam">Value of condition</param>
        /// <returns></returns>
        public int updateRecord(string FileName,string[] valParam,string condParam)
        {
            
            DataSet ds = new DataSet();
            ds = null;
            if (File.Exists(FileName))
            {
                ds.ReadXml(FileName);
            }
            if (ds != null)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        if (ds.Tables[0].Rows[i][0].ToString() == condParam)
                        {
                            for (int j = 1; j < ds.Tables[0].Columns.Count; j++)
                            {
                                ds.Tables[0].Rows[i][j] = valParam[j - 1].ToString();
                            }

                            ds.AcceptChanges();
                        }
                    }
                    ds.WriteXml(FileName);
                    Errval = 1;
                    
                }
            }
            else 
            {
                Errval = 12;
            }
            return Errval;
            
        }


//###########################################################################################################//
       
        
        /// <summary>
        /// Mehtod is used to Insert Records in Xml file.
        /// </summary>
        /// <param name="FileName">Full Path of Xml file.</param>
        /// <param name="colValueParam">Array of column values.</param>
        /// <returns></returns>
        public int insertRecord(string FileName,string []colValueParam)
        {

            DataSet ds = new DataSet();
            if (File.Exists(FileName))
            {
                ds.ReadXml(FileName);
            }
            if (ds != null)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    DataRow dr = ds.Tables[0].NewRow();
                    for (int i = 0; i < dr.ItemArray.Length; i++)
                    {
                        dr[i] = colValueParam[i].ToString();
                    }
                    ds.Tables[0].Rows.Add(dr);
                    ds.AcceptChanges();
                    ds.WriteXml(FileName);
                    Errval = 0;
                }
            
            }
            else
            {
                Errval = 12;
            }
            return Errval;
        }


//###########################################################################################################//
       
        
        /// <summary>
        /// Mehtod is used to Delete Records in Xml file.
        /// </summary>
        /// <param name="FileName">Full Path of Xml file.</param>
        /// <param name="condParam">Value of condition</param>
        /// <returns></returns>
        public int deleteRecord(string FileName,string condParam)
        {

            DataSet ds = new DataSet();
            if (File.Exists(FileName))
            {
                ds.ReadXml(FileName);
            }
            if (ds != null)
            {
                if (ds.Tables[0].Rows.Count > 0)
                {
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        if (ds.Tables[0].Rows[i][0].ToString() == condParam)
                        {
                            ds.Tables[0].Rows[i].Delete();

                        }

                    }
                    ds.AcceptChanges();
                    ds.WriteXml(FileName);
                    Errval = 2;
                }
            }
            else 
            {
                Errval = 12;
            }
            return Errval;
            
        }


//###########################################################################################################//

    }
}
