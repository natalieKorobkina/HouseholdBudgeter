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
    public class CategoryCheckOwner : System.Web.Http.Filters.ActionFilterAttribute
    {
        public ApplicationDbContext DbContext = new ApplicationDbContext();
        public virtual IDictionary<string, object> ActionArguments { get; }
        private HBHelper hBHelper;

        public CategoryCheckOwner()
        {
            hBHelper = new HBHelper(DbContext);
        }

        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var modelState = actionContext.ModelState;
            if (!modelState.IsValid)
                actionContext.Response = actionContext.Request.CreateErrorResponse(
                    HttpStatusCode.BadRequest, actionContext.ModelState);

            var actionParamentrCategory = actionContext.ActionArguments.SingleOrDefault(p => p.Key == "id").Value;
            var categoryId = 0;
            if (actionParamentrCategory != null && Int32.TryParse(actionParamentrCategory.ToString(), out categoryId))
            {
                var userId = hBHelper.GetCurrentUserId();
                var category = DbContext
                    .Categories
                    .FirstOrDefault(p => p.Id == categoryId &&
                    p.CategoryHousehold.OwnerId == userId);

                if (category == null)
                    actionContext.Response = actionContext.Request.CreateErrorResponse(
                        HttpStatusCode.BadRequest, "Category doens't exist or your have no right to perfom operation");
            }
        }
    }
}