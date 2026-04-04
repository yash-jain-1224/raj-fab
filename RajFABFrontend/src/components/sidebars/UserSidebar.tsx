import { NavLink, useLocation } from "react-router-dom";
import {
  Building2,
  FileText,
  Search,
  CreditCard,
  GraduationCap,
  MessageSquare,
  Award,
  User,
  Home,
  Clock,
  LogOut,
  PlusCircle,
  RotateCcw,
  ClipboardCheck,
  FileBarChart,
  Flame,
  Edit,
  ChevronDown,
  FileCheck,
  XCircle,
  FilePlus,
  Repeat,
  RefreshCw,
  Map,
  UserCog,
  PlayCircle,
  Gavel,
  Grid,
  Factory,
  List,
} from "lucide-react";

import {
  Sidebar,
  SidebarContent,
  SidebarGroup,
  SidebarGroupContent,
  SidebarGroupLabel,
  SidebarMenu,
  SidebarMenuButton,
  SidebarMenuItem,
  SidebarTrigger,
  SidebarHeader,
  SidebarFooter,
  useSidebar,
} from "@/components/ui/sidebar";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";
import { Button } from "@/components/ui/button";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { useAuth } from "@/utils/AuthProvider";

const dashboardItems = [{ title: "Dashboard", url: "/user", icon: Home }];
const documentItems = [{ title: "Documents", url: "/user/documents", icon: FileText }];

const amendmentMenuItems = [
  {
    title: "Factory Map Approval Amendment",
    url: "/user/amendment/map-approval",
    icon: FileCheck,
  },
  {
    title: "Factory License Amendment",
    url: "/user/amendment/factory-registration",
    icon: FileText,
  },
];

const boilerServicesMenuItems = [
  {
    title: "All Services",
    url: "/user/boiler-services/dashboard",
    icon: FilePlus,
  },
  {
    title: "Registration",
    url: "/user/boilerNew-services/list",
    icon: FilePlus,
  },
  { title: "Renewal", url: "/user/boiler-services/renewal", icon: Repeat },
  {
    title: "Modification",
    url: "/user/boiler-services/modification",
    icon: Edit,
  },
  { title: "Transfer", url: "/user/boiler-services/transfer", icon: RefreshCw },
];

