import React, { useState } from "react";
import { Building2 } from "lucide-react";

import Step1Establishment from "./Step1Establishment";
import Step2Employer from "./Step2Employer";
import Step3ContractLabour from "./Step3ContractLabour";
import Step4CommonLicence from "./Step4CommonLicence";
import Step5SingleLicence from "./Step5SingleLicence";
import Step6Amendment from "./Step6Amendment";


export default function Form27Wizard() {
  const [step, setStep] = useState<number>(1);

  // ✅ TOTAL STEPS = 7 (INCLUDING REVIEW)
  const totalSteps = 7;

  // ✅ SAFE INITIAL STRUCTURE (NO BLANK UI)
  const [formData, setFormData] = useState<any>({
    // Establishment
    establishmentName: "",
    telephone: "",
    headOfficeAddress: "",
    headOfficeEmail: "",
    corporateOfficeAddress: "",
    corporateOfficeEmail: "",
    nicActivities: "",
    nicCodeDetails: "",
    natureOfWork: "",
    identifier: "",

    // Employer
    employerName: "",
    employerRelation: "",
    employerAddress: "",
    employerEmail: "",
    employerMobile: "",

    // Contract Labour
    contractLabourRows: [],
    maxWorkmen: "",
    licenceFee: "",
    securityDeposit: "",

    // Common Licence
    commonLicenceRows: [],

    // Single Licence
    singleLicenceRows: [],

    // Amendment
    amendLicenceNo: "",
    amendDate: "",
    linPan: "",
    amendEstablishment: "",
    presentWorkers: "",
    feesDetails: "",
    otherAmendment: "",
  });

  const update = (k: string, v: any) => {
    setFormData(prev => ({ ...prev, [k]: v }));
  };

  return (
    <div className="min-h-screen bg-slate-100 p-4">
      <div className="max-w-6xl mx-auto">

        {/* HEADER */}
        <div className="rounded-xl overflow-hidden shadow-md mb-6">
          <div className="bg-gradient-to-r from-blue-500 to-blue-600 px-6 py-5 text-white">
            <div className="flex gap-4">
              <div className="bg-white/20 p-3 rounded-lg">
                <Building2 className="h-7 w-7" />
              </div>
              <div>
                <h1 className="text-3xl font-bold">
                  Form-27 : Application for Licence
                </h1>
                <p className="text-blue-100 text-sm mt-1">
                  Online Application for License / Renewal / Amendment
                </p>
              </div>
            </div>
          </div>

          {/* PROGRESS */}
          <div className="bg-white px-6 py-4">
            <div className="flex justify-between text-sm mb-2">
              <span>Step {step} of {totalSteps}</span>
              <span>{Math.round((step / totalSteps) * 100)}%</span>
            </div>
            <div className="h-2 bg-gray-200 rounded">
              <div
                className="h-2 bg-blue-600 rounded transition-all"
                style={{ width: `${(step / totalSteps) * 100}%` }}
              />
            </div>
          </div>
        </div>

        {/* BODY */}
        <div className="bg-white rounded-xl shadow p-6">

          {step === 1 && <Step1Establishment data={formData} onChange={update} />}
          {step === 2 && <Step2Employer data={formData} onChange={update} />}
          {step === 3 && <Step3ContractLabour data={formData} onChange={update} />}
          {step === 4 && <Step4CommonLicence data={formData} onChange={update} />}
          {step === 5 && <Step5SingleLicence data={formData} onChange={update} />}
          {step === 6 && <Step6Amendment data={formData} onChange={update} />}

          {/* ✅ FINAL REVIEW PAGE */}
          {/* // {step === 7 && <Step7ReviewForm27 data={formData} />} */}

          {/* NAVIGATION */}
          <div className="flex justify-between mt-8 pt-6 border-t">
            <button
              disabled={step === 1}
              onClick={() => setStep(s => s - 1)}
              className="btn btn-outline"
            >
              Previous
            </button>

            {step < totalSteps ? (
              <button
                onClick={() => setStep(s => s + 1)}
                className="btn btn-primary"
              >
                Next
              </button>
            ) : (
              <button
                onClick={() => console.log("FINAL SUBMIT", formData)}
                className="bg-green-500 hover:bg-green-600 text-white px-8 py-3 rounded-md shadow"
              >
                Submit Application
              </button>
            )}
          </div>

        </div>
      </div>
    </div>
  );
}
