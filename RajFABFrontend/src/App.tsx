import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import Index from "./pages/Index";
import Portal from "./pages/Portal";
import SSOLogin from "./pages/SSOLogin";
import SelectMode from "./pages/SelectMode";
import NotFound from "./pages/NotFound";
import AdminLayout from "./components/layouts/AdminLayout";
import InspectorLayout from "./components/layouts/InspectorLayout";
import InspectorDashboard from "./pages/inspector/InspectorDashboard";
import InspectorApplicationList from "./pages/inspector/InspectorApplicationList";
import BoilerApplicationAssignmentPage from "./pages/admin/BoilerApplicationAssignmentPage";
import UserLayout from "./components/layouts/UserLayout";
import AdminDashboard from "./pages/admin/AdminDashboard";
import UserDashboard from "./pages/user/UserDashboard";
import NewRegistration from "./pages/user/NewRegistration";
// import BoilerRegistrationNewForm from "./pages/boilernew/boilerform";
import FactoryRegistration from "./pages/user/FactoryRegistration";
import FactoryMapApproval from "./pages/user/FactoryMapApproval";
import LicenseRenewal from "./pages/user/LicenseRenewal";
import InspectionRequest from "./pages/user/InspectionRequest";
import ComplianceCertificate from "./pages/user/ComplianceCertificate";
import AnnualReturns from "./pages/user/AnnualReturns-old";
import GrievanceForm from "./pages/user/GrievanceForm";
import FormManagement from "./pages/admin/ModuleManagementPage";
import FactoryTypeManagement from "./pages/admin/FactoryTypeManagement";
// import DocumentType from "./pages/admin/DocumentTypePage";
import PoliceStationPage from "./pages/admin/PoliceStationPage";
import RailwayStationPage from "./pages/admin/RailwayStationPage";
import OfficePostManagementPage from "./pages/admin/OfficePostManagementPage";
import UserCreationPage from "./pages/admin/UserManagementPage";
import UserPrivilegesPageEnhanced from "./pages/admin/UserPrivilegesPageEnhanced";
import DivisionManagementPage from "./pages/admin/DivisionManagementPage";
import DistrictManagementPage from "./pages/admin/DistrictManagementPage";
import AreaManagementPage from "./pages/admin/AreaManagementPage";
import CityManagementPage from "./pages/admin/CityManagementPage";
import UserHierarchyManagement from "./pages/admin/UserHierarchyPage";
import RoleP from "./pages/admin/RolePrivilegesPageEnhanced";
import BoilerServices from "./pages/BoilerServices";
import BoilerRegistrationForm from "./pages/boiler/BoilerRegistrationForm";
import BoilerRenewalForm from "./pages/boiler/BoilerRenewalForm";
import BoilerModificationForm from "./pages/boiler/BoilerModificationForm";
import BoilerTransferForm from "./pages/boiler/BoilerTransferForm";
import ApplicationReviewList from "./pages/admin/ApplicationReviewList";
import ApplicationDetailView from "./pages/admin/ApplicationDetailView";
import LicenseRenewalReview from "./pages/admin/LicenseRenewalReview";
import TrackApplications from "./pages/user/TrackApplications";
import ApplicationDetails from "./pages/user/ApplicationDetails";
import AmendFactoryRegistration from "./pages/user/AmendFactoryRegistration";
import AmendFactoryMapApproval from "./pages/user/AmendFactoryMapApproval";
import AmendLicenseRenewal from "./pages/user/AmendLicenseRenewal";
import SelectMapApproval from "./pages/user/SelectMapApproval";
import FactoryRegistrationFee from "./pages/user/FactoryRegistrationFee";
import AmendmentMapApprovalList from "./pages/user/AmendmentMapApprovalList";
import AmendmentLicenseRenewalList from "./pages/user/AmendmentLicenseRenewalList";
import AmendmentFactoryRegistrationList from "./pages/user/AmendmentFactoryRegistrationList";
import FactoryClosureList from "./pages/user/FactoryClosureList";
import FactoryClosure from "./pages/user/FactoryClosure";
import ManagerChange from "./pages/user/ManagerChange";
import FactoryLicense from "./pages/user/FactoryLicense";
import FactoryLicenseNew from "./components/user/factoryLicense/FactoryLicenseNew";
import FactoryLicenseRenew from "./components/user/factoryLicense/FactoryLicenseRenew";
import FactoryClosureReviewList from "./pages/admin/FactoryClosureReviewList";
import FactoryClosureDetailView from "./pages/admin/FactoryClosureDetailView";
import ManagerChangeReviewList from "./pages/admin/ManagerChangeReviewList";
import ManagerChangeDetailView from "./pages/admin/ManagerChangeDetailView";
import FactoryLicenseReviewList from "./pages/admin/FactoryLicenseReviewList";
import FactoryLicenseDetailView from "./pages/admin/FactoryLicenseDetailView";
import GenerateFactoryLicenseCertificate from "./pages/admin/GenerateFactoryLicenseCertificate";
import NewEstablishment from "./pages/user/NewEstablishment";
import NewEstablishmentForm from "./components/user/newEstablishment/NewEstablishmentForm";
import FactoryCommenceAndCessation from "./pages/user/FactoryCommenceandCessation";
import FactoryForm7 from "./pages/user/FactoryForm7";
import AnnualReturnForm25 from "./components/user/annualReturns/AnnualReturnForm25";
import AnnualReturnList from "./pages/user/AnnualReturns";
import AnnualReturnDetailView from "./components/user/annualReturns/AnnualReturnDetailView";
import Form27Wizard from "./pages/user/Form27/Form27Wizard";
import Form6Wizard from "./pages/user/Form6Wizard";
import MapApproval from "./pages/user/MapApproval";
import MapApprovalForm from "./components/user/map-approval/MapApprovalForm";
// admin page
import AdminProtectedRoute from "./components/utils/AdminProtectedRoute";
import UserProtectedRoute from "./components/utils/UserProtectedRoute";
import { AuthProvider } from "./utils/AuthProvider";
import AssignRolesToUserManagementPage from "./pages/admin/AssignRolesToUserManagementPage";
import PostManagementPage from "./pages/admin/PostManagementPage";
import ActManagementPage from "./pages/admin/ActManagementPage";
import RuleManagementPage from "./pages/admin/RuleManagementPage";
import OfficePostApplicationPrivilegesPage from "./pages/admin/OfficePostApplicationPrivilegesPage";
import WorkerRangeManagementPage from "./pages/admin/WorkerRangeManagementPage";
import FactoryCategoryManagementPage from "./pages/admin/FactoryCategoryManagementPage";
import OfficePostInspectionPrivilegesPage from "./pages/admin/OfficePostInspectionPrivilegesPage";
import OfficeLevelManagementPage from "./pages/admin/OfficeLevelManagementPage";
import OfficePostLevelManagementPage from "./pages/admin/OfficePostLevelManagementPage";
import ApplicationWorkFlowManagementPage from "./pages/admin/ApplicationWorkFlowManagementPage";
import BoilerWorkflowManagementPage from "./pages/admin/BoilerWorkflowManagementPage";
import OfficeManagementPage from "./pages/admin/OfficeManagemnetPage";
import CategorySelection from "./pages/user/CategorySelection";
import EstablishmentReviewList from "./pages/admin/EstablishmentReviewList";
import VerifyRegistration from "./pages/user/VerifyAlreadyRegister";
import FactoryServices from "./pages/FactoryServices";
import Form2 from "./pages/user/Form2";
import ApplicationView from "./pages/user/ApplicationView";
import GenerateEstFactoryCertificate from "./pages/admin/GenerateEstFactoryCertificate";
import { LocationProvider } from "./context/LocationContext";
import ManagerChangeForm from "./components/user/managerChange/ManagerChangeForm";
import AppealPage from "./pages/user/Appeal";
import AppealForm from "./components/user/appeal/AppealForm";
import AppealDetailsPage from "./components/user/appeal/AppealDetails";
import CommenceandCessationForm from "./components/user/CommenceandCessation/CommenceandCessationForm";
import CommenceAndCessationPage from "./pages/user/CommenceandCessation";
import CommenceandCessationDetails from "./components/user/CommenceandCessation/CommenceandCessationDetails";
import BRNDetails from "./pages/user/BRNDetails";
import UserLayoutNoSidebar from "./components/layouts/UserLayoutNoSidebar";

