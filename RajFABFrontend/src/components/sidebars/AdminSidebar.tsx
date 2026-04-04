import { NavLink, useLocation } from "react-router-dom";
import {
  Building2,
  FileCheck,
  ClipboardCheck,
  CreditCard,
  BarChart3,
  GraduationCap,
  MessageSquare,
  Settings,
  Users,
  Home,
  LogOut,
  Shield,
  XCircle,
  Edit,
  Layers,
} from "lucide-react";

import {
  Sidebar,
  SidebarContent,
  SidebarFooter,
  SidebarHeader,
  SidebarTrigger,
  useSidebar,
} from "@/components/ui/sidebar";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion"; // shadcn accordion
import { useAuth } from "@/utils/AuthProvider";

export function AdminSidebar() {
  const { user } = useAuth();
  const IsAdmin = user?.userType === "admin";
  const adminMenuItems = [{ title: "Dashboard", url: "/admin", icon: Home }];

  const userManagementMenuItems = [
    {
      title: "Office Management",
      url: "/admin/officemanagement",
      icon: Building2,
    },
    {
      title: "Office Post Management",
      url: "/admin/officepostmanagement",
      icon: FileCheck,
    },
    {
      title: "Assign Application Privileges To Office Post",
      url: "/admin/assignapplicationprivilegestoofficepost",
      icon: Users,
    },
    {
      title: "Assign Inspectcion Privileges To Office Post",
      url: "/admin/assigninspectionprivilegestoofficepost",
      icon: Users,
    },
    {
      title: "Create Department User",
      url: "/admin/usermanagement",
      icon: Users,
    },
    { title: "Assign Posts To User", url: "/admin/roleassign", icon: Users },
    {
      title: "Factory Workflow Management",
      url: "/admin/applicationworkflowmanagement",
      icon: Users,
    },
    {
      title: "Boiler Workflow Management",
      url: "/admin/boilerworkflowmanagement",
      icon: Layers,
    },
    // {
    //   title: "Office Post Level Management",
    //   url: "/admin/officepostlevelmanagement",
    //   icon: Users,
    // },

    // { title: "Office Master", url: "/admin/office", icon: Building2 },
    // { title: "Office Post Management", url: "/admin/userrolemanagement", icon: FileCheck },
    // { title: "Create Department User", url: "/admin/usermanagement", icon: Users },
    // { title: "User Privileges", url: "/admin/userprvleges", icon: Users },
    // { title: "User Hierarchy", url: "/admin/userhierarchy", icon: Users },
  ];

  const mastersMenuItems = [
    {
      title: "Act Management",
      url: "/admin/actmanagement",
      icon: Building2,
    },
    {
      title: "Rule Management",
      url: "/admin/rulemanagement",
      icon: Building2,
    },
    { title: "Application Management", url: "/admin/forms", icon: FileCheck },
    { title: "Post Management", url: "/admin/postmanagement", icon: Building2 },
    {
      title: "Factory Type Master",
      url: "/admin/factory-types",
      icon: ClipboardCheck,
    },
    {
      title: "Workers Master",
      url: "/admin/workersrangmaster",
      icon: ClipboardCheck,
    },
    {
      title: "Factory Category Master",
      url: "/admin/factory-categories",
      icon: Layers,
    },
    // { title: "Office Level Management", url: "/admin/office-levels", icon: Layers },
    {
      title: "Division Management",
      url: "/admin/divisionmanagement",
      icon: FileCheck,
    },
    {
      title: "District Management",
      url: "/admin/districtmanagement",
      icon: FileCheck,
    },
    {
      title: "City/Tehsil Management",
      url: "/admin/citymanagement",
      icon: FileCheck,
    },
    // { title: "Area Management", url: "/admin/areamanagement", icon: FileCheck },
    {
      title: "Add Railway Station",
      url: "/admin/railwaystation",
      icon: Building2,
    },
    {
      title: "Add Police Station",
      url: "/admin/policestation",
      icon: Building2,
    },
    // { title: "Documents Master", url: "/admin/document", icon: FileCheck },
  ];

  const operationsMenuItems = [
    // {
    //   title: "Establishment Review",
    //   url: "/admin/establishment-review",
    //   icon: ClipboardCheck,
    // },
    { title: "Application Review", url: "/admin/applications", icon: ClipboardCheck },
    ...(IsAdmin ? [{ title: "Boiler Application Assignment", url: "/admin/boiler-assignment", icon: Users }] : []),
    // { title: "Renewal Review", url: "/admin/renewal-review", icon: FileCheck },
    // { title: "Closure Review", url: "/admin/closure-review", icon: XCircle },
    // { title: "Manager Change Notices", url: "/admin/manager-change-review", icon: Edit },
    // { title: "Licensing & Applications", url: "/admin/licensing", icon: Building2 },
    // { title: "Approvals & Certification", url: "/admin/approvals", icon: FileCheck },
    // { title: "Inspections", url: "/admin/inspections", icon: ClipboardCheck },
    // { title: "Payments", url: "/admin/payments", icon: CreditCard },
    // { title: "Reports & Analytics", url: "/admin/reports", icon: BarChart3 },
    // { title: "Training Management", url: "/admin/training", icon: GraduationCap },
    // { title: "Grievances", url: "/admin/grievances", icon: MessageSquare },
    // { title: "System Settings", url: "/admin/settings", icon: Settings },
  ];
  const { state } = useSidebar();
  const location = useLocation();
  const currentPath = location.pathname;
  const collapsed = state === "collapsed";

  const isActive = (path: string) => {
    if (path === "/admin" && currentPath === "/admin") return true;
    if (path !== "/admin" && currentPath.startsWith(path)) return true;
    return false;
  };

  const getNavCls = (path: string) =>
    isActive(path)
      ? "bg-primary/10 text-primary font-medium border-r-2 border-primary"
      : "hover:bg-muted/50 text-muted-foreground hover:text-foreground";

  const renderMenuItems = (
    items: { title: string; url: string; icon: any }[]
  ) => (
    <div className="flex flex-col gap-1">
      {items.map((item) => (
        <NavLink
          key={item.title}
          to={item.url}
          className={`flex items-center gap-2 px-3 py-2 rounded-md text-sm ${getNavCls(
            item.url
          )}`}
        >
          <item.icon className="h-5 w-5" />
          {!collapsed && <span>{item.title}</span>}
        </NavLink>
      ))}
    </div>
  );

  return (
    <Sidebar className={collapsed ? "w-16" : "w-64"} collapsible="icon">
      {/* Header */}
      <SidebarHeader className="border-b p-4">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <Building2 className="h-6 w-6" />
          </div>
          {!collapsed && (
            <div>
              <h2 className="font-semibold text-lg">RajFAB Portal</h2>
              <Badge
                variant="secondary"
                className="flex items-center gap-1 text-xs mt-1"
              >
                <Shield className="h-3 w-3" />
                {IsAdmin ? "Admin" : "Dept. User"}
              </Badge>
            </div>
          )}
        </div>
      </SidebarHeader>

      {/* Content with Accordion */}
      <SidebarContent className="px-2">
        <Accordion type="multiple" defaultValue={["main"]} className="w-full">
          <AccordionItem value="main">
            <AccordionTrigger className="px-3 py-2">Main</AccordionTrigger>
            <AccordionContent>
              {renderMenuItems(adminMenuItems)}
            </AccordionContent>
          </AccordionItem>

          {IsAdmin && (
            <AccordionItem value="user">
              <AccordionTrigger className="px-3 py-2">
                User Management
              </AccordionTrigger>
              <AccordionContent>
                {renderMenuItems(userManagementMenuItems)}
              </AccordionContent>
            </AccordionItem>
          )}
          {IsAdmin && (
            <AccordionItem value="masters">
              <AccordionTrigger className="px-3 py-2">Masters</AccordionTrigger>
              <AccordionContent>
                {renderMenuItems(mastersMenuItems)}
              </AccordionContent>
            </AccordionItem>
          )}

          <AccordionItem value="operations">
            <AccordionTrigger className="px-3 py-2">
              Operations
            </AccordionTrigger>
            <AccordionContent>
              {renderMenuItems(operationsMenuItems)}
            </AccordionContent>
          </AccordionItem>
        </Accordion>
      </SidebarContent>

      {/* Footer */}
      <SidebarFooter className="border-t p-4">
        <div className="flex items-center gap-3">
          <Avatar className="h-8 w-8">
            <AvatarFallback>
              {user?.fullName
                ?.trim()
                .split(/\s+/)
                .slice(0, 2)
                .map((name) => name.charAt(0).toUpperCase())
                .join("")}
            </AvatarFallback>
          </Avatar>
          {!collapsed && (
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium truncate">
                {IsAdmin ? "Admin" : "Department User"}
              </p>
              <p className="text-xs text-muted-foreground truncate">
                via RajSSO
              </p>
            </div>
          )}
          <Button
            variant="ghost"
            size="sm"
            className="h-8 w-8 p-0"
            onClick={() => (window.location.href = "/")}
          >
            <LogOut className="h-4 w-4" />
          </Button>
        </div>
        <SidebarTrigger className="mt-2 w-full" />
      </SidebarFooter>
    </Sidebar>
  );
}
