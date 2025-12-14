using LocationDeVoiture.Data;
using LocationDeVoiture.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LocationDeVoiture.Pages
{
    public class ContactModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ContactModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Contact Contact { get; set; } = new();
        
        public bool MessageEnvoye { get; set; } = false;

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Contact.DateEnvoi = DateTime.Now;
            _context.Contacts.Add(Contact);
            await _context.SaveChangesAsync();

            MessageEnvoye = true;
            Contact = new Contact(); // Reset form
            
            return Page();
        }
    }
}