import BoilerRegistrationNew from "./pages/boilernew/boileregistrationnew";
import BoilerRenewalNew from "./pages/boilernew/boilerrenewalnew";
import BoilerRepairModificationNew from "./pages/boilernew/boilermodificaion";
import BoilerTransferClosureNew from "./pages/boilernew/boilerclosure";
import BoilerErectorRegistrationNew from "./pages/boilernew/boilererectorregistration";
import BoilerRepairerList from "./pages/boilernew/boilerRepairerList";
import BoilerRepairerRenewal from "./pages/boilernew/boilerRepairerRenewal";
import BoilerRepairerClosure from "./pages/boilernew/boilerRepairerClosure";
import BoilerTransferNew from "./pages/boilernew/boilertransfer";
import BoilerClosureNew from "./pages/boilernew/boilerclosure";
import BoilerRepairNew from "./pages/boilernew/boilerrepair";
import BoilerModificationNew from "./pages/boilernew/boilermodificaion";
import BoilerManufactureDrawing from "./pages/boilernew/boilermanufacturedrawing";
import StplNew from "./pages/boilernew/stplnew";
import SteamPipelineList from "./pages/boilernew/steamPipelineList";
import BoilerComponentFittingNew from "./pages/boilernew/boilercomponentfittingNew";
import BoilerComponentFittingView from "./pages/boilernew/boilercomponentfittingView";
import BoilerManufactureDrawingView from "./pages/boilernew/boilerManufactureDrawingView";
import BoilerManufactureRegistrationNew from "./pages/boilermanufacture/BoilerManufactureRegistrationNew";
import BoilerManufactureRenewalNew from "./pages/boilermanufacture/boilermanufacturerenewal";
import BoilerManufactureClosureNew from "./pages/boilermanufacture/boilermanufactureClosure";
import BoilerRegistrationList from "./pages/boilernew/boilerRegistrationList";
import ErrorPage from "./pages/ErrorPage";