export function UserSidebar() {
  const { state } = useSidebar();
  const location = useLocation();
  const currentPath = location.pathname;
  const collapsed = state === "collapsed";
  const { user } = useAuth();
  const isActive = (path: string) => {
    if (path === "/user" && currentPath === "/user") return true;
    if (path !== "/user" && currentPath.startsWith(path)) return true;
    return false;
  };

  const getNavCls = (path: string) =>
    isActive(path)
      ? "bg-primary/10 text-primary font-medium border-r-2 border-primary"
      : "hover:bg-muted/50 text-muted-foreground hover:text-foreground";
  const factoryMenuItems = [
    {
      title: "All Services",
      url: "/user/factory-services/dashboard",
      icon: Grid,
    },
    {
      title: "Registration",
      url: "/user/new-establishment",
      icon: Building2,
    },
    user?.userModuleStatus?.new_establishment_registration && {
      title: "Plan Approval",
      url: "/user/map-approval",
      icon: Map,
    },
    (user?.userModuleStatus?.new_establishment_registration && user?.userModuleStatus?.map_approval) && {
      title: "Factory Licence",
      url: "/user/factory-license",
      icon: FileText,
    },
    user?.userModuleStatus?.factory_license && {
      title: "Manager Change",
      url: "/user/manager-change",
      icon: UserCog,
    },
    user?.userModuleStatus?.factory_license && {
      title: "Commencement/Cessation",
      url: "/user/commence-cessation",
      icon: PlayCircle,
    },
    user?.userModuleStatus?.factory_license && {
      title: "Appeal",
      url: "/user/appeal",
      icon: Gavel,
    },
    user?.userModuleStatus?.factory_license && {
      title: "Annual Return",
      url: "/user/annual-returns",
      icon: FileText,
    },
    user?.userModuleStatus?.factory_license && { title: "Non Hazardous Factory", url: "/user/non-hazardous-factory", icon: Building2 },
    // {
    //   title: "Form 2",
    //   url: "/user/form2",
    //   icon: PlusCircle,
    // },
    // !user?.userModuleStatus?.map_approval &&
    // { title: "New Registration", url: "/user/new-registration", icon: PlusCircle },
    // { title: "License Renewal", url: "/user/license-renewal", icon: RotateCcw },
    // {
    //   title: "Licence Application",
    //   url: "/user/licence-application",
    //   icon: Edit,
    // },
    // { title: "Factory Closure", url: "/user/factory-closure-list", icon: XCircle },
    // { title: "Track Applications", url: "/user/track", icon: Clock },
  ];

  const boilerNewServicesMenuItems = [
    {
      title: "All Services",
      url: "/user/boiler-services/dashboard",
      icon: FilePlus,
    },
    {
      title: "Registration",
      url: "/user/boilerNew-services/list",
      icon: FilePlus,
    },
    // { title: "Renewal", url: "/user/boilernew-services/renewalnew", icon: FilePlus },
    {
      title: "Boiler Modification / Repair",
      url: "/user/boilerNew-services/modificationRepair/list",
      icon: FilePlus,
    },
    {
      title: "Boiler Repair",
      url: "/user/boilerNew-services/repairnew",
      icon: FilePlus,
    },
    // { title: "Boiler Transfer", url: "/user/boilerNew-services/boilertransfernew", icon: FilePlus },
    {
      title: "Boiler Closer",
      url: "/user/boilerNew-services/boilercloser/list",
      icon: FilePlus,
    },
    {
      title: "Boiler Repairer",
      url: "/user/boilernew-services/erector/list",
      icon: FilePlus,
    },
    // { title: "Boiler Repairer Renewal", url: "/user/boilernew-services/erector-renewal", icon: FilePlus, },
    // { title: "Boiler Repairer Closure", url: "/user/boilernew-services/erector-closure", icon: FilePlus, },
    {
      title: "Boiler Manufacturer",
      url: "/user/boiler-services/boilermanufacturer/list",
      icon: FilePlus,
    },
    { title: "Stpl", url: "/user/boiler-services/stpl/list", icon: FilePlus },
    {
      title: "Economiser",
      url: "/user/boilernew-services/economiser/list",
      icon: FilePlus,
    },
    {
      title: "Welder",
      url: "/user/boilernew-services/weldertest/list",
      icon: FilePlus,
    },
    {
      title: "Boiler Component Fitting",
      url: "/user/boiler-services/boilercomponentfitting",
      icon: FilePlus,
    },
    {
      title: "Boiler Manufacture Drawing",
      url: "/user/boilerNew-services/boilermanufacturedrawing",
      icon: FilePlus,
    },
    {
      title: "Competant Person List",
      url: "/user/competent-person/list",
      icon: List,
    },
    {
      title: "CompetantPerson Equipment",
      url: "/user/competent-person-equipment/create",
      icon: FilePlus,
    },
    {
      title: "BOE",
      url: "/user/boe-registration/create",
      icon: FilePlus,
    },
    {
      title: "FOE",
      url: "/user/foe-registration/create",
      icon: FilePlus,
    },
    {
      title: "FO Attandant",
      url: "/user/foattandant-registration/create",
      icon: FilePlus,
    },
    {
      title: "BO Attandant",
      url: "/user/boattandant-registration/create",
      icon: FilePlus,
    },
     {
      title: "Hazardous Worker",
      url: "/user/HazardousWorkerRegistration/create",
      icon: FilePlus,
    },
    
    {
      title: "SMTC",
      url: "/user/smtc/create",
      icon: FilePlus,
    },
  ];

  return (
    <Sidebar className={collapsed ? "w-16" : "w-64"} collapsible="icon">
      <SidebarHeader className="border-b p-4">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-primary text-primary-foreground">
            <Building2 className="h-6 w-6" />
          </div>
          {!collapsed && (
            <div>
              <h2 className="font-semibold text-lg">RajFAB Portal</h2>
              <div className="flex items-center gap-2">
                <Badge variant="secondary" className="text-xs">
                  <User className="h-3 w-3 mr-1" />
                  Citizen
                </Badge>
              </div>
            </div>
          )}
        </div>
      </SidebarHeader>

      <SidebarContent className="px-2">
        {/* Dashboard */}
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              {dashboardItems.map((item) => (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton asChild className="h-12">
                    <NavLink to={item.url} className={getNavCls(item.url)}>
                      <item.icon className="h-5 w-5" />
                      {!collapsed && <span className="ml-3">{item.title}</span>}
                    </NavLink>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>

       
        {/* Factory Menu */}
        <SidebarGroup>
          <Collapsible defaultOpen={false} className="group/collapsible">
            <SidebarGroupLabel asChild className={collapsed ? "sr-only" : ""}>
              <CollapsibleTrigger className="flex items-center justify-between w-full hover:bg-muted/50 rounded-md p-2">
                <div className="flex items-center gap-2">
                  <Factory className="h-4 w-4" />
                  {!collapsed && <span>Factory Services</span>}
                </div>
                {!collapsed && (
                  <ChevronDown className="h-4 w-4 transition-transform group-data-[state=open]/collapsible:rotate-180" />
                )}
              </CollapsibleTrigger>
            </SidebarGroupLabel>
            <CollapsibleContent>
              <SidebarGroupContent>
                <SidebarMenu>
                  {factoryMenuItems.map(
                    (item) =>
                      item && (
                        <SidebarMenuItem key={item.title}>
                          <SidebarMenuButton asChild className="h-12">
                            <NavLink
                              to={item.url}
                              className={getNavCls(item.url)}
                            >
                              <item.icon className="h-5 w-5" />
                              {!collapsed && (
                                <span className="ml-3">{item.title}</span>
                              )}
                            </NavLink>
                          </SidebarMenuButton>
                        </SidebarMenuItem>
                      ),
                  )}
                </SidebarMenu>
              </SidebarGroupContent>
            </CollapsibleContent>
          </Collapsible>
        </SidebarGroup>

        {/* Amendment Section */}
        {/* <SidebarGroup>
          <Collapsible defaultOpen className="group/collapsible">
            <SidebarGroupLabel asChild className={collapsed ? "sr-only" : ""}>
              <CollapsibleTrigger className="flex items-center justify-between w-full hover:bg-muted/50 rounded-md p-2">
                <div className="flex items-center gap-2">
                  <Edit className="h-4 w-4" />
                  {!collapsed && <span>Amendment</span>}
                </div>
                {!collapsed && (
                  <ChevronDown className="h-4 w-4 transition-transform group-data-[state=open]/collapsible:rotate-180" />
                )}
              </CollapsibleTrigger>
            </SidebarGroupLabel>
            <CollapsibleContent>
              <SidebarGroupContent>
                <SidebarMenu>
                  {amendmentMenuItems.map((item) => (
                    <SidebarMenuItem key={item.title}>
                      <SidebarMenuButton asChild className="h-12">
                        <NavLink to={item.url} className={getNavCls(item.url)}>
                          <item.icon className="h-5 w-5" />
                          {!collapsed && <span className="ml-3">{item.title}</span>}
                        </NavLink>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  ))}
                </SidebarMenu>
              </SidebarGroupContent>
            </CollapsibleContent>
          </Collapsible>
        </SidebarGroup> */}
        {/* Boiler Services Section */}
        {/* <SidebarGroup>
          <Collapsible defaultOpen={false} className="group/collapsible">
            <SidebarGroupLabel asChild className={collapsed ? "sr-only" : ""}>
              <CollapsibleTrigger className="flex items-center justify-between w-full hover:bg-muted/50 rounded-md p-2">
                <div className="flex items-center gap-2">
                  <Edit className="h-4 w-4" />
                  {!collapsed && <span>Boiler Services</span>}
                </div>
                {!collapsed && (
                  <ChevronDown className="h-4 w-4 transition-transform group-data-[state=open]/collapsible:rotate-180" />
                )}
              </CollapsibleTrigger>
            </SidebarGroupLabel>
            <CollapsibleContent>
              <SidebarGroupContent>
                <SidebarMenu>
                  {boilerServicesMenuItems.map((item) => (
                    <SidebarMenuItem key={item.title}>
                      <SidebarMenuButton asChild className="h-12">
                        <NavLink to={item.url} className={getNavCls(item.url)}>
                          <item.icon className="h-5 w-5" />
                          {!collapsed && <span className="ml-3">{item.title}</span>}
                        </NavLink>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  ))}
                </SidebarMenu>
              </SidebarGroupContent>
            </CollapsibleContent>
          </Collapsible>
        </SidebarGroup> */}

       {/* Boiler menu */}
        <SidebarGroup>
          <Collapsible defaultOpen={false} className="group/collapsible">
            <SidebarGroupLabel asChild className={collapsed ? "sr-only" : ""}>
              <CollapsibleTrigger className="flex items-center justify-between w-full hover:bg-muted/50 rounded-md p-2">
                <div className="flex items-center gap-2">
                  <Edit className="h-4 w-4" />
                  {!collapsed && <span>Boiler Services</span>}
                </div>
                {!collapsed && (
                  <ChevronDown className="h-4 w-4 transition-transform group-data-[state=open]/collapsible:rotate-180" />
                )}
              </CollapsibleTrigger>
            </SidebarGroupLabel>
            <CollapsibleContent>
              <SidebarGroupContent>
                <SidebarMenu>
                  {boilerNewServicesMenuItems.map((item) => (
                    <SidebarMenuItem key={item.title}>
                      <SidebarMenuButton asChild className="h-12">
                        <NavLink to={item.url} className={getNavCls(item.url)}>
                          <item.icon className="h-5 w-5" />
                          {!collapsed && (
                            <span className="ml-3">{item.title}</span>
                          )}
                        </NavLink>
                      </SidebarMenuButton>
                    </SidebarMenuItem>
                  ))}
                </SidebarMenu>
              </SidebarGroupContent>
            </CollapsibleContent>
          </Collapsible>
        </SidebarGroup>
      
       {/* Documents menu */}
        <SidebarGroup>
          <SidebarGroupContent>
            <SidebarMenu>
              {documentItems.map((item) => (
                <SidebarMenuItem key={item.title}>
                  <SidebarMenuButton asChild className="h-12">
                    <NavLink to={item.url} className={getNavCls(item.url)}>
                      <item.icon className="h-5 w-5" />
                      {!collapsed && <span className="ml-3">{item.title}</span>}
                    </NavLink>
                  </SidebarMenuButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarGroupContent>
        </SidebarGroup>
      
      </SidebarContent>

      <SidebarFooter className="border-t p-4">
        <div className="flex items-center gap-3">
          <Avatar className="h-8 w-8">
            <AvatarFallback>CT</AvatarFallback>
          </Avatar>
          {!collapsed && (
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium truncate">Citizen User</p>
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
