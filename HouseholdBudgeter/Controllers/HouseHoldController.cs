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
    [RoutePrefix("api/household")]
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

        public IHttpActionResult PostHousehold(HouseholdBindingModel bindingModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var household = Mapper.Map<Household>(bindingModel);
            var owner = hBHelper.GetCurrentUser();

            household.Created = DateTime.Now;
            household.OwnerId = owner.Id;
            household.Participants.Add(owner);

            DbContext.Households.Add(household);
            DbContext.SaveChanges();

            var url = Url.Link("DefaultApi", new { Controller = "HouseHold", Id = household.Id });
            var householdModel = Mapper.Map<HouseholdViewModel>(household);

            return Created(url, householdModel);
        }

        public IHttpActionResult PutHousehold(int id, HouseholdBindingModel bindingModel)
        {
            if (!ModelState.IsValid)
                return BadRequest("Not a valid model");

            var household = hBHelper.GetHouseholdById(id);
            if (household == null)
                return NotFound();

            if (!hBHelper.CheckIfOwner(household.OwnerId))
                return BadRequest("Just owners can update household' information");
            
            Mapper.Map(bindingModel, household);
            household.Updated = DateTime.Now;
            DbContext.SaveChanges();

            var householdModel = Mapper.Map<HouseholdViewModel>(household);

            return Ok(householdModel);
        }

        public IHttpActionResult Invite(string email, int householdId)
        {
            var invitingUser = hBHelper.GetUserByEmail(email);
            if (invitingUser == null)
                return BadRequest("Cannot invite unregistered user");

            var household = hBHelper.GetHouseholdById(householdId);
            if (household == null)
                return NotFound();

            if (hBHelper.isParticipant(household, invitingUser.Id))
                return BadRequest("User is already participant");

            if (!hBHelper.CheckIfOwner(household.OwnerId))
                return BadRequest("Just owners can invite users");

            var invitation = CreateInvitation(invitingUser.Id, invitingUser.Email, householdId);
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

        public IHttpActionResult PostLeave(int leavingHouseholdId)
        {
            var household = hBHelper.GetHouseholdById(leavingHouseholdId);
            if (household == null)
                return NotFound();

            var currentUser = hBHelper.GetCurrentUser();
            if (!hBHelper.currentIsParticipant(household))
                return BadRequest("Just participants can leave their households");

            if (hBHelper.CheckIfOwner(household.OwnerId))
                return BadRequest("Owner cannot leave their households");

            household.Participants.Remove(currentUser);
            DbContext.SaveChanges();

            return Ok();
        }

        public IHttpActionResult GetParticipants(int householdId)
        {
            var household = hBHelper.GetHouseholdById(householdId);
            if (household == null)
                return NotFound();

            if (!hBHelper.currentIsParticipant(household))
                return BadRequest("Only participants can see information about their household");

            var participants = household.Participants
                .Select(h => new ParticipantViewModel
                {
                    Id = h.Id,
                    UserName = h.UserName

                }).ToList();

            return Ok(participants);
        }

        public IHttpActionResult DeleteHousehold(int householdId)
        {
            var household = hBHelper.GetHouseholdById(householdId);
            if (household == null)
                return NotFound();

            if (!hBHelper.CheckIfOwner(household.OwnerId))
                return BadRequest("Just owners can delete their households");

            DbContext.Households.Remove(household);
            DbContext.SaveChanges();

            return Ok();
        }
    }
}
