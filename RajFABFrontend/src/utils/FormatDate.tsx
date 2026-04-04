//  const FormatDate = (date?: Date | string) => date ? new Date(date).toLocaleDateString("en-IN") : "-";

//  export default FormatDate

//   const formatDate = (dateString: string) => {
//     return new Date(dateString).toLocaleDateString("en-IN", {
//       year: "numeric",
//       month: "short",
//       day: "numeric",
//       hour: "2-digit",
//       minute: "2-digit",
//     });
//   };
import { format } from "date-fns";

const formatDate = (dateString: string) => {
  if (!dateString) return "N/A";
  try {
    return format(new Date(dateString), "dd MMM yyyy");
  } catch {
    return dateString;
  }
};
export const formatInputDate = (dateString?: string) => {
  if (!dateString) return "";
  try {
    return format(new Date(dateString), "yyyy-MM-dd");
  } catch {
    return "";
  }
};
export default formatDate;
