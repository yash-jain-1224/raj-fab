using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RajFabAPI.Models
{

    public class ESignTransaction
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public string EncryptedPrn { get; set; }
        public string PrnHash { get; set; }
        public string TxnId { get; set; }

        public string Status { get; set; } // INITIATED / SIGNED / FAILED
        public string SignedPdfPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}