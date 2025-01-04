using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RestaurantReservationSystem.CustomControllers
{
    /// <summary>
    /// Makes the controller "self aware" knowing it's own name
    /// and what Action was called.
    /// </summary>
    public class CognizantController : Controller
    {
        internal string ControllerName()
        {
            return ControllerContext.RouteData.Values["controller"].ToString();
        }
        internal string ActionName()
        {
            return ControllerContext.RouteData.Values["action"].ToString();
        }

      
        internal static string SplitCamelCase(string input)
        {
            return System.Text.RegularExpressions.Regex
                .Replace(input, "([A-Z])", " $1",
                System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
        }

    
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            //Add the Controller and Action names to ViewData
            string ControllerFriendlyName = SplitCamelCase(ControllerName());
            ViewData["ControllerName"] = ControllerName();
            ViewData["ControllerFriendlyName"] = ControllerFriendlyName;
            ViewData["ActionName"] = ActionName();
            ViewData["Title"] = ControllerFriendlyName + " " + ActionName();
            base.OnActionExecuting(context);
        }

       
        public override Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            //Add the Controller and Action names to ViewData
            string ControllerFriendlyName = SplitCamelCase(ControllerName());
            ViewData["ControllerName"] = ControllerName();
            ViewData["ControllerFriendlyName"] = ControllerFriendlyName;
            ViewData["ActionName"] = ActionName();
            ViewData["Title"] = ControllerFriendlyName + " " + ActionName();
            return base.OnActionExecutionAsync(context, next);
        }
    }

}
