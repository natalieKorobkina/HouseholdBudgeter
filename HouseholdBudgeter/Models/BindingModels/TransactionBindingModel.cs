using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models.BindingModels
{
    public class TransactionBindingModel
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public decimal Ammount { get; set; }
        [Required]
        public DateTime TransactionDate { get; set; }
        [Required]
        public int CategoryId { get; set; }
    }
}