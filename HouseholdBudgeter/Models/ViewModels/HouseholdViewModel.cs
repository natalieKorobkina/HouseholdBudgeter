using HouseholdBudgeter.Models.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models.ViewModels
{
    public class HouseholdViewModel
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        //public string OwnerId { get; set; }

        //public List<ApplicationUser> Participants { get; set; }
        //public List<Category> HouseholdCategories { get; set; }
    }
}