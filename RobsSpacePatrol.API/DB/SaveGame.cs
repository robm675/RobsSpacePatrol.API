using System.ComponentModel.DataAnnotations;

namespace RobsSpacePatrol.API.DB;

public class SaveGame
{
    [Key]
    public Guid Key { get; set; }

    public string JSON { get; set; } = string.Empty;
    public required string UserId { get; set; }

    [Timestamp]
    public byte[] Version { get; set; } = [];
}