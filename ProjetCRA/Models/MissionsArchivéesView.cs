using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProjetCRA.Models
{
    public class MissionsArchivéesView
    {
        public string Code { get; set; }
        public string Libelle { get; set; }
        public string Matricule { get; set; }
        public string Nom { get; set; }
        public string Prenom { get; set; }
        public double TempsTotal { get; set; }

        public DateTime DateDebut { get; set; }
        public DateTime DateFin {get;set;}

    }
}