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
        // Base de données :
        BD_CRAEntities _dbContext = new BD_CRAEntities();

        public ActionResult Index()
        {

            return RedirectToAction("Login");
        }

        #region Login
        public ActionResult Login() {
            using (DAL dal = new DAL()) { 
                // Vérifier que l'utilisateur est déjà authentifié :
                if (Request.IsAuthenticated) // Si l'utilisateur est déjà authentifié
                {
                    // Vérifier le rôle de l'utilisateur :
                    bool isAdmin = dal.RecupererRole(@User.Identity.Name);
                    if (isAdmin == true) return RedirectToAction("AdminListeEmployes", "Utilisateur"); // Si l'utilisateur est un admin : rediriger l'user vers l'interface Admin

                    return RedirectToAction("InterfaceUser", "Home", new { id = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday) }); // Sinon : aller vers la vue réservée aux employés
                }
            }

            return View();
        }

        // Action du Login : Prend en paramètre un objet de type UserModel (l'username, le mot de passe et le status de l'utilisateur)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(UserModel user)
        {
            using (DAL dal = new DAL())
            {
                if (ModelState.IsValid) // Si le modèle de donnée est valide
                {
                    bool IsValidUser = _dbContext.UTILISATEUR.Any(u => u.MATRICULE == user.Username && user.Password == u.MOTDEPASSE); // On vérifie si dans la base de donnée, l'utilisateur existe
                    if (IsValidUser) // Si l"utilisateur existe
                    {
                        FormsAuthentication.SetAuthCookie(user.Username, false); // Stockage de l'username dans les cookies

                        // Vérifier que l'utilisateur est un administrateur ou non :
                        bool isAdmin = dal.RecupererRole(user.Username);
                        if (isAdmin == true) return RedirectToAction("AdminListeEmployes", "Utilisateur"); // Si l'utilisateur est un admin : rediriger l'user vers l'interface Admin

                        return RedirectToAction("InterfaceUser", "Home", new { id = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday) }); // Sinon : aller vers la vue réservée aux employés
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
        #endregion

        #region Logout
        // Déconnexion de l'utilisateur de l'application
        public ActionResult Logout()
        {
            // Suppression de son username de ses cookies
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
        #endregion

        #region Interface Employé
        // Permet de contôler l'affichage de la semaine utilisateur lorsqu'il modifie la semaine à afficher.
        // id = le numéro de la semaine à afficher.
        [Authorize(Roles = "User")]
        public ActionResult InterfaceUser(int id)
        {
            using (DAL dal = new DAL()) // Utilisation du Data Access Layer, permettant l'accès au données dans la BDD
            {           
                DateTime lundiSemaineCourante = DateExtensions.GetStartOfWeek(2020, id); // Récupérer le lundi de la semaine courante

                // Stockage de différentes valeurs dans le ViewBag qui seront utiles dans la vue InterfaceUser
                ViewBag.numsemaine = id; // Le numéro de la semaine courante
                ViewBag.LundiDate = lundiSemaineCourante; // La date du lundi de la semaine courante
                ViewBag.MardiDate = lundiSemaineCourante.AddDays(1); // La date du mardi de la semaine courante
                ViewBag.MercrediDate = lundiSemaineCourante.AddDays(2);
                ViewBag.JeudiDate = lundiSemaineCourante.AddDays(3);
                ViewBag.VendrediDate = lundiSemaineCourante.AddDays(4);
                ViewBag.SamediDate = lundiSemaineCourante.AddDays(5);
                ViewBag.DimancheDate = lundiSemaineCourante.AddDays(6);

                // Récupérer les différentes missionsJours pour chacun des jours dans la BDD :
                ViewBag.LundiMissions = dal.ListeMissionsJoursPourJourPourUser(User.Identity.Name, lundiSemaineCourante); // Récupérer les missions du lundi de la semaine courante
                ViewBag.MardiMissions = dal.ListeMissionsJoursPourJourPourUser(User.Identity.Name, lundiSemaineCourante.AddDays(1));
                ViewBag.MercrediMissions = dal.ListeMissionsJoursPourJourPourUser(User.Identity.Name, lundiSemaineCourante.AddDays(2));
                ViewBag.JeudiMissions = dal.ListeMissionsJoursPourJourPourUser(User.Identity.Name, lundiSemaineCourante.AddDays(3));
                ViewBag.VendrediMissions = dal.ListeMissionsJoursPourJourPourUser(User.Identity.Name, lundiSemaineCourante.AddDays(4));
                ViewBag.SamediMissions = dal.ListeMissionsJoursPourJourPourUser(User.Identity.Name, lundiSemaineCourante.AddDays(5));
                ViewBag.DimancheMissions = dal.ListeMissionsJoursPourJourPourUser(User.Identity.Name, lundiSemaineCourante.AddDays(6));

            }
            return View();
        }
        #endregion

        #region Afficher l'aide (côté User)
        // Permet d'afficher l'aide dans l'interface User : affiche les instructions d'utilisation de son semainier
        public ActionResult AideInterfaceUser(int id)
        {
            // Afficher le message d'aide :
            MessageBox.Show("• Cliquez sur le bouton \"Ajouter +\" sous le jour de la semaine auquel vous voulez ajouter une mission. \n\n• Choisissez une mission parmi la liste des missions disponibles pour le jour sélectionné ainsi que le temps que vous pensez y accorder. Cliquez sur \"Ajouter\" pour que la mission soit ajoutée. \n\n• Votre mission apparaît dans le semainier en couleur blanche. \n\n• Si vous souhaitez modifier une mission, cliquez sur l'icône crayon, qui vous permettra de modifier le champs \"Temps accordé\". \n\n• Si vous souhaitez sauvegarder une mission, cliquez sur l'icône disquette. La mission s'affichera en orange. \n\n• Si vous souhaitez supprimer une mission, cliquez sur l'icône croix. \n\n• Cliquez sur le bouton \"Envoyer\" sous le jour de la semaine correspondant aux missions que vous souhaitez envoyer pour validation à l'administrateur. \n\n• Le bouton \"Pré-initialiser semaine\" vous permet de dupliquer les missions affichées sur la semaine précédente.", "Aide");
            return RedirectToAction("InterfaceUser", "Home", new { id = id }); // Retourner sur la semaine où l'utilisateur se situait
        }
        #endregion

    }
}