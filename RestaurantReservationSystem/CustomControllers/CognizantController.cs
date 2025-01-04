using Microsoft.AspNetCore.Mvc;

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
            return ControllerContext.RouteData.Values["controller"]?.ToString() ?? string.Empty;
        }
        internal string ActionName()
        {
            return ControllerContext.RouteData.Values["action"]?.ToString() ?? string.Empty;
        }
    }

}
