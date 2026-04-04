import { Navigate } from "react-router-dom";
import { useAuth } from "@/utils/AuthProvider";

export default function AdminProtectedRoute({ children }: { children: React.ReactNode }) {

    const { isAuthenticated, user, isLoading } = useAuth();

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
    
    if (user.userType === "department" || user.userType === "superadmin" || user.userType === "admin") {
        return children;
    }

    return <Navigate to="/user" replace />;
}
