namespace RestaurantReservationSystem.Utilities
{
    public static class MaintainURL
    {
       
        public static string ReturnURL(HttpContext httpContext, string ControllerName)
        {
            string cookieName = ControllerName + "URL";
            string SearchText = "/" + ControllerName + "?";
            //Get the URL of the page that sent us here
            string returnURL = httpContext.Request.Headers["Referer"].ToString();
            if (returnURL.Contains(SearchText))
            {
                //Came here from the Index with some parameters
                //Save the Parameters in a Cookiein
                returnURL = returnURL[returnURL.LastIndexOf(SearchText)..];
                CookieHelper.CookieSet(httpContext, cookieName, returnURL, 30);
                return returnURL;
            }
            else
            {
                //Get it from the Cookie
                
                returnURL = httpContext.Request.Cookies[cookieName] ?? "/" + ControllerName;
                return returnURL;
            }
        }
    }
}
