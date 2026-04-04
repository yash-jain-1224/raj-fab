import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Eye, PenSquare } from "lucide-react";
import {
  useApplicationsByUser,
} from "@/hooks/api";
import { format } from "date-fns";
import {
  normalizeStatus,
  APPLICATION_STATUS,
} from "@/constants/applicationStatus";
import { DataTable, TableColumn } from "@/components/common/DataTable";
import { eSignApi } from "@/services/api/index";
import { paymentApi } from "@/services/api/payment";

interface TrackedApplication {
  id: string;
  dbId: string;
  type: string;
  title: string;
  status: string;
  originalStatus: string;
  submittedAt: string;
  reviewedAt: string;
  comments: string | null;
  factoryName: string;
  amendmentCount: number;
  verificationStatus: string;
  officePost: string;
  applicationId: string;
  isPaymentCompleted: boolean;
  isESignCompleted: boolean;
  isPaymentPending: boolean;
}

interface MapApproval {
  id: string;
  acknowledgementNumber?: string;
  factoryName: string;
  status: string;
  createdAt: string;
  updatedAt: string;
  comments?: string;
  amendmentCount?: number;
}

export default function TrackApplications() {
  const navigate = useNavigate();
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedStatus, setSelectedStatus] = useState("all");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");
  const [pageLoader, setPageLoader] = useState(false);

  const { data: applications = [], isLoading: applicationListLoading } = useApplicationsByUser();
  const isLoading = applicationListLoading || pageLoader;

  const getStatusColor = (status: string) => {
    const normalized = normalizeStatus(status);
    switch (normalized) {
      case APPLICATION_STATUS.APPROVED:
        return "default";
      case APPLICATION_STATUS.REJECTED:
      case APPLICATION_STATUS.OBJECTION_RAISED:
      case APPLICATION_STATUS.RETURNED_TO_APPLICANT:
        return "destructive";
      case APPLICATION_STATUS.UNDER_REVIEW:
      case APPLICATION_STATUS.SUBMITTED:
        return "secondary";
      default:
        return "outline";
    }
  };

  const getApplicationType = (type: string) => {
    switch (type) {
      case "factory-registration":
        return "Factory Registration";
      case "map-approval":
        return "Factory Map Approval";
      case "Map Approval":
        return "Map Approval";
      case "New Establishment Registration":
        return "New Establishment Registration";
      case "form-submission":
        return "Form Submission";
      default:
        return type;
    }
  };

  const allApplications = applications.length > 0 ? applications.map((app) => ({
    applicationId: app.applicationId,
    applicationRegistrationId: app.applicationRegistrationId,
    title: app.applicationTitle,
    type: app.applicationType,
    approvalRequestId: app.approvalRequestId,
    moduleId: app.moduleId,
    id: app.applicationRegistrationId,
    dbId: app.applicationId,
    status: normalizeStatus(app.status),
    verificationStatus: "Pending", // Assuming default, adjust if API provides
    originalStatus: app.status,
    submittedAt: app.createdDate,
    reviewedAt: app.createdDate, // Assuming same as created
    comments: null,
    factoryName: app.applicationTitle,
    amendmentCount: 0,
    officePost: "", // Assuming not provided
    isPaymentCompleted: app.isPaymentCompleted,
    isESignCompleted: app.isESignCompleted,
    isPaymentPending: app.isPaymentPending,
  })) : [];

  const filteredApplications = allApplications.filter((app) => {
    const matchesSearch =
      searchTerm === "" ||
      app.id.toLowerCase().includes(searchTerm.toLowerCase()) ||
      app.title.toLowerCase().includes(searchTerm.toLowerCase()) ||
      getApplicationType(app.type)
        .toLowerCase()
        .includes(searchTerm.toLowerCase());

    const matchesStatus =
      selectedStatus === "all" ||
      app.status === normalizeStatus(selectedStatus);

    const submittedDate = new Date(app.submittedAt);
    const matchesFromDate = !fromDate || submittedDate >= new Date(fromDate);
    const matchesToDate =
      !toDate || submittedDate <= new Date(toDate + "T23:59:59");

    return matchesSearch && matchesStatus && matchesFromDate && matchesToDate;
  });

  if (isLoading) {
    return (
      <div className="flex items-center justify-center min-h-[400px]">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
          <p className="mt-4 text-muted-foreground">
            Loading your applications...
          </p>
        </div>
      </div>
    );
  }

  const handleApplicationAction = async (
    actionType: "payment" | "esign" | "verify",
    applicationId: string
  ) => {
    setPageLoader(true);
    try {
      let response;

      if (actionType === "payment") {
        response = await paymentApi.paymentByApplicationId(applicationId);
      }

      if (actionType === "esign") {
        response = await eSignApi.eSignByApplicationId(applicationId);
      }
      if (actionType === "verify") {
        // response = await paymentApi.verifyByApplicationId(applicationId);
      }
      setPageLoader(false);
      if (response?.html) {
        document.open();
        document.write(response.html);
        document.close();
      }
    } catch (error) {
      setPageLoader(false);
      console.error(`${actionType} failed`, error);
    }
  };

  const columns: TableColumn<TrackedApplication>[] = [
    {
      header: "Application Number",
      accessor: (app) => app.applicationId.toUpperCase(),
      className: "font-medium",
    },
    {
      header: "Type",
      accessor: (app) => app.type,
      className: "font-semibold",
    },
    {
      header: "Factory Name",
      accessor: "factoryName",
    },
    {
      header: "Submitted",
      accessor: (app) => format(new Date(app.submittedAt), "dd MMM yyyy"),
    },

    // {
    //   header: "Amendments",
    //   accessor: (app) =>
    //     app.amendmentCount > 0 ? (
    //       <Badge
    //         variant="outline"
    //         className="bg-amber-50 text-amber-700 border-amber-300"
    //       >
    //         {app.amendmentCount}
    //       </Badge>
    //     ) : (
    //       <span className="text-muted-foreground text-sm">None</span>
    //     ),
    // },
    {
      header: "Application Status",
      accessor: (app) => (
        <div className="flex flex-col gap-1 w-fit">
          <Badge variant={getStatusColor(app.status)}>{app.status}</Badge>
        </div>
      ),
    },
    // {
    //   header: "Verification Status",
    //   accessor: (app) => (
    //     <div className="flex flex-col gap-1 w-fit text-center">
    //       <Badge variant={getStatusColor(app.verificationStatus)}>
    //         {app.verificationStatus + " - " + (app.officePost ?? "Unknown")}
    //       </Badge>
    //     </div>
    //   ),
    // },
    {
      header: "Action",
      accessor: (app) => (
        <div className="flex items-center gap-2">
          {/* Payment Button */}
          {app?.isPaymentPending === true && (
            <Button
              onClick={() =>
                handleApplicationAction("verify", app.applicationId)
              }
              size="sm"
              variant="outline"
              className="me-2"
            >
              Payment Verify
            </Button>
          )}
          {app?.isPaymentCompleted === false && (
            <Button
              onClick={() =>
                handleApplicationAction("payment", app.applicationId)
              }
              size="sm"
              variant="outline"
              className="me-2"
            >
              Payment
            </Button>
          )}

          {((app?.isPaymentCompleted === undefined || app?.isPaymentCompleted === true) &&
            !app?.isESignCompleted) && (
              <Button
                onClick={() => handleApplicationAction("esign", app.applicationId)}
                size="sm"
                variant="outline"
                className="me-2"
              >
                <Eye className="h-4 w-4 mr-2" />
                E-Sign
              </Button>
            )}
          <Button
            onClick={() =>
              navigate(
                `/user/applicationView/${app.type
                  .replace(/ /g, "_")
                  .toLowerCase()}/${app.applicationId}`
              )
            }
            size="sm"
            variant="outline"
          >
            <Eye className="h-4 w-4 mr-2" />
            View
          </Button>
        </div>
      ),
      className: "text-right",
      headerClassName: "text-right",
    },
  ];

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold tracking-tight">
          Track Applications
        </h1>
        <p className="text-muted-foreground">
          Monitor the status and progress of all your submitted applications.
        </p>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Search and Filter</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            <Input
              placeholder="Search by application ID, factory name, or type..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
            />

            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-2">
                <label className="text-sm font-medium">From Date</label>
                <Input
                  type="date"
                  value={fromDate}
                  onChange={(e) => setFromDate(e.target.value)}
                />
              </div>
              <div className="space-y-2">
                <label className="text-sm font-medium">To Date</label>
                <Input
                  type="date"
                  value={toDate}
                  onChange={(e) => setToDate(e.target.value)}
                />
              </div>
            </div>

            <div className="flex gap-2 flex-wrap">
              <Button
                variant={selectedStatus === "all" ? "default" : "outline"}
                onClick={() => setSelectedStatus("all")}
                size="sm"
              >
                All ({allApplications.length})
              </Button>
              <Button
                variant={selectedStatus === "submitted" ? "default" : "outline"}
                onClick={() => setSelectedStatus("submitted")}
                size="sm"
              >
                Submitted (
                {
                  allApplications.filter(
                    (app) => app.status === APPLICATION_STATUS.SUBMITTED
                  ).length
                }
                )
              </Button>
              <Button
                variant={selectedStatus === "approved" ? "default" : "outline"}
                onClick={() => setSelectedStatus("approved")}
                size="sm"
              >
                Approved (
                {
                  allApplications.filter(
                    (app) => app.status === APPLICATION_STATUS.APPROVED
                  ).length
                }
                )
              </Button>
              <Button
                variant={selectedStatus === "rejected" ? "default" : "outline"}
                onClick={() => setSelectedStatus("rejected")}
                size="sm"
              >
                Rejected (
                {
                  allApplications.filter(
                    (app) => app.status === APPLICATION_STATUS.REJECTED
                  ).length
                }
                )
              </Button>
              <Button
                variant={
                  selectedStatus === APPLICATION_STATUS.RETURNED_TO_APPLICANT
                    ? "default"
                    : "outline"
                }
                onClick={() =>
                  setSelectedStatus(APPLICATION_STATUS.RETURNED_TO_APPLICANT)
                }
                size="sm"
              >
                Returned (
                {
                  allApplications.filter(
                    (app) =>
                      app.status === APPLICATION_STATUS.RETURNED_TO_APPLICANT
                  ).length
                }
                )
              </Button>
              {(fromDate || toDate) && (
                <Button
                  variant="outline"
                  size="sm"
                  onClick={() => {
                    setFromDate("");
                    setToDate("");
                  }}
                >
                  Clear Dates
                </Button>
              )}
            </div>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Applications ({filteredApplications.length})</CardTitle>
        </CardHeader>
        <CardContent>
          <DataTable
            columns={columns}
            data={filteredApplications}
            rowKey="id"
            emptyMessage={
              searchTerm || selectedStatus !== "all"
                ? "No applications match your current filters."
                : "You haven't submitted any applications yet."
            }
          />
        </CardContent>
      </Card>
      {/* 
      {allApplications.length > 0 && (
        <Card>
          <CardHeader>
            <CardTitle>Application Summary</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
              {[APPLICATION_STATUS.SUBMITTED, APPLICATION_STATUS.APPROVED, APPLICATION_STATUS.REJECTED, APPLICATION_STATUS.UNDER_REVIEW].map((status) => {
                const count = allApplications.filter(app => 
                  app.status === status
                ).length || 0;
                
                return (
                  <div key={status} className="text-center">
                    <div className="text-2xl font-bold">{count}</div>
                    <div className="text-sm text-muted-foreground capitalize">
                      {status}
                    </div>
                  </div>
                );
              })}
            </div>
          </CardContent>
        </Card>
      )} */}
    </div>
  );
}
