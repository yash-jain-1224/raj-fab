import React, { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ArrowLeft, Flame, Loader2 } from "lucide-react";
import BoilerTransferNew from "./boilertransfer";
import { useBoilersCreate } from "@/hooks/api/useBoilers";
import { DocumentUploader } from "@/components/ui/DocumentUploader";
import { useLocationContext } from "@/context/LocationContext";


/* ===================================================== */

// export default function BoilerRegistrationNewForm() {
//   const [selection, setSelection] = useState<"" | "new" | "transfer">("");

//   // 🔁 If selection made, render component directly
//   if (selection === "new") {
//     return <BoilerRegistrationNew />;
//   }

//   if (selection === "transfer") {
//     return <BoilerTransferNew />;
//   }

//   // 🔹 Selection UI
//   return (
//     <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center">
//       <Card className="w-full max-w-xl shadow-lg">
//         <CardHeader>
//           <CardTitle className="text-xl text-center">
//             Boiler Registration  Type
//           </CardTitle>
//         </CardHeader>

//         <CardContent className="space-y-6">

//           <div className="space-y-4">
//             <label className="flex items-center gap-3 cursor-pointer">
//               <input
//                 type="radio"
//                 name="boilerType"
//                 value="new"
//                 onChange={() => setSelection("new")}
//               />
//               <span className="font-medium">
//                 New Boiler Registration
//               </span>
//             </label>

//             <label className="flex items-center gap-3 cursor-pointer">
//               <input
//                 type="radio"
//                 name="boilerType"
//                 value="transfer"
//                 onChange={() => setSelection("transfer")}
//               />
//               <span className="font-medium">
//                 Boiler Transfer
//               </span>
//             </label>
//           </div>

          

//         </CardContent>
//       </Card>
//     </div>
//   );
// }
