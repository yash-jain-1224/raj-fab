using System;
using System.ComponentModel.DataAnnotations;

namespace RajFabAPI.DTOs
{

    #region RajSSO Login, it will be used to authenticate user using rajsso, it will return user details along with roles and other details which are coming from rajsso, this API is used in mobile app for login using rajsso, and also it can be used in future for web login using rajsso.
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
    public class SsoLoginRequest
    {
        public string Application { get; set; } = "RAJFAB";
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class SsoLoginResponse
    {
        public bool valid { get; set; }
        public string message { get; set; }
        public List<object> roles { get; set; }
        public string displayName { get; set; }
        public string dateOfBirth { get; set; }
        public string designation { get; set; }
        public string mobile { get; set; }
        public string mailPersonal { get; set; }
        public string aadhaarId { get; set; }
        public string bhamashahId { get; set; }
        public object bhamashahMId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public object oldSSOIDs { get; set; }
        public string janaadhaarId { get; set; }
        public object janaadhaarMId { get; set; }
        public string userType { get; set; }
        public string userStatus { get; set; }
        public string mfa { get; set; }
        public object sansthaAadhaar { get; set; }
    }
    #endregion
    #region Sso User Profile,used to fetch user details using samAccountName. its not mandatory to use this API, but it is used in some cases where we need to fetch user details using samAccountName(SSOID).
    public class UserProfileRequest
    {
        public string samAccountName { get; set; }
    }
    public class SsoUserProfileRequest
    {
        public string SSOID { get; set; }
        public string WSUSERNAME { get; set; }
        public string WSPASSWORD { get; set; }
    }
    public class SsoUserProfileResponse
    {
        public string SSOID { get; set; }
        public string aadhaarId { get; set; }
        public string bhamashahId { get; set; }
        public string bhamashahMemberId { get; set; }
        public string displayName { get; set; }
        public string dateOfBirth { get; set; }
        public string gender { get; set; }
        public string mobile { get; set; }
        public string telephoneNumber { get; set; }
        public string ipPhone { get; set; }
        public string mailPersonal { get; set; }
        public string postalAddress { get; set; }
        public string postalCode { get; set; }
        public string l { get; set; }
        public string st { get; set; }
        public string jpegPhoto { get; set; }
        public string designation { get; set; }
        public string department { get; set; }
        public string mailOfficial { get; set; }
        public string employeeNumber { get; set; }
        public string departmentId { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string oldSSOIDs { get; set; }
        public string janaadhaarId { get; set; }
        public string janaadhaarMemberId { get; set; }
        public string userType { get; set; }
        public string mfa { get; set; }
        public string sansthaAadhaar { get; set; }
    }
    #endregion
}