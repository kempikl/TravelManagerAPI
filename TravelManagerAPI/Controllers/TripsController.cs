using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelManagerAPI.DTOs;
using TravelManagerAPI.Models;

namespace TravelManagerAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripsController(TripDbContext context) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TripDto>>> GetTrips()
    {
        var trips = await context.Trips
            .Include(t => t.CountryTrips)
            .ThenInclude(ct => ct.Country)
            .Include(t => t.ClientTrips)
            .ThenInclude(ct => ct.Client)
            .OrderByDescending(t => t.DateFrom)
            .Select(t => new TripDto
            {
                Name = t.Name,
                Description = t.Description,
                DateFrom = t.DateFrom,
                DateTo = t.DateTo,
                MaxPeople = t.MaxPeople,
                Countries = t.CountryTrips.Select(ct => new CountryDto
                {
                    Name = ct.Country.Name
                }).ToList(),
                Clients = t.ClientTrips.Select(ct => new ClientDto
                {
                    FirstName = ct.Client.FirstName,
                    LastName = ct.Client.LastName
                }).ToList()
            }).ToListAsync();

        return Ok(trips);
    }

    [HttpDelete("api/clients/{idClient}")]
    public async Task<IActionResult> DeleteClient(int idClient)
    {
        var client = await context.Clients.Include(c => c.ClientTrips).FirstOrDefaultAsync(c => c.IdClient == idClient);
        if (client == null)
        {
            return NotFound();
        }

        if (client.ClientTrips.Any())
        {
            return BadRequest("Client has assigned trips.");
        }

        context.Clients.Remove(client);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("api/trips/clients")]
    public async Task<IActionResult> AssignClientToTrip([FromBody] AssignClientDto dto)
    {
        var trip = await context.Trips.FindAsync(dto.IdTrip);
        if (trip == null || trip.Name != dto.TripName)
        {
            return NotFound("Trip not found or name does not match.");
        }
        
        var client = await context.Clients.FirstOrDefaultAsync(c => c.Pesel == dto.Pesel);
        if (client == null)
        {
            client = new Client
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = dto.Email,
                Telephone = dto.Telephone,
                Pesel = dto.Pesel
            };
            context.Clients.Add(client);
            await context.SaveChangesAsync();
        }
        
        var clientTrip = await context.ClientTrips.FirstOrDefaultAsync(ct => ct.IdClient == client.IdClient && ct.IdTrip == dto.IdTrip);
        if (clientTrip != null)
        {
            return BadRequest("Client is already assigned to this trip.");
        }
        
        clientTrip = new ClientTrip
        {
            IdClient = client.IdClient,
            IdTrip = dto.IdTrip,
            PaymentDate = dto.PaymentDate,
            RegisteredAt = DateTime.Now
        };

        context.ClientTrips.Add(clientTrip);
        await context.SaveChangesAsync();
        
        var responseDto = new ClientAssignmentResponseDto
        {
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            Telephone = client.Telephone,
            Pesel = client.Pesel,
            IdTrip = dto.IdTrip,
            TripName = trip.Name,
            PaymentDate = dto.PaymentDate
        };

        return Ok(responseDto);
    }
}