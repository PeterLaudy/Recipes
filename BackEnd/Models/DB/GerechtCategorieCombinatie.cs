using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Recepten.Models.DB
{
    public class GerechtCategorieCombinatie
    
    {
        [Required]
        public int GerechtCategorieCombinatieID { get; set; }

        [Required]
        public int GerechtID { get; set; }

        [Required]
        public int CategorieID { get; set; }

        [JsonIgnore]
        public Gerecht Gerecht { get; set; }

        internal void SaveToContext(Context context)
        {
            if (0 == GerechtCategorieCombinatieID)
            {
                context.GerechtCategorieCombinaties.Add(this);
            }
            else
            {
                context.GerechtCategorieCombinaties.Update(this);
            }
        }
    }
}