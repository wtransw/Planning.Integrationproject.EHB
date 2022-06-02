using Crm.Link.UUID.Model;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace Crm.Link.UUID
{
    public class UUIDGateAway : IUUIDGateAway
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<UUIDGateAway> _logger;

        public UUIDGateAway(IHttpClientFactory httpClientFactory, ILogger<UUIDGateAway> logger)
        {
            _httpClient = httpClientFactory.CreateClient("UuidMasterApi");
            _logger = logger;
        }

        public async Task<ResourceDto?> GetResource(Guid id, string source)
        {
            var response = await _httpClient.GetAsync($"api/resources/{id}/{source}");
            if (!response.IsSuccessStatusCode)
                return null;
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
                return null;

            return JsonConvert.DeserializeObject<ResourceDto>(content);
        }

        public async Task<ResourceDto?> GetGuid(string id, string sourceType, EntityTypeEnum entityType)
        {
            var response = await _httpClient.GetAsync($"resources/search?source={sourceType}&entityType={entityType}&sourceEntityId={id}");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                _logger.LogError($"{nameof(GetGuid)} failed: {response.StatusCode}");
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var resource = JsonConvert.DeserializeObject<ResourceDto>(content);

            return resource;
        }

        //public async Task<ResourceDto?> PublishEntity(string source, EntityTypeEnum entityType, string sourceEntityId, int version)
        //{
        //    var body = new
        //    {
        //        Source = source,
        //        EntityType = entityType.ToString(),
        //        SourceEntityId = sourceEntityId,
        //        EntityVersion = version
        //    };

        //    var json = JsonConvert.SerializeObject(body);
        //    var contentBody = new StringContent(json, Encoding.UTF8, Application.Json);
        //    var response = await _httpClient.PostAsync("resources", contentBody);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        var content = await response.Content.ReadAsStringAsync();
        //        return JsonConvert.DeserializeObject<ResourceDto>(content);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        public async Task<ResourceDto?> PublishEntity(Guid uuid, string source, EntityTypeEnum entityType, string sourceEntityId, int version)
        {
            var body = new
            {
                Uuid = uuid,
                Source = source,
                EntityType = entityType.ToString(),
                SourceEntityId = sourceEntityId,
                EntityVersion = version
            };

            var json = JsonConvert.SerializeObject(body);
            var contentBody = new StringContent(json, Encoding.UTF8, Application.Json);
            var response = await _httpClient.PostAsync($"api/resources/{uuid}", contentBody);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ResourceDto>(content);
            }
            else
            {
                return null;
            }
        }


        public async Task<ResourceDto?> UpdateEntity(string id, string sourceType, EntityTypeEnum entityType)
        {
            var response = await GetGuid(id, sourceType, entityType);
            _ = response ?? throw new ArgumentNullException(nameof(response));

            var body = new
            {
                Source = sourceType,
                EntityType = entityType,
                SourceEntityId = id,
                EntityVersion = ++response!.EntityVersion
            };

            var json = JsonConvert.SerializeObject(body);
            var contentBody = new StringContent(json, Encoding.UTF8, Application.Json);
            var resp = await _httpClient.PatchAsync($"resources/{response.Uuid}", contentBody);
            if (resp.IsSuccessStatusCode)
            {
                var content = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ResourceDto>(content);
            }
            else
            {
                return null;
            }
        }

        public async Task<ResourceDto?> UpdateEntity(string id, string sourceType, EntityTypeEnum entityType, int newVersion)
        {
            var response = await GetGuid(id, sourceType, entityType);
            _ = response ?? throw new ArgumentNullException(nameof(response));

            var body = new
            {
                Source = sourceType,
                EntityType = entityType,
                SourceEntityId = id,
                EntityVersion = newVersion
            };

            var json = JsonConvert.SerializeObject(body);
            var contentBody = new StringContent(json, Encoding.UTF8, Application.Json);
            var resp = await _httpClient.PatchAsync($"resources/{response.Uuid}", contentBody);
            if (resp.IsSuccessStatusCode)
            {
                var content = await resp.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ResourceDto>(content);
            }
            else
            {
                return null;
            }
        }
    }
}
