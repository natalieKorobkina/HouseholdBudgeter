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
    public class HouseHoldController : ApiController
    {
        private ApplicationDbContext DbContext;
        private HBHelper hBHelper;

        public HouseHoldController()
        {
            DbContext = new ApplicationDbContext();
            hBHelper = new HBHelper(DbContext);
        }

        public IHttpActionResult GetAll()
        {
            var userId = User.Identity.GetUserId();
            var households = DbContext.Households
                .Where(h => h.Participants.Where(p =>p.Id == userId).Any())
                .Select(p => new HouseholdInListViewModel
                {
                    Id = p.Id,
                    Created = p.Created,
                    Updated = p.Updated,
                    Name = p.Name,
                    Description = p.Description,
                    IsOwner = (p.OwnerId == userId)
                }).ToList();
           
            return Ok(households);
        }

        public IHttpActionResult GetInvitingHouseholds()
        {
            var userId = User.Identity.GetUserId();
            var households = DbContext.Households
                .Where(h => h.HouseholdInvitation.Where(p => p.IsInvitedId == userId).Any())
                .ProjectTo<HouseholdViewModel>()
                .ToList();

            return Ok(households);
        }

        [HouseholdCheckOwner]
        public IHttpActionResult GetHousehold(int id)
        {
            var household = hBHelper.GetHouseholdById(id);

            if (household == null)
                return BadRequest();

            var householdModel = Mapper.Map<HouseholdViewModel>(household);

            return Ok(householdModel);

        }

        [HouseholdCheckOwner]
        public IHttpActionResult PostHousehold(HouseholdBindingModel bindingModel)
        {
            if (bindingModel == null)
                return BadRequest("Provide required parameters");

            var household = Mapper.Map<Household>(bindingModel);
            var owner = hBHelper.GetCurrentUser();

            household.OwnerId = owner.Id;
            household.Participants.Add(owner);

            DbContext.Households.Add(household);
            DbContext.SaveChanges();

            var url = Url.Link("DefaultApi", new { Controller = "HouseHold", Id = household.Id });
            var householdModel = Mapper.Map<HouseholdViewModel>(household);

            return Created(url, householdModel);
        }

        [HouseholdCheckOwner]
        public IHttpActionResult PutHousehold(int id, HouseholdBindingModel bindingModel)
        {
            if (bindingModel == null)
                return BadRequest("Provide required parameters");

            var household = hBHelper.GetHouseholdById(id);
            
            Mapper.Map(bindingModel, household);
            household.Updated = DateTime.Now;
            DbContext.SaveChanges();

            var householdModel = Mapper.Map<HouseholdViewModel>(household);

            return Ok(householdModel);
        }

        [HouseholdCheckOwner]
        public IHttpActionResult Invite(int id, string email)
        {
            var invitingUser = hBHelper.GetUserByEmail(email);
            if (invitingUser == null)
                return BadRequest("Cannot invite unregistered user");

            if (hBHelper.isParticipant(hBHelper.GetHouseholdById(id), invitingUser.Id))
                return BadRequest("User is already participant");

            var invitation = CreateInvitation(invitingUser.Id, invitingUser.Email, id);
            var url = Url.Link("DefaultApi", new { Controller = "HouseHold", Id = invitation.Id });
            var invitationModel = new InvitationViewModel
            {
                Id = invitation.Id,
                Inviter = invitation.Inviter.UserName,
                IsInvited =invitation.IsInvited.UserName,
                HouseholdToJoin = invitation.HouseholdToJoin.Name
            };

            return Created(url, invitationModel);
        }

            private Invitation CreateInvitation(string userId, string email, int householdId)
        {
            var method = $"api/HouseHold/PostJoin";
            var householdName = hBHelper.GetHouseholdById(householdId).Name;

            EmailService emailService = new EmailService();

            emailService.Send(email, $"You are invited to participate in household '{householdName}' \n" +
                $"In order to join this household use {method} and provide householdId {householdId}.", "Invitation");

            var invitation = new Invitation();

            invitation.InviterId = User.Identity.GetUserId();
            invitation.IsInvitedId = userId;
            invitation.HouseholdToJoinId = householdId;

            DbContext.Invitations.Add(invitation);
            DbContext.SaveChanges();

            return invitation;
        }

        public IHttpActionResult PostJoin(int joiningHouseholdId)
        {
            var household = hBHelper.GetHouseholdById(joiningHouseholdId);
            if (household == null)
                return NotFound();

            var currentUser = hBHelper.GetCurrentUser();
            var invitation = hBHelper.GetInvitationByUserAndHousehold(currentUser.Id, household.Id);
            if (invitation == null)
                return BadRequest("Just invited users can join households");

            household.Participants.Add(currentUser);
            DbContext.Invitations.Remove(invitation);
            DbContext.SaveChanges();

            return Ok();
        }

        [HouseholdCheckParticipant]
        public IHttpActionResult PostLeave(int id)
        {
            var household = hBHelper.GetHouseholdById(id);
            if (hBHelper.CheckIfOwner(household.OwnerId))
                return BadRequest("Owner cannot leave their households");

            household.Participants.Remove(hBHelper.GetCurrentUser());
            DbContext.SaveChanges();

            return Ok();
        }

        [HouseholdCheckParticipant]
        public IHttpActionResult GetParticipants(int id)
        {
            var household = hBHelper.GetHouseholdById(id);
            var participants = household.Participants
                .Select(h => new ParticipantViewModel
                {
                    Id = h.Id,
                    UserName = h.UserName

                }).ToList();

            return Ok(participants);
        }

        [HouseholdCheckOwner]
        public IHttpActionResult DeleteHousehold(int id)
        {
            var categories = hBHelper.GetCategoriesOfHousehold(id);
            if (categories.Count() != 0)
                categories.ForEach(c => DbContext.Categories.Remove(c));

            DbContext.Households.Remove(hBHelper.GetHouseholdById(id));
            DbContext.SaveChanges();

            return Ok();
        }
    }
}
