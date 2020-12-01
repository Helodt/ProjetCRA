using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ProjetCRA.Models;
using System.Diagnostics;
using System.Data.Entity.Validation;
using System.Windows.Forms;

namespace ProjetCRA.Controllers
{
    [Authorize(Roles = "Admin")]
    public class MissionController : Controller
    {
        BD_CRAEntities db = new BD_CRAEntities();
        // GET: Mission
        public ActionResult Index()
        {
            return View();
        }

        #region Missions en cours
        #region Listes des missions en cours
        public ActionResult AdminMissionsEnCours()
        {
            using (DAL dal = new DAL())
            {
                ViewBag.listMissionsEnCours = dal.ObtenirListeMissionsEnCours();
                return View();
            }

        }
        #endregion

        #region Ajouter une mission
        public ActionResult AjouterMission()
        {
            try
            {
                return View();
            }
            catch (Exception e)
            {
                return HttpNotFound();
            }
        }

        [HttpPost]
        public ActionResult AjouterMission(MISSION mission)
        {
            using (DAL dal = new DAL())
            {
                Boolean missionAjoutée = dal.AjouterMission(mission);
                if (missionAjoutée == true)
                {
                    MessageBox.Show("La mission a bien été ajoutée", "Succès");
                    return RedirectToAction("AdminMissionsEnCours");
                }
                else
                {
                    MessageBox.Show("La mission n'a pas pu être ajoutée : données invalides ", "Erreur");
                    return RedirectToAction("AjouterMission");
                }
            }

        }
        #endregion

        #region Modifier une mission
        public ActionResult ModifierMission(string id)
        {
            using (DAL dal = new DAL())
            {
                MISSION mission = dal.MissionExiste(id);
                if (mission != null) return View("ModifierMission", mission);
                else return RedirectToAction("AdminMissionsEnCours");
            }
            
            /*try
            {
                // Rechercher la mission dans la BDD :
                MISSION mission = db.MISSION.Find(id);

                if (mission != null) // Si la mission existe
                {
                    return View("ModifierMission", mission);
                }
                return RedirectToAction("AdminMissionsEnCours");

            }
            catch (Exception e)
            {
                return RedirectToAction("AdminMissionsEnCours");
            }*/

        }

        [HttpPost]
        public ActionResult ModifierMission(MISSION mission)
        {
            try
            {
            if (ModelState.IsValid)
            {
                db.Entry(mission).State = EntityState.Modified; // Modification de la mission
                db.SaveChanges();
                MessageBox.Show("La mission a bien été modifiée", "Succès");
            }
            return RedirectToAction("AdminMissionsEnCours");

            }
            /*catch (DbEntityValidationException ex)
            {
                foreach (var entityValidationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in entityValidationErrors.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("Property: " + validationError.PropertyName + " Error: " + validationError.ErrorMessage + "\n");
                    }
                }
                return RedirectToAction("AdminMissionsEnCours");
            }*/
            catch (Exception e)
            {
                MessageBox.Show("La mission n'a pas pu être modifiée", "Echec");
                return RedirectToAction("AdminMissionsEnCours");
            }

        }
        #endregion

        #region Supprimer une mission
        public ActionResult SupprimerMission(string id)
        {
            using (DAL dal = new DAL())
            {
                // Demander confirmation à l'user pour réaliser la suppression :
                DialogResult result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer la mission " + id + " ?", "Supprimer la mission", MessageBoxButtons.YesNo);

                // Si OK pour la suppression de la mission :
                if (result == DialogResult.Yes)
                {
                    Boolean MissionSupprimee = dal.SupprimerMission(id);
                    if (MissionSupprimee) MessageBox.Show("La mission a bien été supprimée", "Succès");
                    else MessageBox.Show("La mission n'a pas pu être supprimée", "Echec");
                }
                return RedirectToAction("AdminMissionsEnCours");
            }


            /*
            // Demander confirmation à l'user pour réaliser la suppression :
            DialogResult result = MessageBox.Show("Êtes-vous sûr de vouloir supprimer la mission " + id + " ?", "Supprimer la mission", MessageBoxButtons.YesNo);

            // Si OK pour la suppression de la mission :
            if (result == DialogResult.Yes)
            {
                try
                {
                    MISSION mission = db.MISSION.Find(id); // Rechercher la mission dans la BDD
                    if (mission != null) // Si la mission existe
                    {
                        //Suppression de la mission :
                        db.MISSION.Remove(mission);

                        // Sauvegarder les changements de la BDD :
                        db.SaveChanges();
                        MessageBox.Show("La mission a bien été supprimée", "Succès");
                    }
                    return RedirectToAction("AdminMissionsEnCours");
                }
                catch (Exception e)
                {
                    MessageBox.Show("La mission n'a pas pu être supprimée", "Echec");
                    return RedirectToAction("AdminMissionsEnCours");
                }
            }
            else return RedirectToAction("AdminMissionsEnCours");*/

        }
        #endregion

