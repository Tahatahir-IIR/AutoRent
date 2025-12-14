namespace LocationDeVoiture.Models
{
    public enum StatutPaiement
    {
        EnAttente,
        Effectue,
        Echoue,
        Rembourse
    }

    public class Paiement
    {
        public int Id { get; set; }
        
        public decimal Montant { get; set; }
        
        public StatutPaiement Statut { get; set; } = StatutPaiement.EnAttente;
        
        public DateTime DatePaiement { get; set; }
        
        public string? Reference { get; set; } // Référence de transaction
        
        public string? MessageErreur { get; set; }
        
        // Détails du paiement
        public decimal MontantBase { get; set; } // Prix de base (jours * prix/jour)
        public decimal FraisUrgence { get; set; } // Frais si réservation le jour même
        public decimal Remise { get; set; } // Montant de la remise
        public decimal PourcentageRemise { get; set; } // % de remise appliqué
        
        // Relations
        public int ReservationId { get; set; }
        public Reservation? Reservation { get; set; }
        
        public int? CarteBancaireId { get; set; }
        public CarteBancaire? CarteBancaire { get; set; }
    }
}

