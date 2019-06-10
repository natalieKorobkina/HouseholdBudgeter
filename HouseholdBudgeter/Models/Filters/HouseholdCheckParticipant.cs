using HouseholdBudgeter.Models.Helpers;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;

namespace HouseholdBudgeter.Models.Filters
{
    public class HouseholdCheckParticipant : System.Web.Http.Filters.ActionFilterAttribute
    {
        public ApplicationDbContext DbContext = new ApplicationDbContext();
        public virtual IDictionary<string, object> ActionArguments { get; }
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var modelState = actionContext.ModelState;
            if (!modelState.IsValid)
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest, actionContext.ModelState);

            var actionParamentrHousehold = actionContext.ActionArguments.SingleOrDefault(p => p.Key == "id").Value;
            var householdId = 0;
            if (actionParamentrHousehold != null && Int32.TryParse(actionParamentrHousehold.ToString(), out householdId))
            {
                var userId = HttpContext.Current.User.Identity.GetUserId(); 
                var household = DbContext
               .Households
               .FirstOrDefault(p => p.Id == householdId &&
               (p.Participants.Any(t => t.Id == userId)));

                if (household == null)
                    actionContext.Response = actionContext.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest, "Household doens't exist or your have no right to perform operation");
            }
        }
    }
}