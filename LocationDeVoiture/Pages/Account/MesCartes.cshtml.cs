using LocationDeVoiture.Data;
using LocationDeVoiture.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace LocationDeVoiture.Pages.Account
{
    [Authorize]
    public class MesCartesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MesCartesModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<CarteBancaire> Cartes { get; set; } = new();
        
        [BindProperty]
        public InputModel Input { get; set; } = new();
        
        [TempData]
        public string? Message { get; set; }
        
        [TempData]
        public string? MessageType { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Le numéro de carte est requis")]
            [Display(Name = "Numéro de carte")]
            [RegularExpression(@"^\d{16}$", ErrorMessage = "Le numéro doit contenir 16 chiffres")]
            public string NumeroCarte { get; set; } = string.Empty;
            
            [Required(ErrorMessage = "Le nom sur la carte est requis")]
            [Display(Name = "Nom sur la carte")]
            public string NomTitulaire { get; set; } = string.Empty;
            
            [Required(ErrorMessage = "La date d'expiration est requise")]
            [Display(Name = "Date d'expiration")]
            [RegularExpression(@"^(0[1-9]|1[0-2])\/([0-9]{2})$", ErrorMessage = "Format: MM/YY")]
            public string DateExpiration { get; set; } = string.Empty;
            
            [Required(ErrorMessage = "Le CVV est requis")]
            [RegularExpression(@"^\d{3}$", ErrorMessage = "Le CVV doit contenir 3 chiffres")]
            public string CVV { get; set; } = string.Empty;
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            Cartes = await _context.CartesBancaires
                .Where(c => c.UserId == user.Id)
                .OrderByDescending(c => c.DateAjout)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage();

            if (!ModelState.IsValid)
            {
                Cartes = await _context.CartesBancaires
                    .Where(c => c.UserId == user.Id)
                    .ToListAsync();
                return Page();
            }

            // Vérifier si la carte existe déjà
            var carteExistante = await _context.CartesBancaires
                .FirstOrDefaultAsync(c => c.UserId == user.Id && c.NumeroCarte == Input.NumeroCarte);

            if (carteExistante != null)
            {
                Message = "Cette carte est déjà enregistrée.";
                MessageType = "warning";
                return RedirectToPage();
            }

            var carte = new CarteBancaire
            {
                UserId = user.Id,
                NumeroCarte = Input.NumeroCarte,
                NomTitulaire = Input.NomTitulaire,
                DateExpiration = Input.DateExpiration,
                CVV = Input.CVV,
                Solde = 50000, // Solde fictif initial de 50,000 MAD
                EstActive = true,
                DateAjout = DateTime.Now
            };

            _context.CartesBancaires.Add(carte);
            await _context.SaveChangesAsync();

            Message = "Carte ajoutée avec succès ! Solde fictif: 50,000 MAD";
            MessageType = "success";
            
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage();

            var carte = await _context.CartesBancaires
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (carte != null)
            {
                _context.CartesBancaires.Remove(carte);
                await _context.SaveChangesAsync();
                
                Message = "Carte supprimée.";
                MessageType = "success";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostToggleAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage();

            var carte = await _context.CartesBancaires
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);

            if (carte != null)
            {
                carte.EstActive = !carte.EstActive;
                await _context.SaveChangesAsync();
                
                Message = carte.EstActive ? "Carte activée." : "Carte désactivée.";
                MessageType = "success";
            }

            return RedirectToPage();
        }
    }
}

