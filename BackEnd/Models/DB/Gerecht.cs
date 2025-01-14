using System.ComponentModel.DataAnnotations;

namespace Recepten.Models.DB
{
    public class Gerecht
    {
        [Required]
        public int GerechtID { get; set; }

        [Required]
        public string Naam { get; set; }

        [Required]
        [Display(Name = "Bereidingswijze")]
        public string Omschrijving { get; set; }

        [Required]
        public int Minuten { get; set; }

        internal void SaveToContext(Context context)
        {
            if (0 == GerechtID)
            {
                context.Gerechten.Add(this);
            }
            else
            {
                context.Gerechten.Update(this);
            }
        }
    }
}