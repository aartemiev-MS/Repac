using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Repac.Data.Models
{
    public class ScanDTO
    {
        public Guid ScanId { get; set; }
        public Guid ContainerTagId { get; set; }
        public Guid ScanSessionId { get; set; }
    }
}
