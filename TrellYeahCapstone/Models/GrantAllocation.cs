using System.ComponentModel.DataAnnotations.Schema;

namespace TrellYeahCapstone.Models
{
    public class GrantAllocation
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrentRoundAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal PreviousRoundAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal CutoutPercentage { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
