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
            public const string PendingInspection = "Pending Inspection";
            public const string SentBack = "Sent Back";

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
            public const string BoilerRegistration = "Boiler Registration";
            public const string BoilerAmendment = "Boiler Amendment";
            public const string BoilerRenewal = "Boiler Renewal";
            public const string BoilerManufactureRegistration = "Boiler Manufacture Registration";
            public const string BoilerManufactureAmend= "Boiler Manufacture Amendment";
            public const string BoilerManufactureRenewal = "Boiler Manufacture Renewal";
            public const string Stplregistration = "Stpl Registration";
            public const string StplAmendment = "Stpl Amendment";
            public const string Stplrenew = "Stpl Renewal";
            public const string EconomiserRegistration = "Economiser Registration";
            public const string Economiserrenew = "Economiser Renewal";
            public const string BoilerRepairerRegistration = "Boiler Repairer Registration";
            public const string BoilerRepairerRenew = "Boiler Repairer Renewal";
            public const string WelderRegistration = "Welder Registration";
            public const string WelderRenew = "Welder Renewal";
            public const string BoilerDrawingRegistration = "Boiler Drawing Registration";
            public const string BoierDrawingRenewal = "Boier Drawing Renewal";
            public const string BoilerInspection = "Boiler Inspection";
            public const string SMTCRegistration = "SMTC Registration";
            public const string CompetentPersonRegistration = "Competent Person Registration";
            public const string CompetentPersonRenewal = "Competent Person Renewal";
            public const string CompetentEquipmentRegistration = "Competent Equipment Registration";
            public const string CompetentEquipmentRenewal = "Competent Equipment Renewal";
            public const string BOECertificate = "BOE Certificate";
            public const string FOECertificate = "FOE Certificate";
            public const string BOAttendantCertificate = "BO Attendant Certificate";
            public const string FOAttendantCertificate = "FO Attendant Certificate";
            public const string HazardousWorkerRegistration = "Hazardous Worker Registration";
            public const string BoilerComponentFitting = "Boiler Component Fitting";
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
