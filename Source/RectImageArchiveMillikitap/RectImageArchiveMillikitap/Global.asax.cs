using Newtonsoft.Json;
using RectImageArchiveMillikitap.Models.CustomAuthentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace RectImageArchiveMillikitap
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);           
        }

        protected void Application_PostAuthenticateRequest(Object sender, EventArgs e)
        {
            HttpCookie authCookie = Request.Cookies["Cookie1"];
            if (authCookie != null)
            {
                try
                {
                    FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                    var serializeModel = JsonConvert.DeserializeObject<CustomSerializeModel>(authTicket.UserData);
                    CustomPrincipal principal = new CustomPrincipal(authTicket.Name);
                    principal.UserId = serializeModel.UserId;
                    principal.FirstName = serializeModel.FirstName;
                    principal.LastName = serializeModel.LastName;
                    principal.Roles = serializeModel.RoleName.ToArray<string>();
                    var user = (CustomMembershipUser)Membership.GetUser(authTicket.Name);
                    if (user != null)
                    {
                        HttpContext.Current.User = principal;
                    }
                    else
                    {
                        FormsAuthentication.SignOut();                        
                    }                    
                }
                catch
                {

                }
            }
        }
    }

    public class CustomSerializeModel { public int UserId { get; set; } public string FirstName { get; set; } public string LastName { get; set; } public List<string> RoleName { get; set; } public string Telephone { get; set; } public bool sms_Authentication { get; set; } public string sms_Code { get; set; } }
}
