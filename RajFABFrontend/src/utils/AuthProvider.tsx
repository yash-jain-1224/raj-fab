import { createContext, useContext } from "react";
import { useAuthSession } from "@/hooks/api/useAuth";
import { authApi } from "@/services/api/authApi";

const AuthContext = createContext(null);

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const session = useAuthSession();

  const logout = async () => {
    await authApi.logout();
    window.location.href = "/login";
  };

  return (
    <AuthContext.Provider
      value={{
        user: session.user,
        isAuthenticated: session.isAuthenticated,
        isLoading: session.isLoading,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export const useAuth = () => useContext(AuthContext);
