using System;
using System.Linq;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Recepten.Models.DB
{
    public class Context : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
    {
        ILogger<Context> logger;

        public Context(DbContextOptions<Context> options, ILogger<Context> logger)
            : base(options)
        {
            this.logger = logger;
            Database.EnsureCreated();
        }

        internal DbSet<Categorie> Categorieen { get; set; }
        internal DbSet<Gerecht> Gerechten { get; set; }
        internal DbSet<Hoeveelheid> Hoeveelheden { get; set; }
        internal DbSet<Eenheid> Eenheden { get; set; }
        internal DbSet<Ingredient> Ingredienten { get; set; }

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
                b.HasOne(x => x.Categorie).WithMany().HasForeignKey(x => x.CategorieID);
            });
            modelBuilder.Entity<Hoeveelheid>(b => {
                b.ToTable("hoeveelheid");
                b.HasKey(x => new { x.HoeveelheidID });
                b.HasOne<Gerecht>().WithMany().HasForeignKey(x => x.GerechtID);
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