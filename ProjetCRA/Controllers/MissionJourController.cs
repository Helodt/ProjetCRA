using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Windows.Forms;
using ProjetCRA.Models;

namespace ProjetCRA.Controllers
{
    public class MissionJourController : Controller
    {
        BD_CRAEntities db = new BD_CRAEntities();

        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        #region Liste des missions en Attente de Validation (côté Admin)
        // Récupérer la liste des missionsJours qui sont en attente de validation, et les envoyer dans la vue via le ViewBag
        public ActionResult AdminListeMissionJourAttenteValidation()
        {

            using (DAL dal = new DAL()) // Utiliser le Data Access Layer
            {
                ViewBag.listMissionsEnAttenteValidation = dal.ListeMissionsEnAttenteValidation(); // Récupérer la liste des missionsJours qui ont l'état "EnAttenteValidation" dans la BDD, et les stocker dans le ViewBag.
                return View();
            }
            
        }
        #endregion

        [Authorize(Roles = "Admin")]
        #region Accepter une missionJour (côté Admin) 
        // Accepter une missionJour : id = l'identifiant de la missionJour
        public ActionResult AccepterMissionJour(int id) 
        {

            using (DAL dal = new DAL())
            {
                dal.AccepterMissionJour(id); // Permet de passer l'état d'une missionJour de "EnAttenteValidation" à "Acceptée"
                return RedirectToAction("AdminListeMissionJourAttenteValidation"); // Retourner à la vue permettant de voir les missions en attente de validation
            }

        }
        #endregion

        [Authorize(Roles = "Admin")]
        #region Refuser une missionJour
        // Refuser une missionJour : id = l'identifiant de la missionJour
        public ActionResult RefuserMissionJour(int id) 
        {
            using (DAL dal = new DAL())
            {
                dal.RefuserMissionJour(id); // Permet de passer l'état d'une missionJour de "EnAttenteValidation" à "Refusée"
                return RedirectToAction("AdminListeMissionJourAttenteValidation");  // Retourner à la vue permettant de voir les missions en attente de validation
            }
        }

        #endregion

        [Authorize(Roles = "User")]
        #region Ajouter une missionJour (côté User)
        // Ajouter une missionJour dans la BDD
        // En paramètre : objet de type DateView (permettant d'avoir le matricule de l'utilisateur et la date du jour pour laquelle la mission doit être ajoutée)
        public ActionResult AjouterMissionJour(DateView dateUserMissionjour) 
        {
            using (DAL dal = new DAL())
            {
                // Vérifier que l'utilisateur qui souhaite ajouter une mission jour dans son calendrier correspond bien à ce même utilisateur
                if (dateUserMissionjour.Matricule != @User.Identity.Name) return RedirectToAction("InterfaceUser", "Home", new { id = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday) });

                ViewBag.dateJour = dateUserMissionjour.DateJour; // Stocker dans le ViewBag la date du jour
                ViewBag.numSemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateUserMissionjour.DateJour, CalendarWeekRule.FirstDay, DayOfWeek.Monday); // Stocker dans le ViewBag le numéro de la semaine

                // Récupérer la liste des missions en cours que l'utilisateur peut choisir pour le jour sélectionné :
                ViewBag.MissionsDisponibles = dal.MissionsDisponiblesJourUser(dateUserMissionjour.DateJour, User.Identity.Name);

            }

