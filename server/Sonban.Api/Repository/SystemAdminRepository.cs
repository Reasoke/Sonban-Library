using System.Net;
using Dapper;
using Sonban.Api.Dto;
using Sonban.Api.Classes;
using Sonban.Common.Classes;

namespace Sonban.Api.Repository;

public interface ISystemAdminRepository {
    Task<IEnumerable<SubscriptionDto>> GetSubscriptions();
    Task<SubscriptionDto> GetSubscriptionDetails(int subscriptionId);
    Task AddSubscription(SubscriptionRequest request);
    Task ModifySubscriptionDetails(int subscriptionId, SubscriptionRequest request);
    Task DeleteSubscription(int subscriptionId);
}

public class SystemAdminRepository : BaseRepository, ISystemAdminRepository {
        
    public SystemAdminRepository(ISettingsProvider settingsProvider) : base(settingsProvider) {
    }

    public async Task<IEnumerable<SubscriptionDto>> GetSubscriptions() {
        return await GetConnection().QueryAsync<SubscriptionDto>(
            @"SELECT subscriptionId as Id, subscriptionName as Name, price
FROM  Subscription");
    }

    public async Task<SubscriptionDto> GetSubscriptionDetails(int subscriptionId) {
        return await GetConnection().QueryFirstOrDefaultAsync<SubscriptionDto>(
            @"SELECT subscriptionId as Id, subscriptionName as Name, price
FROM  Subscription
WHERE subscriptionId = @subscriptionId",
            new {subscriptionId});
    }

    public async Task AddSubscription(SubscriptionRequest request) {
        await GetConnection().ExecuteAsync(
            @"INSERT INTO Subscription (subscriptionName, price) values (@name, @price)",
            new {name = request.Name, price = request.Price});
    }

    public async Task ModifySubscriptionDetails(int subscriptionId, SubscriptionRequest request) {
        var rowsAffected = await GetConnection().ExecuteAsync(
            @"UPDATE Subscription
SET  subscriptionName = @name, price = @price
WHERE subscriptionId = @subscriptionId", 
            new { subscriptionId, name = request.Name, price = request.Price});
        
        if (rowsAffected == 0) {
            throw new DomainException(HttpStatusCode.NotFound, "Subscription is not found");
        }
    }

    public async Task DeleteSubscription(int subscriptionId) {
        await GetConnection().ExecuteAsync(
            @"DELETE FROM  Subscription
WHERE subscriptionId = @subscriptionId",
            new {subscriptionId});
    }
}