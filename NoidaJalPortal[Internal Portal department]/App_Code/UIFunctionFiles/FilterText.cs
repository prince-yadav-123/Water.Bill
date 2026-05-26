using System;
using System.Data;
using System.Configuration;
using System.Web;
//using System.Web.Security;
//using System.Web.UI;
//using System.Web.UI.HtmlControls;
//using System.Web.UI.WebControls;
//using System.Web.UI.WebControls.WebParts;



namespace UI
{
    public class FilterText
    {
        public FilterText()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        public static string Filter(string txt)
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
            while (OutPut.Contains(","))
            {
                OutPut = OutPut.Replace(",", " ");
            }
           
            OutPut = OutPut.Replace("'", "''");
            return OutPut;
        }
        public static string Filter1(string txt)
        {
            string OutPut = txt;
            
            while (OutPut.Contains(","))
            {
                OutPut = OutPut.Replace(",", " ");
            }

            OutPut = OutPut.Replace("'", "''");
            return OutPut;
        }
        public static string XFilter(string txt)
        {
            string OutPut = txt;
            while (OutPut.Contains("&LT;"))
            {
                OutPut = OutPut.Replace("&LT;", "<");
            }
            while (OutPut.Contains("&GT;"))
            {
                OutPut = OutPut.Replace("&GT;", ">");
            }
            return OutPut;
        }
    }
}
