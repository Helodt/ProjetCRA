using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetCRA.Models
{
    public class MissionsJourUserView
    {
        public int CodeMissionJour { get; set; }
        public string CodeMission { get; set; }
        public string Libelle { get; set; }
        public string Matricule { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public DateTime Jour { get; set; }
        public int Temps { get; set; }
        public string EtatMissionJour { get; set; }
    }

}