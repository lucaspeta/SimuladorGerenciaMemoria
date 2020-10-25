using SimuladorGerenciaMemoria.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;

namespace SimuladorGerenciaMemoria.Utils
{
    public class RedirectAction : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            int? userId = filterContext.HttpContext.Session.GetInt32("UserID");

            if (userId == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Account",
                    action = "Login"
                }));

                return;
            }
        }
    }
}