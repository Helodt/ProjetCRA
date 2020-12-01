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
using System.Web.UI.WebControls;

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
            using (DAL dal = new DAL())
            {
                if (ModelState.IsValid)
                {
                    bool IsValidUser = _dbContext.UTILISATEUR.Any(u => u.MATRICULE == user.Username && user.Password == u.MOTDEPASSE);

                    if (IsValidUser)
                    {
                        FormsAuthentication.SetAuthCookie(user.Username, false);

                        // Vérifier que l'utilisateur est bien un administrateur :
                        bool isAdmin = dal.RecupererRole(user.Username);
                        if (isAdmin == true) return RedirectToAction("AdminListeEmployes", "Utilisateur"); // Aller vers la vue Admin

                        return RedirectToAction("InterfaceUser", "Home", new { id = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday) }); // Aller vers la vue User
                    }
                    else
                    {
                        MessageBox.Show("Identifiant ou mot de passe incorrect");
                    }
                }
                ModelState.AddModelError("", "invalid Username or Password");
                return View();
            }
        }


        // Permet de contôler l'affichage de la semaine utilisateur lorsqu'il modifie la semaine à afficher.
        // id = le numéro de la semaine à afficher.
        [Authorize(Roles = "User")]
        public ActionResult InterfaceUser(int id)
        {
            using (DAL dal = new DAL())
            {           

                DateTime lundiSemaineCourante = DateExtensions.GetStartOfWeek(2020, id);

                ViewBag.numsemaine = id;

                ViewBag.LundiDate = lundiSemaineCourante;
                ViewBag.MardiDate = lundiSemaineCourante.AddDays(1);
                ViewBag.MercrediDate = lundiSemaineCourante.AddDays(2);
                ViewBag.JeudiDate = lundiSemaineCourante.AddDays(3);
                ViewBag.VendrediDate = lundiSemaineCourante.AddDays(4);
                ViewBag.SamediDate = lundiSemaineCourante.AddDays(5);
                ViewBag.DimancheDate = lundiSemaineCourante.AddDays(6);

                ViewBag.LundiMissions = dal.ListeMissionsJoursPourJourPourUser(User.Identity.Name, lundiSemaineCourante);
                ViewBag.MardiMissions = dal.ListeMissionsJoursPourJourPourUser(User.Identity.Name, lundiSemaineCourante.AddDays(1));
                ViewBag.MercrediMissions = dal.ListeMissionsJoursPourJourPourUser(User.Identity.Name, lundiSemaineCourante.AddDays(2));
                ViewBag.JeudiMissions = dal.ListeMissionsJoursPourJourPourUser(User.Identity.Name, lundiSemaineCourante.AddDays(3));
                ViewBag.VendrediMissions = dal.ListeMissionsJoursPourJourPourUser(User.Identity.Name, lundiSemaineCourante.AddDays(4));
                ViewBag.SamediMissions = dal.ListeMissionsJoursPourJourPourUser(User.Identity.Name, lundiSemaineCourante.AddDays(5));
                ViewBag.DimancheMissions = dal.ListeMissionsJoursPourJourPourUser(User.Identity.Name, lundiSemaineCourante.AddDays(6));

            }
            return View();
        }


        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}