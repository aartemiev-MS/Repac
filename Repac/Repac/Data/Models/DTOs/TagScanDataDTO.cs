using System;
using System.Collections.Generic;
using System.Text;

namespace Repac.Data.Models.DTOs
{
    class TagScanDataDTO
    {
        public Guid TagId { get; set; }
        public Guid ScanId { get; set; }
        public Guid ScanSessionId { get; set; }
        public Guid ScannerId { get; set; }
        public Guid UserId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
