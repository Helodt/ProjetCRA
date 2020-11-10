using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using ProjetCRA.Models;

namespace ProjetCRA.Controllers
{
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
            try
            {
                ViewBag.listEmployes = db.UTILISATEUR.ToList();
                return View();
            }
            catch (Exception e)
            {
                return HttpNotFound();
            }
        }
    }
}