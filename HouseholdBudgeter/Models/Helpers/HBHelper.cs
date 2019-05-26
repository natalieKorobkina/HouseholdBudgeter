using HouseholdBudgeter.Models.Domain;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace HouseholdBudgeter.Models.Helpers
{
    public class HBHelper
    {
        private ApplicationDbContext DbContext = new ApplicationDbContext();

        public HBHelper(ApplicationDbContext context)
        {
            DbContext = context;
        }

        public ApplicationUser GetHouseholdOwner(Household household)
        {
            return DbContext.Users.FirstOrDefault(u => u.Id == household.OwnerId);
        }

        public string GetCurrentUserId()
        {
            return HttpContext.Current.User.Identity.GetUserId();
        }

        public ApplicationUser GetCurrentUser()
        {
            var currentUserId = GetCurrentUserId();

            return DbContext.Users.FirstOrDefault(u => u.Id == currentUserId);
        }

        public ApplicationUser GetUserByEmail(string email)
        {
            return DbContext.Users.FirstOrDefault(u => u.Email == email);
        }

        public Household GetHouseholdById(int id)
        {
            return DbContext.Households.FirstOrDefault(h => h.Id == id);
        }

        public Invitation GetInvitationByUserAndHousehold(string id, int householdId)
        {
            return DbContext.Invitations
                .FirstOrDefault(i => i.IsInvitedId == id && i.HouseholdToJoinId == householdId);
        }

        public Category GetCategoryById(int id)
        {
            return DbContext.Categories.FirstOrDefault(c => c.Id == id);
        }

        public BankAccount GetBankAccountById(int id)
        {
            return DbContext.BankAccounts.FirstOrDefault(c => c.Id == id);
        }

        public Transaction GetTransactionById(int id)
        {
            return DbContext.Transactions.FirstOrDefault(c => c.Id == id);
        }

        public IQueryable<Transaction> GetTransactionOfAccount(int id)
        {
            return DbContext.Transactions.Where(t => t.BankAccountId == id);
        }

        public IQueryable<Transaction> GetTransactionWithCategory(int id)
        {
            return DbContext.Transactions.Where(t => t.CategoryId == id);
        }

        public List <Category> GetCategoriesOfHousehold(int id)
        {
            return DbContext.Categories.Where(c => c.CategoryHouseholdId == id).ToList();
        }

        public decimal CalculateBankAccountBalance(int id)
        {
            var transactions = GetTransactionOfAccount(id).Where(t => t.Voided == false).ToList();
            var balance = (transactions.Count() != 0) ? transactions.Select(t => t.Ammount).ToList().Sum() : 0;

            return balance;
        }

        public bool currentIsParticipant(Household household)
        {
            if (household.Participants.Where(p => p.Id == GetCurrentUserId()).Any())
                return true;

            return false;
        }

        public bool isParticipant(Household household, string userId)
        {
            if (household.Participants.Where(p => p.Id == userId).Any())
                return true;

            return false;
        }

        public bool CheckIfOwner(string householdOwnerId)
        {
            if (GetCurrentUserId() != householdOwnerId)
            {
                return false;
            }

            return true;
        }
    }
}