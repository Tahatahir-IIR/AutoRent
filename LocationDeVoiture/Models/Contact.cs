using System.ComponentModel.DataAnnotations;

namespace LocationDeVoiture.Models
{
    public class Contact
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Le nom est requis")]
        [Display(Name = "Nom complet")]
        public string Nom { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "L'email est requis")]
        [EmailAddress(ErrorMessage = "Email invalide")]
        public string Email { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le sujet est requis")]
        public string Sujet { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "Le message est requis")]
        [MinLength(10, ErrorMessage = "Le message doit contenir au moins 10 caractères")]
        public string Message { get; set; } = string.Empty;
        
        public DateTime DateEnvoi { get; set; } = DateTime.Now;
        
        public bool Lu { get; set; } = false;
    }
}

