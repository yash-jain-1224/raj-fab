import { useQuery } from "@tanstack/react-query";
import { ssoDetailsApi } from "@/services/api/ssoDetails";

export function useSsoDetails(id: string) {
  return useQuery({
    queryKey: ["sso-details", id],
    queryFn: () => ssoDetailsApi.getUserDetails(id),
    enabled: false,
  });
}

