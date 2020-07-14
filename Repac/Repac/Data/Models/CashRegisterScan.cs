using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Repac.Data.Models
{
    class CashRegisterScan
    {
        [Key]
        public Guid ScanId { get; set; }
        public Guid TagId { get; set; }
        public Guid UserId { get; set; }
        public bool ScanDirection { get; set; }
        public int ResultCreditValue { get; set; }
        public DateTime Timestamp { get; set; }
        
    }
}
