//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré à partir d'un modèle.
//
//     Des modifications manuelles apportées à ce fichier peuvent conduire à un comportement inattendu de votre application.
//     Les modifications manuelles apportées à ce fichier sont remplacées si le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ProjetCRA.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class RAPPORT
    {
        public int IDRAPPORT { get; set; }
        public string UTILISATEUR_MATRICULE { get; set; }
        public System.DateTime MOIS { get; set; }
    
        public virtual UTILISATEUR UTILISATEUR { get; set; }
    }
}