// Economiser flows
import EconomiserRegistration from "./pages/boilernew/boilereconomiserregistration";
import EconomiserList from "./pages/boilernew/economiserList";
import EconomiserClosure from "./pages/boilernew/economiserClosure";
import EconomiserRenewal from "./pages/boilernew/economiserRenewal";
import EconomiserView from "./pages/boilernew/economiserView";

import BoilerClosureList from "./pages/boilernew/boilerClosureList";
import BoilerModificationRepairList from "./pages/boilernew/boilerModificationList";

// Welder flows
import WelderTestApplication from "./pages/boilernew/weldertestapplication";
import WelderList from "./pages/boilernew/welderList";
import WelderRenewal from "./pages/boilernew/welderRenewal";
import WelderView from "./pages/boilernew/welderView";

import BoilerManufactureList from "./pages/boilermanufacture/boilerManufactureRegistrationList";
import GenerateMapApprovalCertificate from "./pages/admin/GenerateMapApprovalCertificate";
import CompetentPersonRegistrationForm from "./pages/Competant person/CompetentPersonRegistrationForm";
import CompetentPersonListPage from "./pages/Competant person/CompetentPersonListPage";
import CompetentPersonDetailsPage from "./pages/Competant person/CompetentPersonDetailsPage";
import CompetentPersonEquipmentRegistrationForm from "./pages/Competant person/CompetentPersonEquipmentRegistrationForm";
import CompetentPersonEquipmentView from "./pages/Competant person/CompetentPersonEquipmentView";
import BOECertificateRegistrationForm from "./pages/user/BOECertificateRegistrationForm";
import SMTCRegistrationForm from "./pages/user/SMTCRegistrationForm";
import FOECertificateRegistrationForm from "./pages/user/FOECertificateRegistrationForm";
import FOAttendantCertificateRegistration from "./pages/user/FOAttendantCertificateRegistration";
import BOAttendantCertificateRegistration from "./pages/user/BOAttendantCertificateRegistration";
import HazardousWorkerRegistration from "./pages/user/HazardousWorkerRegistration";
import HazardousWorkerView from "./pages/user/HazardousWorkerView";
import DocumentList from "./pages/user/Documents";

