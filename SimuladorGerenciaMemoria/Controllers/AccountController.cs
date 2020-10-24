using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SimuladorGerenciaMemoria.Models;
using SimuladorGerenciaMemoria.Utils;
using System;
using System.Threading.Tasks;

namespace SimuladorGerenciaMemoria.Controllers
{
    public class AccountController : Controller
    {
        private readonly SimuladorContext _context;

        public AccountController(SimuladorContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(Login login)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View();
                }
                else
                {
                    string userLogin = login.User;
                    string userPass = login.Password;

                    var user = await _context.Users.SingleOrDefaultAsync(u => u.Login == userLogin);

                    if (user != null)
                    {
                        if (userPass == PassGenerator.Decrypt(user.Password))
                        {
                            HttpContext.Session.SetString("Name", user.Name);
                            HttpContext.Session.SetInt32("UserID", user.ID);
                            HttpContext.Session.SetString("UserName", user.Login);
                            HttpContext.Session.SetString("UserEmail", user.Email);
                           
                            return RedirectToAction("Index", "Home");
                        }
                        else 
                        {
                            throw new Exception("Senha inválida.");
                        }
                    }
                    else 
                    {
                        throw new Exception("Usuário não encontrado.");
                    }                    
                }
            }
            catch (Exception e)
            {
                ViewBag.Error = e.Message;
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
                if (await _context.Memories.CountAsync() > 0)
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

                    return RedirectToAction("Login");
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
        public IActionResult ChangePassword()
        {
            try
            {
                ViewBag.userName = HttpContext.Session.GetString("UserName");
                ViewBag.userID = HttpContext.Session.GetInt32("UserID");

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
        public async Task<IActionResult> ChangePassword(ChangePassword _changePassword)
        {
            ViewBag.userName = HttpContext.Session.GetString("UserName");
            ViewBag.userID = HttpContext.Session.GetInt32("UserID");

            try
            {
                User usuario = await _context.Users.FindAsync(_changePassword.Id);
                
                if (PassGenerator.Decrypt(usuario.Password) != _changePassword.SenhaAntiga)
                    throw new Exception("Senha antiga inválida!");
                else
                {
                    if (_changePassword.NovaSenha == PassGenerator.Decrypt(usuario.Password))
                        throw new Exception("A nova senha tem que ser diferente da antiga.");

                    if(_changePassword.NovaSenha != _changePassword.RepeteNovaSenha)
                        throw new Exception("As senhas devem ser iguais.");

                    usuario.Password = PassGenerator.Encrypt(_changePassword.NovaSenha);

                    await _context.SaveChangesAsync();

                    return RedirectToAction("Index", "Home", new { id = usuario.ID });
                }         
            }
            catch (Exception e)
            {
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