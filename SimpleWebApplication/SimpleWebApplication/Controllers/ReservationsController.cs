using Microsoft.AspNetCore.Mvc;
using SimpleWebApplication.DTOs;
using SimpleWebApplication.Models;

namespace SimpleWebApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ReservationsController : ControllerBase
{
    public static List<Reservation> reservations = new List<Reservation>()
    {
        new Reservation { Id = 1, RoomId = 1, OrganizerName = "Jan Kowalski", Topic = "Wykład PRI", Date = new DateTime(2026, 05, 10), StartTime = "10:00:00", EndTime = "12:30:00", Status = "confirmed" },
        new Reservation { Id = 2, RoomId = 2, OrganizerName = "Mariusz Nowak", Topic = "Wykład APBD", Date = new DateTime(2026, 05, 15), StartTime = "09:00:00", EndTime = "11:00:00", Status = "planned" },
        new Reservation { Id = 3, RoomId = 1, OrganizerName = "Samorząd", Topic = "Warsztaty", Date = new DateTime(2026, 05, 10), StartTime = "13:00:00", EndTime = "15:00:00", Status = "confirmed" },
        new Reservation { Id = 4, RoomId = 4, OrganizerName = "Zarząd", Topic = "Szkolenie BHP", Date = new DateTime(2026, 06, 20), StartTime = "08:00:00", EndTime = "10:00:00", Status = "planned" },
        new Reservation { Id = 5, RoomId = 3, OrganizerName = "Anna Kowal", Topic = "Wykład TPO", Date = new DateTime(2026, 05, 20), StartTime = "08:00:00", EndTime = "10:00:00", Status = "confirmed" },
        new Reservation { Id = 6, RoomId = 4, OrganizerName = "Samorząd", Topic = "Koło naukowe", Date = new DateTime(2026, 05, 20), StartTime = "09:00:00", EndTime = "11:00:00", Status = "cancelled" }
    };
    
    // GET /api/reservations
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(reservations);
    }
    
    // GET /api/reservations/{id}
    [Route("{id:int}")]
    [HttpGet]
    public IActionResult GetById(int id)
    {
        var reservationById = reservations.FirstOrDefault(x => x.Id == id);

        if (reservationById == null)
        {
            return NotFound("Brak rezerwacji o podanym ID");
        }
        
        return Ok(reservationById);
    }
    
    // GET /api/reservations?date=2026-05-10&status=confirmed&roomId=2
    [Route("filter")]
    [HttpGet]
    public IActionResult GetByFilter([FromQuery] DateTime? date, [FromQuery] string? status, [FromQuery] int? roomId)
    {
        var query = reservations.AsQueryable();

        if (date.HasValue)
        {
            query = query.Where(x => x.Date == date.Value);
        }

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase));
        }
        
        if (roomId.HasValue)
        {
            query = query.Where(x => x.RoomId == roomId.Value);
        }
        
        var result = query.ToList();

        if (result.Count == 0)
        {
            return NotFound("Brak rezerwacji o podanych filtrach");
        }
        
        return Ok(result);
    }
    
    // POST /api/reservations
    [HttpPost]
    public IActionResult Create([FromBody] CreateReservationDto createReservationDto)
    {
        var room = RoomsController.rooms.FirstOrDefault(x => x.Id == createReservationDto.RoomId);

        if (room == null)
        {
            return BadRequest("Nie można utworzyć rezerwacji do nieistniejącej sali");
        }

        if (!room.IsActive)
        {
            return BadRequest("Nie można utworzyć rezerwacji do nieaktywnej sali");
        }

        var startTime = TimeSpan.Parse(createReservationDto.StartTime);
        var endTime = TimeSpan.Parse(createReservationDto.EndTime);

        if (startTime >= endTime)
        {
            return BadRequest("Nie można utworzyć rezerwacji, której godzina rozpoczęcie jest później niż godzina zakończenia");
        }

        var isOverlapping = reservations.Any(x => 
            x.RoomId == createReservationDto.RoomId && 
            x.Date.Date == createReservationDto.Date.Date && 
            x.Status != "cancelled" &&
            startTime < TimeSpan.Parse(x.EndTime) && 
            endTime > TimeSpan.Parse(x.StartTime)
        );

        if (isOverlapping)
        {
            return Conflict("Nie można utworzyć rezerwacji do zarezerwowanej sali w podanym terminie");
        }

        var reservationToPost = new Reservation
        {
            Id = reservations.Any() ? reservations.Max(x => x.Id) + 1 : 1,
            RoomId = createReservationDto.RoomId,
            OrganizerName = createReservationDto.OrganizerName,
            Topic = createReservationDto.Topic,
            Date = createReservationDto.Date,
            StartTime = createReservationDto.StartTime,
            EndTime = createReservationDto.EndTime,
            Status = "planned"
        };
        
        reservations.Add(reservationToPost);

        return CreatedAtAction(nameof(GetById), new { id = reservationToPost.Id }, reservationToPost);
    }
    
    // PUT /api/reservations/{id}
    [Route("{id:int}")]
    [HttpPut]
    public IActionResult Put(int id, [FromBody] CreateReservationDto createReservationDto)
    {
        var reservationToPut = reservations.FirstOrDefault(x => x.Id == id);

        if (reservationToPut == null)
        {
            return NotFound("Brak rezerwacji o podanym ID");
        }
        
        var startTime = TimeSpan.Parse(createReservationDto.StartTime);
        var endTime = TimeSpan.Parse(createReservationDto.EndTime);

        if (startTime >= endTime)
        {
            return BadRequest("godzina rozpoczęcia musi być wcześniej niż godzina zakończenia");
        }
        
        var room = RoomsController.rooms.FirstOrDefault(x => x.Id == createReservationDto.RoomId);

        if (room == null || !room.IsActive)
        {
            return BadRequest("Nie można zmienić rezerwacji do nieistniejącej sali bądź nieaktywnej");
        } 
        
        var isOverlapping = reservations.Any(x => 
            x.Id != id &&
            x.RoomId == createReservationDto.RoomId && 
            x.Date.Date == createReservationDto.Date.Date && 
            x.Status != "cancelled" &&
            startTime < TimeSpan.Parse(x.EndTime) && 
            endTime > TimeSpan.Parse(x.StartTime)
        );

        if (isOverlapping)
        {
            return Conflict("Nowy termin koliduje z inną rezerwacją");
        }
        
        reservationToPut.RoomId = createReservationDto.RoomId;
        reservationToPut.OrganizerName = createReservationDto.OrganizerName;
        reservationToPut.Topic = createReservationDto.Topic;
        reservationToPut.Date = createReservationDto.Date;
        reservationToPut.StartTime = createReservationDto.StartTime;
        reservationToPut.EndTime = createReservationDto.EndTime;

        return Ok(reservationToPut);
    }
    
    // DELETE /api/reservations/{id}
    [Route("{id:int}")]
    [HttpDelete]
    public IActionResult Delete(int id)
    {
        var reservationToDelete = reservations.FirstOrDefault(x => x.Id == id);

        if (reservationToDelete == null)
        {
            return NotFound("Brak rezerwacji o podanym ID");
        }
        
        reservations.Remove(reservationToDelete);
        
        return NoContent();
    }
}