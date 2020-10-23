using Microsoft.AspNetCore.Mvc;

namespace SimuladorGerenciaMemoria.Controllers
{
    public class ErrosController : Controller
    {
        public ActionResult Error403()
        {
            return View();
        }

        public ActionResult Error404()
        {
            return View();
        }

        public ActionResult Error500()
        {
            return View();
        }
    }
}
