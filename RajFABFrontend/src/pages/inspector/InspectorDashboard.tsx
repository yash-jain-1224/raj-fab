import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { ClipboardList, ClipboardCheck, Clock, AlertTriangle } from "lucide-react";
import { useAuth } from "@/utils/AuthProvider";
import { useInspectorApplications } from "@/hooks/api/useInspectorApplications";
import formatDate from "@/utils/formatDate";
import getStatusColor from "@/utils/getStatusColor";
import { useNavigate } from "react-router-dom";

export default function InspectorDashboard() {
  const { user } = useAuth();
  const { applications, isLoading } = useInspectorApplications();
  const navigate = useNavigate();

  const pending = applications.filter((a) => a.status === "Pending").length;
  const forwarded = applications.filter((a) => a.status === "Forwarded").length;
  const returned = applications.filter((a) => a.status === "ReturnedToCitizen").length;

  const stats = [
    { title: "Total Assigned", value: applications.length.toString(), icon: ClipboardList, color: "text-blue-600" },
    { title: "Pending Review", value: pending.toString(), icon: Clock, color: "text-orange-600" },
    { title: "Forwarded", value: forwarded.toString(), icon: ClipboardCheck, color: "text-green-600" },
    { title: "Returned to Citizen", value: returned.toString(), icon: AlertTriangle, color: "text-red-600" },
  ];

  const recent = applications.slice(0, 4);

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight capitalize">
            Welcome, {user?.fullName}
          </h1>
          <p className="text-muted-foreground">Inspector Dashboard — Boiler Applications assigned to you</p>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        {stats.map((stat) => (
          <Card key={stat.title} className="relative overflow-hidden">
            <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
              <CardTitle className="text-sm font-medium">{stat.title}</CardTitle>
              <stat.icon className={`h-4 w-4 ${stat.color}`} />
            </CardHeader>
            <CardContent>
              <div className="text-2xl font-bold">{stat.value}</div>
            </CardContent>
          </Card>
        ))}
      </div>

      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Clock className="h-5 w-5" />
            Recent Assigned Applications
          </CardTitle>
        </CardHeader>
        <CardContent>
          {isLoading ? (
            <div className="flex items-center justify-center py-8 text-muted-foreground">
              Loading applications...
            </div>
          ) : recent.length === 0 ? (
            <div className="flex items-center justify-center py-8 text-muted-foreground">
              No applications assigned yet
            </div>
          ) : (
            <div className="space-y-3">
              {recent.map((app) => (
                <div
                  key={app.id}
                  className="flex items-start justify-between p-4 border rounded-lg"
                >
                  <div className="space-y-1">
                    <p className="font-medium">{app.applicationTitle || app.applicationRegistrationNumber}</p>
                    <p className="text-xs text-muted-foreground">Type: {app.applicationType}</p>
                    <p className="text-xs text-muted-foreground">Assigned: {formatDate(app.assignedDate)}</p>
                  </div>
                  <Badge className={getStatusColor(app.status)}>{app.status}</Badge>
                </div>
              ))}
              <Button
                className="w-full"
                variant="outline"
                onClick={() => navigate("/inspector/applications")}
              >
                View All Applications
              </Button>
            </div>
          )}
        </CardContent>
      </Card>
    </div>
  );
}
