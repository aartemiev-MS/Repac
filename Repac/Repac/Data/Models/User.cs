using System;
using System.ComponentModel.DataAnnotations;

namespace Repac.Data.Models
{
    class User
    {
        [Key]
        public Guid UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime RegistryDate { get; set; }
        public int OwnedCredits { get; set; }
        public int UsedCredits { get; set; }
        //public int RemainingCredits
        //{
        //    get
        //    {
        //        return OwnedCredits - RemainingCredits < 0 ? 0 : OwnedCredits - RemainingCredits;
        //    }
        //}

    }
}
