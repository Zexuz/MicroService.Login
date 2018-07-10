using System;

namespace MicroService.Login.Models.RepoService
{
    public class Transaction
    {
        public int            Id         { get; set; }
        public int            NrOfCoins  { get; set; }
        public int            Fee        { get; set; }
        public DateTimeOffset Created    { get; set; }
        public int            FromUserId { get; set; }
        public int            ToUserId   { get; set; }
    }
}