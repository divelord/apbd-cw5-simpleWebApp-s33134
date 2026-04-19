using System.ComponentModel.DataAnnotations;

namespace SimpleWebApplication.DTOs;

public class CreateRoomDto
{
    [StringLength(100)]
    [MinLength(3)]
    public string Name { get; set; } = string.Empty;
    [Range(1, 100)]
    public int Capacity { get; set; }
}