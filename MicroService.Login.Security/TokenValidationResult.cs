using System.Collections.Generic;
using MicroService.Login.Models.RepoService;

namespace MicroService.Login.Security
{
    public class TokenValidationResult
    {
        public User                       TokenOwner { get; set; }
        public bool                       Success    { get; set; }
        public Dictionary<string, string> Token      { get; set; }
        public TokenValidationError       Error      { get; set; }
    }
}