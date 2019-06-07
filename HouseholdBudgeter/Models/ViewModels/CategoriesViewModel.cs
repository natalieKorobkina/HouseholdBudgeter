using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models.ViewModels
{
    public class CategoriesViewModel
    {
        public string HouseholdName { get; set; }
        public bool IsOwner {get;set;}

        public List<CategoryViewModel> Categories { get; set; }

        public CategoriesViewModel()
        {
            Categories = new List<CategoryViewModel>();
        }
    }

}