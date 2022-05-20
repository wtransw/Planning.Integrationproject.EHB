using Crm.Link.UUID.Model;

namespace Crm.Link.UUID
{
    public interface IUUIDGateAway
    {
        Task<ResourceDto?> GetGuid(string id, string sourceType, EntityTypeEnum entityType);
        Task<ResourceDto?> PublishEntity(string source, EntityTypeEnum entityType, string sourceEntityId, int version);
        Task<ResourceDto?> UpdateEntity(string id, string sourceType, EntityTypeEnum entityType);
        Task<ResourceDto?> UpdateEntity(string id, string sourceType, EntityTypeEnum entityType, int newVersion);
    }
}