namespace RajFabAPI.Constants
{
    public static class AppConstants
    {
        // Example: public const string DefaultStatus = "Pending";
        // Add your application-level constants here

        public static class ApplicationStatus
        {
            public const string Pending = "Pending";
            public const string Approved = "Approved";
            public const string Rejected = "Rejected";
            public const string Forwarded = "Forwarded";
            public const string ReturnedToApplicant = "Returned to applicant";

        }

        public static class Roles
        {
            public const string Employer = "Employer";
            public const string Manager = "Manager";
            public const string Contractor = "Contractor";
        }

        public static class ApplicationTypeNames
        {
            public const string NewEstablishment = "New Establishment Registration";
            public const string FactoryAmendment = "Factory Amendment";
            public const string FactoryRenewal = "Factory Renewal";
            public const string MapApproval = "Map Approval";
            public const string MapApprovalAmendment = "Map Approval Amendment";
            public const string ManagerChange = "Manager Change";
            public const string FactoryLicense = "Factory License";
            public const string FactoryLicenseAmendment = "Factory License Amendment";
            public const string FactoryLicenseRenewal = "Factory License Renewal";
            public const string FactoryCommencementCessation = "Factory Commencement And Cessation";
            public const string Appeal = "Appeal";
        }

        public static class BoilerApplicationType
        {
            public const string New = "new";
            public const string Renew = "renew";
            public const string Repair = "repair";
            public const string Modification = "modification";
            public const string Transfer = "transfer";
            public const string Closure = "closure";
        }

        public static class UserTypeNames
        {
            public const string Citizen = "citizen";
            public const string Admin = "admin";
            public const string Department = "department";
        }

        // Add more nested classes or constants as needed
    }
}
