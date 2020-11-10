using System;
using System.Collections.Generic;
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
            try
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
            }
        }
        #endregion

        #region Accepter une missionJour
        public ActionResult AccepterMissionJour(int id)
        {
            {
                try
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
                }
            }
        }
        #endregion

        #region Refuser une missionJour
        public ActionResult RefuserMissionJour(int id)
        {
            {
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
                }
            }
        }

        #endregion

    }
}