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
    
    public partial class UTILISATEUR
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public UTILISATEUR()
        {
            this.MISSION = new HashSet<MISSION>();
            this.RAPPORT = new HashSet<RAPPORT>();
        }
    
        public string MATRICULE { get; set; }
        public string MOTDEPASSE { get; set; }
        public string NOM { get; set; }
        public string PRENOM { get; set; }
        public bool ISADMIN { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<MISSION> MISSION { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<RAPPORT> RAPPORT { get; set; }
    }
}
