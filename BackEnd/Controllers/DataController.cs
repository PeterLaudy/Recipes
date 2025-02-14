using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Recepten.Models.DB;
using Recepten.Services;

namespace Recepten.Controllers
{
    public class Recept
    {
        public Recept() { }

        internal Recept(int gerechtIndex, Context context)
        {
            this.Gerecht = context.Gerechten.First(g => g.GerechtID == gerechtIndex);
            this.Hoeveelheden = new();
            this.Categorieen = new();
            Hoeveelheden.AddRange(
                context.Hoeveelheden
                    .Include(h => h.Ingredient)
                    .Include(h => h.Eenheid)
                    .Where(h => h.GerechtID == gerechtIndex));
            Categorieen.AddRange(
                context.GerechtCategorieCombinaties
                    .Where(combi => combi.GerechtID == gerechtIndex)
                    .Join(context.Categorieen, combi => combi.CategorieID, cat => cat.CategorieID, (combi, cat) => cat));
        }

        [Required]
        public Gerecht Gerecht { get; set; }

        [Required]
        public List<Categorie> Categorieen { get; set; }

        [Required]
        public List<Hoeveelheid> Hoeveelheden { get; set; }

        /// <summary>
        /// Save the recipe to the database.
        /// </summary>
        /// <remarks>
        /// The trick is to do this with the least amount of actual writes
        /// to the underlying database.
        /// Another less obvious challenge is to make sure that we don't duplicate
        /// entries in the database. When we get the data from the frontend, we don't
        /// always have the database ID's. We must make sure that we don't duplicate
        /// ID's but also keep the amount of entries in the database to a minimum.
        /// When deleting Hoeveelheden it can be that not all Eenheden en Ingredienten
        /// are still used. For now there is no check on that. On the other hand it is
        /// also not a problem as these entities may be used by new recipes.
        /// </remarks>
        /// <param name="context"></param>
        /// <returns></returns>
        internal string SaveToDB(Context context)
        {
            Gerecht.SaveToContext(context);

            // Make sure that if a Hoeveelheid has an Eenheid or Ingredient
            // with the same ID as another Hoeveelheid, they also point to the same
            // Eenheid or Ingredient entity instance. Otherwise we have two entities with
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

            var huidigeCategorieen = context.GerechtCategorieCombinaties
                .Where(c => c.GerechtID == Gerecht.GerechtID)
                .ToList();

            foreach (var categorie in Categorieen)
            {
                var gcc = huidigeCategorieen.FirstOrDefault(g => g.CategorieID == categorie.CategorieID);
                if (null == gcc)
                {
                    new GerechtCategorieCombinatie(){ CategorieID = categorie.CategorieID, Gerecht = this.Gerecht }.SaveToContext(context);
                }
            }

            foreach (var gcc in huidigeCategorieen)
            {
                if (!Categorieen.Any(cat => cat.CategorieID == gcc.CategorieID))
                {
                    context.Remove(gcc);
                }
            }

            foreach (var hoeveelheid in Hoeveelheden)
            {
                hoeveelheid.Gerecht = Gerecht;

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

                hoeveelheid.Ingredient.SaveToContext(context);
                hoeveelheid.Eenheid.SaveToContext(context);
                hoeveelheid.SaveToContext(context);
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
        [Required]
        public string MailAddress { get; set; }
        public int GerechtIndex { get; set; }
    }

    [Route("api/[controller]")]
    public class DataController : BaseController
    {
        private readonly IMyEmailSender emailSender;
        private readonly IContactsServer contactsServer;
        private readonly IWebHostEnvironment environment;
        private readonly IConfiguration configuration;

        public DataController(
            ILogger<DataController> logger,
            Context context,
            UserManager<ApplicationUser> userManager,
            AuthenticationService authenticationManager,
            IMyEmailSender emailSender,
            IContactsServer contactsServer,
            IWebHostEnvironment environment,
            IConfiguration configuration)
            : base(logger, context, userManager, authenticationManager)
        {
            this.emailSender = emailSender;
            this.contactsServer = contactsServer;
            this.environment = environment;
            this.configuration = configuration;
        }

        [Authorize(AuthenticationSchemes=JwtBearerDefaults.AuthenticationScheme, Roles=ApplicationRole.AdminRole)]
        [HttpPost("[action]")]
        public async Task<JsonResult> MailRecept([FromBody] MailRecipeData mrd)
        {
            var recept = new Recept(mrd.GerechtIndex, this.context);

            var ingredienten = new StringBuilder();
            recept.Hoeveelheden.ForEach(h => {
                if (h.Eenheid.Naam != "stuks")
                {
                    ingredienten.AppendLine($"{h.Aantal} {h.Eenheid.Naam} {h.Ingredient.Naam}<br/>");
                }
                else
                {
                    ingredienten.AppendLine($"{h.Aantal} {h.Ingredient.Naam}<br/>");
                }
            });

            string fileName = Path.Combine(environment.ContentRootPath, "MailTemplates", "recipe.html");
            await this.emailSender.SendEmailAsync(
                mrd.MailAddress,
                "Recept",
                System.IO.File.ReadAllText(fileName)
                .Replace("<TITEL>", recept.Gerecht.Naam)
                .Replace("<INGREDIENTEN>", ingredienten.ToString())
                .Replace("<BEREIDINGSDUUR>", recept.Gerecht.Minuten.ToString())
                .Replace("<BEREIDINGSWIJZE>", recept.Gerecht.Omschrijving)
            );

            return ResultOK();
        }

        [HttpGet("[action]")]
        public JsonResult Recepten()
        {
            // First we get the recipes we need to return.
            var gerechten = context.Gerechten.OrderBy(g => g.Naam).ToList();

            // Then we add the categorieen ID' to them.
            var temp = gerechten.GroupJoin(context.GerechtCategorieCombinaties,
                gerecht => gerecht.GerechtID,
                combi => combi.GerechtID,
                (gerecht, combi) => new { name = gerecht.Naam, index = gerecht.GerechtID, categorieen = combi.Select(c => c.CategorieID).ToList() })
            .ToList();

            // Then we add the whole categorie to the records.
            var result = temp.Select(r => new
            {
                name = r.name,
                index = r.index,
                categorieen = r.categorieen
                    .Join(context.Categorieen, c => c, cat => cat.CategorieID, (c, cat) => cat )
            });

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
            var result = this.context.Gerechten
            .Where(g => gerechten.Contains(g.GerechtID))
            .OrderBy(g => g.Naam)
            .Select(g => new {
                    index = g.GerechtID,
                    name = g.Naam
            })
            .ToList();

            return Json(result);
        }

        [Authorize(AuthenticationSchemes=JwtBearerDefaults.AuthenticationScheme, Roles=ApplicationRole.EditorRole)]
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

        [Authorize(AuthenticationSchemes=JwtBearerDefaults.AuthenticationScheme, Roles=ApplicationRole.EditorRole)]
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
            return Json(this.context.Categorieen.ToList());
        }

        [Authorize(AuthenticationSchemes=JwtBearerDefaults.AuthenticationScheme, Roles=ApplicationRole.AdminRole)]
        [HttpPost("[action]")]
        public JsonResult Categorieen([FromBody] List<Categorie> model)
        {
            var existingCategories = this.context.Categorieen.ToList();
            var newCategories = new List<Categorie>();
            var foundDoubleName = string.Empty;
            model.ForEach(c =>
            {
                Categorie cat;
                if (0 != c.CategorieID)
                {
                    // Check if we have a categorie with the same ID...
                    cat = existingCategories.FirstOrDefault(ec => ec.CategorieID == c.CategorieID);
                }
                else
                {
                    // Check if we have a categorie with the same name...
                    cat = existingCategories.FirstOrDefault(ec => ec.Naam == c.Naam);
                }
                if (null != cat)
                {
                    if (newCategories.Any(newC => newC.Naam == c.Naam))
                    {
                        // We have two categories with the same name. That is not going to work.
                        foundDoubleName = c.Naam;
                    }

                    // ...In that case we change the name and icon for that categorie...
                    cat.IconIndex = c.IconIndex;
                    cat.Naam = c.Naam;
                    newCategories.Add(cat);
                }
                else
                {
                    // ...Otherwise we create a new categorie.
                    newCategories.Add(new Categorie()
                    {
                        CategorieID = 0,
                        Naam = c.Naam,
                        IconIndex = c.IconIndex
                    });
                }
            });

            if (!string.IsNullOrEmpty(foundDoubleName))
            {
                // No need to continue, we are not going to update the database.
                return ResultNOK($"De naam {foundDoubleName} kwam twee keer voor in de lijst");
            }

            // newCategorie now contains all categories the user wanted.
            // But we need to check if there are catgories we should delete
            // and check if they are not in use. In that case we skip deleting them.
            var categoriesToDelete = new List<Categorie>();
            var couldNotDeleteAllCategories = false;
            existingCategories.ForEach(ec =>
            {
                // If the categorie cannot be deleted we add it to the newCategories list.
                if (!newCategories.Contains(ec))
                {
                    // Check if we can delete this categorie.
                    if (this.context.GerechtCategorieCombinaties.Any(combi => combi.CategorieID == ec.CategorieID))
                    {
                        newCategories.Add(ec);
                        couldNotDeleteAllCategories = true;
                    }
                    else
                    {
                        categoriesToDelete.Add(ec);
                    }
                }
            });
            newCategories.ForEach(c => c.SaveToContext(this.context));
            categoriesToDelete.ForEach(c => this.context.Remove(c));
            this.context.SaveChanges();

            if (couldNotDeleteAllCategories)
            {
                return ResultNOK("Niet alle categorieën konden worden verwijdert.");
            }
            else
            {
                return ResultOK();
            }
        }

        [Authorize(AuthenticationSchemes=JwtBearerDefaults.AuthenticationScheme, Roles=ApplicationRole.AdminRole)]
        [HttpGet("[action]")]
        public async Task<JsonResult> EmailAddresses()
        {
            return Json(await contactsServer.GetAllEmailAdressesAsync());
        }
    }
}
