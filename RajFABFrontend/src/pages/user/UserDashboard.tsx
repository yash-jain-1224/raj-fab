import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  FileText,
  Clock,
  CheckCircle,
  AlertCircle,
  Award,
  Calendar,
  Bell,
  Plus,
} from "lucide-react";
import { Progress } from "@/components/ui/progress";
import {
  useFactoryMapApprovalsList,
  useEstablishmentRegistrations,
} from "@/hooks/api";
import { formatDistanceToNow } from "date-fns";
import {
  normalizeStatus,
  APPLICATION_STATUS,
} from "@/constants/applicationStatus";
import { useAuth } from "@/utils/AuthProvider";
import { useNavigate } from "react-router-dom";

export default function UserDashboard() {
  const { user } = useAuth();
  const navigate = useNavigate();
  const { data: registrations, isLoading: isLoadingRegistrations } =
    useEstablishmentRegistrations();
  // const { data: mapApprovals, isLoading: isLoadingMapApprovals } =
  //   useFactoryMapApprovalsList();
  const isLoading = isLoadingRegistrations;
  // Combine and format applications for display with normalized status
  const allApplications = [
    ...(registrations ?? []).map((reg) => ({
      id: reg.registrationNumber,
      type: reg.applicationType || "Establishment Registration",
      status: normalizeStatus(reg.status),
      originalStatus: reg.status,
      progress:
        normalizeStatus(reg.status) === APPLICATION_STATUS.APPROVED
          ? 100
          : normalizeStatus(reg.status) === APPLICATION_STATUS.REJECTED
            ? 0
            : 60,
      submittedDate: reg.createdDate
        ? new Date(reg.createdDate).toLocaleDateString()
        : new Date().toLocaleDateString(),
      expectedDate: new Date(
        new Date(reg.createdDate).getTime() + 30 * 24 * 60 * 60 * 1000,
      ).toLocaleDateString(),
      rawData: reg,
    })),
    // ...(mapApprovals || []).map(app => ({
    //   id: app.acknowledgementNumber,
    //   type: "Map Approval",
    //   status: normalizeStatus(app.status),
    //   originalStatus: app.status,
    //   progress: normalizeStatus(app.status) === APPLICATION_STATUS.APPROVED ? 100 : normalizeStatus(app.status) === APPLICATION_STATUS.REJECTED ? 0 : 40,
    //   submittedDate: app.createdAt ? new Date(app.createdAt).toLocaleDateString() : new Date().toLocaleDateString(),
    //   expectedDate: app.createdAt ? new Date(new Date(app.createdAt).getTime() + 30 * 24 * 60 * 60 * 1000).toLocaleDateString() : new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toLocaleDateString(),
    //   rawData: app,
    // }))
  ];

  const myApplications = allApplications.slice(0, 3);

  const totalApplications = allApplications.length;
  const inProgressApps = allApplications.filter(
    (app) =>
      app.status === APPLICATION_STATUS.UNDER_REVIEW ||
      app.status === APPLICATION_STATUS.PENDING ||
      app.status === APPLICATION_STATUS.SUBMITTED,
  ).length;
  const approvedApps = allApplications.filter(
    (app) => app.status === APPLICATION_STATUS.APPROVED,
  ).length;
  const actionRequiredApps = allApplications.filter(
    (app) =>
      app.status === APPLICATION_STATUS.OBJECTION_RAISED ||
      app.status === APPLICATION_STATUS.REJECTED ||
      app.status === APPLICATION_STATUS.RETURNED_TO_APPLICANT,
  ).length;

  const applicationStats = [
    {
      title: "Total Applications",
      value: totalApplications.toString(),
      icon: FileText,
      color: "text-blue-600",
    },
    {
      title: "In Progress",
      value: inProgressApps.toString(),
      icon: Clock,
      color: "text-orange-600",
    },
    {
      title: "Approved",
      value: approvedApps.toString(),
      icon: CheckCircle,
      color: "text-green-600",
    },
    {
      title: "Requires Action",
      value: actionRequiredApps.toString(),
      icon: AlertCircle,
      color: "text-red-600",
    },
  ];

  const upcomingTasks = [
    {
      task: "Submit additional documents for FAC/2024/001",
      due: "2024-01-25",
      priority: "High",
    },
    {
      task: "Schedule factory inspection",
      due: "2024-01-28",
      priority: "Medium",
    },
    { task: "Renew boiler certificate", due: "2024-02-05", priority: "Low" },
  ];

  const IsEstablishmentRegistrationStartOrCompleted = allApplications.some(
    (reg) => reg.type === "New Establishment Registration",
  );

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">
            Dashboard {user?.fullName || "Citizen"}
          </h1>
          <p className="text-muted-foreground">
            Welcome back! Here's an overview of your applications and
            activities.
          </p>
        </div>
        {/* <Button className="bg-gradient-to-r from-primary to-primary/80">
          <Plus className="mr-2 h-4 w-4" />
          New Application
        </Button> */}
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {applicationStats.map((stat) => (
          <Card key={stat.title} className="relative overflow-hidden">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">
                {stat.title}
              </CardTitle>
              <stat.icon className={`h-4 w-4 ${stat.color}`} />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stat.value}</div>
            </CardContent>
          </Card>
        ))}
      </div>

      {/* Main Content Grid */}
      <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
        {/* My Applications */}
        <Card className="col-span-2">
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <FileText className="h-5 w-5" />
              My Applications
            </CardTitle>
          </CardHeader>
          <CardContent>
            {isLoading ? (
              <div className="flex items-center justify-center py-8">
                <div className="text-muted-foreground">
                  Loading applications...
                </div>
              </div>
            ) : myApplications.length === 0 ? (
              <div className="flex items-center justify-center py-8">
                <div className="text-muted-foreground">
                  No applications found
                </div>
              </div>
            ) : (
              <div className="space-y-4">
                {myApplications.map((app, i) => (
                  <div key={i} className="p-4 border rounded-lg space-y-3">
                    <div className="flex items-center justify-between">
                      <div>
                        <p className="font-medium">{app.id}</p>
                        <p className="text-sm text-muted-foreground">
                          {app.type}
                        </p>
                      </div>
                      <Badge
                        variant={
                          app.status === APPLICATION_STATUS.APPROVED
                            ? "default"
                            : app.status ===
                                  APPLICATION_STATUS.OBJECTION_RAISED ||
                                app.status === APPLICATION_STATUS.REJECTED
                              ? "destructive"
                              : "secondary"
                        }
                      >
                        {app.status}
                      </Badge>
                    </div>

                    <div className="space-y-2">
                      <div className="flex justify-between text-sm">
                        <span>Progress</span>
                        <span>{app.progress}%</span>
                      </div>
                      <Progress value={app.progress} className="h-2" />
                    </div>

                    <div className="flex justify-between text-xs text-muted-foreground">
                      <span>Submitted: {app.submittedDate}</span>
                      <span>Expected: {app.expectedDate}</span>
                    </div>
                  </div>
                ))}

                <Button
                  variant="outline"
                  className="w-full"
                  onClick={() => navigate("/user/track")}
                >
                  View All Applications
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
            <Button
              className="w-full justify-start bg-gradient-to-r from-green-600 to-green-500 text-white hover:from-green-700 hover:to-green-600"
              onClick={() => IsEstablishmentRegistrationStartOrCompleted ? navigate("/user/new-establishment"): navigate("/user/new-establishment/create")}
            >
              <Plus className="mr-2 h-4 w-4" />
              Establishment Registration
            </Button>
            <Button disabled={(user?.userModuleStatus?.new_establishment_registration && user?.userModuleStatus?.map_approval)} className="w-full justify-start" variant="outline">
              <FileText className="mr-2 h-4 w-4" />
              Apply for License
            </Button>
            <Button
              className="w-full justify-start"
              variant="outline"
              onClick={() => navigate("/user/track")}
            >
              <Clock className="mr-2 h-4 w-4" />
              Track Application
            </Button>
            <Button className="w-full justify-start" variant="outline">
              <Award className="mr-2 h-4 w-4" />
              Download Certificate
            </Button>
            <Button className="w-full justify-start" variant="outline">
              <Calendar className="mr-2 h-4 w-4" />
              Book Training
            </Button>
          </CardContent>
        </Card>

        {/* Upcoming Tasks */}
        <Card>
          <CardHeader>
            <CardTitle className="flex items-center gap-2">
              <Bell className="h-5 w-5" />
              Upcoming Tasks
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {upcomingTasks.map((task, index) => (
              <div key={index} className="p-3 border rounded-lg">
                <p className="text-sm font-medium">{task.task}</p>
                <div className="flex justify-between items-center mt-2">
                  <span className="text-xs text-muted-foreground">
                    Due: {task.due}
                  </span>
                  <Badge
                    variant={
                      task.priority === "High"
                        ? "destructive"
                        : task.priority === "Medium"
                          ? "default"
                          : "secondary"
                    }
                    className="text-xs"
                  >
                    {task.priority}
                  </Badge>
                </div>
              </div>
            ))}
          </CardContent>
        </Card>

        {/* Recent Notifications */}
        <Card>
          <CardHeader>
            <CardTitle>Recent Notifications</CardTitle>
          </CardHeader>
          <CardContent className="space-y-3">
            {(() => {
              // Get returned applications
              const returnedApplications = allApplications
                .filter(
                  (app) =>
                    app.status === APPLICATION_STATUS.RETURNED_TO_APPLICANT,
                )
                .slice(0, 2);

              // Get approved applications
              const approvedApplications = allApplications
                .filter((app) => app.status === APPLICATION_STATUS.APPROVED)
                .slice(0, 1);

              // Combine notifications
              const notifications: Array<{
                type: string;
                title: string;
                message: string;
                time: string;
                color: string;
                link?: string;
              }> = [
                ...approvedApplications.map((app) => ({
                  type: "success",
                  title: "Application Approved",
                  message: `${app.id} has been approved`,
                  time: app.submittedDate,
                  color: "green",
                })),
                ...returnedApplications.map((app) => ({
                  type: "warning",
                  title: "Action Required - Corrections Needed",
                  message: `${app.id} - Please review admin comments`,
                  time: app.submittedDate,
                  color: "orange",
                  link: `/user/applications/${app.type === "Factory Registration" ? "factory-registration" : "map-approval"}/${app.id}`,
                })),
              ];

              if (notifications.length === 0) {
                return (
                  <div className="text-center py-6 text-muted-foreground text-sm">
                    No recent notifications
                  </div>
                );
              }

              return notifications.map((notification, index) => (
                <div
                  key={index}
                  className={`p-3 bg-${notification.color}-50 border-l-4 border-${notification.color}-400 rounded ${notification.link ? "cursor-pointer hover:bg-" + notification.color + "-100 transition-colors" : ""}`}
                  onClick={() =>
                    notification.link &&
                    navigate(notification.link)
                  }
                >
                  <p
                    className={`text-sm font-medium text-${notification.color}-800`}
                  >
                    {notification.title}
                  </p>
                  <p className={`text-xs text-${notification.color}-600`}>
                    {notification.message}
                  </p>
                  <p className={`text-xs text-${notification.color}-500 mt-1`}>
                    {notification.time}
                  </p>
                </div>
              ));
            })()}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
