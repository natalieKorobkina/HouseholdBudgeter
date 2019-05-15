using AutoMapper;
using HouseholdBudgeter.Models;
using HouseholdBudgeter.Models.BindingModels;
using HouseholdBudgeter.Models.Domain;
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

        public CategoryController()
        {
            DbContext = new ApplicationDbContext();
        }

        public IHttpActionResult PostCategory(CategoryBindingModel bindingModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = User.Identity.GetUserId();
            var owner = DbContext.Households.FirstOrDefault(h => h.Id == bindingModel.CategoryHouseholdId && h.OwnerId == userId);

            if (owner == null)
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
            {
                return BadRequest(ModelState);
            }

            var existingHousehold = DbContext.Households.FirstOrDefault(h => h.Id == bindingModel.CategoryHouseholdId);
            if (existingHousehold == null) 
                return BadRequest(ModelState);

            var existingCategory= DbContext.Categories.FirstOrDefault(c => c.Id == bindingModel.Id);
            if (existingCategory == null)
                return BadRequest(ModelState);

            var userId = User.Identity.GetUserId();
            if (userId != existingHousehold.OwnerId)
                return BadRequest();

            Mapper.Map(bindingModel, existingCategory);
            existingCategory.Updated = DateTime.Now;
            DbContext.SaveChanges();
            
            var categoryModel = Mapper.Map<CategoryViewModel>(existingCategory);

            return Ok(categoryModel);
        }

        public IHttpActionResult Deletecategory(int categoryId)
        {
            var category = DbContext.Categories.FirstOrDefault(p => p.Id == categoryId);

            if (category == null)
                return NotFound();

            var household = DbContext.Households.FirstOrDefault(h => h.Id == category.CategoryHouseholdId);
            if (household == null)
                return NotFound();

            var currentUserId = User.Identity.GetUserId();
            var isOwner = household.OwnerId == currentUserId;
            if (!isOwner)
                return BadRequest("Just owners can delete categories in their households");

            DbContext.Categories.Remove(category);
            DbContext.SaveChanges();

            return Ok();
        }

        public IHttpActionResult GetCategories(int householdId)
        {
            var household = DbContext.Households.FirstOrDefault(h => h.Id == householdId);

            if (household == null)
            {
                return NotFound();
            }

            var currentUserId = User.Identity.GetUserId();
            var isParticipant = household.Participants.Any(p =>p.Id == currentUserId) || (household.OwnerId == currentUserId);
            if (!isParticipant)
                return BadRequest("Just participants can see categories of the household");

            var categories = DbContext.Categories.Where(c => c.CategoryHouseholdId == household.Id)
                .Select(c => new CategoryViewModel()
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                CategoryHouseholdId = c.CategoryHouseholdId

            }).ToList();

            if (categories.Count() == 0)
            {
                return NotFound();
            }

            return Ok(categories);
        }

    }
}
