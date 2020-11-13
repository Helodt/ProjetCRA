using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetCRA.Models
{
    public class MissionJourJourneeUser
    {
        public int CodeMissionJour { get; set; }
        public double Temps { get; set; }
        public string EtatMissionJour { get; set; }
    }
}