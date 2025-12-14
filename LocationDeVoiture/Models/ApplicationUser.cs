using Microsoft.AspNetCore.Identity;

namespace LocationDeVoiture.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string? Adresse { get; set; }
        public string? Ville { get; set; }
        public DateTime DateInscription { get; set; } = DateTime.Now;
        
        public ICollection<Reservation>? Reservations { get; set; }
        public ICollection<CarteBancaire>? CartesBancaires { get; set; }
    }
}
