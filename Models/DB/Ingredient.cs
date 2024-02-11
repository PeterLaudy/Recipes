using System.ComponentModel.DataAnnotations;

namespace Recepten.Models.DB
{
    public class Ingredient
    {
        [Required]
        public int IngredientID { get; set; }

        [Required]
        public string Naam { get; set; }

        internal void SaveToDB(Context context)
        {
            if (0 == IngredientID)
            {
                context.Ingredienten.Add(this);
                context.SaveChanges();
            }
            else
            {
                context.Ingredienten.Update(this);
            }
        }
    }
}