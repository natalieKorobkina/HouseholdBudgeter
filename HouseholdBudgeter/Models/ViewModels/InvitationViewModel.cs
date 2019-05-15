using HouseholdBudgeter.Models.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models.ViewModels
{
    public class InvitationViewModel
    {
        public int Id { get; set; }
        public string InviterId { get; set; }
        public string IsInvitedId { get; set; }
        public int HouseholdToJoinId { get; set; }
    }
}