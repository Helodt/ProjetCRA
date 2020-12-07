using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
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

        #region MISSION
        #region Liste des MISSIONS en cours
        // Renvoie la liste des missions en cours dans la BDD :
        public List<MissionsEnCoursView> ObtenirListeMissionsEnCours()
        {
            // Récupérer la liste des missions en cours :
            try
            {
                var query = from m in db.MISSION
                            from u in db.UTILISATEUR
                            where m.UTILISATEUR_MATRICULE == u.MATRICULE
                            where m.ETAT == "EnCours" // Chercher les missions dont l'état est "EnCours"
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

                return query.ToList(); // Retourner une liste des missions en cours
            }
            catch (Exception e)
            {
                return null;
            }
        }
        #endregion

        #region Ajouter une MISSION
        // Ajoute une mission dans la BDD :
        // Prend en paramètre la mission à ajouter dans la BDD
        // Renvoie true si la mission a été correctement ajoutée
        public Boolean AjouterMission(MISSION mission)
        {

            Boolean missionAjoutée = true;
            try
            {
                mission.ETAT = "EnCours"; // L'état de la mission est initialisée à "EnCours"
                if (mission.DATE_DEBUT > mission.DATE_FIN) throw new Exception(); // La Date de fin doit être > à la date de début
                db.MISSION.Add(mission); // Ajout de la mission dans la BDD
                db.SaveChanges(); // Sauvegarder les changements dans la BDD
            }
            catch (Exception e)
            {
                missionAjoutée = false;
            }
            return missionAjoutée;
        }
        #endregion

        #region Vérifier si une MISSION existe
        // Vérifie si une mission existe dans la BDD et la renvoie true/false en fonction : 
        // Prend en paramètre l'identifiant de la Mission
        // Renvoie le mission si elle existe
        public MISSION MissionExiste(string id)
        {
            try
            {
                MISSION mission = db.MISSION.Find(id); // Chercher la mission dans la BDD
                if (mission != null) return mission; // Si elle existe, retourner la mission
                else return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        #endregion

        #region Supprimer une MISSION
        // Supprimer une mission de la BDD :
        // Prend en paramètre l'identifiant de la mission
        // Renvoie true si la mission a bien été supprimée
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
        #endregion

        #region Archiver une MISSION
        // Archiver une mission dans la BDD
        // Prend en paramètre l'identifiant de la mission
        // Retourne true si la mission a bien été archivée
        public Boolean ArchiverMission(string id)
        {
            try
            {
                MISSION mission = db.MISSION.Find(id); // Rechercher la mission dans la BDD
                if (mission != null) // Si la mission existe
                {
                    mission.ETAT = "Archivé"; // Passer l'état de la mission à "Archivé"
                    db.SaveChanges(); // Sauvegarder les changements dans la BDD
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        #endregion

        #region Archiver toutes les MISSIONS terminées
        // Passe l'état de toutes les missions dont la date de fin est inférieur à la date actuelle à "Archivé".
        public bool ArchiverTout()
        {
            DateTime dateAjd = DateTime.Now; // Récupérer la date d'aujourd'hui

            // Récupérer toutes les MISSION qui n'ont pas l'etat "Archivé" et dont la date de fin est supérieur à la date du jour
            var query = from m in db.MISSION
                        where m.DATE_FIN < dateAjd
                        where m.ETAT != "Archivé"
                        select m;

            // Faire passer l'état de toutes les missions à "Archivé"
            foreach (MISSION m in query)
            {
                m.ETAT = "Archivé";
            }

            try
            {
                db.SaveChanges(); // Sauvegarder les changements
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Listes des MISSIONS archivées
        // Renvoie la liste des missions archivées
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
                                 Prenom = u.PRENOM,
                                 DateDebut = m.DATE_DEBUT,
                                 DateFin = m.DATE_FIN
                             } into g
                             select new MissionsArchivéesView()
                             {
                                 Code = g.Key.Code,
                                 Matricule = g.Key.Matricule,
                                 Libelle = g.Key.Libelle,
                                 Nom = g.Key.Nom,
                                 Prenom = g.Key.Prenom,
                                 TempsTotal = g.Sum(x => x.TEMPS_ACCORDE),
                                 DateDebut = g.Key.DateDebut,
                                 DateFin = g.Key.DateFin
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
                                 TempsTotal = 0,
                                 DateDebut = m.DATE_DEBUT,
                                 DateFin = m.DATE_FIN
                             };
                query1 = query1.Union(query2); // On réalise une union des deux requêtes
                return query1.Distinct().ToList(); // Retourner la liste du query
            }
            catch (Exception e)
            {
                return null;
            }
        }
        #endregion

        #region Désarchiver une MISSION
        // Désarchiver une mission
        // Prend en paramètre l'identifiant de la mission
        // Renvoie true si la mission a bien été désarchivée
        public Boolean DesarchiverMission(string id)
        {
            try
            {
                MISSION mission = db.MISSION.Find(id); // Rechercher la mission dans la BDD
                if (mission != null) // Si la mission existe
                {
                    mission.ETAT = "EnCours"; // Mettre l'état de la mission à "EnCours"
                    db.SaveChanges(); // Sauvegarder les changements dans la BDD
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                return false;
            }
        }
        #endregion

        #region Liste des MISSION en cours pour un jour, pour un employé
        // Renvoie la liste des missions disponibles pour un jour donné, pour un utilisateur
        // Prend en paramètre la date du jour choisi, et le matricule de l'utilisateur concerné
        public List<MissionsEnCoursView> MissionsDisponiblesJourUser(DateTime date, string matricule)
        {
            try
            {
                // Récupérer les missions qui ont l'état "EnCours" et dont la date de la journée est comprise entre la date de fin et la date de début de réalisation de la mission
                var query = from m in db.MISSION
                            where m.ETAT == "EnCours"
                            where m.DATE_DEBUT <= date // La date donnée doit être supérieure ou égale à la date de début de la mission
                            where m.DATE_FIN >= date // La date donnée doit être inférieure ou égale à la date de fin de la mission
                            where m.UTILISATEUR_MATRICULE == matricule // Le matricule associé à la mission doit être correct
                            select new MissionsEnCoursView()
                            {
                                Code = m.CODE,
                                Libelle = m.LIBELLE,
                                DateDebut = m.DATE_DEBUT,
                                DateFin = m.DATE_FIN,
                                Matricule = m.UTILISATEUR_MATRICULE
                            };
                return query.Distinct().ToList();
            }
            catch (Exception e)
            {

            }
            return null;
        }
        #endregion

        #region Liste les MISSION réalisées durant un mois donné pour un employé
        // Renvoie la liste des missions réalisées durant le mois donné pour un utilisateur, afin d'en faire le rapport
        // Prend en paramètre le premier jour du mois, le dernier jour du mois et le matricule de l'employé concerné
        public List<MissionsArchivéesView> RapportMois(DateTime firstDayOfMonth, DateTime lastDayOfMonth, string Matricule)
        {
            try
            {
                // Sélectionner toutes les missions jours réalisées durant le mois, et faire le total du temps des missionJours associées (seulement les missionsJour dont l'état est "Accepté")
                var query1 = from m in db.MISSION
                             join u in db.UTILISATEUR on m.UTILISATEUR_MATRICULE equals u.MATRICULE
                             join mj in db.MISSIONJOUR on m.CODE equals mj.MISSION_CODE
                             where u.MATRICULE == Matricule
                             where mj.ETAT == "Accepté" // Les MissionJour associées présentent un état "Accepté"
                             where mj.JOUR >= firstDayOfMonth // Ne sont sélectionnées seulement les MissionsJours qui ont été réalisées durant le mois
                             where mj.JOUR <= lastDayOfMonth // Ne sont sélectionnées seulement les MissionsJours qui ont été réalisées durant le mois
                             group mj by new // Group by afin de pouvoir réaliser la somme des temps des missions jours associées
                             {
                                 mj.MISSION_CODE,
                                 Code = m.CODE,
                                 Matricule = u.MATRICULE,
                                 Libelle = m.LIBELLE,
                                 Nom = u.NOM,
                                 Prenom = u.PRENOM,
                                 DateDebut = m.DATE_DEBUT,
                                 DateFin = m.DATE_FIN
                             } into g
                             select new MissionsArchivéesView() // Créer un objet permettant de récupérer les informations concernant les missions
                             {
                                 Code = g.Key.Code,
                                 Matricule = g.Key.Matricule,
                                 Libelle = g.Key.Libelle,
                                 Nom = g.Key.Nom,
                                 Prenom = g.Key.Prenom,
                                 TempsTotal = g.Sum(x => x.TEMPS_ACCORDE),
                                 DateDebut = g.Key.DateDebut,
                                 DateFin = g.Key.DateFin
                             };
                return query1.Distinct().ToList(); // Retourner la liste
            }

            catch (Exception e)
            {
                return null;
            }
        }
        #endregion

        #region Liste des MISSION en cours pour un employé
        // Renvoie la liste des missions en cours pour un employé donné
        // Prend en paramètre le matricule de l'employé
        public List<MissionsEnCoursView> ListeMissionsEnCoursEmployé(string Matricule)
        {
            try
            {
                // Récupérer toutes les Missions Jours dont l'état est "EnCours" de l'employé définit par son matricule "Matricule" :
                var query = from m in db.MISSION
                            where m.UTILISATEUR_MATRICULE == Matricule // dont le matricule associé est égal à "Matricule"
                            where m.ETAT == "EnCours"
                            select new MissionsEnCoursView()
                            {
                                Code = m.CODE,
                                Libelle = m.LIBELLE,
                                Matricule = m.UTILISATEUR_MATRICULE,
                                Nom = "",
                                Prenom = "",
                                DateDebut = m.DATE_DEBUT,
                                DateFin = m.DATE_FIN
                            };
                return query.Distinct().ToList(); // Retourner la liste
            }
            catch (Exception e)
            {
            }
            return null;
        }
        #endregion

        #region Liste des MISSION archivées pour un employé
        // Renvoie la liste des missions archivées pour un employé donné
        // Prend en paramètre le matricule de l'employé
        public List<MissionsArchivéesView> ListeMissionsArchivéesEmployé(string Matricule)
        {

            try
            {
                // Sélectionner toutes les missions dont l'état est "Archivé", et faire le total du temps des missionJours associées (seulement les missionsJour dont l'état est "Accepté")
                var query1 = from m in db.MISSION
                             join u in db.UTILISATEUR on m.UTILISATEUR_MATRICULE equals u.MATRICULE
                             join mj in db.MISSIONJOUR on m.CODE equals mj.MISSION_CODE
                             where m.UTILISATEUR_MATRICULE == Matricule // L'utilisateur associé à la missionJour a le matricule "Matricule"
                             where m.ETAT == "Archivé" // L'état de la mission est "Archivée"
                             where mj.ETAT == "Accepté" // Les MissionJour associées présentent un état "Accepté"
                             group mj by new // Group by afin de pouvoir réaliser la somme des temps des missions jours associées
                             {
                                 mj.MISSION_CODE,
                                 Code = m.CODE,
                                 Matricule = u.MATRICULE,
                                 Libelle = m.LIBELLE,
                                 Nom = u.NOM,
                                 Prenom = u.PRENOM,
                                 DateDebut = m.DATE_DEBUT,
                                 DateFin = m.DATE_FIN
                             } into g
                             select new MissionsArchivéesView() // Création d'un nouvel objet regroupant les attributs :
                             {
                                 Code = g.Key.Code,
                                 Matricule = g.Key.Matricule,
                                 Libelle = g.Key.Libelle,
                                 Nom = g.Key.Nom,
                                 Prenom = g.Key.Prenom,
                                 TempsTotal = g.Sum(x => x.TEMPS_ACCORDE),
                                 DateDebut = g.Key.DateDebut,
                                 DateFin = g.Key.DateFin
                             };

                // Sélectionner toutes les missions dont l'état est "Archivé" et qui n'ont pas de missionJours associées (et mettre le tempsTotal à 0)
                var query2 = from m in db.MISSION
                             join u in db.UTILISATEUR on m.UTILISATEUR_MATRICULE equals u.MATRICULE
                             where m.ETAT == "Archivé" // L'état de la mission est "Archivé'
                             where m.UTILISATEUR_MATRICULE == Matricule // L'utilisateur associé à la missionJour a le matricule "Matricule"
                             where !(from mj in db.MISSIONJOUR
                                     select mj.MISSION_CODE
                                     ).Contains(m.CODE)
                             select new MissionsArchivéesView() // Création d'un nouvel objet regroupant les attributs :
                             {
                                 Code = m.CODE,
                                 Matricule = u.MATRICULE,
                                 Libelle = m.LIBELLE,
                                 Nom = u.NOM,
                                 Prenom = u.PRENOM,
                                 TempsTotal = 0, // Mettre le temps total à 0 puisqu'aucune missionJour n'est associée
                                 DateDebut = m.DATE_DEBUT,
                                 DateFin = m.DATE_FIN
                             };
                query1 = query1.Union(query2); // On réalise une union des deux requêtes
                return query1.Distinct().ToList();
            }
            catch (Exception e)
            {
                return null;
            }
        }
        #endregion

        #endregion



        #region MISSIONJOUR
        #region Listes des MISSIONJOUR en attente de validation
        // Renvoie la liste des les missionJours en attente de validation :
        public List<MissionsJourUserView> ListeMissionsEnAttenteValidation()
        {
            try
            {
                // Récupère la liste des missionsJours pour lesquels l'attribut ETAT est égal à "EnAttenteValidation" :
                var query = from mj in db.MISSIONJOUR
                            join m in db.MISSION on mj.MISSION_CODE equals m.CODE
                            join u in db.UTILISATEUR on m.UTILISATEUR_MATRICULE equals u.MATRICULE
                            where mj.ETAT == "EnAttenteValidation" // Les missionsJours dont l'état est "EnAttenteValidation"
                            select new MissionsJourUserView() // Créer un nouvel objet de type MissionsJourUserView
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

                return query.Distinct().ToList(); // Retourner la liste des missionsJours en attente de validation
            }
            catch (Exception e)
            {
                return null;
            }
        }
        #endregion

        #region Accepter une MISSIONJOUR
        // Accepter une mission jour 
        // Prend en paramètre l'identifiant de la missionJour
        public void AccepterMissionJour(int id)
        {
            try
            {
                MISSIONJOUR mission = db.MISSIONJOUR.Find(id); // Rechercher la missionJour dans la BDD
                if (mission != null) // Si la mission existe
                {
                    mission.ETAT = "Accepté"; // Passer l'état de la missionJour à "Accepté"
                    db.SaveChanges(); // Sauvegarder les changements dans la BDD
                }
            }
            catch (Exception e)
            {
            }
        }
        #endregion

        #region Refuser une MISSIONJOUR
        // Refuser une mission
        // Prend en paramètre l'identifiant de la missionJour
        public void RefuserMissionJour(int id)
        {
            try
            {
                MISSIONJOUR mission = db.MISSIONJOUR.Find(id); // Rechercher la mission dans la BDD
                if (mission != null) // Si la mission existe
                {
                    mission.ETAT = "Refusé"; // Passer l'état de la missionJour à "Refusé"
                    db.SaveChanges(); // Sauvegarder les changements dans la BDD
                }
            }
            catch (Exception e)
            {
            }
        }
        #endregion

        #region Liste des MISSIONSJOURS pour un employé pour une journée
        // Renvoie la liste des missionsJour d'un employé pour une journée donnée
        // Prend en paramètre le matricule de l'employé et la date de la journée choisie
        public List<MissionsJourUserView> ListeMissionsJoursPourJourPourUser(string matricule, DateTime Jour)
        {
            try
            {
                // Récupérer la liste des missionsJours de l'employé qui a le matricule donné, et pour la journée Jour
                var query = from mj in db.MISSIONJOUR
                            join m in db.MISSION on mj.MISSION_CODE equals m.CODE
                            where mj.JOUR == Jour // Ne sélectionner que les missionJours qui sont réalisées le jour "Jour"
                            where m.UTILISATEUR_MATRICULE == matricule // Ne sélectionner que les missionJour qui sont associées au matricule donné
                            select new MissionsJourUserView()
                            {
                                CodeMissionJour = mj.IDJOUR,
                                CodeMission = mj.MISSION_CODE,
                                Libelle = m.LIBELLE,
                                Matricule = matricule,
                                Nom = "",
                                Prenom = "",
                                Jour = Jour,
                                Temps = mj.TEMPS_ACCORDE,
                                EtatMissionJour = mj.ETAT
                            };

                return query.Distinct().ToList(); // Retourne la liste 
            }
            catch (Exception e)
            {

            }
            return null;
        }
        #endregion

        #region Ajouter une MISSIONJOUR
        // Ajoute une missionJour dans la BDD
        // Prend en paramètre la MISSIONJOUR qui doit être ajoutée
        // Renvoie true si la mission a été correctement ajoutée
        public Boolean AjouterMissionJour(MISSIONJOUR mission)
        {
            Boolean missionAjoutée = true; // Intialiser le boolean à true
            try
            {
                mission.ETAT = "NonSauvegardé"; // L'état de la mission est initialisée à "NonSauvegardé"
                db.MISSIONJOUR.Add(mission); // Ajout de la mission dans la BDD
                db.SaveChanges(); // Sauvegarder les changements dans la BDD
            }
            catch (Exception e)
            {
                missionAjoutée = false; // Si la mission n'a pas pu être ajoutée, renvoyer false
            }
            return missionAjoutée;
        }
        #endregion

        #region Vérifier que la journée peut être envoyée à validation : vérifier l'ensemble des MISSIONJOUR d'une journée d'un employé
        // Vérifier que la journée peut être envoyée à validation
        // Prend en paramètre une journée donnée et le matricule d'un utilisateur
        // Renvoie true si la journée peut être envoyé à validation
        public Boolean VerifierJourneeValidation(DateTime date, string matricule)
        {
            try
            {
                // Récupérer toutes les Missions Jours de la journée "date" de l'utilisateur "matricule" :
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

                double temps = 0; // Initialiser le temps à 0

                // Parcours de l'ensemble des missionsJours récupérées :
                foreach (var data in query)
                {
                    temps += data.Temps; // Calcul du temps total
                }

                if (temps <= 1) return true; // Si le temps total est valide (inférieur à 1) : retourner true
            }
            catch (Exception e)
            {

            }
            return false;
        }
        #endregion

        #region Envoyer à validation toutes les MISSIONJOUR d'une journée d'un employé
        // Envoyer la journée à validation : passer l'état de toutes les missionsJours de la journée donnée de l'employé concerné à "EnAttenteValidation"
        // Prend en paramètre la date choisie et le matricule de l'employé concerné
        // Renvoie true si toutes les missions ont pu être envoyées à validation
        public Boolean EnvoyerJourneeValidation(DateTime date, string matricule)
        {
            try
            {
                // Récupérer toutes les Missions Jours dont l'état n'est pas "Accepté" de la journée "date" de l'utilisateur "matricule":
                var query = from mj in db.MISSIONJOUR
                            join m in db.MISSION on mj.MISSION_CODE equals m.CODE
                            where mj.JOUR == date
                            where mj.ETAT != "Accepté"
                            where m.UTILISATEUR_MATRICULE == matricule

                            select new MissionJourJourneeUser() // Sélectionner toutes les missionsJours correspondantes :
                            {
                                CodeMissionJour = mj.IDJOUR,
                                Temps = mj.TEMPS_ACCORDE,
                                EtatMissionJour = mj.ETAT
                            };

                // Parcourir toutes les missionJours récupérées :
                foreach (var data in query)
                {
                    if (data.EtatMissionJour != "Accepté") // Si l'état de la missionJour n'est pas "Accepté"
                    {
                        MISSIONJOUR mission = db.MISSIONJOUR.Find(data.CodeMissionJour); // Rechercher la mission jour dans la BDD
                        if (mission != null) // Si la mission existe
                        {
                            mission.ETAT = "EnAttenteValidation"; // Mettre à jour son état par "EnAttenteValidation"
                        }
                    }
                }
                db.SaveChanges(); // Sauvegarder les changements dans la BDD
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }
        #endregion

        #region Vérifier qu'une MISSIONJOUR existe
        // Vérifier qu'une missionJour existe dans la BDD
        // Renvoie la missionJour si elle existe
        // Prend en paramètre l'identifiant de la MISSIONJOUR
        public MISSIONJOUR MissionJourExiste(int id)
        {
            try
            {
                MISSIONJOUR mission = db.MISSIONJOUR.Find(id); // Chercher la MISSIONJOUR dans la BDD
                if (mission != null) return mission; // Si la mission existe, renvoyer la mission
                else return null;
            }
            catch (Exception e)
            {
                return null;
            }
        }
        #endregion

        #region Supprimer une MISSIONJOUR
        // Supprimer une mission jour de la BDD
        // Prend en paramètre l'identifiant de la missionJour
        // Renvoie true si la missionJour a été correctement supprimée
        public Boolean SupprimerMissionJour(int id)
        {
            try
            {
                MISSIONJOUR mission = db.MISSIONJOUR.Find(id); // Rechercher la mission dans la BDD
                if (mission != null) // Si la mission existe
                {
                    //Suppression de la mission :
                    db.MISSIONJOUR.Remove(mission);

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
        #endregion

        #region Sauvegarder une MISSIONJOUR
        // Sauvegarder une mission jour : l'état de la mission passe à "Sauvegardé"
        // Prend en paramètre l'identifiant de la missionJour
        public void SauvegarderMissionJour(int id)
        {
            try
            {
                MISSIONJOUR mission = db.MISSIONJOUR.Find(id); // Rechercher la missionJour dans la BDD
                if (mission != null) // Si la mission existe
                {
                    if (mission.ETAT != "Accepté")
                    {
                        mission.ETAT = "Sauvegardé"; // Passer l'état de la mission à "Sauvegardé"
                        db.SaveChanges(); // Sauvegarder les changements dans la BDD
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
        #endregion

        #region Liste des MISSIONJOURS de la semaine donnée précédente
        // Renvoie la liste des missions Jours de la semaine précédente.
        // Prend en paramètre le lundi de la semaine courante et le matricule de l'utilisateur choisi
        public List<MissionsJourUserView> MissionsJoursSemainePrecedente(DateTime date, string Matricule)
        {
            try
            {
                // Définir l'intervalle de jours pour lequel on récupèrera les missions Jours :
                DateTime debutSemaine = date.AddDays(-7); // Le lundi de la semaine précédente
                DateTime finSemaine = date.AddDays(-1); // Le dimanche de la semaine précédente

                // Récupérer toutes les Missions Jours de l'employé définit par son matricule "Matricule" :
                var query = from m in db.MISSION
                            join mj in db.MISSIONJOUR on m.CODE equals mj.MISSION_CODE
                            where mj.JOUR <= finSemaine // La mission jour doit être comprise dans l'intervalle défini précedemment
                            where mj.JOUR >= debutSemaine // La mission jour doit être comprise dans l'intervalle défini précedemment
                            where m.ETAT == "EnCours" // La mission doit avoir un statut "EnCours"
                            where m.UTILISATEUR_MATRICULE == Matricule // Le matricule de la mission est celui de l'employé ciblé

                            select new MissionsJourUserView() // Création d'un nouvel objet regroupant les attributs :
                            {
                                CodeMissionJour = mj.IDJOUR,
                                CodeMission = m.CODE,
                                Libelle = m.LIBELLE,
                                Matricule = m.UTILISATEUR_MATRICULE,
                                Jour = mj.JOUR,
                                Temps = mj.TEMPS_ACCORDE,
                                EtatMissionJour = mj.ETAT,
                                DateFin = m.DATE_FIN
                            };
                return query.Distinct().ToList(); // Retourne la liste
            }
            catch (Exception e)
            {
            }
            return null;

        }
        #endregion

        #region Pré-initialiser la semaine en fonction des MISSIONJOUR de la semaine précédente
        // Pré-initialiser la semaine actuelle en fonction des missionsJours de la semaine précédente
        // Prend en paramètre la liste des missionsJour de la semaine précédente
        public void PréinitialiserSemaine(List<MissionsJourUserView> listeMissions)
        {
            // Parcours de la liste des missions de la semaine précédente :
            foreach (var data in listeMissions)
            {
                if (data.DateFin >= data.Jour.AddDays(7)) // Récupérer les missionsJours qui seront valides encore la semaine d'après (dont la date de fin de la mission correspondante est supérieure à la date de la missionJour + 7 )
                {
                    // Définir la nouvelle missionJour :
                    MISSIONJOUR mission = new MISSIONJOUR
                    {
                        MISSION_CODE = data.CodeMission,
                        ETAT = "NonSauvegardé", // L'état est par défaut à "NonSauvegardé"
                        JOUR = data.Jour.AddDays(7), // Le jour est égal à 7+l'ancien jour
                        TEMPS_ACCORDE = data.Temps // Le temps est identique que l'ancienne missionJour
                    };

                    try
                    {
                        db.MISSIONJOUR.Add(mission); // Ajout de la mission dans la BDD
                        db.SaveChanges(); // Sauvegarder les changements dans la BDD
                    }
                    catch (Exception e)
                    {
                    }
                }

            }
        }
        #endregion

        #region Jour d'une MISSIONJOUR
        // Renvoie le jour d'une MISSIONJOUR ou par défaut la date courante si la missionJour n'existe pas
        // Prend en paramètre l'identifiant de la missionJour
        public DateTime JourMissionJourOrDefault(int id)
        {
            try
            {
                MISSIONJOUR mission = db.MISSIONJOUR.Find(id); // Rechercher la mission dans la BDD
                if (mission != null) // Si la mission existe
                {
                    return mission.JOUR; // Retourner le jour de la MissionJour
                }
            }
            catch (Exception e)
            {
            }
            return DateTime.Now;
        }
        #endregion

        #endregion



        #region UTILISATEUR
        #region Liste les UTILISATEURS
        // Recupère la liste des employés dans la BDD : 
        public List<UTILISATEUR> ListeEmployes()
        {
            try
            {
                return db.UTILISATEUR.ToList(); // Retourne la liste des UTILISATEUR récupéré de la BDD
            }
            catch (Exception e)
            {
                return null;
            }
        }
        #endregion

        #region Récupérer le matricule de l'UTILISATEUR d'une MISSIONJOUR donnée
        // Renvoie le matricule de l'utilisateur d'une mission jour 
        // Prend en paramètre la MISSIONJOUR donnée
        public MissionsEnCoursView RecupérerMatriculeMJ(MISSIONJOUR missionJour)
        {
            try
            {
                // Sélectionner l'utilisateur qui est associé à la missionJour
                var query = from m in db.MISSION
                            where m.CODE == missionJour.MISSION_CODE // Le code de la MISSION est identique à la Mission_Code de la MISSIONJOUR
                            select new MissionsEnCoursView
                            {
                                Matricule = m.UTILISATEUR_MATRICULE
                            };
                return query.FirstOrDefault(); // Récupérer le premier (et l'unique) utilisateur de la missionJour donnée

            }
            catch (Exception e)
            {

            }
            return null;
        }
        #endregion

        #region Récupérer le matricule de l'UTILISATEUR d'une MISSIONJOUR donnée à partir de l'id de la missionjour
        // Renvoie le matricule de l'utilisateur d'une mission jour à partir de l'identifiant de la mission jour
        // Prend en paramètre l'identifiant de la missionJour
        public MissionsEnCoursView RecupérerMatriculeMJid(int id)
        {
            try
            {
                var query = from m in db.MISSION
                            join mj in db.MISSIONJOUR on m.CODE equals mj.MISSION_CODE
                            where mj.IDJOUR == id
                            select new MissionsEnCoursView
                            {
                                Matricule = m.UTILISATEUR_MATRICULE
                            };
                return query.FirstOrDefault();

            }
            catch (Exception e)
            {

            }
            return null;
        }
        #endregion

        #region Récupérer le rôle d'un UTILISATEUR
        // Récupérer le rôle d'un utilisateur donné
        // Prend en paramètre le matricule de l'utilisateur
        // Renvoie true si l'utilisateur est un admin
        public bool RecupererRole(string matricule)
        {
            // Chercher dans la bdd l'utilisateur ayant le matricule donné
            var result = from u in db.UTILISATEUR
                         where u.MATRICULE == matricule
                         select new { isAdmin = u.ISADMIN };
            var t = result.FirstOrDefault(); // Récupérer le premier élément de la liste (qui n'est composée que d'un seul élément, vu que les matricules sont uniques dans la bdd)

            return t.isAdmin; // Renvoie true si l'utilisateur est un admin 
        }
        #endregion
        #endregion







        









    }
}