import { useEffect } from "react";

export function useAutoWorkersCalculation({ formData, setFormData }: any) {
  useEffect(() => {
    const male = parseInt(formData.totalNoOfWorkersMale || "0", 10) || 0;
    const female = parseInt(formData.totalNoOfWorkersFemale || "0", 10) || 0;
    const tg = parseInt(formData.totalNoOfWorkersTransgender || "0", 10) || 0;
    const total = male + female + tg;
    if (formData.totalWorkers !== String(total)) {
      setFormData(prev => ({ ...prev, totalWorkers: String(total) }));
    }
  }, [formData.totalNoOfWorkersMale, formData.totalNoOfWorkersFemale, formData.totalNoOfWorkersTransgender]);
}
