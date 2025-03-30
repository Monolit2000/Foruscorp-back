using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Foruscorp.FuelStations.Domain.FuelStations
{
    public class Address
    {
       public string StationAddress { get; private set; }

        public Address(string stationAddress)
        {
            if (string.IsNullOrWhiteSpace(stationAddress))
                throw new ArgumentException("Address cannot be empty", nameof(stationAddress));
            StationAddress = stationAddress;
        }   

        public static Address CreateNew(string stationAddress)
            => new Address(stationAddress); 

    }
}
