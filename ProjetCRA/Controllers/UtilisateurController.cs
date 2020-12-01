using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ProjetCRA.Models;
using System.Windows.Forms;


namespace ProjetCRA.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UtilisateurController : Controller
    {

        BD_CRAEntities db = new BD_CRAEntities();

        // GET: Utilisateur
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult AdminListeEmployes()
        {
            using (DAL dal = new DAL())
            {
                DateTime jourCourant = DateTime.Now;
                ViewBag.jourCourant =jourCourant;

                ViewBag.listEmployes = db.UTILISATEUR.ToList();
                return View();
            }
        }


        public ActionResult RapportActivitéMensuel(EmployéMoisView employeMois)
        {
            using (DAL dal = new DAL())
            {
                ViewBag.JourMoisPrecedent = employeMois.JourMois.AddMonths(-1);
                ViewBag.JourMoisSuivant = employeMois.JourMois.AddMonths(1);
                ViewBag.Matricule = employeMois.Matricule;
                ViewBag.Nom = employeMois.Nom;
                ViewBag.Prenom = employeMois.Prenom;
                ViewBag.MoisActuel = employeMois.JourMois.ToString("MMMM");


                var firstDayOfMonth = new DateTime(employeMois.JourMois.Year, employeMois.JourMois.Month, 1);
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

                ViewBag.ListeMissionsMois = dal.RapportMois(firstDayOfMonth, lastDayOfMonth, employeMois.Matricule);


                return View();
            }
        }
    }
}