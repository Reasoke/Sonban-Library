using System;

namespace Sonban.Common.Dto;

public class McpTokenDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DateTime? LastUsedDate { get; set; }
}