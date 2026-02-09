using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{
    // Token API
    public class GenerateTokenRequest
    {
        public string SSOID { get; set; }
        public string SecretKey { get; set; }
    }

    public class GenerateTokenResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public string Exception { get; set; }
        public TokenData? Data { get; set; }
    }

    public class TokenData
    {
        public string SsoId { get; set; }
        public string JwtId { get; set; }
        public string EncryptedToken { get; set; }
    }

    // Signed XML API
    public class GenerateSignedXmlRequest
    {
        public Stream PdfFileStream { get; set; }
        public string PdfFileName { get; set; }
        public string ApplicationCode { get; set; }
        public string AspId { get; set; }
        public int Xcord { get; set; }
        public int Ycord { get; set; }
        public string Prn { get; set; }
        public string SignatureOnPageNumber { get; set; }
        public string PersonName { get; set; }
        public string PersonDesignation { get; set; }
        public string PersonLocation { get; set; }
        public string SignatureSize { get; set; }
        public string ResponseUrl { get; set; }
    }

    public class SignedXmlResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public SignedXmlData? Data { get; set; }
    }

    public class SignedXmlData
    {
        public string ResponseCode { get; set; }
        public string ResponseMsg { get; set; }
        public string SignedXMLData { get; set; }
        public string Prn { get; set; }
        public string TxnId { get; set; }
    }

    // Signed PDF API
    public class SigningPdfRequest
    {
        public string EsignResponseBase64 { get; set; }
        public string Prn { get; set; }
        public string TxnId { get; set; }
        public string ApplicationCode { get; set; }
        public string AspId { get; set; }
    }

    public class SignedPdfResponse
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public SignedPdfData? Data { get; set; }
    }

    public class SignedPdfData
    {
        public string ResponseCode { get; set; }
        public string ResponseMsg { get; set; }
        public string SignedPDFBase64 { get; set; }
        public string Prn { get; set; }
        public string TxnId { get; set; }
    }
    public class ESignRequest
    {
        public IFormFile PdfFile { get; set; }
        public string ApplicationCode { get; set; }
        public string AspId { get; set; }
        public int Xcord { get; set; }
        public int Ycord { get; set; }
        public string Prn { get; set; }
        public string SignatureOnPageNumber { get; set; }
        public string PersonName { get; set; }
        public string PersonDesignation { get; set; }
        public string PersonLocation { get; set; }
        public string SignatureSize { get; set; }
        public string ResponseUrl { get; set; }

        public string SsoId { get; set; }
        public string SecretKey { get; set; }
    }

    public class ESignResult
    {
        public byte[] SignedPdfBytes { get; set; }
        public string Prn { get; set; }
        public string TxnId { get; set; }
    }
    public class ESignSettings
    {
        public string TokenUrl { get; set; }
        public string SignedXmlUrl { get; set; }
        public string SignPdfUrl { get; set; }
        public string ApplicationCode { get; set; }
        public string AspId { get; set; }
        public string ResponseUrl { get; set; }
        public string SecretKey { get; set; }
        public string SsoId { get; set; }
        public string EspRedirectUrl { get; set; }
    }

}
