using AutoMapper;
using AutoMapper.QueryableExtensions;
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
            var transaction = hBHelper.GetTransactionById(id);
            
            Mapper.Map(bindingModel, transaction);
            transaction.Updated = DateTime.Now;

            hBHelper.GetBankAccountById(transaction.BankAccountId).Balance += bindingModel.Ammount;
            DbContext.SaveChanges();
            
            var transactionModel = Mapper.Map<TransactionViewModel>(transaction);

            return Ok(transactionModel);
        }

        [TransactionCheckOwner]
        public IHttpActionResult PostVoidTransaction(int id)
        {
            var transaction = hBHelper.GetTransactionById(id);
            
            transaction.Voided = true;
            hBHelper.GetBankAccountById(transaction.BankAccountId).Balance -= transaction.Ammount;
            DbContext.SaveChanges();

            return Ok(transaction);
        }

        [TransactionCheckOwner]
        public IHttpActionResult DeleteTransaction(int id)
        {
            var transaction = hBHelper.GetTransactionById(id);
            
            DbContext.Transactions.Remove(transaction);
            hBHelper.GetBankAccountById(transaction.BankAccountId).Balance -= transaction.Ammount;
            DbContext.SaveChanges();

            return Ok();
        }

        [TransactionCheckParticipant]
        public IHttpActionResult GetTransaction(int id)
        {
            var transactions = hBHelper.GetTransactionOfAccount(id)
                .ProjectTo<TransactionsViewModel>().ToList();

            if (transactions.Count() == 0)
                return BadRequest("There are no transactions in this bank account");

            return Ok(transactions);
        }
    }
}
