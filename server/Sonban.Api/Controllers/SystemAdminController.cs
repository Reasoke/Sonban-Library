using Microsoft.AspNetCore.Mvc;
using Sonban.Api.Classes;
using Sonban.Api.Dto;
using Sonban.Api.Services;

namespace Sonban.Api.Controllers;

[AuthCookieCheckFilter]
[CheckSysAdminRights]
public class SystemAdminController : BaseApiController {
    private readonly ISystemAdminService systemAdminService;

    public SystemAdminController(ISystemAdminService systemAdminService) {
        this.systemAdminService = systemAdminService;
    }
    
    [HttpGet, Route("api/sysAdmin/subscription")]
    public async Task<IEnumerable<SubscriptionDto>> GetSubscriptions() {
        return await systemAdminService.GetSubscriptions();
    }
    
    [HttpGet, Route("api/sysAdmin/subscription/{subscriptionId:int}")]
    public async Task<SubscriptionDto> GetSubscriptionDetails(int subscriptionId) {
        return await systemAdminService.GetSubscriptionDetails(subscriptionId);
    }
    
    [HttpPost, Route("api/sysAdmin/subscription")]
    public async Task AddSubscription(SubscriptionRequest request) {
        await systemAdminService.AddSubscription(request);
    }
    
    [HttpPost, Route("api/sysAdmin/subscription/{subscriptionId:int}")]
    public async Task ModifySubscriptionDetails(int subscriptionId, SubscriptionRequest request) {
        await systemAdminService.ModifySubscriptionDetails(subscriptionId, request);
    }
    
    [HttpDelete, Route("api/sysAdmin/subscription/{subscriptionId:int}")]
    public async Task DeleteSubscription(int subscriptionId) {
        await systemAdminService.DeleteSubscription(subscriptionId);
    } 
    
}