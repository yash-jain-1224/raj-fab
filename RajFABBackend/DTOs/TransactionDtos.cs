using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    public class CreateTransactionDto
    {
        public string PrnNumber { get; set; }
        public string ModuleId { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal? PaidAmount { get; set; }
        public string? Status { get; set; }
        public string? ApplicationId { get; set; }
        public string? PaymentReq { get; set; }
        public string? PaymentRes { get; set; }
        public string? Remarks { get; set; }
        public string? Message { get; set; }
    }

    public class UpdateTransactionDto
    {
        public string PrnNumber { get; set; }
        public string ModuleId { get; set; }
        public string UserId { get; set; }
        public decimal Amount { get; set; }
        public decimal? PaidAmount { get; set; }
        public string Status { get; set; }
        public string? ApplicationId { get; set; }
        public string? PaymentReq { get; set; }
        public string? PaymentRes { get; set; }
    }
}
