using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models.ViewModels
{
    public class BankAccountsViewModel
    {
        public string HouseholdName { get; set; }
        public bool IsOwner { get; set; }

        public List<BankAccountViewModel> BankAccounts { get; set; }

        public BankAccountsViewModel()
        {
            BankAccounts = new List<BankAccountViewModel>();
        }
    }
}