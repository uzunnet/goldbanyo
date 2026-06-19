using DesaDoor.Ortak.Modellers.Audit;
using System;

namespace DesaDoor.Ortak.Modellers.Audit
{
    public class EntityBase
    {
        public int Id { get; set; }
        public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
        public DateTime? GuncellenmeTarihi { get; set; }
        public bool SilindiMi { get; set; }
        public DateTime? SilinmeTarihi { get; set; }
    }
}