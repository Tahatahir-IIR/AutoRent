using LocationDeVoiture.Data;
using LocationDeVoiture.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocationDeVoiture.Pages.Voitures
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Voiture> Voitures { get; set; } = new();
        public List<string> Categories { get; set; } = new();
        
        [BindProperty(SupportsGet = true)]
        public string? Categorie { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public string? Recherche { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public decimal? PrixMin { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public decimal? PrixMax { get; set; }

        public async Task OnGetAsync()
        {
            Categories = await _context.Voitures
                .Select(v => v.Categorie)
                .Distinct()
                .ToListAsync();

            var query = _context.Voitures.Where(v => v.Disponible);

            if (!string.IsNullOrEmpty(Categorie))
            {
                query = query.Where(v => v.Categorie == Categorie);
            }

            if (!string.IsNullOrEmpty(Recherche))
            {
                query = query.Where(v => 
                    v.Marque.Contains(Recherche) || 
                    v.Modele.Contains(Recherche) ||
                    v.Description.Contains(Recherche));
            }

            if (PrixMin.HasValue)
            {
                query = query.Where(v => v.PrixParJour >= PrixMin.Value);
            }

            if (PrixMax.HasValue)
            {
                query = query.Where(v => v.PrixParJour <= PrixMax.Value);
            }

            Voitures = await query.OrderBy(v => v.PrixParJour).ToListAsync();
        }
    }
}

