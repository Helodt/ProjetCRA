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
        // GET: MissionJour
        public ActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        #region Liste des missions en Attente de Validation
        public ActionResult AdminListeMissionJourAttenteValidation()
        {

            using (DAL dal = new DAL())
            {
                ViewBag.listMissionsEnAttenteValidation = dal.ListeMissionsEnAttenteValidation();
                return View();
            }
            
        }
        #endregion

        [Authorize(Roles = "Admin")]
        #region Accepter une missionJour
        public ActionResult AccepterMissionJour(int id)
        {

            using (DAL dal = new DAL())
            {
                dal.AccepterMissionJour(id);
                return RedirectToAction("AdminListeMissionJourAttenteValidation");
            }

        }
        #endregion

        [Authorize(Roles = "Admin")]
        #region Refuser une missionJour
        public ActionResult RefuserMissionJour(int id)
        {
            using (DAL dal = new DAL())
            {
                dal.RefuserMissionJour(id);
                return RedirectToAction("AdminListeMissionJourAttenteValidation");
            }
        }

        #endregion

        [Authorize(Roles = "User")]
        #region Ajouter une missionJour (côté User)
        public ActionResult AjouterMissionJour(DateView dateUserMissionjour)
        {
            using (DAL dal = new DAL())
            {
                // Vérifier que l'utilisateur qui souhaite ajouter une mission jour dans son calendrier correspond bien à ce même utilisateur
                if (dateUserMissionjour.Matricule != @User.Identity.Name) return RedirectToAction("InterfaceUser", "Home", new { id = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday) });

                ViewBag.dateJour = dateUserMissionjour.DateJour;
                ViewBag.numSemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateUserMissionjour.DateJour, CalendarWeekRule.FirstDay, DayOfWeek.Monday);

                // Récupérer la liste des missions en cours que l'utilisateur peut choisir pour le jour sélectionné :
                ViewBag.MissionsDisponibles = dal.MissionsDisponiblesJourUser(dateUserMissionjour.DateJour, User.Identity.Name);

            }

            return View("AjouterMissionJour");
        }

        [HttpPost]
        public ActionResult AjouterMissionJourX(MISSIONJOUR mission)
        {
            using (DAL dal = new DAL())
            {
                if (mission.TEMPS_ACCORDE > 1)
                {
                    MessageBox.Show("Le temps accordé à la mission doit être compris entre 0 et 1", "Echec");
                    return RedirectToAction("InterfaceUser", "Home", new { id = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday) });
                }
                
                Boolean missionJourAjoutée = dal.AjouterMissionJour(mission);
                if (missionJourAjoutée == true)
                {
                    MessageBox.Show("Mission ajoutée", "Succès");
                } else
                {
                    MessageBox.Show("Echec lors de l'ajout de la mission", "Echec");
                }

            }
            return RedirectToAction("InterfaceUser", "Home", new { id = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday) });
        }
        #endregion

        [Authorize(Roles = "User")]
        #region Envoyer une journée à validation (côté User)
        public ActionResult EnvoyerJourneeValidation(DateView dateUserMissionjour)
        {
            using (DAL dal = new DAL())
            {

                if (dateUserMissionjour.Matricule != @User.Identity.Name) return RedirectToAction("InterfaceUser", "Home", new { id = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday) });
                
                DialogResult result = MessageBox.Show("Êtes-vous sûr de vouloir envoyer la journée à validation ?", "Envoyer la journée à validation", MessageBoxButtons.YesNo);

                if (result == DialogResult.Yes)
                {
                    Boolean journeeValide = dal.VerifierJourneeValidation(dateUserMissionjour.DateJour, dateUserMissionjour.Matricule);

                    if (journeeValide == false) MessageBox.Show("Impossible d'envoyer la semaine à validation : le temps total de la journée est supérieur à 1", "Erreur");
                    else
                    {
                        Boolean envoiJournee = dal.EnvoyerJourneeValidation(dateUserMissionjour.DateJour, dateUserMissionjour.Matricule);
                        if (envoiJournee == true) MessageBox.Show("Journée envoyée à validation avec succès", "Succès");
                        else MessageBox.Show("Erreur lors de l'envoi de la journée à validation ", "Erreur");

                    }
                }

                return RedirectToAction("InterfaceUser", "Home", new { id = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday) });
            }
            
        }
        #endregion

        [Authorize(Roles = "User")]
        #region Modifier une missionJour (côté user)
        public ActionResult ModifierMissionJour(int id)
        {
            using (DAL dal = new DAL())
            {
                int numsemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                ViewBag.numsemaine = numsemaine;

                MISSIONJOUR mission = dal.MissionJourExiste(id);

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


        [HttpPost]
        public ActionResult ModifierMissionJour(MISSIONJOUR mission)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (mission.TEMPS_ACCORDE > 1) throw new Exception();
                    mission.ETAT = "NonSauvegardé";
                    db.Entry(mission).State = EntityState.Modified; // Modification de la mission
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
            }
            int numsemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            return RedirectToAction($"InterfaceUser/{numsemaine}", "Home");
        }
        #endregion

        [Authorize(Roles = "User")]
        #region Supprimer une missionJour (côté User)
        public ActionResult SupprimerMissionJour(int id)
        {
            using (DAL dal = new DAL())
            {
                
                // Vérifier que le matricule de l'utilisateur connecté est bien identique au matricule affecté à la missionJour qui est en train d'être supprimée
                MissionsEnCoursView verifMatricule = dal.RecupérerMatriculeMJid(id);
                string matriculeMission = verifMatricule.Matricule.Substring(0, @User.Identity.Name.Length);
                if (matriculeMission == @User.Identity.Name) // Si le matricule est correct :
                {

                    // Demander confirmation à l'user pour réaliser la suppression :
                    DialogResult result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer la mission " + id + " ?", "Supprimer la mission", MessageBoxButtons.YesNo);

                    // Si OK pour la suppression de la mission :
                    if (result == DialogResult.Yes)
                    {
                        Boolean MissionSupprimee = dal.SupprimerMissionJour(id);
                        if (MissionSupprimee) MessageBox.Show("La mission a bien été supprimée", "Succès");
                        else MessageBox.Show("La mission n'a pas pu être supprimée", "Echec");
                    }
                }
                int numsemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                return RedirectToAction($"InterfaceUser/{numsemaine}", "Home");
            }
        }
        #endregion

        [Authorize(Roles = "User")]
        #region Sauvegarder une missionJour (côté User)
        public ActionResult SauvegarderMissionJour(int id)
        {
            using (DAL dal = new DAL())
            {
                // Vérifier que le matricule de l'utilisateur connecté est bien identique au matricule affecté à la missionJour qui est en train d'être sauvegardée
                MissionsEnCoursView verifMatricule = dal.RecupérerMatriculeMJid(id);
                string matriculeMission = verifMatricule.Matricule.Substring(0, @User.Identity.Name.Length);
                if (matriculeMission == @User.Identity.Name) // Si le matricule est correct :
                {
                    dal.SauvegarderMissionJour(id);
                }
                int numsemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(DateTime.Now, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                return RedirectToAction($"InterfaceUser/{numsemaine}", "Home");
            }
        }
        #endregion

        [Authorize(Roles = "Admin")]
        #region Missions en cours pour un employé (côté Admin)
        public ActionResult MissionsEnCoursEmploye(EmployéMoisView employeMois)
        {
            using (DAL dal = new DAL())
            {
                ViewBag.Matricule = employeMois.Matricule;
                ViewBag.Nom = employeMois.Nom;
                ViewBag.Prenom = employeMois.Prenom;

                ViewBag.ListeMissions = dal.ListeMissionsEnCoursEmployé(employeMois.Matricule);

                return View();
            }
        }
        #endregion

        [Authorize(Roles = "Admin")]
        #region Missions archivées pour un employé (côté Admin)
        public ActionResult MissionsArchivéesEmploye(EmployéMoisView employeMois)
        {
            using (DAL dal = new DAL())
            {
                ViewBag.Matricule = employeMois.Matricule;
                ViewBag.Nom = employeMois.Nom;
                ViewBag.Prenom = employeMois.Prenom;

                ViewBag.ListeMissions = dal.ListeMissionsArchivéesEmployé(employeMois.Matricule);
                return View();
            }
        }
        #endregion

        [Authorize(Roles = "User")]
        #region Pré-initialiser semaine (côté User)
        public ActionResult PreinitialiserSemaine(DateView dateUserMissionjour)
        {
            using (DAL dal = new DAL())
            {
                // Vérifier que le matricule de l'utilisateur connecté est équivalent au matricule de l'utilisateur qui voit sa semaine pré-initialisée
                if (@User.Identity.Name == dateUserMissionjour.Matricule)
                {
                    List<MissionsJourUserView> listeMissions = dal.MissionsJoursSemainePrecedente(dateUserMissionjour.DateJour, dateUserMissionjour.Matricule);
                    dal.PréinitialiserSemaine(listeMissions);
                }

                int numsemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateUserMissionjour.DateJour, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
                return RedirectToAction($"InterfaceUser/{numsemaine}", "Home");

            }


        }

        #endregion


    }
}