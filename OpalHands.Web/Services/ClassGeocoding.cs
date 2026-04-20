using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace OpalHands.Web.Services
{
    public class ClassGeocoding
    {
        private const string ApiKey = "AIzaSyCSYgzcr-KiVxVXJ_jwvxnmap9o6_sfvUk";

        public static async Task<(decimal lat, decimal lng)?> GetCoordinatesAsync(string address)
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
                        var location = json["results"]?[0]?["geometry"]?["location"];

                        if (location != null)
                        {
                            // Esta forma de extraer el valor elimina el error de "referencia nula"
                            decimal lat = location["lat"]?.Value<decimal>() ?? 0;
                            decimal lng = location["lng"]?.Value<decimal>() ?? 0;

                            return (lat, lng);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error Geocoding: " + ex.Message);
            }
            return null;
        }
    }
}