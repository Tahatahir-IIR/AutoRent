using LocationDeVoiture.Data;
using LocationDeVoiture.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocationDeVoiture.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Voiture> VoituresPopulaires { get; set; } = new();
        public List<string> Categories { get; set; } = new();

        public async Task OnGetAsync()
        {
            VoituresPopulaires = await _context.Voitures
                .Where(v => v.Disponible)
                .Take(6)
                .ToListAsync();

            Categories = await _context.Voitures
                .Select(v => v.Categorie)
                .Distinct()
                .ToListAsync();
        }
    }
}
