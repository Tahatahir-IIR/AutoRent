using LocationDeVoiture.Data;
using LocationDeVoiture.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocationDeVoiture.Pages
{
    [Authorize]
    public class MesReservationsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MesReservationsModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Reservation> ReservationsEnCours { get; set; } = new();
        public List<Reservation> ReservationsAVenir { get; set; } = new();
        public List<Reservation> ReservationsPassees { get; set; } = new();
        
        [TempData]
        public string? Message { get; set; }
        
        [TempData]
        public string? MessageType { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var allReservations = await _context.Reservations
                .Include(r => r.Voiture)
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.DateReservation)
                .ToListAsync();

            var today = DateTime.Today;

            ReservationsEnCours = allReservations
                .Where(r => r.DateDebut <= today && r.DateFin >= today && 
                       r.Statut != StatutReservation.Annulee && 
                       r.Statut != StatutReservation.Refusee)
                .ToList();

            ReservationsAVenir = allReservations
                .Where(r => r.DateDebut > today && 
                       r.Statut != StatutReservation.Annulee && 
                       r.Statut != StatutReservation.Refusee)
                .ToList();

            ReservationsPassees = allReservations
                .Where(r => r.DateFin < today || 
                       r.Statut == StatutReservation.Annulee || 
                       r.Statut == StatutReservation.Refusee ||
                       r.Statut == StatutReservation.Terminee)
                .ToList();
        }

        public async Task<IActionResult> OnPostDemanderAnnulationAsync(int id, string raison)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToPage();

            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id && r.UserId == user.Id);

            if (reservation == null)
            {
                Message = "Réservation introuvable.";
                MessageType = "danger";
                return RedirectToPage();
            }

            if (reservation.Statut == StatutReservation.Annulee || 
                reservation.Statut == StatutReservation.DemandeAnnulation)
            {
                Message = "Cette réservation ne peut pas être annulée.";
                MessageType = "danger";
                return RedirectToPage();
            }

            reservation.Statut = StatutReservation.DemandeAnnulation;
            reservation.RaisonAnnulation = raison;
            reservation.DateDemandeAnnulation = DateTime.Now;

            await _context.SaveChangesAsync();

            Message = "Votre demande d'annulation a été envoyée. Vous serez notifié une fois traitée.";
            MessageType = "success";
            return RedirectToPage();
        }
    }
}

