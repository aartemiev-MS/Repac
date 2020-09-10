using System;
using System.ComponentModel.DataAnnotations;

namespace Repac.Data.Models
{
    class UserDTO
    {
        public Guid UserId { get; set; }
        public Guid KeyChainId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime RegistryDate { get; set; }
        public int RemainingCredits { get; set; }
    }
}
