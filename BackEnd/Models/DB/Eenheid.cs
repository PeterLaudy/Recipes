using System.ComponentModel.DataAnnotations;

namespace Recepten.Models.DB
{
    public class Eenheid
    {
        [Required]
        public int EenheidID { get; set; }

        [Required]
        public string Naam { get; set; }

        internal void SaveToContext(Context context)
        {
            if (0 == EenheidID)
            {
                context.Eenheden.Add(this);
            }
            else
            {
                context.Eenheden.Update(this);
            }
        }
    }
}