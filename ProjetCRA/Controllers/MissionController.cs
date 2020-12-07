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

        #region Missions en cours (côté Admin)
        #region Listes des missions en cours (côté Admin)
        // Récupérer la liste des missions en cours
        public ActionResult AdminMissionsEnCours()
        {
            using (DAL dal = new DAL())
            {
                ViewBag.listMissionsEnCours = dal.ObtenirListeMissionsEnCours(); // Stocker la liste des missions en cours dans le ViewBag
                return View();
            }

        }
        #endregion

        #region Ajouter une mission (côté Admin)
        // Ajout d'une mission dans la BDD par un formulaire
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
                Boolean missionAjoutée = dal.AjouterMission(mission); // Ajout de la mission dans la BDD, et stocker un boolean permettant de savoir si la mission a été ajoutée avec succès
                // Affichage de messages d'erreur/de confirmation selon si la mission a bien été ajoutée ou non :
                if (missionAjoutée == true)
                {
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

        #region Modifier une mission (côté Admin)
        // Modifier une mission de la BDD : id = l'identifiant de la mission
        public ActionResult ModifierMission(string id)
        {
            using (DAL dal = new DAL())
            {
                MISSION mission = dal.MissionExiste(id); // Vérifier que la mission existe dans la BDD
                if (mission != null) return View("ModifierMission", mission); // Si la mission existe : modifier la mission dans la vue ModifierMission
                else return RedirectToAction("AdminMissionsEnCours"); // Sinon, retourner vers la vue affichant toutes les missions en cours
            }
        }

        // Modifier une mission de la BDD : mission = l'objet MISSION qui doit être modifié
        [HttpPost]
        public ActionResult ModifierMission(MISSION mission)
        {
            try
            {
                if (ModelState.IsValid) // Si le modèle de donnée est valide
                {
                    db.Entry(mission).State = EntityState.Modified; // Modification de la mission dans la BDD
                    db.SaveChanges(); // Enregistrer les modifications de la BDD
                }
                return RedirectToAction("AdminMissionsEnCours");
            }
            catch (Exception e)
            {
                MessageBox.Show("La mission n'a pas pu être modifiée", "Echec");
                return RedirectToAction("AdminMissionsEnCours");
            }
        }
        #endregion

        #region Supprimer une mission (côté Admin)
        // Modifier une mission de la BDD : id = l'identifiant de la mission
        public ActionResult SupprimerMission(string id)
        {
            using (DAL dal = new DAL())
            {
                Boolean MissionSupprimee = dal.SupprimerMission(id); // Suppression de la mission, et le boolean indique si la mission a pu être effectivement supprimée
                if (!MissionSupprimee)  MessageBox.Show("La mission n'a pas pu être supprimée", "Echec");
                
                return RedirectToAction("AdminMissionsEnCours");
            }

        }
        #endregion

        #region Archiver une mission (côté Admin)
        // Modifier une mission de la BDD : id = l'identifiant de la mission
        public ActionResult ArchiverMission(string id)
        {
            using (DAL dal = new DAL())
            {
                if (!dal.ArchiverMission(id)) MessageBox.Show("La mission n'a pas pu être archivée", "Echec"); // Archiver la mission et si elle n'a pas été archivée : affichage d'un message d'erreur
                return RedirectToAction("AdminMissionsEnCours");
            }
        }
        #endregion

        #endregion

        #region Missions archivées (côté Admin)
        #region Liste des missions archivées (côté Admin)
        // Afficher la liste des missions archivées
        public ActionResult AdminMissionsArchivées()
        {
            using (DAL dal = new DAL())
            {
                ViewBag.listMissionsArchivées = dal.ListeMissionsArchivées(); // Récupérer dans la BDD la liste des missions archivées
                return View();
            }
        }
        #endregion

        #region Désarchiver une mission (côté Admin)
        // Modifier une mission de la BDD : id = l'identifiant de la mission
        public ActionResult DesarchiverMission(string id)
        {
            using (DAL dal = new DAL())
            {
                if (!dal.DesarchiverMission(id)) MessageBox.Show("La mission n'a pas pu être désarchivée", "Echec"); // Désarchiver la mission, et si elle n'a pas pu être désarchivée : affichage d'un message d'echec
                
                return RedirectToAction("AdminMissionsArchivées");
            }
        }
        #endregion
        #endregion
    }



}