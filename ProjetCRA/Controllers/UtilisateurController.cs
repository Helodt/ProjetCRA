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

        public ActionResult Index()
        {
            return View();
        }

        #region Liste des employés (côté Admin)
        public ActionResult AdminListeEmployes()
        {
            using (DAL dal = new DAL())
            {
                // Récupérer le jour courant et le stocket dans le ViewBag pour la vue :
                DateTime jourCourant = DateTime.Now; 
                ViewBag.jourCourant =jourCourant;

                ViewBag.listEmployes = db.UTILISATEUR.ToList(); // Récupérer la liste des employés de la BDD et la stocker dans le ViewBag
                return View();
            }
        }
        #endregion

        #region Rapport d'activité mensuel d'un employé (côté Admin)
        // Obtenir le rapport d'activité mensuel pour un employé pour un mois donné
        // Paramètre : objet de type EmployéMoisView (pour avoir le matricule, le nom et le prénom de l'employé, ainsi que le jour du mois où l'on se situe)
        public ActionResult RapportActivitéMensuel(EmployéMoisView employeMois)
        {
            using (DAL dal = new DAL())
            {
                // Stocker dans le ViewBag différentes valeurs
                ViewBag.JourMoisPrecedent = employeMois.JourMois.AddMonths(-1); // Le jour d'un mois précédent
                ViewBag.JourMoisSuivant = employeMois.JourMois.AddMonths(1); // Le jour d'un mois suivant
                ViewBag.Matricule = employeMois.Matricule; 
                ViewBag.Nom = employeMois.Nom;
                ViewBag.Prenom = employeMois.Prenom;
                ViewBag.MoisActuel = employeMois.JourMois.ToString("MMMM"); // Le nom du mois courant


                var firstDayOfMonth = new DateTime(employeMois.JourMois.Year, employeMois.JourMois.Month, 1); // Récupérer le premier jour du mois courant
                var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1); // Récupérer le dernier jour du mois courant

                ViewBag.ListeMissionsMois = dal.RapportMois(firstDayOfMonth, lastDayOfMonth, employeMois.Matricule); // Récupérer la liste des missions réalisées durant le mois choisi, et la stocker dans le ViewBag
                return View();
            }
        }
        #endregion
    }
}