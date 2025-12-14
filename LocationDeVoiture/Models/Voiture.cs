namespace LocationDeVoiture.Models
{
    public class Voiture
    {
        public int Id { get; set; }
        public string Marque { get; set; } = string.Empty;
        public string Modele { get; set; } = string.Empty;
        public int Annee { get; set; }
        public decimal PrixParJour { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Carburant { get; set; } = string.Empty; // Essence, Diesel, Électrique, Hybride
        public string Transmission { get; set; } = string.Empty; // Manuelle, Automatique
        public int NombrePlaces { get; set; }
        public int NombrePortes { get; set; }
        public bool Climatisation { get; set; }
        public bool GPS { get; set; }
        public bool Disponible { get; set; } = true;
        public string Categorie { get; set; } = string.Empty; // Économique, Confort, Premium, SUV, Sport
        
        public ICollection<Reservation>? Reservations { get; set; }
    }
}

