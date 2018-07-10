using Newtonsoft.Json;

namespace MicroService.Login.Models.Resources
{
    public class IpLookupResource
    {
        [JsonProperty("ip")]
        public string Ip { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; }

        [JsonProperty("region_code")]
        public string RegionCode { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("country_name")]
        public string CountryName { get; set; }

        [JsonProperty("continent_code")]
        public string ContinentCode { get; set; }

        [JsonProperty("postal")]
        public string Postal { get; set; }

        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        [JsonProperty("timezone")]
        public string Timezone { get; set; }

        [JsonProperty("utc_offset")]
        public string UtcOffset { get; set; }

        [JsonProperty("country_calling_code")]
        public string CountryCallingCode { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("languages")]
        public string Languages { get; set; }

        [JsonProperty("asn")]
        public string Asn { get; set; }

        [JsonProperty("org")]
        public string Org { get; set; }
    }
}