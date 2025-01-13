using System.ComponentModel.DataAnnotations;

namespace Recepten.Models.DB
{
    public class Categorie
    {
        [Required]
        public int CategorieID { get; set; }

        [Required]
        [Display(Name = "Categorie")]
        public string Naam { get; set; }

        internal void SaveToContext(Context context)
        {
            if (0 == CategorieID)
            {
                context.Categorieen.Add(this);
            }
            else
            {
                context.Categorieen.Update(this);
            }
        }
    }
}