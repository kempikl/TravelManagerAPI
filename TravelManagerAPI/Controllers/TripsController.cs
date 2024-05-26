using TravelManagerAPI.DTOs;
using TravelManagerAPI.Models;

namespace TravelManagerAPI.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly TripDbContext _context;

    public TripsController(TripDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TripDto>>> GetTrips()
    {
        var trips = await _context.Trips
            .OrderByDescending(t => t.StartDate)
            .Select(t => new TripDto
            {
                IdTrip = t.IdTrip,
                Name = t.Name,
                StartDate = t.StartDate,
                EndDate = t.EndDate
            }).ToListAsync();

        return Ok(trips);
    }

    [HttpDelete("api/clients/{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        var client = await _context.Clients.Include(c => c.ClientTrips).FirstOrDefaultAsync(c => c.IdClient == idClient);
        if (client == null)
        {
            return NotFound();
        }

        if (client.ClientTrips.Any())
        {
            return BadRequest("Client has assigned trips.");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("api/trips/{idTrip}/clients")]
    public async Task<IActionResult> AssignClientToTrip(int idTrip, AssignClientDto dto)
    {
        var trip = await _context.Trips.FindAsync(idTrip);
        if (trip == null)
        {
            return NotFound("Trip not found.");
        }

        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Pesel == dto.Pesel);
        if (client == null)
        {
            client = new Client { Pesel = dto.Pesel, FirstName = dto.FirstName, LastName = dto.LastName };
            _context.Clients.Add(client);
            await _context.SaveChangesAsync();
        }

        var clientTrip = await _context.ClientTrips.FirstOrDefaultAsync(ct => ct.IdClient == client.IdClient && ct.IdTrip == idTrip);
        if (clientTrip != null)
        {
            return BadRequest("Client is already assigned to this trip.");
        }

        clientTrip = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = idTrip,
            PaymentDate = dto.PaymentDate,
            RegisteredAt = DateTime.Now
        };

        _context.ClientTrips.Add(clientTrip);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
