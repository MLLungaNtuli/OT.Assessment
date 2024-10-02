using System;
using System.Collections.Generic;

namespace OT.Assessment.App.Models
{
    public class Player
    {
        public Guid AccountId { get; set; }
        public string Username { get; set; }
        public List<CasinoWager> CasinoWagers { get; set; }
    }
}
