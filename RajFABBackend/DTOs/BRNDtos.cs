using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{

    internal class BRNRequest
    {
        public string user_id { get; set; } = "rajfab"; // hardcoded
        public string BRN { get; set; }
        public string salt { get; set; } = "46899062"; // hardcoded
        public string user_pwd { get; set; } = "ODQ0RTJEMDc4NkE4NTZCRTc3Q0IzNjExNjNDRjVDOUY="; // hardcoded
    }
    public class BRNResponse
    {
        public int Status { get; set; }
        public string BRN { get; set; }
        public string Area { get; set; }
        public string District { get; set; }
        public string Tehsil { get; set; }
        public string Village { get; set; }
        public string LocalBody { get; set; }
        public string Ward { get; set; }
        public string BO_Name { get; set; }
        public string BO_HouseNo { get; set; }
        public string BO_Lane { get; set; }
        public string BO_Locality { get; set; }
        public string BO_PinCode { get; set; }
        public string BO_TelNo { get; set; }
        public string BO_Email { get; set; }
        public string BO_PanNo { get; set; }
        public string BO_TanNo { get; set; }
        public string HO_Name { get; set; }
        public string HO_HouseNo { get; set; }
        public string HO_Lane { get; set; }
        public string HO_Locality { get; set; }
        public string HO_PinCode { get; set; }
        public string HO_TelNo { get; set; }
        public string HO_EMail { get; set; }
        public string HO_PanNo { get; set; }
        public string HO_TanNo { get; set; }
        public string NIC_Code { get; set; }
        public string Year { get; set; }
        public string Ownership { get; set; }
        public int Total_Person { get; set; }
        public string ActAuthorityRegNo { get; set; }
        public string Applicant_Name { get; set; }
        public string Applicant_No { get; set; }
        public string Applicant_EMail { get; set; }
        public string Applicant_Address { get; set; }
    }
}