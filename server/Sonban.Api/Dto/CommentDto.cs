namespace Sonban.Api.Dto {
    public class CommentDto {
        public int CommentId { get; set; }
        public int ProductId { get; set; }
        public int ProductType { get; set; } 
        public string Message { get; set; }
        public DateTime createdTime { get; set; }

        public int userCreatorId { get; set; }
        public string userCreatorName { get; set; }

        public int replyToUserId { get; set; }
        public string replyToUserName { get; set; }
    }

    public class AddCommentDto {
        public int ProductId { get; set; }
        public int ProductType { get; set; }

        public string Message { get; set; }

        public int userCreatorId { get; set; }
        public int replyToUserId { get; set; }
    }
}
