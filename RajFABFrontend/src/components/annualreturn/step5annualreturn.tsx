import React, { useState } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Label } from "@/components/ui/label";

export default function Step5AnnualReturn() {
  const [data, setData] = useState({
    canteen: "",
    creches: "",
    ambulanceRoom: "",
    safetyCommittee: "",
    safetyOfficers: "",
    medicalPractitioners: "",
  });

  const updateValue = (key: string, value: string) => {
    setData((prev) => ({ ...prev, [key]: value }));
  };

  const YesNoRadio = ({
    value,
    onChange,
  }: {
    value: string;
    onChange: (v: string) => void;
  }) => (
    <RadioGroup
      value={value}
      onValueChange={onChange}
      className="flex justify-center gap-6"
    >
      <div className="flex items-center gap-2">
        <RadioGroupItem value="Yes" id={`${value}-yes`} />
        <Label>Yes</Label>
      </div>
      <div className="flex items-center gap-2">
        <RadioGroupItem value="No" id={`${value}-no`} />
        <Label>No</Label>
      </div>
    </RadioGroup>
  );

  return (
    <Card className="shadow-md">
      <CardContent className="p-4 space-y-4">
        <h2 className="text-lg font-semibold">
          E. Details of various Health and Welfare Amenities provided.
        </h2>

        <div className="border border-black text-xs">
          {/* HEADER */}
          <div className="grid grid-cols-12 border-b border-black font-semibold text-center">
            <div className="col-span-1 border-r border-black p-2">Sl. No.</div>
            <div className="col-span-4 border-r border-black p-2">
              Nature of various welfare amenities provided
            </div>
            <div className="col-span-3 border-r border-black p-2">
              Statutory (specify the statute)
            </div>
            <div className="col-span-4 p-2">Instructions for filling</div>
          </div>

          {/* ROW 1 */}
          <Row
            sl="1."
            title="Whether facility of Canteen provided (as per section 24(v) of OSH Code, 2020)"
            control={
              <YesNoRadio
                value={data.canteen}
                onChange={(v) => updateValue("canteen", v)}
              />
            }
            instruction="Applicable to all establishments wherein hundred or more workers including contract labour were ordinarily employed"
          />

          {/* ROW 2 */}
          <Row
            sl="2."
            title="Crèches (as per section 67 of Code on Social Security Code, 2020 and Section 24 of the OSH Code 2020)"
            control={
              <YesNoRadio
                value={data.creches}
                onChange={(v) => updateValue("creches", v)}
              />
            }
            instruction="Applicable to all establishments where fifty or more workers are employed"
          />

          {/* ROW 3 */}
          <Row
            sl="3."
            title="Ambulance Room (as per section 24(2)(i) of OSH Code, 2020)"
            control={
              <YesNoRadio
                value={data.ambulanceRoom}
                onChange={(v) => updateValue("ambulanceRoom", v)}
              />
            }
            instruction="Applicable to mine, building and other construction work wherein more than five hundred workers are ordinarily employed"
          />

          {/* ROW 4 */}
          <Row
            sl="4."
            title="Safety Committee (as per Section 22(1) of OSH Code, 2020)"
            control={
              <YesNoRadio
                value={data.safetyCommittee}
                onChange={(v) => updateValue("safetyCommittee", v)}
              />
            }
            instruction="Applicable to establishments and factories employing 500 workers or more, factory carrying on hazardous process and BoCW employing 250 workers or more, and mines employing 100 or more workers"
          />

          {/* ROW 5 */}
          <Row
            sl="5."
            title="Safety Officer (as per section 22(2) of OSH Code, 2020)"
            control={
              <Input
                type="number"
                min="0"
                value={data.safetyOfficers}
                onChange={(e) =>
                  updateValue("safetyOfficers", e.target.value)
                }
                className="h-8"
              />
            }
            instruction="In case of mine 100 or more workers and in case of BoCW 250 or more workers are ordinarily employed"
          />

          {/* ROW 6 */}
          <Row
            sl="6."
            title="Qualified Medical Practitioner (as per Section 12(2) of OSH Code, 2020)"
            control={
              <Input
                type="number"
                min="0"
                value={data.medicalPractitioners}
                onChange={(e) =>
                  updateValue("medicalPractitioners", e.target.value)
                }
                className="h-8"
              />
            }
            instruction="There is no specification for minimum number of Qualified Medical Practitioner employed in establishment. However, this detail is required to have data on occupational health."
          />
        </div>
      </CardContent>
    </Card>
  );
}

/* ---------------- Row Component ---------------- */

function Row({
  sl,
  title,
  control,
  instruction,
}: {
  sl: string;
  title: string;
  control: React.ReactNode;
  instruction: string;
}) {
  return (
    <div className="grid grid-cols-12 border-b border-black">
      <div className="col-span-1 border-r border-black p-2 text-center">
        {sl}
      </div>
      <div className="col-span-4 border-r border-black p-2">
        {title}
      </div>
      <div className="col-span-3 border-r border-black p-2 flex justify-center">
        {control}
      </div>
      <div className="col-span-4 p-2">{instruction}</div>
    </div>
  );
}
