using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models.ViewModels
{
    public class TransactionsViewModel
    {
        public int BankAccountId { get; set; }
        public string BankAccountName { get; set; }

        public List<TransactionViewModel> Transactions { get; set; }

        public TransactionsViewModel()
        {
            Transactions = new List<TransactionViewModel>();
        }
    }
}