const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: false,
    },
    mutations: {
      retry: false,
    },
  },
});

const RedirectToExternal = () => {
  window.location.href = "http://10.70.234.214:81";
  return null;
};

const App = () => (
  <QueryClientProvider client={queryClient}>
    <TooltipProvider>
      <LocationProvider>
        <Toaster />
        <Sonner />
        <AuthProvider>
          <BrowserRouter>
            <Routes>
              {/* <Route path="/" element={<Index />} /> */}
              <Route path="/" element={<RedirectToExternal />} />
              <Route path="/portal" element={<Portal />} />
              <Route path="/sso-landing" element={<SSOLogin />} />
              <Route path="/select-mode" element={<SelectMode />} />

              {/* Inspector Routes */}
              <Route
                path="/inspector"
                element={
                  <AdminProtectedRoute>
                    <InspectorLayout />
                  </AdminProtectedRoute>
                }
              >
                <Route index element={<InspectorDashboard />} />
                <Route path="applications" element={<InspectorApplicationList />} />
              </Route>

              {/* Admin Routes */}
              <Route
                path="/admin"
                element={
                  <AdminProtectedRoute>
                    <AdminLayout />
                  </AdminProtectedRoute>
                }
              >
                <Route index element={<AdminDashboard />} />
                <Route
                  path="factory-types"
                  element={<FactoryTypeManagement />}
                />
                <Route
                  path="licensing"
                  element={<div>Licensing & Applications</div>}
                />
                <Route
                  path="approvals"
                  element={<div>Approvals & Certification</div>}
                />
                <Route path="inspections" element={<div>Inspections</div>} />
                <Route path="payments" element={<div>Payments</div>} />
                <Route
                  path="reports"
                  element={<div>Reports & Analytics</div>}
                />
                <Route
                  path="training"
                  element={<div>Training Management</div>}
                />
                <Route path="grievances" element={<div>Grievances</div>} />
                <Route path="users" element={<div>User Management</div>} />
                <Route path="settings" element={<div>System Settings</div>} />
                {/* <Route path="document" element={<DocumentType />} /> */}
                <Route path="userprvleges" element={<RoleP />} />
                <Route
                  path="userhierarchy"
                  element={<UserHierarchyManagement />}
                />
                <Route
                  path="applications"
                  element={<ApplicationReviewList />}
                />
                <Route
                  path="applications/:applicationType/:applicationId/:applicationApprovalRequestId"
                  element={<ApplicationDetailView />}
                />
                <Route
                  path="boiler-assignment"
                  element={<BoilerApplicationAssignmentPage />}
                />

                {/* Department Operation Routes */}
                <Route
                  path="establishment-review"
                  element={<EstablishmentReviewList />}
                />
                {/* <Route
                path="establishment-review/:establishmentId/:applicationApprovalRequestId"
                element={<EstablishmentDetailsPage />}
              /> */}
                <Route
                  path="generate-factory-est-certificate/:establishmentId"
                  element={<GenerateEstFactoryCertificate />}
                />
                <Route
                  path="renewal-review"
                  element={<LicenseRenewalReview />}
                />
                <Route
                  path="closure-review"
                  element={<FactoryClosureReviewList />}
                />
                <Route
                  path="closure-review/:closureId"
                  element={<FactoryClosureDetailView />}
                />
                <Route
                  path="manager-change-review"
                  element={<ManagerChangeReviewList />}
                />
                <Route
                  path="manager-change-review/:noticeId"
                  element={<ManagerChangeDetailView />}
                />
                <Route
                  path="factory-license-review"
                  element={<FactoryLicenseReviewList />}
                />
                <Route
                  path="factory-license-review/:licenseId"
                  element={<FactoryLicenseDetailView />}
                />

                {/* Office Master Routes */}
                <Route
                  path="officemanagement"
                  element={<OfficeManagementPage />}
                />
                <Route
                  path="officepostmanagement"
                  element={<OfficePostManagementPage />}
                />
                <Route
                  path="assignapplicationprivilegestoofficepost"
                  element={<OfficePostApplicationPrivilegesPage />}
                />
                <Route
                  path="assigninspectionprivilegestoofficepost"
                  element={<OfficePostInspectionPrivilegesPage />}
                />
                <Route path="usermanagement" element={<UserCreationPage />} />

                <Route
                  path="roleassign"
                  element={<AssignRolesToUserManagementPage />}
                />
                <Route
                  path="applicationworkflowmanagement"
                  element={<ApplicationWorkFlowManagementPage />}
                />
                <Route
                  path="boilerworkflowmanagement"
                  element={<BoilerWorkflowManagementPage />}
                />
                <Route
                  path="officepostlevelmanagement"
                  element={<OfficePostLevelManagementPage />}
                />

                {/* Masters Routes */}
                <Route path="actmanagement" element={<ActManagementPage />} />
                <Route path="rulemanagement" element={<RuleManagementPage />} />
                <Route path="forms" element={<FormManagement />} />
                <Route path="postmanagement" element={<PostManagementPage />} />
                <Route
                  path="divisionmanagement"
                  element={<DivisionManagementPage />}
                />
                <Route
                  path="districtmanagement"
                  element={<DistrictManagementPage />}
                />
                <Route path="areamanagement" element={<AreaManagementPage />} />
                <Route path="citymanagement" element={<CityManagementPage />} />
                <Route path="railwaystation" element={<RailwayStationPage />} />
                <Route path="policestation" element={<PoliceStationPage />} />
                <Route
                  path="factory-types"
                  element={<FactoryTypeManagement />}
                />
                <Route
                  path="workersrangmaster"
                  element={<WorkerRangeManagementPage />}
                />
                <Route
                  path="factory-categories"
                  element={<FactoryCategoryManagementPage />}
                />
                <Route
                  path="office-levels"
                  element={<OfficeLevelManagementPage />}
                />
                <Route
                  path="generate-map-approval-certificate/:mapApprovalId"
                  element={<GenerateMapApprovalCertificate />}
                />
                <Route
                  path="generate-factory-license-certificate/:licenseId"
                  element={<GenerateFactoryLicenseCertificate />}
                />
              </Route>

              {/* User Routes */}
              <Route path="/user" element={<UserProtectedRoute />}>
                <Route element={<UserLayout />}>
                  <Route index element={<UserDashboard />} />
                  <Route
                    path="documents"
                    element={<DocumentList />} />
                  <Route
                    path="factory-services/dashboard"
                    element={<FactoryServices />}
                  />
                  <Route
                    path="new-establishment"
                    element={<NewEstablishment />}
                  />
                  <Route
                    path="new-establishment/create"
                    element={<NewEstablishmentForm />}
                  />
                  <Route
                    path="new-establishment/:establishmentId"
                    element={<NewEstablishmentForm />}
                  />
                  <Route path="form2" element={<Form2 />} />
                  <Route
                    path="new-registration"
                    element={<NewRegistration />}
                  />
                  <Route
                    path="new-registration/register-form"
                    element={<BoilerRegistrationNew />}
                  />
                  <Route
                    path="select-map-approval"
                    element={<SelectMapApproval />}
                  />
                  <Route
                    path="factory-registration"
                    element={<FactoryRegistration />}
                  />
                  <Route
                    path="Factory-CommenceAndCessation"
                    element={<FactoryCommenceAndCessation />}
                  />
                  <Route path="non-hazardous-factory" element={<FactoryForm7 />} />
                  {/* <Route path="Appeal-Form38" element={<AppealForm38 />} /> */}
                  <Route path="appeal" element={<AppealPage />} />
                  <Route path="appeal/create" element={<AppealForm />} />
                  <Route
                    path="appeal/:appealIdParam"
                    element={<AppealDetailsPage />}
                  />
                  <Route path="annual-returns" element={<AnnualReturnList />} />
                  <Route
                    path="annual-returns/create"
                    element={<AnnualReturnForm25 />}
                  />
                  <Route
                    path="annual-returns/:recordId"
                    element={<AnnualReturnDetailView />}
                  />
                  <Route
                    path="registration-fee"
                    element={<FactoryRegistrationFee />}
                  />
                  <Route
                    path="factory-map-approval"
                    element={<FactoryMapApproval />}
                  />
                  <Route path="license-renewal" element={<LicenseRenewal />} />
                  <Route
                    path="inspection-request"
                    element={<InspectionRequest />}
                  />
                  <Route
                    path="compliance-certificate"
                    element={<ComplianceCertificate />}
                  />
                  <Route path="annual-returns-1" element={<AnnualReturns />} />
                  <Route path="grievance-form" element={<GrievanceForm />} />
                  <Route path="apply" element={<div>Apply for License</div>} />
                  <Route
                    path="boiler-services/dashboard"
                    element={<BoilerServices />}
                  />
                  <Route
                    path="boiler-services/registration"
                    element={<BoilerRegistrationForm />}
                  />
                  <Route
                    path="boiler-services/renewal"
                    element={<BoilerRenewalForm />}
                  />
                  <Route
                    path="boiler-services/modification"
                    element={<BoilerModificationForm />}
                  />
                  <Route
                    path="boiler-services/transfer"
                    element={<BoilerTransferForm />}
                  />
                  <Route path="track" element={<TrackApplications />} />
                  <Route
                    path="applicationView/:applicationType/*"
                    element={<ApplicationView />}
                  />
                  <Route
                    path="applications/:applicationType/:applicationId"
                    element={<ApplicationDetails />}
                  />
                  <Route
                    path="amendment/map-approval"
                    element={<AmendmentMapApprovalList />}
                  />
                  <Route
                    path="amendment/license-renewal"
                    element={<AmendmentLicenseRenewalList />}
                  />
                  <Route
                    path="amendment/factory-registration"
                    element={<AmendmentFactoryRegistrationList />}
                  />
                  <Route
                    path="amend/factory-registration/:applicationId"
                    element={<AmendFactoryRegistration />}
                  />
                  <Route
                    path="amend/map-approval/:applicationId"
                    element={<AmendFactoryMapApproval />}
                  />
                  <Route
                    path="amend/license-renewal/:applicationId"
                    element={<AmendLicenseRenewal />}
                  />
                  <Route
                    path="factory-closure-list"
                    element={<FactoryClosureList />}
                  />
                  <Route
                    path="factory-closure/:factoryId"
                    element={<FactoryClosure />}
                  />
                  <Route path="manager-change" element={<ManagerChange />} />
                  <Route
                    path="manager-change/create"
                    element={<ManagerChangeForm />}
                  />
                  <Route
                    path="manager-change/:changeReqId"
                    element={<ManagerChangeForm />}
                  />
                  <Route path="factory-license" element={<FactoryLicense />} />
                  <Route
                    path="factory-license/create"
                    element={<FactoryLicenseNew />}
                  />
                  <Route
                    path="factory-license/:licenseId"
                    element={<FactoryLicenseNew />}
                  />
                  <Route
                    path="factory-license/renew/:registrationNumber"
                    element={<FactoryLicenseRenew />}
                  />
                  <Route
                    path="certificates"
                    element={<div>My Certificates</div>}
                  />
                  <Route path="payments" element={<div>Payment History</div>} />
                  <Route
                    path="training"
                    element={<div>Training Applications</div>}
                  />
                  <Route
                    path="grievances"
                    element={<div>Submit Grievance</div>}
                  />
                  <Route
                    path="inspections"
                    element={<div>Inspection Schedule</div>}
                  />
                  <Route path="profile" element={<div>Profile</div>} />
                  <Route
                    path="licence-application"
                    element={<Form27Wizard />}
                  />
                  <Route path="map-approval" element={<MapApproval />} />
                  <Route
                    path="map-approval/create"
                    element={<MapApprovalForm />}
                  />
                  <Route
                    path="map-approval/:factoryMapApprovalId"
                    element={<MapApprovalForm />}
                  />
                  <Route
                    path="commence-cessation"
                    element={<CommenceAndCessationPage />}
                  />
                  <Route
                    path="commence-cessation/create"
                    element={<CommenceandCessationForm />}
                  />
                  <Route
                    path="commence-cessation/:commenceAndCessationId"
                    element={<CommenceandCessationDetails />}
                  />
                  {/* boiler new urls */}

                  {/* boiler new routes */}
                  <Route
                    path="boiler-services/stpl"
                    element={<SteamPipelineList />}
                  />
                  <Route
                    path="boiler-services/stpl/list"
                    element={<SteamPipelineList />}
                  />
                  <Route path="boiler-services/stpl/create" element={<StplNew />} />
                  <Route
                    path="boiler-services/stpl/:changeReqId"
                    element={<StplNew />}
                  />

                  <Route
                    path="boiler-services/boilercomponentfitting"
                    element={<BoilerComponentFittingNew />}
                  />
                  <Route
                    path="boiler-services/boilercomponentfitting/:applicationId"
                    element={<BoilerComponentFittingView />}
                  />

                  <Route
                    path="boiler-services/boilermanufacturer/list"
                    element={<BoilerManufactureList />}
                  />

                  <Route
                    path="boiler-services/boilermanufacturereg"
                    element={<BoilerManufactureRegistrationNew />}
                  />
                  <Route
                    path="boiler-services/boilermanufacturereg/:changeReqId"
                    element={<BoilerManufactureRegistrationNew />}
                  />

                  <Route
                    path="boiler-services/boilermanufacturerenewal"
                    element={<BoilerManufactureRenewalNew />}
                  />
                  <Route
                    path="boiler-services/boilermanufacturerenewal/:changeReqId"
                    element={<BoilerManufactureRenewalNew />}
                  />
                  <Route
                    path="boiler-services/boilermanufacturecloser"
                    element={<BoilerManufactureClosureNew />}
                  />
                  <Route
                    path="boiler-services/boilermanufacturecloser/:changeReqId"
                    element={<BoilerManufactureClosureNew />}
                  />

                  <Route
                    path="boilerNew-services/list"
                    element={<BoilerRegistrationList />}
                  />

                  <Route
                    path="boilerNew-services/create"
                    element={<BoilerRegistrationNew />}
                  />

                   <Route
                    path="competent-person/create"
                    element={<CompetentPersonRegistrationForm/>}
                  />
                  <Route
                    path="competent-person/list"
                    element={<CompetentPersonListPage/>}
                  />
                  <Route
                    path="competent-person/*"
                    element={<CompetentPersonDetailsPage />}
                  />

                    <Route
                    path="competent-person-equipment/create"
                    element={<CompetentPersonEquipmentRegistrationForm/>}
                  />
                  <Route
                    path="competent-person-equipment/:applicationId"
                    element={<CompetentPersonEquipmentView />}
                  />
                  <Route
                    path="boe-registration/create"
                    element={<BOECertificateRegistrationForm/>}
                  />

                   <Route
                    path="smtc/create"
                    element={<SMTCRegistrationForm/>}
                  />
                  <Route
                    path="foe-registration/create"
                    element={<FOECertificateRegistrationForm/>}
                  />

                   <Route
                    path="foattandant-registration/create"
                    element={<FOAttendantCertificateRegistration/>}
                  />

                   <Route
                    path="HazardousWorkerRegistration/create"
                    element={<HazardousWorkerRegistration/>}
                  />
                  <Route
                    path="HazardousWorkerRegistration/:applicationId"
                    element={<HazardousWorkerView />}
                  />

                    <Route
                    path="boattandant-registration/create"
                    element={<BOAttendantCertificateRegistration/>}
                  />

                  {/* <Route
                    path="boilerNew-services/boilerregistrationnew"
                    element={<BoilerRegistrationNew />}
                  /> */}
                  <Route
                    path="boilerNew-services/renewalnew"
                    element={<BoilerRenewalNew />}
                  />

                  <Route
                    path="boilerNew-services/modificationRepair/create"
                    element={<BoilerModificationNew />}
                  />
                  <Route
                    path="boilerNew-services/modificationRepair/:changeReqId"
                    element={<BoilerModificationNew />}
                  />
                  <Route
                    path="boilerNew-services/modificationRepair/list"
                    element={<BoilerModificationRepairList />}
                  />
                  <Route
                    path="boilerNew-services/boilermanufacturedrawing"
                    element={<BoilerManufactureDrawing />}
                  />
                  <Route
                    path="boilerNew-services/boilermanufacturedrawing/:applicationId"
                    element={<BoilerManufactureDrawingView />}
                  />

                  <Route
                    path="boilerNew-services/repairnew"
                    element={<BoilerRepairNew />}
                  />
                  <Route
                    path="boilerNew-services/boilerclosernew"
                    element={<BoilerClosureNew />}
                  />
                  <Route
                    path="boilerNew-services/boilerclosernew/:changeReqId"
                    element={<BoilerClosureNew />}
                  />
                  <Route
                    path="boilerNew-services/boilercloser/list"
                    element={<BoilerClosureList />}
                  />

                  <Route
                    path="boilerNew-services/boilertransfernew"
                    element={<BoilerTransferNew />}
                  />

                  <Route
                    path="boilernew-services/erector/list"
                    element={<BoilerRepairerList />}
                  />

                  <Route
                    path="boilernew-services/erectorregistrationnew"
                    element={<BoilerErectorRegistrationNew />}
                  />

                  <Route
                    path="boilernew-services/erector-renewal"
                    element={<BoilerRepairerRenewal />}
                  />
                  <Route
                    path="boilernew-services/economiser/list"
                    element={<EconomiserList />}
                  />
                  <Route
                    path="boilernew-services/economiser"
                    element={<EconomiserRegistration />}
                  />
                  <Route
                    path="boilernew-services/economiser/:changeReqId"
                    element={<EconomiserRegistration />}
                  />
                  <Route
                    path="boilernew-services/economiser/close"
                    element={<EconomiserClosure />}
                  />
                  <Route
                    path="boilernew-services/economiser/renew"
                    element={<EconomiserRenewal />}
                  />
                  <Route
                    path="boilernew-services/economiser/view/:applicationId"
                    element={<EconomiserView />}
                  />

                  <Route
                    path="boilerNew-services/:changeReqId"
                    element={<BoilerRegistrationNew />}
                  />

                  <Route
                    path="boilernew-services/weldertest/list"
                    element={<WelderList />}
                  />
                  <Route
                    path="boilernew-services/weldertest"
                    element={<WelderTestApplication />}
                  />
                  <Route
                    path="boilernew-services/weldertest/:changeReqId"
                    element={<WelderTestApplication />}
                  />
                  <Route
                    path="boilernew-services/weldertest/renew"
                    element={<WelderRenewal />}
                  />
                  <Route
                    path="boilernew-services/weldertest/view/:applicationId"
                    element={<WelderView />}
                  />
                </Route>
                <Route element={<UserLayoutNoSidebar />}>
                  <Route
                    path="choose-category"
                    element={<CategorySelection />}
                  />
                  <Route path="brn-details" element={<BRNDetails />} />
                  <Route
                    path="verify-registration"
                    element={<VerifyRegistration />}
                  />
                </Route>
              </Route>
              <Route path="error" element={<ErrorPage />} />

              {/* ADD ALL CUSTOM ROUTES ABOVE THE CATCH-ALL "*" ROUTE */}
              <Route path="*" element={<NotFound />} />
            </Routes>
          </BrowserRouter>
        </AuthProvider>
      </LocationProvider>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;
