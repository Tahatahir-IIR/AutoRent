using LocationDeVoiture.Data;
using Microsoft.EntityFrameworkCore;

namespace LocationDeVoiture.Services
{
    public class PaymentBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<PaymentBackgroundService> _logger;

        public PaymentBackgroundService(IServiceProvider serviceProvider, ILogger<PaymentBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Service de paiement automatique démarré");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<PaymentService>>();
                    
                    var paymentService = new PaymentService(context, logger);
                    await paymentService.ProcessDuePaymentsAsync();
                    
                    // Aussi mettre à jour les réservations terminées
                    await UpdateCompletedReservationsAsync(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erreur dans le service de paiement automatique");
                }

                // Vérifier toutes les heures
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
        }

        private async Task UpdateCompletedReservationsAsync(ApplicationDbContext context)
        {
            var today = DateTime.Today;
            
            // Mettre à jour les réservations terminées
            var reservationsTerminees = await context.Reservations
                .Where(r => r.DateFin.Date < today && 
                           (r.Statut == Models.StatutReservation.EnCours || 
                            r.Statut == Models.StatutReservation.Confirmee))
                .ToListAsync();

            foreach (var reservation in reservationsTerminees)
            {
                reservation.Statut = Models.StatutReservation.Terminee;
            }

            if (reservationsTerminees.Any())
            {
                await context.SaveChangesAsync();
                _logger.LogInformation($"{reservationsTerminees.Count} réservation(s) marquée(s) comme terminée(s)");
            }
        }
    }
}

