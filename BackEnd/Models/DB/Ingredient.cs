using System.ComponentModel.DataAnnotations;

namespace Recepten.Models.DB
{
    public class Ingredient
    {
        [Required]
        public int IngredientID { get; set; }

        [Required]
        [Display(Name = "Ingrediënt")]
        public string Naam { get; set; }

        internal void SaveToContext(Context context)
        {
            if (0 == IngredientID)
            {
                context.Ingredienten.Add(this);
            }
            else
            {
                context.Ingredienten.Update(this);
            }
        }
    }
}