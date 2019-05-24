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
    public class HouseholdCheckParticipant : System.Web.Http.Filters.ActionFilterAttribute
    {
        public ApplicationDbContext DbContext = new ApplicationDbContext();
        public virtual IDictionary<string, object> ActionArguments { get; }
        private HBHelper hBHelper;

        public HouseholdCheckParticipant()
        {
            hBHelper = new HBHelper(DbContext);
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var modelState = actionContext.ModelState;
            if (!modelState.IsValid)
            {
                actionContext.Response = new HttpResponseMessage()
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Not a valid model")
                };
            }

            var actionParamentrHousehold = actionContext.ActionArguments.SingleOrDefault(p => p.Key == "id").Value;
            var householdId = 0;
            if (actionParamentrHousehold != null && Int32.TryParse(actionParamentrHousehold.ToString(), out householdId))
            {
                var userId = hBHelper.GetCurrentUserId();
                var household = DbContext
               .Households
               .FirstOrDefault(p => p.Id == householdId &&
               (p.Participants.Any(t => t.Id == userId)));

                if (household == null)
                    actionContext.Response = new HttpResponseMessage()
                    {
                        StatusCode = HttpStatusCode.NotFound,
                        Content = new StringContent("You have no right see that information")
                    };
            }
        }
    }
}