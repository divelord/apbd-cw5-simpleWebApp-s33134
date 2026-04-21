using Microsoft.AspNetCore.Mvc;
using SimpleWebApplication.DTOs;
using SimpleWebApplication.Models;

namespace SimpleWebApplication.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RoomsController : ControllerBase
{
    public static List<Room> rooms = new List<Room>()
    {
        new Room
        {
            Id = 1, Name = "A1", BuildingCode = "A", Floor = 0, Capacity = 200, HasProjector = true, IsActive = true
        },
        new Room
        {
            Id = 2, Name = "B1", BuildingCode = "B", Floor = 0, Capacity = 250, HasProjector = true, IsActive = true
        },
        new Room
        {
            Id = 3, Name = "C1", BuildingCode = "C", Floor = 1, Capacity = 150, HasProjector = true, IsActive = false
        },
        new Room
        {
            Id = 4, Name = "H102", BuildingCode = "H", Floor = 1, Capacity = 100, HasProjector = false, IsActive = true
        }
    };

    // GET api/rooms
    [HttpGet]
    public IActionResult GetAll()
    {
        return Ok(rooms);
    }

    // GET /api/rooms/{id}
    [Route("{id:int}")]
    [HttpGet]
    public IActionResult GetById(int id)
    {
        var roomById = rooms.FirstOrDefault(x => x.Id == id);

        if (roomById == null)
        {
            return NotFound("Nie ma sali o podanym ID");
        }

        return Ok(roomById);
    }

    // GET /api/rooms/building/{buildingCode}
    [Route("building/{buildingCode}")]
    [HttpGet]
    public IActionResult GetByBuildingCode(string buildingCode)
    {
        var roomByBuildingCode = rooms.Where(x => x.BuildingCode.Equals(buildingCode, StringComparison.OrdinalIgnoreCase));

        if (!roomByBuildingCode.Any())
        {
            return NotFound("Nie ma sal w podanym budynku");
        }

        return Ok(roomByBuildingCode);
    }

    // GET /api/rooms?minCapacity=20&hasProjector=true&activeOnly=true
    [Route("filter")]
    [HttpGet]
    public IActionResult GetByFilter([FromQuery] int? minCapacity, [FromQuery] bool? hasProjector,
        [FromQuery] bool? isActive)
    {
        var query = rooms.AsQueryable();

        if (minCapacity.HasValue)
        {
            query = query.Where(x => x.Capacity >= minCapacity.Value);
        }

        if (hasProjector.HasValue)
        {
            query = query.Where(x => x.HasProjector == hasProjector.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        var result = query.ToList();

        if (result.Count == 0)
        {
            return NotFound("Nie ma sali o podanych filtrach");
        }

        return Ok(result);
    }

    // POST api/rooms
    [HttpPost]
    public IActionResult Post([FromBody] CreateRoomDto createRoomDto)
    {
        var roomToPost = new Room()
        {
            Id = rooms.Any() ? rooms.Max(x => x.Id) + 1 : 1,
            Name = createRoomDto.Name,
            BuildingCode = createRoomDto.BuildingCode,
            Floor = createRoomDto.Floor,
            Capacity = createRoomDto.Capacity,
            HasProjector = createRoomDto.HasProjector,
            IsActive = createRoomDto.IsActive
        };

        rooms.Add(roomToPost);

        return CreatedAtAction(nameof(GetById), new { id = roomToPost.Id }, roomToPost);
    }

    // PUT /api/rooms/{id}
    [Route("{id:int}")]
    [HttpPut]
    public IActionResult Put(int id, [FromBody] CreateRoomDto updateRoomDto)
    {
        var roomToPut = rooms.FirstOrDefault(x => x.Id == id);

        if (roomToPut == null)
        {
            return NotFound("Nie ma sali o podanym ID");
        }

        roomToPut.Name = updateRoomDto.Name;
        roomToPut.BuildingCode = updateRoomDto.BuildingCode;
        roomToPut.Floor = updateRoomDto.Floor;
        roomToPut.Capacity = updateRoomDto.Capacity;
        roomToPut.HasProjector = updateRoomDto.HasProjector;
        roomToPut.IsActive = updateRoomDto.IsActive;

        return Ok(roomToPut);
    }

    // DELETE /api/rooms/{id}
    [Route("{id:int}")]
    [HttpDelete]
    public IActionResult Delete(int id)
    {
        var roomToDelete = rooms.FirstOrDefault(x => x.Id == id);

        if (roomToDelete == null)
        {
            return NotFound("Nie ma sali o podanym ID");
        }

        var hasReservations = ReservationsController.reservations.Any(x => x.RoomId == id);

        if (hasReservations)
        {
            return Conflict("Do tej sali jest przypisana rezerwacja");
        }

        rooms.Remove(roomToDelete);

        return NoContent();
    }
}