import { applicationApi, ApplicationByUser } from "@/services/api/applications";
import { useQuery } from "@tanstack/react-query";

export function useApplicationsByUser() {
  return useQuery({
    queryKey: ["applications", "byuser"],
    queryFn: () => applicationApi.getByUser(),
  });
}