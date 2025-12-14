using LocationDeVoiture.Data;
using LocationDeVoiture.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocationDeVoiture.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalReservations { get; set; }
        public int ReservationsEnAttente { get; set; }
        public int DemandesAnnulation { get; set; }
        public int TotalClients { get; set; }
        public decimal RevenuTotal { get; set; }
        public List<Reservation> DernieresReservations { get; set; } = new();

        public async Task OnGetAsync()
        {
            TotalReservations = await _context.Reservations.CountAsync();
            
            ReservationsEnAttente = await _context.Reservations
                .CountAsync(r => r.Statut == StatutReservation.EnAttente);
            
            DemandesAnnulation = await _context.Reservations
                .CountAsync(r => r.Statut == StatutReservation.DemandeAnnulation);
            
            TotalClients = await _context.Users.CountAsync();
            
            RevenuTotal = await _context.Reservations
                .Where(r => r.Statut == StatutReservation.Confirmee || 
                           r.Statut == StatutReservation.Terminee ||
                           r.Statut == StatutReservation.EnCours)
                .SumAsync(r => r.PrixTotal);

            DernieresReservations = await _context.Reservations
                .Include(r => r.Voiture)
                .Include(r => r.User)
                .OrderByDescending(r => r.DateReservation)
                .Take(10)
                .ToListAsync();
        }
    }
}

