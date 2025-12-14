using System.ComponentModel.DataAnnotations;

namespace LocationDeVoiture.Models
{
    public class CarteBancaire
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le numéro de carte est requis")]
        [Display(Name = "Numéro de carte")]
        [StringLength(16, MinimumLength = 16, ErrorMessage = "Le numéro doit contenir 16 chiffres")]
        public string NumeroCarte { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le nom sur la carte est requis")]
        [Display(Name = "Nom sur la carte")]
        public string NomTitulaire { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "La date d'expiration est requise")]
        [Display(Name = "Date d'expiration (MM/YY)")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Format: MM/YY")]
        public string DateExpiration { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le CVV est requis")]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Le CVV doit contenir 3 chiffres")]
        public string CVV { get; set; } = string.Empty;
        
        // Solde fictif pour la simulation
        public decimal Solde { get; set; } = 50000; // 50,000 MAD par défaut
        
        public bool EstActive { get; set; } = true;
        
        public DateTime DateAjout { get; set; } = DateTime.Now;
        
        // Relation avec l'utilisateur
        public string UserId { get; set; } = string.Empty;
        public ApplicationUser? User { get; set; }
        
        // Méthode pour masquer le numéro de carte
        public string NumeroMasque => $"**** **** **** {NumeroCarte.Substring(Math.Max(0, NumeroCarte.Length - 4))}";
    }
}

