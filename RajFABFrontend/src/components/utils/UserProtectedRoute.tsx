import { Navigate, Outlet, useLocation } from "react-router-dom";
import { useAuth } from "@/utils/AuthProvider";

export default function UserProtectedRoute() {
  const { isAuthenticated, user, isLoading } = useAuth();
  const location = useLocation(); // current path

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary"></div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/" replace />;
  }

  if (user.userType === "citizen") {
    // 1. Check Category
    if (!user.citizenCategory) {
      if (location.pathname !== "/user/choose-category") {
        return <Navigate to="/user/choose-category" replace />;
      }
      return <Outlet />;
    }

    if (user.citizenCategory == "registration_renewal_map_approval_manufacturing") {
      // 2. Check BRN/LIN (Only if Category is set)
      if (!user.brnNumber && !user.linNumber) {
        if (location.pathname !== "/user/brn-details") {
          return <Navigate to="/user/brn-details" replace />;
        }
        return <Outlet />;
      }
    } else {
      if (location.pathname == "/user/choose-category") {
        return <Navigate to="/user/verify-registration" replace />;
      }
    }

    return <Outlet />; // renders the correct layout and page
  }

  return <Navigate to="/admin" replace />;
}
