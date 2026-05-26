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
    /// Summary description for XMLfunctions
    /// </summary>
    public class XMLFunctions
    {

//###########################################################################################################//
       

        private XMLFunctions()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static XMLFunctions CInst()
        {
            return new XMLFunctions();
        }


//###########################################################################################################//
        
        
        /// <summary>
        /// Method is used to Fill Data from XML file.
        /// </summary>
        /// <param name="FileName">Full Path of Xml file .</param>
        /// <returns></returns>
       
        
        public DataSet loadData(string FileName)
        {
            return DAL.xmlFunctions.CInst().loadData(FileName);
        }


//###########################################################################################################//

       
        /// <summary>
        /// Mehtod is used to update Records in Xml file.
        /// </summary>
        /// <param name="FileName">Full Path of Xml file.</param>
        /// <param name="valParam">Array of column values.</param>
        /// <param name="condParam">Value of condition</param>
        /// <returns></returns>
       
        public int updateRecord(string FileName, string[] valParam, string condParam)
        {
            return DAL.xmlFunctions.CInst().updateRecord(FileName, valParam, condParam);
        }


//###########################################################################################################//
        
        
        /// <summary>
        /// Mehtod is used to Insert Records in Xml file.
        /// </summary>
        /// <param name="FileName">Full Path of Xml file.</param>
        /// <param name="colValueParam">Array of column values.</param>
        /// <returns></returns>
      
        
        public int insertRecord(string FileName, string[] colValueParam)
        {
            return DAL.xmlFunctions.CInst().insertRecord(FileName, colValueParam);
        }


//###########################################################################################################//
       
        
        /// <summary>
        /// Mehtod is used to Delete Records in Xml file.
        /// </summary>
        /// <param name="FileName">Full Path of Xml file.</param>
        /// <param name="condParam">Value of condition</param>
        /// <returns></returns>
        
        public int deleteRecord(string FileName, string condParam)
        {
            return DAL.xmlFunctions.CInst().deleteRecord(FileName, condParam);
        }

//###########################################################################################################//
    }
}