            return View("AjouterMissionJour");
        }

        // Ajoute une MissionJour dans la BDD
        [HttpPost]
        public ActionResult AjouterMissionJourX(MISSIONJOUR mission)
        {
            using (DAL dal = new DAL()) // Utiliser la BDD
            {
                if (mission.TEMPS_ACCORDE > 1) // Si le temps accordé est supérieur à 1 : la missionJour n'est pas prise en compte et affichage d'un message d'erreur
                {
                    MessageBox.Show("Le temps accordé à la mission doit être compris entre 0 et 1", "Echec");
                    return RedirectToAction("InterfaceUser", "Home", new { id = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday) });
                }
                
                Boolean missionJourAjoutée = dal.AjouterMissionJour(mission); // Ajouter la mission jour dans la BDD, et récupérer un boolean indiquant si la missionJour a bien été ajoutée dans la BDD
                if (missionJourAjoutée == false) // Si elle a bien été ajoutée : affichage d'un message de confirmation
                {
                    MessageBox.Show("Echec lors de l'ajout de la mission", "Echec");
                } 

            }
            return RedirectToAction("InterfaceUser", "Home", new { id = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(mission.JOUR, CalendarWeekRule.FirstDay, DayOfWeek.Monday) });
        }
        #endregion

        [Authorize(Roles = "User")]
        #region Envoyer une journée à validation (côté User)
        // Ajouter une missionJour dans la BDD
        // En paramètre : objet de type DateView (permettant d'avoir le matricule de l'utilisateur et la date du jour pour laquelle la journée doit être envoyée à validation)
        public ActionResult EnvoyerJourneeValidation(DateView dateUserMissionjour)
        {
            using (DAL dal = new DAL()) // Utiliser le Data Acess Layer
            {
                // Vérifier que l'utilisateur qui souhaite envoyer une journée à validation est le bon utilisateur
                if (dateUserMissionjour.Matricule != @User.Identity.Name) return RedirectToAction("InterfaceUser", "Home", new { id = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday) });
                
                // Message de confirmation :
                DialogResult result = MessageBox.Show("Êtes-vous sûr de vouloir envoyer la journée à validation ?", "Envoyer la journée à validation", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    Boolean journeeValide = dal.VerifierJourneeValidation(dateUserMissionjour.DateJour, dateUserMissionjour.Matricule); // Vérifier que la journée peut être envoyée à validation (que le temps de la journée ne dépasse pas 1)

                    if (journeeValide == false) MessageBox.Show("Impossible d'envoyer la semaine à validation : le temps total de la journée est supérieur à 1", "Erreur"); // Si la journée n'est pas valide, affichage d'un message d'erreur
                    else
                    {
                        Boolean envoiJournee = dal.EnvoyerJourneeValidation(dateUserMissionjour.DateJour, dateUserMissionjour.Matricule); // Envoi de la journée à validation
                        if (envoiJournee == false) MessageBox.Show("Erreur lors de l'envoi de la journée à validation ", "Erreur");

                    }
                }
                return RedirectToAction("InterfaceUser", "Home", new { id = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateUserMissionjour.DateJour, CalendarWeekRule.FirstDay, DayOfWeek.Monday) });
            }
            
        }
        #endregion

        [Authorize(Roles = "User")]
        #region Modifier une missionJour (côté user)
        // Modifier une missionJour : id = l'identifiant de la missionJour
        public ActionResult ModifierMissionJour(int id)
        {
            using (DAL dal = new DAL()) // Utiliser le Data Acess Layer
            {
                int numsemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dal.JourMissionJourOrDefault(id), CalendarWeekRule.FirstDay, DayOfWeek.Monday); // Récupérer le numéro de la semaine courante
                ViewBag.numsemaine = numsemaine; // Stocker le numéro dans le ViewBag

                MISSIONJOUR mission = dal.MissionJourExiste(id); // Vérifier que la MissionJour existe dans la BDD

                if (mission != null)
                {
                    // Vérifier que le matricule de l'utilisateur connecté est bien identique au matricule affecté à la missionJour qui est en train d'être modifié
                    MissionsEnCoursView verifMatricule = dal.RecupérerMatriculeMJ(mission);
                    string matriculeMission = verifMatricule.Matricule.Substring(0, @User.Identity.Name.Length);
                    if (matriculeMission == @User.Identity.Name) // Si le matricule est correct, renvoyer la vue correspondant à la modification de la mission
                    {
                        return View("ModifierMissionJour", mission);
                    }
                }

                return RedirectToAction($"InterfaceUser/{numsemaine}", "Home");
            }
        }

        // Modifier une missionJour : prend en paramètre un objet MISSIONJOUR qui sera la MissionJour à modifier
        [HttpPost]
        public ActionResult ModifierMissionJour(MISSIONJOUR mission)
        {
            try
            {
                if (ModelState.IsValid) // Si le modèle est invalide
                {
                    if (mission.TEMPS_ACCORDE > 1) throw new Exception(); // Si le temps de la mission est supérieur à 1 : lancer une exception afin de ne pas modifier la missionJour dans la BDD
                    mission.ETAT = "NonSauvegardé"; // Mettre l'état de la mission à "NonSauvegardé"
                    db.Entry(mission).State = EntityState.Modified; // Modification de la mission dans la BDD
                    db.SaveChanges(); // Enregistrement des changements dans la BDD
                }
            }
            catch (Exception e)
            {
            }
            int numsemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(mission.JOUR, CalendarWeekRule.FirstDay, DayOfWeek.Monday); // Récupérer le numéro de la semaine courante
            return RedirectToAction($"InterfaceUser/{numsemaine}", "Home");
        }
        #endregion

        [Authorize(Roles = "User")]
        #region Supprimer une missionJour (côté User)
        // Supprimer une missionJour : id = l'identifiant de la missionJour
        public ActionResult SupprimerMissionJour(int id)
        {
            using (DAL dal = new DAL())
            {
                // Récupérer le numéro de la semaine correspondant à la missionJour qui va être supprimée :
                int numsemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dal.JourMissionJourOrDefault(id), CalendarWeekRule.FirstDay, DayOfWeek.Monday);

                // Vérifier que le matricule de l'utilisateur connecté est bien identique au matricule affecté à la missionJour qui est en train d'être supprimée
                MissionsEnCoursView verifMatricule = dal.RecupérerMatriculeMJid(id);
                string matriculeMission = verifMatricule.Matricule.Substring(0, @User.Identity.Name.Length);
                if (matriculeMission == @User.Identity.Name) // Si le matricule est correct :
                {

                    Boolean MissionSupprimee = dal.SupprimerMissionJour(id); // Supprimer la missionJour, et conserver un boolean : true = la mission a été supprimée, false = la mission n'a pas été supprimée
                    if (!MissionSupprimee) MessageBox.Show("La mission n'a pas pu être supprimée", "Echec"); // Si la mission n'a pas été supprimée : affichage d'un message d'erre
                    
                }
                
                return RedirectToAction($"InterfaceUser/{numsemaine}", "Home");
            }
        }
        #endregion

        [Authorize(Roles = "User")]
        #region Sauvegarder une missionJour (côté User)
        // Sauvegarder une missionJour : id = l'identifiant de la missionJou
        public ActionResult SauvegarderMissionJour(int id)
        {
            using (DAL dal = new DAL())
            {
                // Vérifier que le matricule de l'utilisateur connecté est bien identique au matricule affecté à la missionJour qui est en train d'être sauvegardée
                MissionsEnCoursView verifMatricule = dal.RecupérerMatriculeMJid(id);
                string matriculeMission = verifMatricule.Matricule.Substring(0, @User.Identity.Name.Length);

                if (matriculeMission == @User.Identity.Name) // Si le matricule est correct :
                {
                    dal.SauvegarderMissionJour(id); // Passer l'état de la missionJour à "Sauvegardée" dans la BDD
                }
                int numsemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dal.JourMissionJourOrDefault(id), CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                return RedirectToAction($"InterfaceUser/{numsemaine}", "Home");
            }
        }
        #endregion

        [Authorize(Roles = "Admin")]
        #region Missions en cours pour un employé (côté Admin)
        // Récupérer les missions en cours d'un employé et les stocker dans le ViewBag pour pouvoir les afficher dans la vue
        // Paramètre : un objet EmployéMoisView (afin d'avoir le matricule, le nom et le prénom de l'employé concerné)
        public ActionResult MissionsEnCoursEmploye(EmployéMoisView employeMois)
        {
            using (DAL dal = new DAL()) // Utiliser le Data Access Layer
            {
                // Stocker différentes valeurs dans la ViewBag qui seront utiles à la vue
                ViewBag.Matricule = employeMois.Matricule;
                ViewBag.Nom = employeMois.Nom;
                ViewBag.Prenom = employeMois.Prenom;
                ViewBag.ListeMissions = dal.ListeMissionsEnCoursEmployé(employeMois.Matricule); // Récupérer les missions en cours d'un employés et les stocker dans le ViewBag

                return View();
            }
        }
        #endregion

        [Authorize(Roles = "Admin")]
        #region Missions archivées pour un employé (côté Admin)
        // Récupérer les missions terminées d'un employé et les stocker dans le ViewBag pour pouvoir les afficher dans la vue
        // Paramètre : un objet EmployéMoisView (afin d'avoir le matricule, le nom et le prénom de l'employé concerné)
        public ActionResult MissionsArchivéesEmploye(EmployéMoisView employeMois)
        {
            using (DAL dal = new DAL()) // Utiliser le Data Acess Layer
            {
                // Stocker dans le ViewBag différentes valeurs utiles à la vue :
                ViewBag.Matricule = employeMois.Matricule;
                ViewBag.Nom = employeMois.Nom;
                ViewBag.Prenom = employeMois.Prenom;
                ViewBag.ListeMissions = dal.ListeMissionsArchivéesEmployé(employeMois.Matricule); // Récupérer les missions archivées d'un employés et les stocker dans le ViewBag
                return View();
            }
        }
        #endregion

        [Authorize(Roles = "User")]
        #region Pré-initialiser semaine (côté User)
        // Pré-initialiser une semaine du semainier de l'employé
        // Paramètre : un objet DateView (afin d'avoir le matricule de l'employé concerné, et la date du lundi de la semaine à pré-initialiser)
        public ActionResult PreinitialiserSemaine(DateView dateUserMissionjour)
        {
            using (DAL dal = new DAL())
            {
                // Vérifier que le matricule de l'utilisateur connecté est équivalent au matricule de l'utilisateur qui voit sa semaine pré-initialisée
                if (@User.Identity.Name == dateUserMissionjour.Matricule)
                {
                    // Récupérer la liste des missionsJours de la semaine précédente :
                    List<MissionsJourUserView> listeMissions = dal.MissionsJoursSemainePrecedente(dateUserMissionjour.DateJour, dateUserMissionjour.Matricule);
                    
                    // Affecter les missionsJours récupérées à la semaine voulue :
                    dal.PréinitialiserSemaine(listeMissions); // Pré-initialiser la semaine avec les missionsJours de la semaine précédente
                }

                int numsemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateUserMissionjour.DateJour, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                return RedirectToAction($"InterfaceUser/{numsemaine}", "Home");
            }
        }

        #endregion
    }
}