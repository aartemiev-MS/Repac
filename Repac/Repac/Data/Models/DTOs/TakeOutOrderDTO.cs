using System;
using System.Collections.Generic;
using System.Text;

namespace Repac.Data.Models.DTOs
{
    public class TakeOutOrderDTO
    {
        public Guid Id { get; set; }
        public Guid ClerkId { get; set; }
        public Guid ScanSessionId { get; set; }
        public int ContainersEstimated { get; set; }
        public TakeOutLocationDTO Location { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
