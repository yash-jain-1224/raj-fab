import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Building2,
  FileCheck,
  ClipboardCheck,
  MessageSquare,
  TrendingUp,
  AlertTriangle,
  Users,
  Clock,
} from "lucide-react";
import { Progress } from "@/components/ui/progress";
import {
  useFactoryRegistrationsList,
  useFactoryMapApprovalsList,
} from "@/hooks/api";
import { useAuth } from "@/utils/AuthProvider";
import { useAppApprovalRequest } from "@/hooks/api/useAppApprovalRequest";
import getStatusColor from "@/utils/getStatusColor";
import formatDate from "@/utils/formatDate";
import { useNavigate } from "react-router-dom";

export default function AdminDashboard() {
  // const { data: registrations, isLoading: isLoadingRegistrations } =
  //   useFactoryRegistrationsList();
  // const { data: mapApprovals, isLoading: isLoadingMapApprovals } =
  //   useFactoryMapApprovalsList();
  const {
    approvalRequests: allApplications,
    isLoading: isLoadingApprovalRequests,
  } = useAppApprovalRequest();
  const { user } = useAuth();
  const navigate = useNavigate()
  const isLoading =
    // isLoadingRegistrations ||
    // isLoadingMapApprovals ||
    isLoadingApprovalRequests;

  // Combine and format applications for display
  // const allApplications = [
  //   ...(registrations || []).map(reg => ({
  //     id: reg.registrationNumber,
  //     company: reg.factoryName,
  //     type: "Factory Registration",
  //     status: reg.status || "Pending",
  //     priority: "Medium",
  //     submittedDate: reg.createdAt,
  //   })),
  //   ...(mapApprovals || []).map(app => ({
  //     id: app.acknowledgementNumber,
  //     company: app.factoryName,
  //     type: "Map Approval",
  //     status: "Pending", // Default since backend doesn't have status yet
  //     priority: "Medium",
  //     submittedDate: app.id, // Using id as placeholder for date
  //   }))
  // ];

  const recentApplications = allApplications.slice(0, 4);

  const totalApplications = allApplications.length;
  const pendingApprovals = allApplications.filter(
    (app) => app.status === "Pending" || app.status === "Under Review",
  ).length;
  const approvedApplications = allApplications.filter(
    (app) => app.status === "Approved",
  ).length;
  const forwardedApplications = allApplications.filter(
    (app) => app.status === "Forwarded",
  ).length;

  const stats = [
    {
      title: "Total Applications",
      value: totalApplications.toString(),
      change: "+12%",
      icon: Building2,
      color: "text-blue-600",
    },
    {
      title: "Pending Approvals",
      value: pendingApprovals.toString(),
      change: "-5%",
      icon: FileCheck,
      color: "text-orange-600",
    },
    {
      title: "Forwarded Approvals",
      value: forwardedApplications.toString(),
      change: "-2%",
      icon: MessageSquare,
      color: "text-orange-600",
    },
    {
      title: "Approved",
      value: approvedApplications.toString(),
      change: "+8%",
      icon: ClipboardCheck,
      color: "text-green-600",
    },
    // {
    //   title: "Open Grievances",
    //   value: "12",
    //   change: "-15%",
    //   icon: MessageSquare,
    //   color: "text-red-600",
    // },
  ];

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight capitalize">
            Welcome {user?.fullName || "Admin"} <span className="text-xl lowercase">({user?.email || ""})</span>
          </h1>
          <p className="text-muted-foreground">
            Overview of RajFAB portal activities and metrics
          </p>
        </div>
        <Button className="bg-gradient-to-r from-primary to-primary/80">
          <TrendingUp className="mr-2 h-4 w-4" />
          Generate Report
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => (
          <Card key={stat.title} className="relative overflow-hidden">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">
                {stat.title}
              </CardTitle>
              <stat.icon className={`h-4 w-4 ${stat.color}`} />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stat.value}</div>
              <p className="text-xs text-muted-foreground">
                <span
                  className={
                    stat.change.startsWith("+")
                      ? "text-green-600"
                      : "text-red-600"
                  }
                >
                  {stat.change}
                </span>{" "}
                from last month
              </p>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Main Content Grid */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {/* Recent Applications */}
        <Card className="col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Clock className="h-5 w-5" />
              Recent Applications
            </CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="flex items-center justify-center py-8">
                <div className="text-muted-foreground">
                  Loading applications...
                </div>
              </div>
            ) : recentApplications.length === 0 ? (
              <div className="flex items-center justify-center py-8">
                <div className="text-muted-foreground">
                  No applications found
                </div>
              </div>
            ) : (
              <div className="space-y-4">
                {recentApplications.map((app) => (
                  <div
                    key={app.approvalRequestId}
                    className="flex items-start justify-between p-4 border rounded-lg"
                  >
                    <div className="space-y-1">
                      <p className="font-medium">Application Title: {app.applicationTitle}</p>
                      {/* <p className="text-sm text-muted-foreground">{app.approvalRequestId}</p> */}
                      <p className="text-xs text-muted-foreground">
                        Application Type: {app.applicationType}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        	Total Employees : {app.totalEmployees}
                      </p>
                      <p className="text-xs text-muted-foreground">
                        Created Date: {formatDate(app.createdDate)}
                      </p>
                    </div>
                    <div className="flex gap-2">
                      <Badge
                        variant={
                          app.status === "Approved"
                            ? "default"
                            : app.status === "Objection" ||
                                app.status === "Objection Raised"
                              ? "destructive"
                              : "secondary"
                        }
                        className={
                          getStatusColor(app.status) + " py-1 px-3 rounded"
                        }
                      >
                        {app.status}
                      </Badge>
                    </div>
                  </div>
                ))}
                <Button className="w-full justify-center" variant="outline" onClick={() => navigate("/admin/applications")}>
                  All Applications
                </Button>
              </div>
            )}
          </CardContent>
        </Card>

        {/* Quick Actions */}
        <Card>
          <CardHeader>
            <CardTitle>Quick Actions</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <Button className="w-full justify-start" variant="outline" onClick={() => navigate("/admin/applications")}>
              <Building2 className="mr-2 h-4 w-4" />
              Review Applications
            </Button>
            <Button className="w-full justify-start" variant="outline">
              <ClipboardCheck className="mr-2 h-4 w-4" />
              Schedule Inspection
            </Button>
            <Button className="w-full justify-start" variant="outline">
              <MessageSquare className="mr-2 h-4 w-4" />
              Handle Grievances
            </Button>
            <Button className="w-full justify-start" variant="outline">
              <Users className="mr-2 h-4 w-4" />
              Manage Users
            </Button>
          </CardContent>
        </Card>

        {/* Department Performance */}
        <Card>
          <CardHeader>
            <CardTitle>Department Performance</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <div className="flex justify-between text-sm mb-2">
                <span>Application Processing</span>
                <span>87%</span>
              </div>
              <Progress value={87} className="h-2" />
            </div>
            <div>
              <div className="flex justify-between text-sm mb-2">
                <span>Inspection Completion</span>
                <span>92%</span>
              </div>
              <Progress value={92} className="h-2" />
            </div>
            <div>
              <div className="flex justify-between text-sm mb-2">
                <span>Grievance Resolution</span>
                <span>78%</span>
              </div>
              <Progress value={78} className="h-2" />
            </div>
            <div>
              <div className="flex justify-between text-sm mb-2">
                <span>Training Compliance</span>
                <span>95%</span>
              </div>
              <Progress value={95} className="h-2" />
            </div>
          </CardContent>
        </Card>

        {/* Alerts & Notifications */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <AlertTriangle className="h-5 w-5 text-orange-600" />
              Alerts & Notifications
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="p-3 bg-red-50 border-l-4 border-red-400 rounded">
              <p className="text-sm font-medium text-red-800">High Priority</p>
              <p className="text-xs text-red-600">
                3 urgent inspections overdue
              </p>
            </div>
            <div className="p-3 bg-orange-50 border-l-4 border-orange-400 rounded">
              <p className="text-sm font-medium text-orange-800">
                Medium Priority
              </p>
              <p className="text-xs text-orange-600">
                12 applications pending review
              </p>
            </div>
            <div className="p-3 bg-green-50 border-l-4 border-green-400 rounded">
              <p className="text-sm font-medium text-green-800">
                System Update
              </p>
              <p className="text-xs text-green-600">
                Integration with eMitra successful
              </p>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
