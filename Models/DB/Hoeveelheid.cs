using System.ComponentModel.DataAnnotations;

namespace Recepten.Models.DB
{
    public class Hoeveelheid
    {
        [Required]
        public int HoeveelheidID { get; set; }

        [Required]
        public int GerechtID { get; set; }

        [Required]
        public int IngredientID { get; set; }

        [Required]
        public int EenheidID { get; set; }

        [Required]
        public double Aantal { get; set; }

        [Required]
        public Ingredient Ingredient { get; set; }

        [Required]
        public Eenheid Eenheid { get; set; }

        internal void SaveToDB(Context context)
        {
            if (0 == HoeveelheidID)
            {
                context.Hoeveelheden.Add(this);
                context.SaveChanges();
            }
            else
            {
                context.Hoeveelheden.Update(this);
            }
        }
    }
}