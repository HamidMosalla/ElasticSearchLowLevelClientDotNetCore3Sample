using System.Collections.Generic;

namespace ElasticSearchLowLevelClientDotNetCore3Sample.Models
{
    public class Avatar
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Country { get; set; }
        public string CurrentPosition { get; set; }
        public string About { get; set; }
        public int Age { get; set; }
        public List<string> Interests { get; set; }
    }
}
