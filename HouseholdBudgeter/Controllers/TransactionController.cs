using AutoMapper;
using AutoMapper.QueryableExtensions;
using HouseholdBudgeter.App_Start;
using HouseholdBudgeter.Models;
using HouseholdBudgeter.Models.BindingModels;
using HouseholdBudgeter.Models.Domain;
using HouseholdBudgeter.Models.Filters;
using HouseholdBudgeter.Models.Helpers;
using HouseholdBudgeter.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace HouseholdBudgeter.Controllers
{
    [Authorize]
    public class TransactionController : ApiController
    {
        private ApplicationDbContext DbContext;
        private HBHelper hBHelper;

        public TransactionController()
        {
            DbContext = new ApplicationDbContext();
            hBHelper = new HBHelper(DbContext);
        }

        [TransactionCheckParticipant]
        public IHttpActionResult PostTransaction(int id, TransactionBindingModel bindingModel)
        {
            if (bindingModel == null)
                return BadRequest("Provide required parameters");

            var transaction = Mapper.Map<Transaction>(bindingModel);
            transaction.OwnerId = User.Identity.GetUserId();
            transaction.BankAccountId = id;

            DbContext.Transactions.Add(transaction);
            hBHelper.GetBankAccountById(id).Balance += transaction.Ammount;
            DbContext.SaveChanges();

            var url = Url.Link("DefaultApi", new { Controller = "Transaction", Id = transaction.Id });
            var transactionModel = Mapper.Map<TransactionViewModel>(transaction);

            return Created(url, transactionModel);
        }

        [TransactionCheckOwner]
        public IHttpActionResult PutTransaction(int id, TransactionBindingModel bindingModel)
        {
            if (bindingModel == null)
                return BadRequest("Provide required parameters");

            var transaction = hBHelper.GetTransactionById(id);
            var prevAmount = transaction.Ammount;

            Mapper.Map(bindingModel, transaction);
            transaction.Updated = DateTime.Now;

            var bankAccount = hBHelper.GetBankAccountById(transaction.BankAccountId);

            bankAccount.Balance = !transaction.Voided
                ? bankAccount.Balance - prevAmount + bindingModel.Ammount : bankAccount.Balance;
            DbContext.SaveChanges();

            var transactionModel = Mapper.Map<TransactionViewModel>(transaction);

            return Ok(transactionModel);
        }

        [TransactionCheckOwner]
        public IHttpActionResult PostVoidTransaction(int id)
        {
            var transaction = hBHelper.GetTransactionById(id);

            if (transaction.Voided == false)
            {
                transaction.Voided = true;
                hBHelper.GetBankAccountById(transaction.BankAccountId).Balance -= transaction.Ammount;
                transaction.Updated = DateTime.Now;
                DbContext.SaveChanges();
            }

            var transactionModel = Mapper.Map<TransactionViewModel>(transaction);

            return Ok(transactionModel);
        }

        [TransactionCheckOwner]
        public IHttpActionResult DeleteTransaction(int id)
        {
            var transaction = hBHelper.GetTransactionById(id);

            DbContext.Transactions.Remove(transaction);

            if (!transaction.Voided)
                hBHelper.GetBankAccountById(transaction.BankAccountId).Balance -= transaction.Ammount;

            DbContext.SaveChanges();

            return Ok();
        }

        [TransactionCheckParticipant]
        public IHttpActionResult GetTransactions(int id)
        {
            var transactions = hBHelper.GetTransactionOfAccount(id)
                .ProjectTo<TransactionViewModel>(new { currentUserId = User.Identity.GetUserId() }).ToList();

            var transactionModel = new TransactionsViewModel
            {
                Transactions = transactions,
                BankAccountName = hBHelper.GetBankAccountById(id).Name,
                HouseholdId = hBHelper.GetBankAccountById(id).HouseholdId
            };

            return Ok(transactionModel);
        }

        [TransactionCheckOwner]
        public IHttpActionResult GetTransaction(int id)
        {
            var transaction = hBHelper.GetTransactionById(id);

            if (transaction == null)
                return BadRequest("Transaction doesn't exist");

            var transactionModel = Mapper.Map<TransactionViewModel>(transaction);

            return Ok(transactionModel);
        }
    }
}
