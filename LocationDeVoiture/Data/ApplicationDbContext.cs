using LocationDeVoiture.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LocationDeVoiture.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Voiture> Voitures { get; set; }
        public DbSet<Reservation> Reservations { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<CarteBancaire> CartesBancaires { get; set; }
        public DbSet<Paiement> Paiements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision
            modelBuilder.Entity<Voiture>()
                .Property(v => v.PrixParJour)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Reservation>()
                .Property(r => r.PrixTotal)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<Reservation>()
                .Property(r => r.MontantBase)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<Reservation>()
                .Property(r => r.FraisUrgence)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<Reservation>()
                .Property(r => r.MontantRemise)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<Reservation>()
                .Property(r => r.PrixParJour)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CarteBancaire>()
                .Property(c => c.Solde)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Paiement>()
                .Property(p => p.Montant)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<Paiement>()
                .Property(p => p.MontantBase)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<Paiement>()
                .Property(p => p.FraisUrgence)
                .HasPrecision(18, 2);
            
            modelBuilder.Entity<Paiement>()
                .Property(p => p.Remise)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Voiture>().HasData(
                new Voiture
                {
                    Id = 1,
                    Marque = "Dacia",
                    Modele = "Logan",
                    Annee = 2023,
                    PrixParJour = 250,
                    ImageUrl = "https://s1.cdn.autoevolution.com/images/gallery/DACIA-Logan-2-4667_47.jpg",
                    Description = "Berline économique et fiable, parfaite pour les déplacements en ville et les longs trajets.",
                    Carburant = "Essence",
                    Transmission = "Manuelle",
                    NombrePlaces = 5,
                    NombrePortes = 4,
                    Climatisation = true,
                    GPS = true,
                    Disponible = true,
                    Categorie = "Économique"
                },
                new Voiture
                {
                    Id = 2,
                    Marque = "Renault",
                    Modele = "Clio",
                    Annee = 2023,
                    PrixParJour = 300,
                    ImageUrl = "https://www.autocar.co.uk/sites/autocar.co.uk/files/images/car-reviews/first-drives/legacy/1-renault-clio-e-tech-2020-uk-fd-hero-front_0.jpg",
                    Description = "Citadine moderne avec un design élégant et des équipements de confort.",
                    Carburant = "Essence",
                    Transmission = "Manuelle",
                    NombrePlaces = 5,
                    NombrePortes = 5,
                    Climatisation = true,
                    GPS = true,
                    Disponible = true,
                    Categorie = "Économique"
                },
                new Voiture
                {
                    Id = 3,
                    Marque = "Peugeot",
                    Modele = "308",
                    Annee = 2024,
                    PrixParJour = 450,
                    ImageUrl = "https://ssl.caranddriving.com/f6/images/CD169/new_peugeot308_2022_cd169.jpg",
                    Description = "Berline compacte avec intérieur raffiné et technologie i-Cockpit.",
                    Carburant = "Diesel",
                    Transmission = "Automatique",
                    NombrePlaces = 5,
                    NombrePortes = 5,
                    Climatisation = true,
                    GPS = true,
                    Disponible = true,
                    Categorie = "Confort"
                },
                new Voiture
                {
                    Id = 4,
                    Marque = "Dacia",
                    Modele = "Duster",
                    Annee = 2024,
                    PrixParJour = 500,
                    ImageUrl = "https://cdn.motor1.com/images/mgl/WzMyN/s1/2018-dacia-duster-official-image.jpg",
                    Description = "SUV robuste idéal pour explorer le Maroc. Parfait pour le désert et la montagne.",
                    Carburant = "Diesel",
                    Transmission = "Manuelle",
                    NombrePlaces = 5,
                    NombrePortes = 5,
                    Climatisation = true,
                    GPS = true,
                    Disponible = true,
                    Categorie = "SUV"
                },
                new Voiture
                {
                    Id = 5,
                    Marque = "Mercedes",
                    Modele = "Classe E",
                    Annee = 2024,
                    PrixParJour = 1200,
                    ImageUrl = "https://images.caradisiac.com/images/5/2/2/6/85226/S0-Essai-video-Mercedes-Classe-E-63-AMG-S-4Matic-transmutation-reussie-289996.jpg",
                    Description = "Berline de luxe allemande. Confort exceptionnel et prestations haut de gamme.",
                    Carburant = "Hybride",
                    Transmission = "Automatique",
                    NombrePlaces = 5,
                    NombrePortes = 4,
                    Climatisation = true,
                    GPS = true,
                    Disponible = true,
                    Categorie = "Premium"
                },
                new Voiture
                {
                    Id = 6,
                    Marque = "Toyota",
                    Modele = "Land Cruiser",
                    Annee = 2024,
                    PrixParJour = 1500,
                    ImageUrl = "https://cdn.motor1.com/images/mgl/0e7GJ2/s1/2024-toyota-land-cruiser-j70.jpg",
                    Description = "4x4 légendaire pour les aventures sahariennes. Robustesse et fiabilité.",
                    Carburant = "Diesel",
                    Transmission = "Automatique",
                    NombrePlaces = 7,
                    NombrePortes = 5,
                    Climatisation = true,
                    GPS = true,
                    Disponible = true,
                    Categorie = "4x4"
                },
                new Voiture
                {
                    Id = 7,
                    Marque = "Volkswagen",
                    Modele = "Golf",
                    Annee = 2023,
                    PrixParJour = 400,
                    ImageUrl = "https://cdn.motor1.com/images/mgl/nA9Joj/s1/2024-volkswagen-golf-gti-380.jpg",
                    Description = "La référence des compactes. Fiabilité allemande et polyvalence.",
                    Carburant = "Essence",
                    Transmission = "Manuelle",
                    NombrePlaces = 5,
                    NombrePortes = 5,
                    Climatisation = true,
                    GPS = true,
                    Disponible = true,
                    Categorie = "Confort"
                },
                new Voiture
                {
                    Id = 8,
                    Marque = "BMW",
                    Modele = "Série 3",
                    Annee = 2024,
                    PrixParJour = 900,
                    ImageUrl = "https://www.allcarz.ru/wp-content/uploads/2018/08/foto-bmw-3-g20_09.jpg",
                    Description = "Berline sportive allemande. Plaisir de conduite et finitions premium.",
                    Carburant = "Diesel",
                    Transmission = "Automatique",
                    NombrePlaces = 5,
                    NombrePortes = 4,
                    Climatisation = true,
                    GPS = true,
                    Disponible = true,
                    Categorie = "Premium"
                },
                new Voiture
                {
                    Id = 9,
                    Marque = "Ferrari",
                    Modele = "LaFerrari",
                    Annee = 2022,
                    PrixParJour = 3000,
                    ImageUrl = "https://th.bing.com/th/id/R.b6a31571c1501c2b1f757d6641fad6db?rik=BwpFRmEjzDfm4Q&pid=ImgRaw&r=0",
                    Description = "Hypercar hybride de luxe offrant des performances exceptionnelles.",
                    Carburant = "Hybride",
                    Transmission = "Automatique",
                    NombrePlaces = 2,
                    NombrePortes = 2,
                    Climatisation = true,
                    GPS = true,
                    Disponible = true,
                    Categorie = "Premium"
                }
            );
        }
        
    }
}
