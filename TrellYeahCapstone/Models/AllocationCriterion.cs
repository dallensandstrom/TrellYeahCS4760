using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrellYeahCapstone.Models
{
    public class AllocationCriterion
    {
        public int Id { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal MinScorePercentage { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal MaxScorePercentage { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal AllocationPercentage { get; set; }
    }
}
