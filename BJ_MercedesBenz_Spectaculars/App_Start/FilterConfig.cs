using System.Web;
using System.Web.Mvc;

namespace BJ_MercedesBenz_Spectaculars
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
