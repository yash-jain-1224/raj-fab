using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
        public class FeeResult
        {
            public decimal? Amount { get; set; }
        }
        public class FeeRequest
        {
            public string FormCategory { get; set; }
            public string FormType { get; set; }
            public string? CategorySubType { get; set; }
            public decimal GivenHP { get; set; }
            public int TotalPerson { get; set; }
            public string Type { get; set; } // new, update, renew
        }
}