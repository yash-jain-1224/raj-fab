import { NavLink, useLocation } from "react-router-dom";
import { Home, ClipboardList, LogOut, Building2 } from "lucide-react";
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
import { useAuth } from "@/utils/AuthProvider";

export function InspectorSidebar() {
  const { user, logout } = useAuth();
  const { state } = useSidebar();
  const location = useLocation();
  const currentPath = location.pathname;
  const collapsed = state === "collapsed";

  const menuItems = [
    { title: "Dashboard", url: "/inspector", icon: Home },
    { title: "My Applications", url: "/inspector/applications", icon: ClipboardList },
  ];

  const isActive = (path: string) => {
    if (path === "/inspector" && currentPath === "/inspector") return true;
    if (path !== "/inspector" && currentPath.startsWith(path)) return true;
    return false;
  };

  const getNavCls = (path: string) =>
    isActive(path)
      ? "bg-primary/10 text-primary font-medium border-r-2 border-primary"
      : "hover:bg-muted/50 text-muted-foreground hover:text-foreground";

  return (
    <Sidebar className={collapsed ? "w-16" : "w-64"} collapsible="icon">
      <SidebarHeader className="border-b p-4">
        <div className="flex items-center gap-3">
          <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-orange-600 text-white">
            <Building2 className="h-6 w-6" />
          </div>
          {!collapsed && (
            <div>
              <h2 className="font-semibold text-lg">RajFAB Portal</h2>
              <Badge variant="secondary" className="flex items-center gap-1 text-xs mt-1 bg-orange-100 text-orange-700">
                Inspector
              </Badge>
            </div>
          )}
        </div>
      </SidebarHeader>

      <SidebarContent className="px-2 py-4">
        <div className="flex flex-col gap-1">
          {menuItems.map((item) => (
            <NavLink
              key={item.title}
              to={item.url}
              className={`flex items-center gap-2 px-3 py-2 rounded-md text-sm ${getNavCls(item.url)}`}
            >
              <item.icon className="h-5 w-5" />
              {!collapsed && <span>{item.title}</span>}
            </NavLink>
          ))}
        </div>
      </SidebarContent>

      <SidebarFooter className="border-t p-4">
        <div className="flex items-center gap-3">
          <Avatar className="h-8 w-8">
            <AvatarFallback>
              {user?.fullName
                ?.trim()
                .split(/\s+/)
                .slice(0, 2)
                .map((name: string) => name.charAt(0).toUpperCase())
                .join("")}
            </AvatarFallback>
          </Avatar>
          {!collapsed && (
            <div className="flex-1 min-w-0">
              <p className="text-sm font-medium truncate">Inspector</p>
              <p className="text-xs text-muted-foreground truncate">{user?.fullName}</p>
            </div>
          )}
          <Button
            variant="ghost"
            size="sm"
            className="h-8 w-8 p-0"
            onClick={() => (window.location.href = "/select-mode")}
            title="Switch Mode"
          >
            <LogOut className="h-4 w-4" />
          </Button>
        </div>
        <SidebarTrigger className="mt-2 w-full" />
      </SidebarFooter>
    </Sidebar>
  );
}
