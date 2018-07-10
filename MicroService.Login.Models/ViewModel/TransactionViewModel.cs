using System;

namespace MicroService.Login.Models.ViewModel
{
    public class TransactionViewModel
    {
        public int             NrOfCoins { get; set; }
        public int             Fee       { get; set; }
        public DateTimeOffset  Created   { get; set; }
        public bool            AmISender { get; set; }
        public User            Partner   { get; set; }
        public ReviewViewModel Review    { get; set; }

        public class User
        {
            public string Username         { get; set; }
            public string DomainName       { get; set; }
            public bool   IsDomainVerified => DomainName != null;
        }
    }


    public class ReviewViewModel
    {
        public int            Id          { get; set; }
        public bool           IsMy        { get; set; }
        public bool           IsPositive  { get; set; }
        public string         Text        { get; set; }
        public DateTimeOffset LastUpdated { get; set; }
    }
}