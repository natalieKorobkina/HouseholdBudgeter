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
        public string Inviter { get; set; }
        public string IsInvited { get; set; }
        public string HouseholdToJoin { get; set; }
    }
}