using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Repac.Data.Models
{
    public class Scan
    {
        [Key]
        public Guid ScanId { get; set; }
        public Guid ContainerTagId { get; set; }
        public Guid ScanSessionId { get; set; }
        public DateTime Timestamp { get; set; }

    }
}
