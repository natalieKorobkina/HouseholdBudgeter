using AutoMapper;
using AutoMapper.QueryableExtensions;
using HouseholdBudgeter.Models;
using HouseholdBudgeter.Models.BindingModels;
using HouseholdBudgeter.Models.Domain;
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
    [RoutePrefix("api/category")]
    [Authorize]
    public class CategoryController : ApiController
    {
        private ApplicationDbContext DbContext;
        private HBHelper hBHelper;


        public CategoryController()
        {
            DbContext = new ApplicationDbContext();
            hBHelper = new HBHelper(DbContext);
        }

        public IHttpActionResult PostCategory(CategoryBindingModel bindingModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var household = hBHelper.GetHouseholdById(bindingModel.CategoryHouseholdId);
            if (household == null)
                return NotFound();

            if (!hBHelper.CheckIfOwner(household.OwnerId))
                return BadRequest("Just owners can create categories");

            var category = Mapper.Map<Category>(bindingModel);
            category.Created = DateTime.Now;

            DbContext.Categories.Add(category);
            DbContext.SaveChanges();

            var url = Url.Link("DefaultApi", new { Controller = "Category", Id = category.Id });
            var categoryModel = Mapper.Map<CategoryViewModel>(category);

            return Created(url, categoryModel);
        }

        public IHttpActionResult PutCategory(CategoryBindingModel bindingModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var household = hBHelper.GetHouseholdById(bindingModel.CategoryHouseholdId);
            if (household == null)
                return NotFound();

            if (!hBHelper.CheckIfOwner(household.OwnerId))
                return BadRequest("Just owners can edit categories");

            var category= hBHelper.GetCategoryById(bindingModel.Id);
            if (category == null)
                return BadRequest(ModelState);

            Mapper.Map(bindingModel, category);
            category.Updated = DateTime.Now;
            DbContext.SaveChanges();
            
            var categoryModel = Mapper.Map<CategoryViewModel>(category);

            return Ok(categoryModel);
        }

        public IHttpActionResult DeleteCategory(int categoryId)
        {
            var category = hBHelper.GetCategoryById(categoryId);
            if (category == null)
                return NotFound();

            var household = hBHelper.GetHouseholdById(category.CategoryHouseholdId);
            if (household == null)
                return NotFound();

            if (!hBHelper.CheckIfOwner(household.OwnerId))
                return BadRequest("Just owners can delete categories in their households");

            DbContext.Categories.Remove(category);
            DbContext.SaveChanges();

            return Ok();
        }

        public IHttpActionResult GetCategories(int householdId)
        {
            var household = hBHelper.GetHouseholdById(householdId);
            if (household == null)
                return NotFound();

            if (!hBHelper.currentIsParticipant(household))
                return BadRequest("Just participants can see categories of the household");

            var categories = DbContext.Categories.Where(c => c.CategoryHouseholdId == household.Id)
                .ProjectTo<CategoryViewModel>().ToList();

            if (categories.Count() == 0)
                return NotFound();

            return Ok(categories);
        }
    }
}
