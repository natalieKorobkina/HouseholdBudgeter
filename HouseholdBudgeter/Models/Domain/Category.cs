using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models.Domain
{
    public class Category
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }

        public virtual Household CategoryHousehold { get; set; }
        public int CategoryHouseholdId { get; set; }

        public virtual List<Transaction> Transactions  { get; set; }

        public Category()
        {
            Transactions = new List<Transaction>();
        }
    }
}