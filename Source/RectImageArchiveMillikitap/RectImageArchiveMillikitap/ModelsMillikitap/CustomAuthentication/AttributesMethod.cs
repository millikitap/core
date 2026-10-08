using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace RectImageArchiveMillikitap.Models.CustomAuthentication
{
    public class AttributesMethod
    {
        public class NotAuthoriseAttribute : AuthorizeAttribute
        {
            protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
            {
                var refererUrl = filterContext.HttpContext.Request.Url.OriginalString;
                if (!string.IsNullOrEmpty(refererUrl))
                {
                    if(refererUrl.Contains("BookId=undefined"))
                    {
                        refererUrl = string.Empty;
                    }
                    else
                    refererUrl = refererUrl.Replace("&", "_");
                }
                if(!string.IsNullOrEmpty (refererUrl))
                {
                    filterContext.Result = new RedirectResult($"/Account/Login?ReturnUrl={refererUrl}");
                }
                else
                filterContext.Result = new RedirectResult($"/Account/Login");
            }
        }
    }
}