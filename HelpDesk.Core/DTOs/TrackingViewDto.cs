using System;
using System.Collections.Generic;

namespace HelpDesk.Core.DTOs
{
    // What the public tracking page shows. It never carries ids or anything internal.
    public class TrackingViewDto
    {
        public string OrderNumber { get; set; }
        public string Company { get; set; }
        public string CompanyPhone { get; set; }
        public string CompanyEmail { get; set; }
        public string Customer { get; set; }
        public string ServiceType { get; set; }
        // open, inProgress, completed or approved
        public string Status { get; set; }
        // 1 to 4, for the step indicator
        public int Step { get; set; }
        public int Progress { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<TrackingActivityDto> Activities { get; set; } = new List<TrackingActivityDto>();
    }

    public class TrackingActivityDto
    {
        // First name only.
        public string Who { get; set; }
        public DateTime When { get; set; }
        public string Text { get; set; }
        public bool IsSolution { get; set; }
    }

    // The link of an order, for the staff.
    public class TrackingLinkDto
    {
        public string Url { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
