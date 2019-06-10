using HouseholdBudgeter.Models.BindingModels;
using HouseholdBudgeter.Models.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;

namespace HouseholdBudgeter.Models.Filters
{
    public class TransactionCheckParticipant : System.Web.Http.Filters.ActionFilterAttribute
    {
        public ApplicationDbContext DbContext = new ApplicationDbContext();
        public virtual IDictionary<string, object> ActionArguments { get; }
        private HBHelper hBHelper;

        public TransactionCheckParticipant()
        {
            hBHelper = new HBHelper(DbContext);
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var modelState = actionContext.ModelState;
            if (!modelState.IsValid)
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, actionContext.ModelState);

            var actionParamentrBAccount = actionContext.ActionArguments.SingleOrDefault(p => p.Key == "id").Value;
            var bankAccountId = 0;
            var userId = hBHelper.GetCurrentUserId();
            if (actionParamentrBAccount != null && Int32.TryParse(actionParamentrBAccount.ToString(), out bankAccountId))
            {
                var bankAccount = DbContext
                    .BankAccounts
                    .FirstOrDefault(p => p.Id == bankAccountId &&
                    p.Household.Participants.Where(m => m.Id == userId).Any());

                if (bankAccount == null)
                    actionContext.Response = actionContext.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest, "Bank account doens't exist or your have no right to perfom operation");
                else
                {
                    var actionModel = actionContext.ActionArguments.SingleOrDefault(p => p.Key == "bindingModel").Value;
                    if (actionModel != null)
                    {
                        TransactionBindingModel model = (TransactionBindingModel)actionContext.ActionArguments["bindingModel"];
                        if (model != null)
                        {
                            var category = DbContext.Categories.FirstOrDefault(p => p.Id == model.CategoryId &&
                        p.CategoryHousehold.Participants.Where(m => m.Id == userId).Any());

                            if (category == null)
                                actionContext.Response = actionContext.Request.CreateErrorResponse(
                            HttpStatusCode.BadRequest, "Category doens't exist or your have no right to perform operation");
                        }
                    }
                }
            }
        }
    }
}