using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models.Domain
{
    public class Transaction
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public decimal Ammount { get; set; }
        public bool Voided { get; set; }

        public virtual ApplicationUser Owner { get; set; }
        public string OwnerId { get; set; }

        public virtual BankAccount BankAccount { get; set; }
        public int BankAccountId { get; set; }

        public virtual Category Category { get; set; }
        public int CategoryId { get; set; }

    }
}