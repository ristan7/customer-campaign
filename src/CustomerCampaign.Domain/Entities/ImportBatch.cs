using System;
using System.Collections.Generic;
using System.Text;

namespace CustomerCampaign.Domain.Entities
{
    public class ImportBatch
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public DateTime ImportedAtUtc { get; set; } = DateTime.UtcNow;
        public int ImportedByUserId { get; set; }

        public int TotalRows { get; set; }
        public int MatchedRows { get; set; }
        public int UnmatchedRows { get; set; }
    }
}
