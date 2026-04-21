using System.ComponentModel.DataAnnotations;

namespace SimpleWebApplication.DTOs;

public class CreateReservationDto
{
    [Required]
    public int RoomId { get; set; }
    [Required]
    [StringLength(64)]
    public string OrganizerName { get; set; } = string.Empty;
    [Required]
    [StringLength(64)]
    public string Topic { get; set; } = string.Empty;
    [Required]
    public DateTime Date { get; set; }
    [Required]
    public string StartTime { get; set; } = string.Empty;
    [Required]
    public string EndTime { get; set; } = string.Empty;
}