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
    public class HouseholdCheckOwner : System.Web.Http.Filters.ActionFilterAttribute
    {
        public ApplicationDbContext DbContext = new ApplicationDbContext();
        public virtual IDictionary<string, object> ActionArguments { get; }
        private HBHelper hBHelper;

        public HouseholdCheckOwner()
        {
            hBHelper = new HBHelper(DbContext);
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var modelState = actionContext.ModelState;
            if (!modelState.IsValid)
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, actionContext.ModelState);

            var actionParamentr = actionContext.ActionArguments.SingleOrDefault(p => p.Key == "id").Value;
            var householdId = 0;
            if (actionParamentr != null && Int32.TryParse(actionParamentr.ToString(), out householdId))
            {
                var household = hBHelper.GetHouseholdById(householdId);
                if (household == null)
                    actionContext.Response = actionContext.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest, "Household doens't exist");

                else if (!hBHelper.CheckIfOwner(household.OwnerId))
                    actionContext.Response = actionContext.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest, "You aren't an owner of this household");
            }
        }
    }
}