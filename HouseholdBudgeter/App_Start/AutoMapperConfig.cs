using AutoMapper;
using HouseholdBudgeter.Models;
using HouseholdBudgeter.Models.BindingModels;
using HouseholdBudgeter.Models.Domain;
using HouseholdBudgeter.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.App_Start
{
    public class AutoMapperConfig
    {
        public static void Init()
        {
            Mapper.Initialize(cfg => {
                cfg.CreateMap<Household, HouseholdBindingModel>().ReverseMap();
                cfg.CreateMap<Household, HouseholdViewModel>().ReverseMap();
                cfg.CreateMap<Invitation, InvitationViewModel>().ReverseMap();
                cfg.CreateMap<Category, CategoryBindingModel>().ReverseMap();
                cfg.CreateMap<Category, CategoryViewModel>().ReverseMap();
                cfg.CreateMap<ApplicationUser, ParticipantViewModel>().ReverseMap();
            });
        }
    }
}