using System;
using MicroService.Common.Core.Databases.Repository;
using MongoDB.Bson.Serialization.Attributes;

namespace MicroService.Login.Repo.MongoDb.Models
{
    public class DomainScraper : IEntity<string>
    {
        [BsonId]
        public string Id { get; set; }

        public string Website { get; set; }
        public Guid   Code    { get; set; }
        public int    UserId  { get; set; }
    }
}