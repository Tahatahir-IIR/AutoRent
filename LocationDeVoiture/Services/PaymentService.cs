using LocationDeVoiture.Data;
using LocationDeVoiture.Models;
using Microsoft.EntityFrameworkCore;

namespace LocationDeVoiture.Services
{
    public class PaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(ApplicationDbContext context, ILogger<PaymentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Traite les paiements pour les réservations qui démarrent aujourd'hui
        /// </summary>
        public async Task ProcessDuePaymentsAsync()
        {
            var today = DateTime.Today;
            
            // Récupérer toutes les réservations confirmées qui démarrent aujourd'hui et non payées
            var reservationsDues = await _context.Reservations
                .Include(r => r.User)
                    .ThenInclude(u => u!.CartesBancaires)
                .Include(r => r.Voiture)
                .Where(r => r.DateDebut.Date == today && 
                           r.Statut == StatutReservation.Confirmee && 
                           !r.EstPaye)
                .ToListAsync();

            foreach (var reservation in reservationsDues)
            {
                await ProcessPaymentAsync(reservation);
            }
        }

        /// <summary>
        /// Traite le paiement pour une réservation spécifique
        /// </summary>
        public async Task<(bool Success, string Message)> ProcessPaymentAsync(Reservation reservation)
        {
            try
            {
                if (reservation.User == null)
                {
                    return (false, "Utilisateur non trouvé");
                }

                // Charger les cartes si pas déjà fait
                if (reservation.User.CartesBancaires == null)
                {
                    await _context.Entry(reservation.User)
                        .Collection(u => u.CartesBancaires!)
                        .LoadAsync();
                }

                var carteActive = reservation.User.CartesBancaires?
                    .FirstOrDefault(c => c.EstActive && c.Solde >= reservation.PrixTotal);

                if (carteActive == null)
                {
                    // Créer un paiement échoué
                    var paiementEchoue = new Paiement
                    {
                        ReservationId = reservation.Id,
                        Montant = reservation.PrixTotal,
                        MontantBase = reservation.MontantBase,
                        FraisUrgence = reservation.FraisUrgence,
                        Remise = reservation.MontantRemise,
                        PourcentageRemise = reservation.PourcentageRemise,
                        Statut = StatutPaiement.Echoue,
                        DatePaiement = DateTime.Now,
                        MessageErreur = "Solde insuffisant ou aucune carte valide"
                    };
                    
                    _context.Paiements.Add(paiementEchoue);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogWarning($"Paiement échoué pour réservation #{reservation.Id}: Solde insuffisant");
                    return (false, "Solde insuffisant sur la carte");
                }

                // Déduire le montant de la carte
                carteActive.Solde -= reservation.PrixTotal;
                
                // Créer le paiement
                var paiement = new Paiement
                {
                    ReservationId = reservation.Id,
                    CarteBancaireId = carteActive.Id,
                    Montant = reservation.PrixTotal,
                    MontantBase = reservation.MontantBase,
                    FraisUrgence = reservation.FraisUrgence,
                    Remise = reservation.MontantRemise,
                    PourcentageRemise = reservation.PourcentageRemise,
                    Statut = StatutPaiement.Effectue,
                    DatePaiement = DateTime.Now,
                    Reference = GenerateReference()
                };

                // Mettre à jour la réservation
                reservation.EstPaye = true;
                reservation.DatePaiement = DateTime.Now;
                reservation.Statut = StatutReservation.EnCours;

                _context.Paiements.Add(paiement);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Paiement effectué pour réservation #{reservation.Id}: {reservation.PrixTotal} MAD");
                return (true, $"Paiement de {reservation.PrixTotal:N0} MAD effectué avec succès");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erreur lors du paiement pour réservation #{reservation.Id}");
                return (false, "Erreur lors du traitement du paiement");
            }
        }

        /// <summary>
        /// Rembourse une réservation annulée
        /// </summary>
        public async Task<(bool Success, string Message)> RefundPaymentAsync(int reservationId)
        {
            var paiement = await _context.Paiements
                .Include(p => p.CarteBancaire)
                .Include(p => p.Reservation)
                .FirstOrDefaultAsync(p => p.ReservationId == reservationId && p.Statut == StatutPaiement.Effectue);

            if (paiement == null)
            {
                return (false, "Aucun paiement trouvé à rembourser");
            }

            if (paiement.CarteBancaire != null)
            {
                // Rembourser sur la carte
                paiement.CarteBancaire.Solde += paiement.Montant;
            }

            paiement.Statut = StatutPaiement.Rembourse;
            
            await _context.SaveChangesAsync();
            
            _logger.LogInformation($"Remboursement effectué pour réservation #{reservationId}: {paiement.Montant} MAD");
            return (true, $"Remboursement de {paiement.Montant:N0} MAD effectué");
        }

        /// <summary>
        /// Calcule le prix total d'une réservation avec frais et remises
        /// </summary>
        public static ReservationPricing CalculatePricing(decimal prixParJour, DateTime dateDebut, DateTime dateFin)
        {
            var nombreJours = Math.Max(1, (dateFin - dateDebut).Days);
            var montantBase = nombreJours * prixParJour;
            
            // Vérifier si c'est une réservation urgente (le jour même)
            var estUrgent = dateDebut.Date == DateTime.Today;
            var pourcentageFraisUrgence = estUrgent ? 20m : 0m;
            var fraisUrgence = estUrgent ? (prixParJour * (pourcentageFraisUrgence / 100)) : 0m;

            // Calculer la remise selon la durée
            var pourcentageRemise = Reservation.CalculerPourcentageRemise(nombreJours);
            var montantRemise = montantBase * (pourcentageRemise / 100);
            
            // Prix final = Base + Frais urgence - Remise
            var prixTotal = montantBase + fraisUrgence - montantRemise;
            
            return new ReservationPricing
            {
                NombreJours = nombreJours,
                PrixParJour = prixParJour,
                MontantBase = montantBase,
                EstUrgent = estUrgent,
                PourcentageFraisUrgence = pourcentageFraisUrgence,
                FraisUrgence = fraisUrgence,
                PourcentageRemise = pourcentageRemise,
                MontantRemise = montantRemise,
                PrixTotal = prixTotal
            };
        }

        private string GenerateReference()
        {
            return $"PAY-{DateTime.Now:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
        }
    }

    public class ReservationPricing
    {
        public int NombreJours { get; set; }
        public decimal PrixParJour { get; set; }
        public decimal MontantBase { get; set; }
        public bool EstUrgent { get; set; }
        public decimal PourcentageFraisUrgence { get; set; }
        public decimal FraisUrgence { get; set; }
        public decimal PourcentageRemise { get; set; }
        public decimal MontantRemise { get; set; }
        public decimal PrixTotal { get; set; }
    }
}

