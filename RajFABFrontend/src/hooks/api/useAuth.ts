import { useQuery } from "@tanstack/react-query";
import { authApi } from "@/services/api/authApi";

export function useAuthSession() {
  const query: any = useQuery({
    queryKey: ["auth", "me"],
    queryFn: () => authApi.getCurrentUser(),
    retry: false,
    staleTime: 1000 * 60 * 10, // 10 minutes
  });

  return {
    user: query.data?.data?.user ?? null,
    isAuthenticated: !!query.data?.data?.user,
    isLoading: query.isLoading,
    refetchUser: query.refetch,
  };
}
