using System;

namespace Repac.Data.Models
{
    class UserDTO
    {
        public Guid UserId { get; set; }
        public Guid KeyChainId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime RegistryDate { get; set; }
        public int OwnedCredits { get; set; }
        public int UsedCredits { get; set; }
        public int AvailibleCredits
        {
            get
            {
                return OwnedCredits - UsedCredits;
            }
        }
    }
}
