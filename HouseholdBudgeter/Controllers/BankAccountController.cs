using AutoMapper;
using AutoMapper.QueryableExtensions;
using HouseholdBudgeter.Models;
using HouseholdBudgeter.Models.BindingModels;
using HouseholdBudgeter.Models.Domain;
using HouseholdBudgeter.Models.Filters;
using HouseholdBudgeter.Models.Helpers;
using HouseholdBudgeter.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HouseholdBudgeter.Controllers
{
    [Authorize]
    public class BankAccountController : ApiController
    {
        private ApplicationDbContext DbContext;
        private HBHelper hBHelper;

        public BankAccountController()
        {
            DbContext = new ApplicationDbContext();
            hBHelper = new HBHelper(DbContext);
        }

        [HouseholdCheckOwner]
        public IHttpActionResult PostBankAccount(int id, BankAccountBindingModel bindingModel)
        {
            var bankAccount = Mapper.Map<BankAccount>(bindingModel);

            bankAccount.HouseholdId = id;
            bankAccount.Balance = 0;

            DbContext.BankAccounts.Add(bankAccount);
            DbContext.SaveChanges();

            var url = Url.Link("DefaultApi", new { Controller = "BankAccount", Id = bankAccount.Id });
            var bankAccountModel = Mapper.Map<BankAccountViewModel>(bankAccount);

            return Created(url, bankAccountModel);
        }

        [BankAccountCheckOwner]
        public IHttpActionResult PutBankAccount(int id, BankAccountBindingModel bindingModel)
        {
            var bankAccount = hBHelper.GetBankAccountById(id);
            
            Mapper.Map(bindingModel, bankAccount);
            bankAccount.Updated = DateTime.Now;
            DbContext.SaveChanges();

            var bankAccountModel = Mapper.Map<BankAccountViewModel>(bankAccount);

            return Ok(bankAccountModel);
        }

        [BankAccountCheckOwner]
        public IHttpActionResult DeleteBankAccount(int id)
        {
            DbContext.BankAccounts.Remove(hBHelper.GetBankAccountById(id));
            DbContext.SaveChanges();

            return Ok();
        }

        [HouseholdCheckParticipant]
        public IHttpActionResult GetBankAccounts(int id)
        {
            var bankAccounts = DbContext.BankAccounts.Where(b => b.HouseholdId == id)
                .ProjectTo<BankAccountViewModel>().ToList();

            if (bankAccounts.Count() == 0)
                return NotFound();

            return Ok(bankAccounts);
        }

        [BankAccountCheckOwner]
        public IHttpActionResult GetBalance(int id)
        {
            var bankAccount = hBHelper.GetBankAccountById(id);
            bankAccount.Balance = hBHelper.CalculateBankAccountBalance(id);
               
            return Ok(bankAccount);
        }
    }
}
