using System;
using System.Collections.Generic;
using System.Text;

namespace Repac.Data.Models.DTOs
{
    public class TakeOutLocationDTO
    {
        public string LocationName { get; set; }
        public string PhoneNumber { get; set; }
        public string ResponsiblePersonName { get; set; }
        public int AvailibleCredits { get; set; }
    }
}
