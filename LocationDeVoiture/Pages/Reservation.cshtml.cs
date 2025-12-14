using LocationDeVoiture.Data;
using LocationDeVoiture.Models;
using LocationDeVoiture.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocationDeVoiture.Pages
{
    [Authorize]
    public class ReservationModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReservationModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Voiture? Voiture { get; set; }
        public List<Voiture> Voitures { get; set; } = new();
        public ApplicationUser? CurrentUser { get; set; }
        public List<CarteBancaire> CartesBancaires { get; set; } = new();
        public bool HasActiveCard { get; set; }
        
        [BindProperty]
        public Reservation Reservation { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public int? VoitureId { get; set; }
        
        public bool ReservationReussie { get; set; } = false;
        public ReservationPricing? Pricing { get; set; }

        public async Task OnGetAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null) return;

            Voitures = await _context.Voitures.Where(v => v.Disponible).ToListAsync();
            
            CartesBancaires = await _context.CartesBancaires
                .Where(c => c.UserId == CurrentUser.Id && c.EstActive)
                .ToListAsync();
            HasActiveCard = CartesBancaires.Any();
            
            if (VoitureId.HasValue)
            {
                Voiture = await _context.Voitures.FindAsync(VoitureId.Value);
                Reservation.VoitureId = VoitureId.Value;
            }
            
            Reservation.DateDebut = DateTime.Today.AddDays(1);
            Reservation.DateFin = DateTime.Today.AddDays(3);
            Reservation.LieuPriseEnCharge = "Casablanca - Aéroport Mohammed V";
            Reservation.LieuRetour = "Casablanca - Aéroport Mohammed V";
            
            // Calculer le prix si on a une voiture
            if (Voiture != null)
            {
                Pricing = PaymentService.CalculatePricing(Voiture.PrixParJour, Reservation.DateDebut, Reservation.DateFin);
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null) return RedirectToPage();

            Voitures = await _context.Voitures.Where(v => v.Disponible).ToListAsync();
            CartesBancaires = await _context.CartesBancaires
                .Where(c => c.UserId == CurrentUser.Id && c.EstActive)
                .ToListAsync();
            HasActiveCard = CartesBancaires.Any();
            
            if (Reservation.VoitureId > 0)
            {
                Voiture = await _context.Voitures.FindAsync(Reservation.VoitureId);
            }

            if (Reservation.DateFin <= Reservation.DateDebut)
            {
                ModelState.AddModelError("Reservation.DateFin", "La date de fin doit être après la date de début");
                return Page();
            }

            if (!HasActiveCard)
            {
                ModelState.AddModelError(string.Empty, "Vous devez ajouter une carte bancaire pour réserver.");
                return Page();
            }

            if (Voiture != null && CurrentUser != null)
            {
                // Calculer la tarification
                Pricing = PaymentService.CalculatePricing(Voiture.PrixParJour, Reservation.DateDebut, Reservation.DateFin);
                
                // Remplir la réservation avec les détails de tarification
                Reservation.NombreJours = Pricing.NombreJours;
                Reservation.PrixParJour = Pricing.PrixParJour;
                Reservation.MontantBase = Pricing.MontantBase;
                Reservation.EstUrgent = Pricing.EstUrgent;
                Reservation.PourcentageFraisUrgence = Pricing.PourcentageFraisUrgence;
                Reservation.FraisUrgence = Pricing.FraisUrgence;
                Reservation.PourcentageRemise = Pricing.PourcentageRemise;
                Reservation.MontantRemise = Pricing.MontantRemise;
                Reservation.PrixTotal = Pricing.PrixTotal;
                
                Reservation.UserId = CurrentUser.Id;
                Reservation.DateReservation = DateTime.Now;
                Reservation.Statut = StatutReservation.EnAttente;
                Reservation.EstPaye = false;

                _context.Reservations.Add(Reservation);
                await _context.SaveChangesAsync();

                ReservationReussie = true;
            }

            return Page();
        }
        
        // API endpoint pour calculer le prix en temps réel
        public async Task<IActionResult> OnGetCalculatePriceAsync(int voitureId, string dateDebut, string dateFin)
        {
            var voiture = await _context.Voitures.FindAsync(voitureId);
            if (voiture == null)
            {
                return new JsonResult(new { error = "Véhicule non trouvé" });
            }

            if (!DateTime.TryParse(dateDebut, out var debut) || !DateTime.TryParse(dateFin, out var fin))
            {
                return new JsonResult(new { error = "Dates invalides" });
            }

            var pricing = PaymentService.CalculatePricing(voiture.PrixParJour, debut, fin);
            
            return new JsonResult(new
            {
                nombreJours = pricing.NombreJours,
                prixParJour = pricing.PrixParJour,
                montantBase = pricing.MontantBase,
                estUrgent = pricing.EstUrgent,
                pourcentageFraisUrgence = pricing.PourcentageFraisUrgence,
                fraisUrgence = pricing.FraisUrgence,
                pourcentageRemise = pricing.PourcentageRemise,
                montantRemise = pricing.MontantRemise,
                prixTotal = pricing.PrixTotal
            });
        }
    }
}
