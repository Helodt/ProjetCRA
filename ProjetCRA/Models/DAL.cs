using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Windows.Forms;

namespace ProjetCRA.Models
{
    public class DAL : IDisposable
    {
        BD_CRAEntities db = new BD_CRAEntities();

        public void Dispose()
        {
            db.Dispose();
        }

        // Récupère la liste des missions en cours dans la BDD :
        public List<MissionsEnCoursView> ObtenirListeMissionsEnCours()
        {
            // Récupérer la liste des missions en cours :
            try
            {
                var query = from m in db.MISSION
                            from u in db.UTILISATEUR
                            where m.UTILISATEUR_MATRICULE == u.MATRICULE
                            where m.ETAT == "EnCours"
                            select new MissionsEnCoursView()
                            {
                                Code = m.CODE,
                                Libelle = m.LIBELLE,
                                Matricule = u.MATRICULE,
                                Nom = u.NOM,
                                Prenom = u.PRENOM,
                                DateDebut = m.DATE_DEBUT,
                                DateFin = m.DATE_FIN
                            };

                List<MissionsEnCoursView> a = query.ToList();
                return a;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        // Ajoute une mission dans la BDD :
        public Boolean AjouterMission(MISSION mission)
        {

            Boolean missionAjoutée = true;
            try
            {
                mission.ETAT = "EnCours"; // L'état de la mission est initialisée à "EnCours"
                if (mission.DATE_DEBUT > mission.DATE_FIN) throw new Exception(); // La Date de fin doit être > à la date de début
                db.MISSION.Add(mission); // Ajout de la mission dans la BDD
                db.SaveChanges(); // Sauvegarder les changements dans la BDD
            } catch (Exception e)
            {
                missionAjoutée = false;
            }

            return missionAjoutée;
        }

        // Vérifie si une mission existe dans la BDD et la renvoie si oui : 
        public MISSION MissionExiste(string id)
        {
            try
            {
                MISSION mission = db.MISSION.Find(id);
                if (mission != null) return mission;
                else return null;
            } catch (Exception e)
            {
                return null;
            }
        }

        // Supprimer une mission de la BDD :
        public Boolean SupprimerMission(string id)
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
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        // Archiver une mission dans la BDD :
        public Boolean ArchiverMission(string id)
        {
            try
            {
                MISSION mission = db.MISSION.Find(id); // Rechercher la mission dans la BDD
                if (mission != null) // Si la mission existe
                {
                    mission.ETAT = "Archivé";
                    db.SaveChanges();
                    return true;
                }
                return false;
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
                return false;
            }
        }

        // Récupère la liste des missions archivées :
        public List<MissionsArchivéesView> ListeMissionsArchivées()
        {
            try
            {
                // Sélectionner toutes les missions dont l'état est "Archivé", et faire le total du temps des missionJours associées (seulement les missionsJour dont l'état est "Accepté")
                var query1 = from m in db.MISSION
                             join u in db.UTILISATEUR on m.UTILISATEUR_MATRICULE equals u.MATRICULE
                             join mj in db.MISSIONJOUR on m.CODE equals mj.MISSION_CODE
                             where m.ETAT == "Archivé" // L'état de la mission est "Archivée"
                             where mj.ETAT == "Accepté" // Les MissionJour associées présentent un état "Accepté"
                             group mj by new // Group by afin de pouvoir réaliser la somme des temps des missions jours associées
                             {
                                 mj.MISSION_CODE,
                                 Code = m.CODE,
                                 Matricule = u.MATRICULE,
                                 Libelle = m.LIBELLE,
                                 Nom = u.NOM,
                                 Prenom = u.PRENOM
                             } into g
                             select new MissionsArchivéesView()
                             {
                                 Code = g.Key.Code,
                                 Matricule = g.Key.Matricule,
                                 Libelle = g.Key.Libelle,
                                 Nom = g.Key.Nom,
                                 Prenom = g.Key.Prenom,
                                 TempsTotal = g.Sum(x => x.TEMPS_ACCORDE)
                             };

                // Sélectionner toutes les missions dont l'état est "Archivé" et qui n'ont pas de missionJours associées (et mettre le tempsTotal à 0)
                var query2 = from m in db.MISSION
                             join u in db.UTILISATEUR on m.UTILISATEUR_MATRICULE equals u.MATRICULE
                             where m.ETAT == "Archivé"
                             where !(from mj in db.MISSIONJOUR
                                     select mj.MISSION_CODE
                                     ).Contains(m.CODE)
                             select new MissionsArchivéesView()
                             {
                                 Code = m.CODE,
                                 Matricule = u.MATRICULE,
                                 Libelle = m.LIBELLE,
                                 Nom = u.NOM,
                                 Prenom = u.PRENOM,
                                 TempsTotal = 0
                             };
                query1 = query1.Union(query2); // On réalise une union des deux requêtes
                return query1.Distinct().ToList();
            }

            catch (Exception e)
            {
                return null;
            }
        }

        // Désarchiver une mission :
        public Boolean DesarchiverMission(string id)
        {
            try
            {
                MISSION mission = db.MISSION.Find(id); // Rechercher la mission dans la BDD
                if (mission != null) // Si la mission existe
                {
                    mission.ETAT = "EnCours";
                    db.SaveChanges();
                    return false;
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        // Liste les missions en attente de validation :
        public List<MissionsJourUserView> ListeMissionsEnAttenteValidation()
        {
            try
            {
                var query = from mj in db.MISSIONJOUR
                            join m in db.MISSION on mj.MISSION_CODE equals m.CODE
                            join u in db.UTILISATEUR on m.UTILISATEUR_MATRICULE equals u.MATRICULE
                            where mj.ETAT == "EnAttenteValidation"
                            select new MissionsJourUserView()
                            {
                                CodeMissionJour = mj.IDJOUR,
                                CodeMission = m.CODE,
                                Libelle = m.LIBELLE,
                                Matricule = u.MATRICULE,
                                Nom = u.NOM,
                                Prenom = u.PRENOM,
                                Jour = mj.JOUR,
                                Temps = mj.TEMPS_ACCORDE,
                                EtatMissionJour = mj.ETAT
                            };

                return query.Distinct().ToList();
            }
            catch (Exception e)
            {
                return null;
            }
        }

        // Accepter une mission jour :
        public void AccepterMissionJour(int id)
        {
            try
            {
                MISSIONJOUR mission = db.MISSIONJOUR.Find(id); // Rechercher la mission dans la BDD
                if (mission != null) // Si la mission existe
                {
                    mission.ETAT = "Accepté";
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
            }
        }

        // Refuser une mission :
        public void RefuserMissionJour(int id)
        {
            try
            {
                MISSIONJOUR mission = db.MISSIONJOUR.Find(id); // Rechercher la mission dans la BDD
                if (mission != null) // Si la mission existe
                {
                    mission.ETAT = "Refusé";
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
            }
        }

        // Recupère la liste des employés dans la BDD : 
        public List<UTILISATEUR> ListeEmployes()
        {
            try
            {
                return db.UTILISATEUR.ToList();
            } catch (Exception e)
            {
                return null;
            }
        }

        // Récupère la liste des missionsJour d'un utilisateur pour une journée donnée
        public List<MissionsJourUserView> ListeMissionsJoursPourJourPourUser(string matricule, DateTime Jour)
        {
            try
            {
                var query = from mj in db.MISSIONJOUR
                            join m in db.MISSION on mj.MISSION_CODE equals m.CODE
                            where mj.JOUR == Jour
                            where m.UTILISATEUR_MATRICULE == matricule
                            select new MissionsJourUserView()
                            {
                                CodeMissionJour = mj.IDJOUR,
                                CodeMission = mj.MISSION_CODE,
                                Libelle = m.LIBELLE,
                                Matricule = matricule,
                                Nom = "",
                                Prenom = "",
                                Jour = Jour,
                                Temps = mj.TEMPS_ACCORDE
                            };

                return query.Distinct().ToList();
            } catch(Exception e)
            {

            }
            return null;
        }

        // Récupère la liste des missions disponibles pour un jour donné, pour un utilisateur
        public List<MissionsEnCoursView> MissionsDisponiblesJourUser(DateTime date, string matricule)
        {
            try
            {
                var query = from m in db.MISSION
                            where m.ETAT == "EnCours"
                            where m.DATE_DEBUT < date
                            where m.DATE_FIN > date
                            where m.UTILISATEUR_MATRICULE == matricule
                            select new MissionsEnCoursView()
                            {
                                Code = m.CODE,
                                Libelle = m.LIBELLE,
                                DateDebut = m.DATE_DEBUT,
                                DateFin = m.DATE_FIN,
                                Matricule = m.UTILISATEUR_MATRICULE
                            };
                return query.Distinct().ToList();
            } catch (Exception e)
            {

            }
            return null;
        }

        // Ajoute une missionJour dans la BDD
        public Boolean AjouterMissionJour(MISSIONJOUR mission)
        {
            Boolean missionAjoutée = true;
            try
            {
                mission.ETAT = "NonSauvegardé"; // L'état de la mission est initialisée à "NonSauvegardé"
                db.MISSIONJOUR.Add(mission); // Ajout de la mission dans la BDD
                db.SaveChanges(); // Sauvegarder les changements dans la BDD
            }
            catch (Exception e)
            {
                missionAjoutée = false;
            }

            return missionAjoutée;
        }


        public Boolean VerifierJourneeValidation(DateTime date, string matricule)
        {
            /*try
            {
                // Récupérer toutes les Missions Jours de la journée date :
                var query = from mj in db.MISSIONJOUR
                            join m in db.MISSION on mj.MISSION_CODE equals m.CODE
                            where mj.JOUR == date
                            where m.UTILISATEUR_MATRICULE == matricule
                            
                            select new MissionJourJourneeUser()
                            {
                                CodeMissionJour = mj.IDJOUR,
                                Temps = mj.TEMPS_ACCORDE,
                                EtatMissionJour = mj.ETAT
                            };

                return query.Distinct().ToList();
            } catch (Exception e)
            {

            }*/
            return false;
        }

    }
}