using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Recepten.Models.DB;
using zestien3.carddav;

namespace Recepten.Controllers
{
    public class Recept
    {
        public Recept() { }

        internal Recept(int gerechtIndex, Context context)
        {
            this.Gerecht = context.Gerechten.FirstOrDefault(g => g.GerechtID == gerechtIndex);
            this.Hoeveelheden = new List<Hoeveelheid>();
            Hoeveelheden.AddRange(
                context.Hoeveelheden
                    .Include(h => h.Ingredient)
                    .Include(h => h.Eenheid)
                    .Where(h => h.GerechtID == gerechtIndex));
        }

        [Required]
        public Gerecht Gerecht { get; set; }

        [Required]
        public List<Hoeveelheid> Hoeveelheden { get; set; }

        internal string SaveToDB(Context context)
        {
            Gerecht.SaveToDB(context);

            // Make sure we that if a Hoeveelheid has an Eenheid or Ingredient
            // with the same ID as another Hoeveelheid, they also point to the same
            // Eenheid or Ingredient instance. Otherwise we have two entities with
            // the same key, which the entity framework will not like.
            foreach (var h1 in Hoeveelheden)
            {
                foreach (var h2 in Hoeveelheden)
                {
                    if (h1 != h2)
                    {
                        if ((h1.EenheidID == h2.EenheidID) && 
                            ((h1.EenheidID != 0) || (h1.Eenheid.Naam == h2.Eenheid.Naam)))
                        {
                            h1.Eenheid = h2.Eenheid;
                        }

                        if ((h1.IngredientID == h2.IngredientID) && 
                            ((h1.IngredientID != 0) || (h1.Ingredient.Naam == h2.Ingredient.Naam)))
                        {
                            h1.Ingredient = h2.Ingredient;
                        }
                    }
                }
            }

            foreach (var hoeveelheid in Hoeveelheden)
            {
                hoeveelheid.GerechtID = Gerecht.GerechtID;

                if (0 == hoeveelheid.IngredientID)
                {
                    Ingredient i = context.Ingredienten.FirstOrDefault(ingredient => ingredient.Naam == hoeveelheid.Ingredient.Naam);
                    if (null != i)
                    {
                        hoeveelheid.Ingredient = i;
                        hoeveelheid.IngredientID = i.IngredientID;
                    }
                }

                if (0 == hoeveelheid.EenheidID)
                {
                    Eenheid e = context.Eenheden.FirstOrDefault(eenheid => eenheid.Naam == hoeveelheid.Eenheid.Naam);
                    if (null != e)
                    {
                        hoeveelheid.Eenheid = e;
                        hoeveelheid.EenheidID = e.EenheidID;
                    }
                }

                hoeveelheid.Ingredient.SaveToDB(context);
                hoeveelheid.Eenheid.SaveToDB(context);
                hoeveelheid.SaveToDB(context);
            }

            var hoeveelheden2Delete = context.Hoeveelheden.Where(hoeveelheid => hoeveelheid.GerechtID == Gerecht.GerechtID);
            foreach (var hoeveelheid in hoeveelheden2Delete)
            {
                if (!Hoeveelheden.Contains(hoeveelheid))
                {
                    context.Hoeveelheden.Remove(hoeveelheid);
                }
            }

            context.SaveChanges();

            var result = "OK";
            return result;
        }
    }

    public class MailRecipeData
    {
        public string MailAddress { get; set; }
        public int GerechtIndex { get; set; }
    }

    [Route("api/[controller]")]
    public class DataController : Controller
    {
        private readonly Context context;
        private EmailSender emailSender;
        private IWebHostEnvironment environment;
        private IConfiguration configuration;

        public DataController(
            Context context,
            IEmailSender emailSender,
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            this.context = context;
            this.emailSender = emailSender as EmailSender;
            this.environment = environment;
            this.configuration = configuration;
        }

        [HttpPost("[action]")]
        public JsonResult MailRecept([FromBody] MailRecipeData mrd)
        {
            var recept = new Recept(mrd.GerechtIndex, this.context);

            var ingredienten = new StringBuilder();
            recept.Hoeveelheden.ForEach(h => {
                ingredienten.AppendLine($"<span>{h.Aantal} {h.Eenheid.Naam} {h.Ingredient.Naam}<br/></span>");
            });

            string fileName = Path.Combine(environment.ContentRootPath, "gerecht.html");
            this.emailSender.SendEmailAsync(
                mrd.MailAddress,
                "Recept",
                System.IO.File.ReadAllText(fileName)
                .Replace("<TITEL>", recept.Gerecht.Naam)
                .Replace("<INGREDIENTEN>", ingredienten.ToString())
                .Replace("<BEREIDINGSDUUR>", recept.Gerecht.Minuten.ToString())
                .Replace("<BEREIDINGSWIJZE>", recept.Gerecht.Omschrijving)
            ).Start();

            return Json("OK");
        }

