using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using HouseholdBudgeter.Models.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;

namespace HouseholdBudgeter.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit https://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        [InverseProperty(nameof(Household.Owner))]
        public virtual List<Household> OwnersHouseholds { get; set; }

        [InverseProperty(nameof(Household.Participants))]
        public virtual List<Household> ParticipantsHouseholds { get; set; }

        [InverseProperty(nameof(Invitation.Inviter))]
        public virtual List<Invitation> Inviters { get; set; }

        [InverseProperty(nameof(Invitation.IsInvited))]
        public virtual List<Invitation> Invited { get; set; }

        public virtual List<Transaction> Creators { get; set; }

        public ApplicationUser()
        {
            OwnersHouseholds = new List<Household>();
            ParticipantsHouseholds = new List<Household>();
            Inviters = new List<Invitation>();
            Invited = new List<Invitation>();
            Creators = new List<Transaction>();
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Household> Households { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<BankAccount> BankAccounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Household>()
            .HasMany(h => h.HouseholdCategories)
            .WithRequired(h => h.CategoryHousehold)
            .WillCascadeOnDelete(false);

            modelBuilder.Entity<BankAccount>()
                .Property(p => p.Name).IsRequired().HasMaxLength(100).IsUnicode(false);
            modelBuilder.Entity<BankAccount>()
                .Property(p => p.Description).IsRequired().HasMaxLength(300).IsUnicode(false);

            modelBuilder.Entity<Category>()
                .Property(p => p.Name).IsRequired().HasMaxLength(100).IsUnicode(false);
            modelBuilder.Entity<Category>()
                .Property(p => p.Description).IsRequired().HasMaxLength(300).IsUnicode(false);

            modelBuilder.Entity<Transaction>()
                .Property(p => p.Title).IsRequired().HasMaxLength(100).IsUnicode(false);
            modelBuilder.Entity<Household>()
                .Property(p => p.Description).IsRequired().HasMaxLength(300).IsUnicode(false);
        }
    }
}