namespace Foruscorp.FuelStations.Aplication.Contructs.Services.Models
{
    public class LovesApiResponseModel
    {
        public List<LovesStoreModel> Stores { get; set; } = new();
    }

    public class LovesStoreModel
    {
        public int Number { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string Address1 { get; set; } = string.Empty;
        public string? Address2 { get; set; }
        public int AddressId { get; set; }
        public int AddressTypeId { get; set; }
        public string City { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public string? County { get; set; }
        public string Name { get; set; } = string.Empty;
        public string StoreUrl { get; set; } = string.Empty;
        public string? SpecialInstructions { get; set; }
        public string State { get; set; } = string.Empty;
        public string Zip { get; set; } = string.Empty;
        public string ExitNumber { get; set; } = string.Empty;
        public string Highway { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string MapPinUrl { get; set; } = string.Empty;
        public int MapPinZIndex { get; set; }
        public int FacilityId { get; set; }
        public bool IsHotel { get; set; }
        public bool IsTrillium { get; set; }
        public bool IsPrivate { get; set; }
        public string MapIconLinkText { get; set; } = string.Empty;
        public string? BookNowLink { get; set; }
    }
}