        #region Archiver une mission
        public ActionResult ArchiverMission(string id)
        {
            using (DAL dal = new DAL())
            {
                // Demander confirmation à l'user pour réaliser l'achivage :
                DialogResult result = MessageBox.Show("Êtes-vous sûr de vouloir archiver la mission " + id + " ?", "Archiver la mission", MessageBoxButtons.YesNo);

                // Si OK pour l'archivage de la mission :
                if (result == DialogResult.Yes)
                {
                    if (dal.ArchiverMission(id)) MessageBox.Show("La mission a été archivée", "Succès");
                    else MessageBox.Show("La mission n'a pas pu être archivée", "Echec");
                }
                return RedirectToAction("AdminMissionsEnCours");
            }

            /*
            // Demander confirmation à l'user pour réaliser l'achivage :
            DialogResult result = MessageBox.Show("Êtes-vous sûr de vouloir archiver la mission " + id + " ?", "Archiver la mission", MessageBoxButtons.YesNo);

            // Si OK pour l'archivage de la mission :
            if (result == DialogResult.Yes)
            {
                try
                {
                    MISSION mission = db.MISSION.Find(id); // Rechercher la mission dans la BDD
                    if (mission != null) // Si la mission existe
                    {
                        mission.ETAT = "Archivé";
                        db.SaveChanges();
                        MessageBox.Show("La mission a été archivée", "Succès");
                    }
                    return RedirectToAction("AdminMissionsEnCours");
                }
                
                catch (Exception e)
                {
                    MessageBox.Show("La mission n'a pas pu être archivée", "Echec");
                    return RedirectToAction("AdminMissionsEnCours");
                }
            }
            else return RedirectToAction("AdminMissionsEnCours");
            */
        }
        #endregion

        #endregion

        #region Missions archivées
        #region Liste des missions archivées
        public ActionResult AdminMissionsArchivées()
        {
            using (DAL dal = new DAL())
            {
                ViewBag.listMissionsArchivées = dal.ListeMissionsArchivées();
                return View();
            }
        }
        #endregion

        #region Désarchiver une mission
        public ActionResult DesarchiverMission(string id)
        {
            using (DAL dal = new DAL())
            {
                // Demander confirmation à l'user pour réaliser l'achivage :
                DialogResult result = MessageBox.Show("Êtes-vous sûr de vouloir désarchiver la mission " + id + " ?", "Désarchiver la mission", MessageBoxButtons.YesNo);

                // Si OK pour l'archivage de la mission :
                if (result == DialogResult.Yes)
                {
                    if (dal.DesarchiverMission(id)) MessageBox.Show("La mission a été désarchivée", "Succès");
                    else MessageBox.Show("La mission n'a pas pu être désarchivée", "Echec");
                }
                return RedirectToAction("AdminMissionsArchivées");
            }
        }

        /*
        // Demander confirmation à l'user pour réaliser l'achivage :
        DialogResult result = MessageBox.Show("Êtes-vous sûr de vouloir désarchiver la mission " + id + " ?", "Désarchiver la mission", MessageBoxButtons.YesNo);

        // Si OK pour l'archivage de la mission :
        if (result == DialogResult.Yes)
        {
            try
            {
                MISSION mission = db.MISSION.Find(id); // Rechercher la mission dans la BDD
                if (mission != null) // Si la mission existe
                {
                    mission.ETAT = "EnCours";
                    db.SaveChanges();
                    MessageBox.Show("La mission a été désarchivée", "Succès");
                }
                return RedirectToAction("AdminMissionsArchivées");
            }
            catch (Exception e)
            {
                MessageBox.Show("La mission n'a pas pu être désarchivée", "Echec");
                return RedirectToAction("AdminMissionsArchivées");
            }
        }
        else return RedirectToAction("AdminMissionsArchivées");
    }*/
        #endregion
        #endregion
    }



}