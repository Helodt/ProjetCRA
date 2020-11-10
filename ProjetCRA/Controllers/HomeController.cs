using ProjetCRA.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ProjetCRA.Controllers
{
    public class HomeController : Controller
    {
        BD_CRAEntities _dbContext = new BD_CRAEntities();
        // GET: Home
        public ActionResult Index()
        {

            return RedirectToAction("Login");
        }

        public ActionResult Login(){
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserModel user)
        {
            if (ModelState.IsValid)
            {
                bool IsValidUser = _dbContext.UTILISATEUR
               .Any(u => u.MATRICULE == user.Username && 
               user.Password == u.MOTDEPASSE);

                if (IsValidUser)
                {
                    FormsAuthentication.SetAuthCookie(user.Username, false);

                    if (user.Username == "admin") return RedirectToAction("AdminListeEmployes", "Utilisateur");
                    return RedirectToAction("Login", "Home");
                }
            }
            ModelState.AddModelError("", "invalid Username or Password");
            return View();
        }
 
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}