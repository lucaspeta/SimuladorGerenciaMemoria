using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimuladorGerenciaMemoria.Models;
using SimuladorGerenciaMemoria.Utils;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SimuladorGerenciaMemoria.Controllers
{
    public class AccountController : Controller
    {
        private readonly SimuladorContext _context;

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(Login login)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }
                else
                {
                    string userName = login.Email;
                    string userPass = login.Password;

                    foreach (var user in _context.Users.ToList())
                    {
                        if (user.Email == userName || user.Login == userName)
                        {
                            if (userPass == PassGenerator.Decrypt(user.Password))
                            {
                                HttpContext.Session.SetString("Name", user.Name);
                                HttpContext.Session.SetInt32("UserID", user.ID);
                                HttpContext.Session.SetString("UserName", userName);

                                _context.SaveChanges();

                                return RedirectToAction("Index", "Home");
                            }
                            else
                            {
                                throw new Exception("Senha inválida.");
                            }
                        }
                    }

                    throw new Exception("Usuário não encontrado.");
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                HttpContext.Session.Clear();
                return View();
            }
        }


        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User usuario)
        {
            try
            {
                //Just verify if there is any user with the same nickName
                if (await _context.Users.ToListAsync() != null)
                {
                    foreach (var user in await _context.Users.ToListAsync())
                    {
                        if (user.Login == usuario.Login)
                        {
                            throw new Exception("Não foi possível realizar o cadastro, nome de usuário já utilizado!");
                        }

                        if (user.Email == usuario.Email)
                        {
                            throw new Exception("Não foi possível realizar o cadastro, email já utilizado por outro usuário!");
                        }
                    }
                }                

                usuario.Password = PassGenerator.Encrypt(usuario.Password);

                if (ModelState.IsValid)
                {                    
                    _context.Users.Add(usuario);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("Entrar");
                }
                else
                {
                    return View();
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
                return View();
            }
        }

        [RedirectAction]
        public ActionResult ChangePassword()
        {
            try
            {
                ViewBag.Icon = "fa fa-cog";
                User user = _context.Users.Find(Convert.ToInt32(HttpContext.Session.GetInt32("UserID")));

                if (user == null)
                {
                    throw new Exception("HttpBadRequest");
                }

                return View();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e.Message);
                return RedirectToAction("Error404", "Erros");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RedirectAction]
        public ActionResult ChangePasswod(ChangePassword _changePassword)
        {
            try
            {
                User usuario = _context.Users.Find(_changePassword.Id);

                if (ModelState.IsValid)
                {
                    if (PassGenerator.Decrypt(usuario.Password) != _changePassword.SenhaAntiga)
                    {
                        throw new Exception("Senha antiga inválida!");
                    }
                    else
                    {
                        if (_changePassword.NovaSenha == PassGenerator.Decrypt(usuario.Password))
                        {
                            throw new Exception("A nova senha tem que ser diferente da antiga.");
                        }

                        usuario.Password = PassGenerator.Encrypt(_changePassword.NovaSenha);
                        _context.SaveChanges();
                        return RedirectToAction("Index", "Home", new { id = usuario.ID });
                    }
                }
                else
                {
                    return View();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.Write("------------Erro: " + e);
                ViewBag.Error = e.Message;
                return View();
            }
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}