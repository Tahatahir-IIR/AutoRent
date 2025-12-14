using System.ComponentModel.DataAnnotations;

namespace LocationDeVoiture.Models
{
    public enum StatutReservation
    {
        EnAttente,
        Confirmee,
        EnCours,
        Terminee,
        DemandeAnnulation,
        Annulee,
        Refusee
    }

    public class Reservation
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "La date de début est requise")]
        [Display(Name = "Date de début")]
        [DataType(DataType.Date)]
        public DateTime DateDebut { get; set; }
        
        [Required(ErrorMessage = "La date de fin est requise")]
        [Display(Name = "Date de fin")]
        [DataType(DataType.Date)]
        public DateTime DateFin { get; set; }
        
        [Display(Name = "Lieu de prise en charge")]
        public string LieuPriseEnCharge { get; set; } = string.Empty;
        
        [Display(Name = "Lieu de retour")]
        public string LieuRetour { get; set; } = string.Empty;
        
        // Tarification
        public int NombreJours { get; set; }
        public decimal PrixParJour { get; set; }
        public decimal MontantBase { get; set; } // NombreJours * PrixParJour
        
        // Frais d'urgence (réservation le jour même: +20%)
        public bool EstUrgent { get; set; } = false;
        public decimal FraisUrgence { get; set; } = 0;
        public decimal PourcentageFraisUrgence { get; set; } = 0;
        
        // Remises selon durée
        // 7 jours = 5%, 14 jours = 8%, 21 jours = 12%, 30 jours = 15%
        public decimal PourcentageRemise { get; set; } = 0;
        public decimal MontantRemise { get; set; } = 0;
        
        // Prix final
        public decimal PrixTotal { get; set; }
        
        public StatutReservation Statut { get; set; } = StatutReservation.EnAttente;
        
        public DateTime DateReservation { get; set; } = DateTime.Now;
        
        // Paiement
        public bool EstPaye { get; set; } = false;
        public DateTime? DatePaiement { get; set; }
        
        // Raison d'annulation (si demande d'annulation)
        public string? RaisonAnnulation { get; set; }
        public DateTime? DateDemandeAnnulation { get; set; }
        
        // Commentaire admin
        public string? CommentaireAdmin { get; set; }
        public DateTime? DateTraitementAdmin { get; set; }
        
        // Relations
        public int VoitureId { get; set; }
        public Voiture? Voiture { get; set; }
        
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
        
        public ICollection<Paiement>? Paiements { get; set; }
        
        // Méthodes de calcul
        public static decimal CalculerPourcentageRemise(int nombreJours)
        {
            if (nombreJours >= 30) return 15;
            if (nombreJours >= 21) return 12;
            if (nombreJours >= 14) return 8;
            if (nombreJours >= 7) return 5;
            return 0;
        }
        
        public static decimal CalculerFraisUrgence(decimal montantBase, bool estUrgent)
        {
            return estUrgent ? montantBase * 0.20m : 0; // 20% de frais si urgent
        }
    }
}
