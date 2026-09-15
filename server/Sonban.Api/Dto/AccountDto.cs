namespace Sonban.Api.Dto {

    public class AccountDto {
        public int Id { get; set; }
        public string Name { get; set; }
        public AccountUserType UserType { get; set; }
        public AccountSubscription Subscription { get; set; }
    }
}

