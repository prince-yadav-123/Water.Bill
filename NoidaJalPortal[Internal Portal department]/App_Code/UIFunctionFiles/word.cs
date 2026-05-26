using System;
using System.Data;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
namespace UI
{
    /// <summary>
    /// Summary description for word
    /// </summary>
    public class word
    {
        private static String[] units = { "Zero", "One", "Two", "Three",
           "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Eleven",
           "Twelve", "Thirteen", "Fourteen", "Fifteen", "Sixteen",
           "Seventeen", "Eighteen", "Nineteen" };
        private static String[] tens = { "", "", "Twenty", "Thirty", "Forty",
           "Fifty", "Sixty", "Seventy", "Eighty", "Ninety" };
        public word()
        {
            //
            // TODO: Add constructor logic here
            //
        }



        public String ConvertAmount(double amount)
        {
            try
            {
                int amount_int = (int)amount;
                int amount_dec = (int)Math.Round((amount - (double)(amount_int)) * 100);
                return convert(amount_int) + "  point "
                        + convert(amount_dec);
            }
            catch (Exception e)
            {
                // TODO: handle exception
            }
            return "";
        }

        public String convert(int i)
        {
            //
            if (i < 20)
                return units[i];
            if (i < 100)
                return tens[i / 10] + ((i % 10 > 0) ? " " + convert(i % 10) : "");
            if (i < 1000)
                return units[i / 100] + " Hundred"
                        + ((i % 100 > 0) ? " and " + convert(i % 100) : "");
            if (i < 100000)
                return convert(i / 1000) + " Thousand "
                        + ((i % 1000 > 0) ? " " + convert(i % 1000) : "");
            if (i < 10000000)
                return convert(i / 100000) + " Lakh "
                        + ((i % 100000 > 0) ? " " + convert(i % 100000) : "");
            return convert(i / 10000000) + " Crore "
                    + ((i % 10000000 > 0) ? " " + convert(i % 10000000) : "");
        }
    }
}
