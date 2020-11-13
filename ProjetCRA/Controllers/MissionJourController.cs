using System;
using System.Collections.Generic;
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

        #region Liste des missions en Attente de Validation
        public ActionResult AdminListeMissionJourAttenteValidation()
        {

            using (DAL dal = new DAL())
            {
                ViewBag.listMissionsEnAttenteValidation = dal.ListeMissionsEnAttenteValidation();
                return View();
            }
            
            /*try
            {
                var query = from mj in db.MISSIONJOUR
                            join m in db.MISSION on mj.MISSION_CODE equals m.CODE
                            join u in db.UTILISATEUR on m.UTILISATEUR_MATRICULE equals u.MATRICULE
                            where mj.ETAT == "EnAttenteValidation"
                            select new MissionsJoursAttenteValidationView()
                            {
                                CodeMissionJour = mj.IDJOUR,
                                CodeMission = m.CODE,
                                Libelle = m.LIBELLE,
                                Matricule = u.MATRICULE,
                                Nom = u.NOM,
                                Prenom = u.PRENOM,
                                Jour = mj.JOUR,
                                Temps = mj.TEMPS_ACCORDE
                            };

                ViewBag.listMissionsEnAttenteValidation = query.Distinct().ToList();
                return View();
            }
            catch (Exception e)
            {
                return HttpNotFound();
            }*/
        }
        #endregion

        #region Accepter une missionJour
        public ActionResult AccepterMissionJour(int id)
        {

            using (DAL dal = new DAL())
            {
                dal.AccepterMissionJour(id);
                return RedirectToAction("AdminListeMissionJourAttenteValidation");
            }
            /*try
            {
                MISSIONJOUR mission = db.MISSIONJOUR.Find(id); // Rechercher la mission dans la BDD
                if (mission != null) // Si la mission existe
                {
                    mission.ETAT = "Accepté";
                    db.SaveChanges();

                }
                return RedirectToAction("AdminListeMissionJourAttenteValidation");
            }
            catch (Exception e)
            {
                return RedirectToAction("AdminListeMissionJourAttenteValidation");
            }*/

        }
        #endregion

        #region Refuser une missionJour
        public ActionResult RefuserMissionJour(int id)
        {
            using (DAL dal = new DAL())
            {
                dal.RefuserMissionJour(id);
                return RedirectToAction("AdminListeMissionJourAttenteValidation");
            }
            /*
            try
            {
                MISSIONJOUR mission = db.MISSIONJOUR.Find(id); // Rechercher la mission dans la BDD
                if (mission != null) // Si la mission existe
                {
                    mission.ETAT = "Refusé";
                    db.SaveChanges();

                }
                return RedirectToAction("AdminListeMissionJourAttenteValidation");
            }
            catch (Exception e)
            {
                return RedirectToAction("AdminListeMissionJourAttenteValidation");
            }*/
            
        }

        #endregion

        #region Ajouter une missionJour (côté User)
        public ActionResult AjouterMissionJour(DateView dateUserMissionjour)
        {
            using (DAL dal = new DAL())
            {
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

        #region Envoyer une journée à validation (côté User)
        public ActionResult EnvoyerJourneeValidation(DateView dateUserMissionjour)
        {
            MessageBox.Show("Envoyer à validation ?");
            int numsemaine = CultureInfo.CurrentCulture.Calendar.GetWeekOfYear(dateUserMissionjour.DateJour, CalendarWeekRule.FirstDay, DayOfWeek.Monday);
            return RedirectToAction($"InterfaceUser/{numsemaine}", "Home");
        }
        #endregion
    }
}