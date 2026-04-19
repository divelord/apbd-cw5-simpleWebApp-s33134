using Microsoft.AspNetCore.Mvc;
using SimpleWebApplication.DTOs;
using SimpleWebApplication.Models;

namespace SimpleWebApplication.Controllers;

[Route("api/rooms")]
[ApiController]
public class RoomController : ControllerBase
{
    public static List<Rooms> rooms = new List<Rooms>()
    {
        new Rooms() { Id = 1, Name = "Room 1", Capacity = 5 },
        new Rooms() { Id = 2, Name = "Room 2", Capacity = 3 },
        new Rooms() { Id = 3, Name = "Room 3", Capacity = 7 }
    };

    // GET api/rooms
    [HttpGet]
    public IActionResult Get([FromQuery] int? minCapacity = 0)
    {
        return Ok(rooms.Where(x => x.Capacity >= minCapacity));
    }

    // GET /api/rooms/{id}
    // GET /api/rooms/3
    [Route("{id}")]
    [HttpGet]
    public IActionResult GetById(int id)
    {
        var room = rooms.FirstOrDefault(x => x.Id == id);

        if (room == null)
        {
            return NotFound();
        }

        return Ok(room);
    }

    // POST api/rooms { "Name" : "Room1", "Capacity" : 10}
    [HttpPost]
    public IActionResult Post([FromBody] CreateRoomDto createRoomDto)
    {
        var room = new Rooms()
        {
            Id = rooms.Count + 1,
            Name = createRoomDto.Name,
            Capacity = createRoomDto.Capacity
        };

        rooms.Add(room);

        return CreatedAtAction(nameof(GetById), new { id = room.Id }, room);
    }
}