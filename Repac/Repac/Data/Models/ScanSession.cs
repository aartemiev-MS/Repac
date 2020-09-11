using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Repac.Data.Models
{
    class ScanSession
    {
        public Guid ScanSessionId { get; set; }
        public Guid ScannerId { get; set; }
        public int CheckPointType { get; set; }
        public Guid UserId { get; set; }
        public DateTime ScanSessionStartTimestamp { get; set; }
        public DateTime ScanSessionEndTimestamp { get; set; }
    }
}
