using Newtonsoft.Json.Linq; // Instalar vía NuGet
using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Human_Resources.Data
{
    public class ClassGeocoding
    {
        private const string ApiKey = "AIzaSyCSYgzcr-KiVxVXJ_jwvxnmap9o6_sfvUk";

        public async Task<(decimal lat, decimal lng)?> GetCoordinatesAsync(string address)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    string url = $"https://maps.googleapis.com/maps/api/geocode/json?address={Uri.EscapeDataString(address)}&key={ApiKey}";
                    string response = await client.GetStringAsync(url);
                    JObject json = JObject.Parse(response);

                    if (json["status"]?.ToString() == "OK")
                    {
                        var location = json["results"][0]["geometry"]["location"];
                        return (location["lat"].Value<decimal>(), location["lng"].Value<decimal>());
                    }
                }
            }
            catch { /* Log error */ }
            return null;
        }
    }
}