import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import PersonalAddressNew from "../common/PersonalAddressNew";
import { Button } from "../ui/button";
import { useState } from "react";

interface Step2Props {
  formData: any;
  updateFormData: (fieldPath: string, value: any) => void;
  sectionKey: string; // "contractorDetail"
  errors?: Record<string, string>;
}

/* ========= DEFAULT CONTRACTOR ========= */
const DEFAULT_CONTRACTOR = {
  id: "",
  name: "",
  role: "",
  designation: "",
  addressLine1: "",
  addressLine2: "",
  district: "",
  tehsil: "",
  area: "",
  pinCode: "",
  email: "",
  telephone: "",
  mobile: "",
  type: "",
  nameOfWork: "",
  maxContractWorkerCountMale: "0",
  maxContractWorkerCountFemale: "0",
  maxContractWorkerCountTransgender: "0",
  dateOfCommencement: "",
  dateOfCompletion: "",
};

export default function Step2FEstablishment({
  formData,
  updateFormData,
  sectionKey,
  errors = {},
}: Step2Props) {
  const contractors = formData[sectionKey] || [];
  const [error, setError] = useState("")
  const ErrorMessage = ({ message }: { message?: string }) =>
    message ? (
      <p className="text-destructive text-sm mt-1">{message}</p>
    ) : null;

  const totalAllowed = Number(formData?.establishmentDetails?.totalNumberOfContractEmployee || 0);

  const totalWorkersAssigned = contractors.reduce((sum, c) => {
    const male = Number(c.maxContractWorkerCountMale || 0);
    const female = Number(c.maxContractWorkerCountFemale || 0);
    const transgender = Number(c.maxContractWorkerCountTransgender || 0);
    return sum + male + female + transgender;
  }, 0);

  const canAddContractor = totalWorkersAssigned < totalAllowed;

  /* ========= ADD / REMOVE ========= */
  const addContractor = () => {
    const contractors = formData?.contractorDetail || [];
    const total = Number(formData?.establishmentDetails?.totalNumberOfContractEmployee || 0);
    if (total == 0 && contractors.length == 0) {
      // errors[`contractorDetail.minOneContractor`] = "*  Please add contract employees (4(b)) before entering contractor details.";
      setError("* Please add contract employees (4(b)) before entering contractor details.")
      return;
    }
    if (!canAddContractor) {
      setError(
        `Cannot add new contractor as total workers assigned (${totalWorkersAssigned}) reached the allowed limit (${totalAllowed})`
      );
      return;
    }

    updateFormData(sectionKey, [...contractors, { ...DEFAULT_CONTRACTOR }]);
  };

  const removeContractor = (index: number) => {
    // if (contractors.length === 1) return; // 🚫 prevent deleting last
    updateFormData(
      sectionKey,
      contractors.filter((_: any, i: number) => i !== index)
    );
  };

  const handleWorkerChange = (
    field: string,
    value: string,
    contractor: any,
    base: string,
    totalAllowed: number
  ) => {
    const cleanValue = value.replace(/\D/g, "").slice(0, 6);
    const newValue = Number(cleanValue || 0);

    // Calculate total workers across all contractors
    const contractors = formData[sectionKey] || [];
    let totalWorkers = 0;

    contractors.forEach((c: any) => {
      const male = Number(c.maxContractWorkerCountMale || 0);
      const female = Number(c.maxContractWorkerCountFemale || 0);
      const transgender = Number(c.maxContractWorkerCountTransgender || 0);

      totalWorkers += male + female + transgender;
    });

    // Subtract the old value of this field from total
    const oldValue = Number(contractor[`maxContractWorkerCount${capitalize(field)}`] || 0);
    totalWorkers = totalWorkers - oldValue + newValue;

    // Check against allowed total
    if (totalWorkers <= totalAllowed) {
      updateFormData(`${base}.maxContractWorkerCount${capitalize(field)}`, cleanValue);
    }
  };

  // helper
  const capitalize = (str: string) => str.charAt(0).toUpperCase() + str.slice(1);

  return (
    <div className="space-y-8">
      {/* ===== ADD BUTTON ===== */}
      <div className="grid grid-cols-4 items-end justify-end">
        <div className="col-span-3">
          <p className="font-bold text-lg text-destructive">
            {error ? error : errors[`contractorDetail.minOneContractor`]}
          </p>
          {!canAddContractor && (
            <p className="font-bold text-lg text-destructive">
              Cannot add new contractor as total workers assigned ({totalWorkersAssigned}) reached the allowed limit ({totalAllowed})
            </p>
          )}
        </div>
        <Button
          className="col-span-1"
          onClick={addContractor}
          disabled={!canAddContractor}
        >
          + Add Contractor
        </Button>
      </div>

      {contractors.map((contractor: any, index: number) => {
        const base = `${sectionKey}.${index}`;

        return (
          <div
            key={index}
            className="border rounded-lg p-5 space-y-6"
          >
            {/* ===== HEADER ===== */}
            <div className="flex justify-between items-center">
              <h3 className="font-semibold">
                D.{index + 1} Contractor Details
              </h3>

              {/* {contractors.length > 1 && ( */}
              <Button
                type="button"
                onClick={() => removeContractor(index)}
                variant="destructive"
              >
                Remove
              </Button>
              {/* )} */}
            </div>

            {/* ===== NAME ===== */}
            <div className="space-y-1">
              <Label>
                Name of Contractor <span className="text-red-500">*</span>
              </Label>
              <Input
                value={contractor.name}
                onChange={(e) =>
                  updateFormData(`${base}.name`, e.target.value)
                }
                className={errors[`${base}.name`] ? "border-destructive" : ""}
              />
              <ErrorMessage message={errors[`${base}.name`]} />
            </div>

            {/* ===== ADDRESS ===== */}
            <PersonalAddressNew
              path={base}
              data={contractor}
              updateData={updateFormData}
              errors={errors}
            />

            {/* ===== NAME OF WORK ===== */}
            <div className="space-y-1">
              <Label>
                Name of Work <span className="text-red-500">*</span>
              </Label>
              <Textarea
                value={contractor.nameOfWork}
                onChange={(e) =>
                  updateFormData(`${base}.nameOfWork`, e.target.value)
                }
                className={
                  errors[`${base}.nameOfWork`] ? "border-destructive" : ""
                }
              />
              <ErrorMessage message={errors[`${base}.nameOfWork`]} />
            </div>

            {/* ===== WORKERS ===== */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <div className="space-y-1">
                <Label>
                  Male <span className="text-red-500">*</span>
                </Label>
                <Input
                  placeholder="Enter number of male workers"
                  inputMode="numeric"
                  value={contractor.maxContractWorkerCountMale}
                  onChange={(e) =>
                    handleWorkerChange(
                      "male",
                      e.target.value,
                      contractor,
                      base,
                      Number(formData?.establishmentDetails?.totalNumberOfContractEmployee || 0)
                    )
                  }
                />
                <ErrorMessage
                  message={errors[`${base}.maxContractWorkerCountMale`]}
                />
              </div>
              <div className="space-y-1">
                <Label>
                  Female <span className="text-red-500">*</span>
                </Label>
                <Input
                  placeholder="Enter number of female workers"
                  inputMode="numeric"
                  value={contractor.maxContractWorkerCountFemale}
                  onChange={(e) =>
                    handleWorkerChange(
                      "female",
                      e.target.value,
                      contractor,
                      base,
                      Number(formData?.establishmentDetails?.totalNumberOfContractEmployee || 0)
                    )
                  }
                />
                <ErrorMessage
                  message={errors[`${base}.maxContractWorkerCountFemale`]}
                />
              </div>
              <div className="space-y-1">
                <Label>
                  Transgender <span className="text-red-500">*</span>
                </Label>
                <Input
                  placeholder="Enter number of transgender workers"
                  inputMode="numeric"
                  value={contractor.maxContractWorkerCountTransgender}
                  onChange={(e) =>
                    handleWorkerChange(
                      "transgender",
                      e.target.value,
                      contractor,
                      base,
                      Number(formData?.establishmentDetails?.totalNumberOfContractEmployee || 0)
                    )
                  }
                />
                <ErrorMessage
                  message={errors[`${base}.maxContractWorkerCountTransgender`]}
                />
              </div>
            </div>

            {/* ===== DATES ===== */}
            <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
              <div className="space-y-1">
                <Label>
                  Start Date <span className="text-red-500">*</span>
                </Label>
                <Input
                  type="date"
                  value={contractor.dateOfCommencement}
                  onChange={(e) =>
                    updateFormData(
                      `${base}.dateOfCommencement`,
                      e.target.value
                    )
                  }
                  className={
                    errors[`${base}.dateOfCommencement`]
                      ? "border-destructive"
                      : ""
                  }
                />
                <ErrorMessage
                  message={errors[`${base}.dateOfCommencement`]}
                />
              </div>

              <div className="space-y-1">
                <Label>
                  End Date <span className="text-red-500">*</span>
                </Label>
                <Input
                  type="date"
                  value={contractor.dateOfCompletion}
                  onChange={(e) =>
                    updateFormData(
                      `${base}.dateOfCompletion`,
                      e.target.value
                    )
                  }
                  className={
                    errors[`${base}.dateOfCompletion`]
                      ? "border-destructive"
                      : ""
                  }
                />
                <ErrorMessage
                  message={errors[`${base}.dateOfCompletion`]}
                />
              </div>
            </div>
          </div>
        );
      })}
    </div>
  );
}
