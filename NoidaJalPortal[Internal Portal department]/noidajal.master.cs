using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Linq;

public partial class noidajal : System.Web.UI.MasterPage
{
    protected void Page_Load(object sender, EventArgs e)
    {
        if (Session["USERID"] == null || Page.Session.Count == 0)
        {
            Response.Redirect("~/NoidaJal_login.aspx");
        }
        else
        {

            if (Page.User.Identity.IsAuthenticated)
            {
                Response.Buffer = true;
                Response.ExpiresAbsolute = DateTime.Now.AddDays(-1d);
                Response.Expires = -300000;
                Response.CacheControl = "no-cache";
                //Page.Session.Timeout = 200000;
                try
                {
                    menuGeneration();
                    lbladmin.Text = " Welcome " + Session["NAME"].ToString() + ", DATE: " + System.DateTime.Now.ToString("dd-MMM-yyyy");
                }
                catch (Exception ex)
                {

                    Response.Redirect("~/NoidaJal_login.aspx");

                }

            }
            else
            {
                Session.Clear();
                Session.Abandon();
                FormsAuthentication.SignOut();
                Response.Redirect("~/NoidaJal_login.aspx");
            }
        }

    }
    public void menuGeneration()
    {
        DataSet dsMenu = new DataSet();
        string[] rlecolParam = { "USER_ROLE" };
        string[] rlecondParam = { "USERID='" + Session["USERID"] + "'" };
        string rle = BAL.BAL.CInst().ReadSingleStringFromDb("USER_MASTER", rlecolParam, rlecondParam, "USERID");


        dsMenu = BAL.BAL.CInst().SelectTable("MENUMASTER", "MENU", "MENUID");

        string menu = "";
        menu = "<ul id='MenuBar1' class='MenuBarHorizontal'>";

        foreach (DataRow dr in dsMenu.Tables["MENU"].Rows)
        {
            if (!dr["ROLE"].ToString().Contains(rle))
            {

                menu = menu + "<li><a href='javascript:;' disabled='Disabled'>" + dr["MENUNAME"].ToString() + "</a></li>";
            }
            else
            {
                if (dr["HASCHILD"].ToString() == "1")
                {
                    menu = menu + "<li><a class='MenuBarItemSubmenu' href='#'>" + dr["MENUNAME"].ToString() + "</a>";
                    DataSet dsSUB_Menu = new DataSet();
                    string[] mscolparam = { "*" };
                    string[] msconparam = { "MENUID='" + dr["MENUID"].ToString() + "'" };
                    dsSUB_Menu = BAL.BAL.CInst().SelectTable("MENUSUBMASTER", mscolparam, msconparam, "SUBMENU", "SUBMENUID");


                    if (dsSUB_Menu.Tables["SUBMENU"].Rows.Count > 0)
                    {
                        menu = menu + "<ul>";

                        foreach (DataRow dr1 in dsSUB_Menu.Tables["SUBMENU"].Rows)
                        {
                            if (!dr1["ROLE"].ToString().Contains(rle))
                            {

                                menu = menu + "<li><a href='javascript:;' disabled='Disabled'>" + dr1["MENUNAME"].ToString() + "</a></li>";
                            }
                            else
                            {
                                if (dr1["HASCHILD"].ToString() == "1")
                                {

                                    menu = menu + "<li><a class='MenuBarItemSubmenu' href='#'>" + dr1["MENUNAME"].ToString() + "</a>";
                                    DataSet dsCHILD_Menu = new DataSet();
                                    string[] mschildcolparam = { "*" };
                                    string[] mschildconparam = { "SUBMENUID='" + dr1["SUBMENUID"].ToString() + "'" };
                                    dsCHILD_Menu = BAL.BAL.CInst().SelectTable("MENUSUBCHILDMASTER", mschildcolparam, mschildconparam, "CHILDMENU", "CHILDMENUID");

                                    if (dsCHILD_Menu.Tables["CHILDMENU"].Rows.Count > 0)
                                    {
                                        menu = menu + "<ul>";

                                        foreach (DataRow dr2 in dsCHILD_Menu.Tables["CHILDMENU"].Rows)
                                        {


                                            if (!dr2["ROLE"].ToString().Contains(rle))
                                            {

                                                menu = menu + "<li><a href='javascript:;' disabled='Disabled'>" + dr2["MENUNAME"].ToString() + "</a></li>";
                                            }
                                            else
                                            {
                                                if (dr2["HASCHILD"].ToString() == "1")
                                                {
                                                    menu = menu + "<li><a class='MenuBarItemSubmenu' href='#'>" + dr2["MENUNAME"].ToString() + "</a>";
                                                    DataSet dsSUB_CHILD_Menu = new DataSet();
                                                    string[] mssubchildcolparam = { "*" };
                                                    string[] mssubchildconparam = { "CHILDMENUID='" + dr2["CHILDMENUID"].ToString() + "'" };
                                                    dsSUB_CHILD_Menu = BAL.BAL.CInst().SelectTable("MENU_CHILD_SUBMENU", mssubchildcolparam, mssubchildconparam, "SUBCHILDMENU", "CHILD_SUBMENU_ID");


                                                    if (dsSUB_CHILD_Menu.Tables["SUBCHILDMENU"].Rows.Count > 0)
                                                    {
                                                        menu = menu + "<ul>";
                                                        foreach (DataRow dr3 in dsSUB_CHILD_Menu.Tables["SUBCHILDMENU"].Rows)
                                                        {
                                                            if (!dr3["ROLE"].ToString().Contains(rle))
                                                            {
                                                                menu = menu + "<li><a href='javascript:;' disabled='Disabled'>" + dr3["MENUNAME"].ToString() + "</a></li>";
                                                            }
                                                            else
                                                            {
                                                                if (dr3["HASCHILD"].ToString() == "1")
                                                                {
                                                                    //implement code here for more depth
                                                                }
                                                                else if (dr3["HASCHILD"].ToString() == "9")  // will be open as popup window
                                                                {
                                                                    menu = menu + "<li><a href=" + dr3["LINKURL"].ToString() + " target='_blank' >" + dr3["MENUNAME"].ToString() + "</a></li>";
                                                                }
                                                                else
                                                                {
                                                                    menu = menu + "<li><a href=" + dr3["LINKURL"].ToString() + ">" + dr3["MENUNAME"].ToString() + "</a></li>";
                                                                }
                                                            }
                                                        }
                                                        menu = menu + "</ul>";
                                                    }

                                                }
                                                else if (dr2["HASCHILD"].ToString() == "9")  // will be open as popup window
                                                {
                                                    menu = menu + "<li><a href=" + dr2["LINKURL"].ToString() + " target='_blank' >" + dr2["MENUNAME"].ToString() + "</a></li>";
                                                }
                                                else
                                                {
                                                    menu = menu + "<li><a href=" + dr2["LINKURL"].ToString() + ">" + dr2["MENUNAME"].ToString() + "</a></li>";
                                                }
                                            }


                                        }
                                        menu = menu + "</ul>";
                                    }

                                }
                                else if (dr1["HASCHILD"].ToString() == "9")  // will be open as popup window
                                {
                                    menu = menu + "<li><a href=" + dr1["LINKURL"].ToString() + " target='_blank' >" + dr1["MENUNAME"].ToString() + "</a></li>";
                                }
                                else
                                {
                                    menu = menu + "<li><a href=" + dr1["LINKURL"].ToString().Trim() + ">" + dr1["MENUNAME"].ToString() + "</a>";
                                }


                                menu = menu + "</li>";
                            }
                        }
                        menu = menu + "</ul>";
                    }
                }
                else if (dr["HASCHILD"].ToString() == "9")  // will be open as popup window
                {
                    menu = menu + "<li><a href=" + dr["LINKURL"].ToString() + " target='_blank' >" + dr["MENUNAME"].ToString() + "</a></li>";
                }
                else
                {
                    menu = menu + "<li><a href=" + dr["LINKURL"].ToString().Trim() + ">" + dr["MENUNAME"].ToString() + "</a>";
                }
                menu = menu + "</li>";
            }


        }
        menu = menu + "</ul>";


        mnuDiv.InnerHtml = menu;
    }
    protected void linl_home_Click(object sender, ImageClickEventArgs e)
    {
        Response.Redirect("~/MainPage/Welcome.aspx");
    }
    protected void link_logout_Click(object sender, ImageClickEventArgs e)
    {
        //string[] valparam = {   "@P_USER_ID,"+ Session["USERID"].ToString(),
        //                       "@P_USER_LOG_ID,"+Session["ID"].ToString(),
        //                        "@P_USER_LOGOUT_TIME,"+ DateTime.Now.ToString("HH:mm:ss tt"),
        //                        "@P_OPERATION_TYPE,2"
                            
        //                    };
        //string returnval = BAL.BAL.CInst().ProcedureCallSingle("PRCO_USER_LOGIN_INFO", valparam);
        //if (returnval == "1")
        //{
            if (Session.Count > 0)
            {
                Session.Abandon();
                Session.Contents.Clear();
                Response.Redirect("~/NoidaJal_login.aspx");
            }
        //}
    }
}
