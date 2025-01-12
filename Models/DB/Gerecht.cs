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
        public string Omschrijving { get; set; }

        [Required]
        public int Minuten { get; set; }

        [Required]
        public int CategorieID { get; set; }

        [Required]
        public Categorie Categorie { get; set; }

        internal void SaveToDB(Context context)
        {
            if (0 == CategorieID)
            {
                this.Categorie.SaveToDB(context);
                this.CategorieID = this.Categorie.CategorieID;
            }

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