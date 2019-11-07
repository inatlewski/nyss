﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using RX.Nyss.Data.Models;
using RX.Nyss.Web.Features.Authentication.Policies.BaseAccessHandlers;
using RX.Nyss.Web.Services;

namespace RX.Nyss.Web.Features.Authentication.Policies
{
    public class DataManagerAccessRequirement : IAuthorizationRequirement
    {
    }

    public class DataManagerAccessHandler : BaseUserAccessHandler<DataManagerUser, DataManagerAccessRequirement>
    {
        private const string RouteParameterName = "dataManagerId";

        public DataManagerAccessHandler(IHttpContextAccessor httpContextAccessor, INationalSocietyAccessService nationalSocietyAccessService)
            : base(httpContextAccessor, nationalSocietyAccessService, RouteParameterName)
        {
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, DataManagerAccessRequirement requirement)
        {
            if (!context.User.Identity.IsAuthenticated)
            { 
                return Task.CompletedTask;
            }
            return HandleUserResourceRequirement(context, requirement);
        }
            
    }
}
