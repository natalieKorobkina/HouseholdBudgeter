using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.Models.Domain
{
    public class Invitation
    {
        public int Id { get; set; }

        public virtual ApplicationUser Inviter { get; set; }
        public string InviterId { get; set; }

        public virtual ApplicationUser IsInvited { get; set; }
        public string IsInvitedId { get; set; }

        public virtual Household HouseholdToJoin { get; set; }
        public int HouseholdToJoinId { get; set; }
    }
}