using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace HouseholdBudgeter.Models.Domain
{
    public class Household
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; } 

        public virtual ApplicationUser Owner { get; set; }
        public string OwnerId { get; set; }

        public virtual List<ApplicationUser> Participants { get; set; }
        public virtual List<Category> HouseholdCategories { get; set; }
        public virtual List<Invitation> HouseholdInvitation { get; set; }

        public Household()
        {
            HouseholdCategories = new List<Category>();
            Participants = new List<ApplicationUser>();
            HouseholdInvitation = new List<Invitation>();
        }
    }
}