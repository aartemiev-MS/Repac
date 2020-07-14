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

        public string BankCardId { get; set; }
        public int PaymentSystemType { get; set; }

        public int Credits { get; set; }

        public void Scan(bool scanDirection) => this.Credits = scanDirection ? this.Credits += 1 : this.Credits -= 1;

    }
}
