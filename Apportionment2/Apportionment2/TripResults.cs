using System;
using System.Collections.Generic;
using System.Text;

namespace Apportionment2
{
    public class TripResults
    {
        //public string id { get; set; }
        public string TripId { get; set; }
        public string CostId { get; set; }
        public string UserId { get; set; }
        public double UserCostShare { get; set; }
        public double Spend { get; set; }
        public double UserMustPay { get; set; }
        public double Delta { get; set; }
    }
}
