using LocationDeVoiture.Data;
using LocationDeVoiture.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocationDeVoiture.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class ReservationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReservationsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Reservation> Reservations { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string? Statut { get; set; }

        [TempData]
        public string? Message { get; set; }
        
        [TempData]
        public string? MessageType { get; set; }

        public async Task OnGetAsync()
        {
            var query = _context.Reservations
                .Include(r => r.Voiture)
                .Include(r => r.User)
                .AsQueryable();

            if (!string.IsNullOrEmpty(Statut) && Enum.TryParse<StatutReservation>(Statut, out var statutEnum))
            {
                query = query.Where(r => r.Statut == statutEnum);
            }

            Reservations = await query
                .OrderByDescending(r => r.DateReservation)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostConfirmerAsync(int id)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                reservation.Statut = StatutReservation.Confirmee;
                reservation.DateTraitementAdmin = DateTime.Now;
                await _context.SaveChangesAsync();
                
                Message = $"Réservation #{id} confirmée avec succès.";
                MessageType = "success";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRefuserAsync(int id, string? commentaire)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                reservation.Statut = StatutReservation.Refusee;
                reservation.CommentaireAdmin = commentaire;
                reservation.DateTraitementAdmin = DateTime.Now;
                await _context.SaveChangesAsync();
                
                Message = $"Réservation #{id} refusée.";
                MessageType = "warning";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostAccepterAnnulationAsync(int id, string? commentaire)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                reservation.Statut = StatutReservation.Annulee;
                reservation.CommentaireAdmin = commentaire ?? "Annulation acceptée.";
                reservation.DateTraitementAdmin = DateTime.Now;
                await _context.SaveChangesAsync();
                
                Message = $"Demande d'annulation #{id} acceptée.";
                MessageType = "success";
            }
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRefuserAnnulationAsync(int id, string? commentaire)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation != null)
            {
                reservation.Statut = StatutReservation.Confirmee;
                reservation.CommentaireAdmin = commentaire ?? "Demande d'annulation refusée.";
                reservation.DateTraitementAdmin = DateTime.Now;
                await _context.SaveChangesAsync();
                
                Message = $"Demande d'annulation #{id} refusée. La réservation reste confirmée.";
                MessageType = "warning";
            }
            return RedirectToPage();
        }
    }
}

