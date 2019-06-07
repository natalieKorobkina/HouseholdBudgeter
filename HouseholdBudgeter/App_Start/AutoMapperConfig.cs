using AutoMapper;
using HouseholdBudgeter.Models;
using HouseholdBudgeter.Models.BindingModels;
using HouseholdBudgeter.Models.Domain;
using HouseholdBudgeter.Models.Helpers;
using HouseholdBudgeter.Models.ViewModels;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBudgeter.App_Start
{
    public class AutoMapperConfig
    {
        private HBHelper hBHelper;
        private ApplicationDbContext DbContext;

        public AutoMapperConfig()
        {
            DbContext = new ApplicationDbContext();
            hBHelper = new HBHelper(DbContext);
        }
        public static void Init()
        {
            Mapper.Initialize(cfg =>
            {
                string currentUserId = null;

                cfg.CreateMap<Household, HouseholdBindingModel>().ReverseMap();
                cfg.CreateMap<Household, HouseholdViewModel>().ReverseMap();
                cfg.CreateMap<Household, HouseholdInListViewModel>()
                .ForMember(dest => dest.IsOwner,
                opt => opt.MapFrom(src => src.OwnerId == currentUserId)); 
                cfg.CreateMap<Invitation, InvitationViewModel>().ReverseMap();
                cfg.CreateMap<Category, CategoryBindingModel>().ReverseMap();
                cfg.CreateMap<Category, CategoryViewModel>().ReverseMap();
                cfg.CreateMap<ApplicationUser, ParticipantViewModel>().ReverseMap();
                cfg.CreateMap<BankAccount, BankAccountBindingModel>().ReverseMap();
                cfg.CreateMap<BankAccount, BankAccountViewModel>().ReverseMap();
                cfg.CreateMap<Transaction, TransactionViewModel>()
                .ForMember(dest => dest.CanEdit,
                opt => opt.MapFrom(src => src.BankAccount.Household.OwnerId == currentUserId
                || src.OwnerId == currentUserId));
                cfg.CreateMap<Transaction, TransactionBindingModel>().ReverseMap();
            });
        }
    }
}