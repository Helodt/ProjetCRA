using ProjetCRA.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Windows.Forms;
using Developpez.Dotnet;

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
                    return RedirectToAction("InterfaceUser", "Home");
                }
            }
            ModelState.AddModelError("", "invalid Username or Password");
            return View();
        }

        public ActionResult InterfaceUser()
        {
            int numsemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            DateTime lundiSemaineCourante = DateExtensions.GetStartOfWeek(2020, numsemaine);
            DateTime mardiSemaineCourante = lundiSemaineCourante.AddDays(1);
            DateTime mercrediSemaineCourante = lundiSemaineCourante.AddDays(2);
            DateTime jeudiSemaineCourante = lundiSemaineCourante.AddDays(3);
            DateTime vendrediSemaineCourante = lundiSemaineCourante.AddDays(4);
            DateTime samediSemaineCourante = lundiSemaineCourante.AddDays(5);
            DateTime dimancheSemaineCourante = lundiSemaineCourante.AddDays(6);

            //MessageBox.Show(User.Identity.Name);
            return View();
        }
 
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}