        [HttpGet("[action]")]
        public JsonResult Recepten()
        {
            var result = this.context.Gerechten.Include(g => g.Categorie)
            .OrderBy(g => g.Naam)
            .GroupBy(g => g.CategorieID)
            .Select(group => new {
                categorie = group.First().Categorie.Naam,
                list = group.Select(g => new {
                    index = g.GerechtID,
                    name = g.Naam
                })
            })
            .ToList();

            return Json(result);
        }

        [HttpGet("[action]")]
        public JsonResult Recept(int index)
        {
            var recept = new Recept(index, this.context);

            return Json(recept);
        }

        [HttpPost("[action]")]
        public JsonResult FindRecept([FromBody] List<Ingredient> ingredienten)
        {
            // Make a list of the indices of all ingredients.
            List<int> id = ingredienten.Select(i => i.IngredientID).ToList();

            // Get all quantities (hoeveelheden) which reference one of these ingredients.
            List<Hoeveelheid> hh = this.context.Hoeveelheden.Where(h => id.Contains(h.IngredientID)).ToList();

            // Sort these quantities per recipe.
            Dictionary<int, List<int>> hhSort = new Dictionary<int, List<int>>();
            hh.ForEach(h => {
                if (!hhSort.ContainsKey(h.GerechtID))
                {
                    hhSort[h.GerechtID] = new List<int>();
                }
                hhSort[h.GerechtID].Add(h.IngredientID);
            });

            // Get all recipes which contain all ingredients.
            var gerechten = new List<int>();
            foreach (var gID in hhSort.Keys)
            {
                if (id.All(i => hhSort[gID].Contains(i)))
                {
                    gerechten.Add(gID);
                }
            }

            // Get the names and IDs of all these recipes and return them.
            var result = this.context.Gerechten.Include(g => g.Categorie)
            .Where(g => gerechten.Contains(g.GerechtID))
            .OrderBy(g => g.Naam)
            .GroupBy(g => g.CategorieID)
            .Select(group => new {
                categorie = group.First().Categorie.Naam,
                list = group.Select(g => new {
                    index = g.GerechtID,
                    name = g.Naam
                })
            })
            .ToList();

            return Json(result);
        }

        [HttpPost("[action]")]
        public JsonResult AddRecept([FromBody] Recept recept)
        {
            string result;
            if (ModelState.IsValid)
            {
                if (0 < recept.Gerecht.GerechtID)
                {
                    result = recept.SaveToDB(this.context);
                }
                else
                {
                    Gerecht gerecht = this.context.Gerechten.FirstOrDefault(g => g.Naam.Equals(recept.Gerecht.Naam));
                    if (null == gerecht)
                    {
                        result = recept.SaveToDB(this.context);
                    }
                    else
                    {
                        result = "Een gerecht met dezelfde naam bestaat al.";
                    }
                }
            }
            else
            {
                StringBuilder s = new StringBuilder();
                foreach (var modelState in ModelState.Values) {
                    foreach (var error in modelState.Errors) {
                        s.AppendLine(error.ErrorMessage);
                    }
                }
                result = s.ToString();
            }

            return Json(result);
        }

        [HttpGet("[action]")]
        public JsonResult Eenheden()
        {
            var result = this.context.Eenheden.Select(e => new {
                eenheidID = e.EenheidID,
                naam = e.Naam
            }).ToList();

            return Json(result);
        }

        [HttpGet("[action]")]
        public JsonResult Eenheid(int index)
        {
            var result = this.context.Eenheden.FirstOrDefault(e => e.EenheidID == index);

            return Json(result);
        }

        [HttpGet("[action]")]
        public JsonResult Ingredienten()
        {
            var result = this.context.Ingredienten.Select(i => new {
                ingredientID = i.IngredientID,
                naam = i.Naam
            }).ToList();

            return Json(result);
        }

        [HttpGet("[action]")]
        public JsonResult Ingredient(int index)
        {
            var result = this.context.Ingredienten.FirstOrDefault(i => i.IngredientID == index);

            return Json(result);
        }

        [HttpGet("[action]")]
        public JsonResult Categorieen()
        {
            var result = this.context.Categorieen.Select(c => new {
                categorieID = c.CategorieID,
                naam = c.Naam
            }).ToList();

            return Json(result);
        }

        [HttpGet("[action]")]
        public async Task<JsonResult> EmailAddresses()
        {
            return Json(await new ContactsServer(environment).GetAllEmailAdresses(this.configuration));
        }
    }
}
