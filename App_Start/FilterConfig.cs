using System.Web;
using System.Web.Mvc;

namespace MediLinkCB
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            //filters.Add(new AuthorizeAttribute()); // This line enforces authentication globally
        }
    }
}
