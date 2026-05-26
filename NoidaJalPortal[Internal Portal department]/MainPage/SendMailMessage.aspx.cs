using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;
using System.Data.SqlClient;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.IO;

public partial class MainPage_SendMailMessage : System.Web.UI.Page
{
    protected void Page_Load(object sender, EventArgs e)
    {

    }
    protected void sendbtn_Click(object sender, EventArgs e)
    {
        using (MailMessage mm = new MailMessage())
        {
            StringBuilder sb = new StringBuilder();

            // sb.Append("Your passord is 676767 ");
            sb.Append("<html><head><title>Email Sending By</title></head>");
            sb.Append("<body>");
            sb.Append("<table cellpadding='5' cellspacing='5' width='600px' border='0'>");
            sb.Append("<tr><td><b> Dear </b>&nbsp;&nbsp;" + txtname.Text + "" + "," + "</td></tr>");
            sb.Append("<tr><td><b> You may like to pay your bill online.You can pay from :</b>&nbsp;&nbsp;&nbsp;" + "http://noidajalonline.com/" + "</td></tr>");
            sb.Append("<tr><td><b> Instruction for Payment :</b></td></tr>");
            sb.Append("<tr><td><b>1.</b>Consumer login against property address or consumer no.</td></tr>");
            sb.Append("<tr><td><b>2.</b>Click to pay now to pay your bill amount.</td></tr>");
            sb.Append("</table></body></html>");
            mm.From = new MailAddress("noidajal@noidaauthorityonline.com", "Noida Jal Department");
            mm.To.Add(txtemail.Text);
            mm.Subject = " your Jal  bill for 2021- 2022";
            mm.Body = sb.ToString();
            if (fuAttachment.HasFile)
            {
                string FileName = Path.GetFileName(fuAttachment.PostedFile.FileName);
                mm.Attachments.Add(new Attachment(fuAttachment.PostedFile.InputStream, FileName));
                mm.IsBodyHtml = true;
                mm.Priority = MailPriority.High;
                SmtpClient ss = new SmtpClient();
                ss.Credentials = new NetworkCredential("noidajal@noidaauthorityonline.com", "Jal*&321");
                ss.Port = 587;
                ss.Host = "smtp.gmail.com";
                ss.EnableSsl = true;
                ss.Send(mm);
                Response.Write("<script>alert('email successfully send')</script>");
                txtemail.Text = "";
                txtname.Text = "";
            }
            else
            {
                Response.Write("<script>alert('Please Select File.')</script>");
            }

        }
        
    }
}
