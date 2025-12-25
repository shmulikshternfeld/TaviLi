using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaviLi.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; }

        public int MissionId { get; set; }
        public virtual Mission Mission { get; set; } = null!;

        public required string ReviewerId { get; set; }
        public virtual User Reviewer { get; set; } = null!;

        public required string RevieweeId { get; set; }
        public virtual User Reviewee { get; set; } = null!;

        public int Rating { get; set; } // 1-5
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
