using LocationDeVoiture.Data;
using LocationDeVoiture.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace LocationDeVoiture.Pages.Voitures
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Voiture? Voiture { get; set; }
        public List<Voiture> VoituresSimilaires { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Voiture = await _context.Voitures.FindAsync(id);

            if (Voiture == null)
            {
                return NotFound();
            }

            VoituresSimilaires = await _context.Voitures
                .Where(v => v.Categorie == Voiture.Categorie && v.Id != id && v.Disponible)
                .Take(3)
                .ToListAsync();

            return Page();
        }
    }
}

