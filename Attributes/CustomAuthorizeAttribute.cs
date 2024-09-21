using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MediLinkCB.Attributes // Adjust the namespace according to your project's structure
{
    public class CustomAuthorizeAttribute : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                // Authenticated but not authorized
                filterContext.Result = new ViewResult { ViewName = "Unauthorized" };
            }
            else
            {
                // Not authenticated, redirect to login
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Login", action = "Login", returnUrl = filterContext.HttpContext.Request.RawUrl }));
            }
        }
    }
}