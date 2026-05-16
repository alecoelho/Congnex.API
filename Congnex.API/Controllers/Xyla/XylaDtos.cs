using System.ComponentModel.DataAnnotations;

namespace Congnex.API.Controllers.Xyla;

public class XylaStartResponse
{
    public Guid SessionId { get; set; }
}

public class XylaMessageRequest
{
    [Required]
    public Guid SessionId { get; set; }

    [Required]
    [MinLength(1)]
    [MaxLength(600)]
    public string Message { get; set; } = string.Empty;
}
