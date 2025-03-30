
namespace Foruscorp.FuelStations.Domain.FuelMapProvaiders
{
    public class FuelMapProvaider
    {
        public Guid Id { get; set; }

        public string Name { get; private set; }

        public string Url { get; private set; }  

        public string ApiToken { get; private set; } = string.Empty;

        public DateTime RefreshedAt { get; private set; }

        private FuelMapProvaider() { }
      

        public FuelMapProvaider( 
            string name, 
            string url,
            string apiToken)
        {
            Id = Guid.NewGuid(); 
            Name = name;
            Url = url;

            if (apiToken != null)
            {

                ApiToken = apiToken;
                RefreshedAt = DateTime.UtcNow;  
            }
        }


        public static FuelMapProvaider CreateNew(
            string name,
            string url,
            string apiToken) 
            => new FuelMapProvaider(
                name, 
                url, 
                apiToken);   


        public void RefreshApiToken(string apiToken)
        {
            ApiToken = apiToken;
            RefreshedAt = DateTime.UtcNow;  
        }

    }
}
