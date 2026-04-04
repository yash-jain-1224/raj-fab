import React, { useEffect } from "react";
import { useNavigate, useLocation } from "react-router-dom";
import Cookies from "js-cookie";
import { useAuthSession } from "@/hooks/api/useAuth";
import { authApi } from "@/services/api/authApi";
import { useToast } from "@/hooks/use-toast";

const SSOLogin = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const { toast } = useToast();

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const token = params.get("token");

    if (!token) {
      toast({
        title: "Error",
        description: "Token not found in URL",
        variant: "destructive",
      });
      return;
    }
    sessionStorage.setItem("auth_token", token);

    const fetchUser = async () => {
      try {
        const data: any = await authApi.getCurrentUser();
        const { user } = data.data;
        if (!user) {
          toast({
            title: "Error",
            description: "Failed to fetch user",
            variant: "destructive",
          });
          return;
        }

        if (user.userType == "citizen") {
          if (!user.citizenCategory || user.citizenCategory.trim() === "") {
            navigate("/user/choose-category");
          } else {
            navigate("/user");
          }
        } else if (user.isInspector) {
          navigate("/select-mode");
        } else {
          navigate("/admin");
        }
      } catch (error: any) {
        toast({
          title: "Error",
          description: error.message || "Something went wrong",
          variant: "destructive",
        });
      }
    };

    fetchUser();
  }, [location.search, navigate, toast]);

  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <div className="text-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
      </div>
    </div>
  );
};

export default SSOLogin;
