import { useQuery } from "@tanstack/react-query";
import { brnApi, BrnDetails } from "@/services/api/index";

export function useBrnDetailsByBRNNumber(brnNumber?: string) {
  return useQuery<BrnDetails>({
    queryKey: ["brn", brnNumber],
    queryFn: async () => {
      const res = await brnApi.getByBrnNumber(brnNumber!);
      return res.data;
    },
    enabled: !!brnNumber,
  });
}
