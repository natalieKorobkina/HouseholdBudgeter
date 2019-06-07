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
    public class CategoryController : ApiController
    {
        private ApplicationDbContext DbContext;
        private HBHelper hBHelper;

        public CategoryController()
        {
            DbContext = new ApplicationDbContext();
            hBHelper = new HBHelper(DbContext);
        }

        [HouseholdCheckOwner]
        public IHttpActionResult PostCategory(int id, CategoryBindingModel bindingModel)
        {
            if (bindingModel == null)
                return BadRequest("Provide required parameters");

            var category = Mapper.Map<Category>(bindingModel);
            category.CategoryHouseholdId = id;

            DbContext.Categories.Add(category);
            DbContext.SaveChanges();

            var url = Url.Link("DefaultApi", new { Controller = "Category", Id = category.Id });
            var categoryModel = Mapper.Map<CategoryViewModel>(category);

            return Created(url, categoryModel);
        }

        [CategoryCheckOwner]
        public IHttpActionResult PutCategory(int id, int householdId, CategoryBindingModel bindingModel)
        {
            if (bindingModel == null)
                return BadRequest("Provide required parameters");

            var category= hBHelper.GetCategoryById(id);
            
            Mapper.Map(bindingModel, category);
            category.Updated = DateTime.Now;
            category.CategoryHouseholdId = householdId;
            DbContext.SaveChanges();
            
            var categoryModel = Mapper.Map<CategoryViewModel>(category);

            return Ok(categoryModel);
        }

        [CategoryCheckOwner]
        public IHttpActionResult DeleteCategory(int id)
        {
            var category = hBHelper.GetCategoryById(id);
            var transactions = DbContext.Transactions.Where(t => t.CategoryId == id).ToList();
            if (transactions.Count() != 0)
                transactions.ForEach(t => hBHelper.GetBankAccountById(t.BankAccountId).Balance -= t.Ammount);

            DbContext.Categories.Remove(category);
            DbContext.SaveChanges();

            return Ok();
        }

        [HouseholdCheckParticipant]
        public IHttpActionResult GetCategories(int id)
        {
            var categories = DbContext.Categories.Where(c => c.CategoryHouseholdId == id)
                .ProjectTo<CategoryViewModel>().ToList();

            var categoriesModel = new CategoriesViewModel
            {
                Categories = categories,
                HouseholdName = hBHelper.GetHouseholdById(id).Name,
                IsOwner = hBHelper.GetHouseholdById(id).OwnerId == User.Identity.GetUserId()
            };

            return Ok(categoriesModel);
        }

        [BankAccountCheckParticipant]
        public IHttpActionResult GetCategoriesBA(int id)
        {
            var householdId = DbContext.BankAccounts.FirstOrDefault(b => b.Id == id).HouseholdId;
            var categories = DbContext.Categories.Where(c => c.CategoryHouseholdId == householdId)
                .ProjectTo<CategoryViewModel>().ToList();

            return Ok(categories);
        }

        [CategoryCheckOwner]
        public IHttpActionResult GetCategory(int id)
        {
            var category = hBHelper.GetCategoryById(id);

            if (category == null)
                return BadRequest();

            var categoryModel = Mapper.Map<CategoryViewModel>(category);

            return Ok(categoryModel);
        }
    }
}
