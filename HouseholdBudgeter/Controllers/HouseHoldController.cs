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
    [RoutePrefix("api/household")]
    [Authorize]
    public class HouseHoldController : ApiController
    {
        private ApplicationDbContext DbContext;

        public HouseHoldController()
        {
            DbContext = new ApplicationDbContext();
        }

        public IHttpActionResult PostHousehold(HouseholdBindingModel bindingModel)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var household = Mapper.Map<Household>(bindingModel);

            household.Created = DateTime.Now;
            household.OwnerId = User.Identity.GetUserId();
            var owner = DbContext.Users.FirstOrDefault(u => u.Id == household.OwnerId);
            household.Participants.Add(owner);

            DbContext.Households.Add(household);
            DbContext.SaveChanges();

            var url = Url.Link("DefaultApi", new { Controller = "HouseHold", Id = household.Id });
            var householdModel = Mapper.Map<HouseholdViewModel>(household);

            return Created(url, householdModel);
        }

        // POST: api/Account/PutHousehold
        public IHttpActionResult PutHousehold(HouseholdBindingModel bindingModel)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            var existingHousehold = DbContext.Households.FirstOrDefault(h => h.Id == bindingModel.Id);
            if (existingHousehold == null)
                return NotFound();

            var userId = User.Identity.GetUserId();

            //Check if owner is trying to update information
            if (userId != existingHousehold.OwnerId)
            {
                return BadRequest("Just owners can update household' information");
            }

            Mapper.Map(bindingModel, existingHousehold);
            existingHousehold.Updated = DateTime.Now;
            DbContext.SaveChanges();

            var householdModel = Mapper.Map<HouseholdViewModel>(existingHousehold);

            return Ok(householdModel);
        }

        // POST: api/HouseHold/PostInvite
        public IHttpActionResult PostInvite(int householdId, string email)
        {
            var user = DbContext.Users.FirstOrDefault(u => u.Email == email);
            var currentUserId = User.Identity.GetUserId();
            var owner = DbContext.Households.FirstOrDefault(h => h.OwnerId == currentUserId);

            if (user == null)
                return BadRequest("You cannot invite unregistered user");

            if (owner == null)
                return BadRequest("Just owners can invite users");

            var method = $"api/HouseHold/PostJoin";
            var householdName = DbContext.Households.FirstOrDefault(h => h.Id == householdId).Name;

            EmailService emailService = new EmailService();

            emailService.Send(user.Email, $"You are invited to participate in household '{householdName}' \n" +
                $"In order to join this household use {method}.", "Invitation");

            var invitation = new Invitation();

            invitation.InviterId = User.Identity.GetUserId();
            invitation.IsInvitedId = user.Id;
            invitation.HouseholdToJoinId = householdId;

            DbContext.Invitations.Add(invitation);
            DbContext.SaveChanges();

            var url = Url.Link("DefaultApi", new { Controller = "HouseHold", Id = invitation.Id });
            var invitationModel = Mapper.Map<InvitationViewModel>(invitation);

            return Created(url, invitationModel);
        }

        // POST: api/HouseHold/Join
        public IHttpActionResult Join(int householdId)
        {
            var household = DbContext.Households.FirstOrDefault(p => p.Id == householdId);

            if (household == null)
                return NotFound();

            var currentUserId = User.Identity.GetUserId();
            var currentUser = DbContext.Users.FirstOrDefault(u => u.Id == currentUserId);
            var invitation = DbContext.Invitations
                .FirstOrDefault(i => i.IsInvitedId == currentUserId && i.HouseholdToJoinId == household.Id);

            if (invitation == null)
                return BadRequest("Just invited users can join households");

            household.Participants.Add(currentUser);
            DbContext.Invitations.Remove(invitation);
            DbContext.SaveChanges();

            return Ok();
        }

        // POST: api/HouseHold/Leave
        public IHttpActionResult Leave(int leavinghouseholdId)
        {
            var household = DbContext.Households.FirstOrDefault(p => p.Id == leavinghouseholdId);

            if (household == null)
                return NotFound();

            var currentUserId = User.Identity.GetUserId();
            var currentUser = DbContext.Users.FirstOrDefault(u => u.Id == currentUserId);
            var isParticipant = household.Participants.Where(p => p.Id == currentUserId).Any();

            if (!isParticipant)
                return BadRequest("Just participants can leave their households");

            household.Participants.Remove(currentUser);
            DbContext.SaveChanges();

            return Ok();
        }

        public IHttpActionResult GetParticipants(int householdId)
        {
            var household = DbContext.Households.FirstOrDefault(p => p.Id == householdId);

            if (household == null)
                return NotFound();

            var currentUserId = User.Identity.GetUserId();
            var currentUser = DbContext.Users.FirstOrDefault(u => u.Id == currentUserId);
            var isInHousehold = household.Participants.Where(p => p.Id == currentUserId).Any()
                || household.OwnerId == currentUserId;

            if (!isInHousehold)
                return BadRequest("Only participants can see information about their household");

            var participants = household.Participants
                .Select(h => new ParticipantViewModel
                {
                    Id = h.Id,
                    UserName = h.UserName,
                    Email = h.Email

                })
                .ToList();

            return Ok(participants);
        }

        public IHttpActionResult DeleteHousehold(int householdId)
        {
            var household = DbContext.Households.FirstOrDefault(p => p.Id == householdId);

            if (household == null)
                return NotFound();

            var currentUserId = User.Identity.GetUserId();
            var owner = DbContext.Households.FirstOrDefault(h => h.OwnerId == currentUserId);
            var message = "Just owners can delete their households";
            if (owner == null)
                return BadRequest(message);

            DbContext.Households.Remove(household);
            DbContext.SaveChanges();

            return Ok();
        }
    }
}
