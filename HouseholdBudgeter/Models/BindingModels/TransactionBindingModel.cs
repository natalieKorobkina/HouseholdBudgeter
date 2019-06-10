using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models.BindingModels
{
    public class TransactionBindingModel : IValidatableObject
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

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TransactionDate > DateTime.Now || TransactionDate < DateTime.Now.AddYears(-50))
            {
                yield return
                  new ValidationResult(errorMessage: "TransactionDate cannot be greater than today or less than 50 years ago",
                                       memberNames: new[] { "TransactionDate" });
            }
        }
    }
}