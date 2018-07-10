using System;

namespace MicroService.Login.Models.RepoService
{
    public class Review
    {
        public int            Id         { get; set; }
        public int            UserId     { get; set; }
        public bool           IsPositive { get; set; }
        public string         Text       { get; set; }
        public DateTimeOffset Updated    { get; set; }
        public bool           Valid      { get; set; }
    }
}