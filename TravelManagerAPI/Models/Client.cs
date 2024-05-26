namespace TravelManagerAPI.Models;

using System.Collections.Generic;

public class Client
{
    public int IdClient { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Pesel { get; set; }

    public ICollection<ClientTrip> ClientTrips { get; set; } = new List<ClientTrip>();
}
