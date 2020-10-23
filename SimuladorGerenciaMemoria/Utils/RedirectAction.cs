using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using SimuladorGerenciaMemoria.Models;

namespace SimuladorGerenciaMemoria.Utils
{    
   public class RedirectAction : ActionFilterAttribute 
    {
        /*private readonly IHttpContextAccessor _httpContextAccessor;
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            _httpContextAccessor = httpContextAccessor;


        public void YourMethodName()
        {
            var userId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

            if (user == null)
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new
                {
                    controller = "Conta",
                    action = "Entrar"
                }));
                return;
            }
        }*/
    }
}