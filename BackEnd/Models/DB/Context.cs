using System;
using System.Linq;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Recepten.Models.DB
{
    /// <summary>
    /// Database context for the application.
    /// </summary>
    public class Context : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        private ILogger<Context> logger;

        public Context(DbContextOptions<Context> options, ILogger<Context> logger)
            : base(options)
        {
            this.logger = logger;
            // This will use migrations to initialy create or update the database.
            RelationalDatabaseFacadeExtensions.Migrate(Database);
        }

        public DbSet<Categorie> Categorieen { get; set; }
        public DbSet<Gerecht> Gerechten { get; set; }
        public DbSet<GerechtCategorieCombinatie> GerechtCategorieCombinaties { get; set; }
        public DbSet<Hoeveelheid> Hoeveelheden { get; set; }
        public DbSet<Eenheid> Eenheden { get; set; }
        public DbSet<Ingredient> Ingredienten { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Categorie>(b => {
                b.ToTable("categorie");
                b.HasKey(x => new { x.CategorieID });
            });
            modelBuilder.Entity<Gerecht>(b => {
                b.ToTable("gerecht");
                b.HasKey(x => new { x.GerechtID });
            });
            modelBuilder.Entity<GerechtCategorieCombinatie>(b => {
                b.ToTable("gerechtcategoriecombinatie");
                b.HasKey(x => new { x.GerechtCategorieCombinatieID });
                b.HasOne<Gerecht>().WithMany().HasForeignKey(x => x.GerechtID);
                b.HasOne<Categorie>().WithMany().HasForeignKey(x => x.CategorieID);
            });
            modelBuilder.Entity<Hoeveelheid>(b => {
                b.ToTable("hoeveelheid");
                b.HasKey(x => new { x.HoeveelheidID });
                b.HasOne(x => x.Gerecht).WithMany().HasForeignKey(x => x.GerechtID);
                b.HasOne(x => x.Ingredient).WithMany().HasForeignKey(x => x.IngredientID);
                b.HasOne(x => x.Eenheid).WithMany().HasForeignKey(x => x.EenheidID);
            });
            modelBuilder.Entity<Eenheid>(b => {
                b.ToTable("eenheid");
                b.HasKey(x => new { x.EenheidID });
            });
            modelBuilder.Entity<Ingredient>(b => {
                b.ToTable("ingredient");
                b.HasKey(x => new { x.IngredientID });
            });
 
            foreach (var relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }
}