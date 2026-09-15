using Sonban.Api.Dto;
using Sonban.Api.Repository;

namespace Sonban.Api.Services {
    public interface ISystemAdminService {
        Task<IEnumerable<SubscriptionDto>> GetSubscriptions();
        Task<SubscriptionDto> GetSubscriptionDetails(int subscriptionId);
        Task AddSubscription(SubscriptionRequest request);
        Task ModifySubscriptionDetails(int subscriptionId, SubscriptionRequest request);
        Task DeleteSubscription(int subscriptionId);
    }

    public class SystemAdminService : ISystemAdminService {
        private readonly ISystemAdminRepository systemAdminRepository;

        public SystemAdminService(ISystemAdminRepository systemAdminRepository) {
            this.systemAdminRepository = systemAdminRepository;
        }       
     
        public async Task<IEnumerable<SubscriptionDto>> GetSubscriptions() {
            return await systemAdminRepository.GetSubscriptions();
        }

        public async Task<SubscriptionDto> GetSubscriptionDetails(int subscriptionId) {
            return await systemAdminRepository.GetSubscriptionDetails(subscriptionId);
        }

        public async Task AddSubscription(SubscriptionRequest request) {
            await systemAdminRepository.AddSubscription(request);
        }

        public async Task ModifySubscriptionDetails(int subscriptionId, SubscriptionRequest request) {
            await systemAdminRepository.ModifySubscriptionDetails(subscriptionId, request);
        }

        public async Task DeleteSubscription(int subscriptionId) {
            await systemAdminRepository.DeleteSubscription(subscriptionId);
        }

    }
}