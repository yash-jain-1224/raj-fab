import React, { useState } from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";

/* ---------------- Yes / No Radio ---------------- */

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
      <RadioGroupItem value="Yes" id="yes" />
      <Label>Yes</Label>
    </div>
    <div className="flex items-center gap-2">
      <RadioGroupItem value="No" id="no" />
      <Label>No</Label>
    </div>
  </RadioGroup>
);

/* ---------------- Main Component ---------------- */

export default function Step6AnnualReturn() {
  const [data, setData] = useState({
    worksCommittee: "",
    worksCommitteeDate: "",
    grievanceCommittee: "",
    numberOfUnions: "",
    negotiationUnion: "",
    negotiationCouncil: "",
    discharged: "",
    dismissed: "",
    retrenched: "",
    terminated: "",
  });

  const update = (key: string, value: string) =>
    setData((prev) => ({ ...prev, [key]: value }));

  const grandTotal =
    Number(data.discharged || 0) +
    Number(data.dismissed || 0) +
    Number(data.retrenched || 0) +
    Number(data.terminated || 0);

  return (
    <Card className="shadow-md">
      <CardContent className="p-4 space-y-4">
        <h2 className="text-lg font-semibold">
          F. The Industrial Relations:
        </h2>

        <div className="border border-black text-xs">
          {/* HEADER */}
          <div className="grid grid-cols-12 border-b border-black font-semibold text-center">
            <div className="col-span-1 border-r border-black p-2">Sl. No.</div>
            <div className="col-span-7 border-r border-black p-2">
              Particulars
            </div>
            <div className="col-span-2 border-r border-black p-2">
              Yes / No / No.
            </div>
            <div className="col-span-2 p-2">Instructions for filling</div>
          </div>

          {/* ROW 1 */}
          <Row
            sl="1."
            text="Is the Works Committee has been functioning. (section 3 of IR Code, 2020)"
            control={
              <YesNoRadio
                value={data.worksCommittee}
                onChange={(v) => update("worksCommittee", v)}
              />
            }
            instruction="Industrial establishment in which 100 or more workers are employed"
          />

          {/* ROW 1(a) */}
          <Row
            sl="(a)"
            text="Date of its constitution."
            control={
              <Input
                type="date"
                value={data.worksCommitteeDate}
                onChange={(e) =>
                  update("worksCommitteeDate", e.target.value)
                }
                className="h-8"
              />
            }
            instruction=""
          />

          {/* ROW 2 */}
          <Row
            sl="2."
            text="Whether the Grievance Redressal Committee constituted (section 4 of IR Code, 2020)"
            control={
              <YesNoRadio
                value={data.grievanceCommittee}
                onChange={(v) => update("grievanceCommittee", v)}
              />
            }
            instruction="Industrial establishment employing 20 or more workers are employed"
          />

          {/* ROW 3 */}
          <Row
            sl="3."
            text="Number of Unions in the establishments."
            control={
              <Input
                type="number"
                min="0"
                value={data.numberOfUnions}
                onChange={(e) => update("numberOfUnions", e.target.value)}
                className="h-8 text-center"
              />
            }
            instruction=""
          />

          {/* ROW 4 */}
          <Row
            sl="4."
            text="Whether any negotiation union exist (Section 14 of IR Code, 2020)"
            control={
              <YesNoRadio
                value={data.negotiationUnion}
                onChange={(v) => update("negotiationUnion", v)}
              />
            }
            instruction=""
          />

          {/* ROW 5 */}
          <Row
            sl="5."
            text="Whether any negotiating council is constituted (Section 14 of IR Code, 2020)"
            control={
              <YesNoRadio
                value={data.negotiationCouncil}
                onChange={(v) => update("negotiationCouncil", v)}
              />
            }
            instruction=""
          />

          {/* ROW 6 – SEPARATE TABLE */}
          <div className="border-t border-black">
            <div className="p-2 font-semibold">
              6. Number of workers discharged, dismissed, retrenched or whose
              services were terminated during the year:
            </div>

            <div className="grid grid-cols-5 text-center font-semibold border-t border-black">
              <Cell title="Discharged" />
              <Cell title="Dismissed" />
              <Cell title="Retrenched" />
              <Cell title="Terminated or Removed" />
              <Cell title="Grand Total" />
            </div>

            <div className="grid grid-cols-5 text-center border-t border-black">
              <Cell>
                <Input
                  type="number"
                  min="0"
                  value={data.discharged}
                  onChange={(e) => update("discharged", e.target.value)}
                  className="h-8 text-center"
                />
              </Cell>
              <Cell>
                <Input
                  type="number"
                  min="0"
                  value={data.dismissed}
                  onChange={(e) => update("dismissed", e.target.value)}
                  className="h-8 text-center"
                />
              </Cell>
              <Cell>
                <Input
                  type="number"
                  min="0"
                  value={data.retrenched}
                  onChange={(e) => update("retrenched", e.target.value)}
                  className="h-8 text-center"
                />
              </Cell>
              <Cell>
                <Input
                  type="number"
                  min="0"
                  value={data.terminated}
                  onChange={(e) => update("terminated", e.target.value)}
                  className="h-8 text-center"
                />
              </Cell>
              <Cell>
                <Input
                  readOnly
                  value={grandTotal}
                  className="h-8 text-center bg-muted"
                />
              </Cell>
            </div>
          </div>


          {/* ---------- SECTION 7 ---------- */}
<div className="border-t border-black">
  <div className="p-2 font-semibold">
    7. Man-days lost during the year on account of
  </div>

  <div className="grid grid-cols-12 border-t border-black text-xs font-semibold text-center">
    <div className="col-span-1 border-r border-black p-2">Sl. No.</div>
    <div className="col-span-3 border-r border-black p-2">Reasons</div>
    <div className="col-span-3 border-r border-black p-2">Period / Date</div>
    <div className="col-span-2 border-r border-black p-2">
      No. of man-days lost
    </div>
    <div className="col-span-3 p-2">Loss in term of money</div>
  </div>

  {/* Strike */}
  <div className="grid grid-cols-12 border-t border-black text-xs">
    <div className="col-span-1 border-r border-black p-2 text-center">(a)</div>
    <div className="col-span-3 border-r border-black p-2">Strike</div>
    <div className="col-span-3 border-r border-black p-1">
      <Input className="h-8" />
    </div>
    <div className="col-span-2 border-r border-black p-1">
      <Input type="number" className="h-8 text-center" />
    </div>
    <div className="col-span-3 p-1">
      <Input type="number" className="h-8 text-center" />
    </div>
  </div>

  {/* Lockout */}
  <div className="grid grid-cols-12 border-t border-black text-xs">
    <div className="col-span-1 border-r border-black p-2 text-center">(b)</div>
    <div className="col-span-3 border-r border-black p-2">Lockout</div>
    <div className="col-span-3 border-r border-black p-1">
      <Input className="h-8" />
    </div>
    <div className="col-span-2 border-r border-black p-1">
      <Input type="number" className="h-8 text-center" />
    </div>
    <div className="col-span-3 p-1">
      <Input type="number" className="h-8 text-center" />
    </div>
  </div>
</div>


{/* ---------- SECTION 8 ---------- */}
<div className="border-t border-black">
  <div className="p-2 font-semibold">
    8. Details of retrenchment / lay off
  </div>

  <div className="grid grid-cols-12 border-t border-black text-xs font-semibold text-center">
    <div className="col-span-1 border-r border-black p-2">Sl. No.</div>
    <div className="col-span-3 border-r border-black p-2">
      No. of persons retrenched during the period
    </div>
    <div className="col-span-3 border-r border-black p-2">
      Details of payment paid to retrenched employees
    </div>
    <div className="col-span-3 border-r border-black p-2">
      No. of workers laid off during the period
    </div>
    <div className="col-span-2 p-2">
      No. of man-days lost due to lay-off
    </div>
  </div>

  {/* ROW */}
  <div className="grid grid-cols-12 border-t border-black text-xs">
    <div className="col-span-1 border-r border-black p-2 text-center">1</div>
    <div className="col-span-3 border-r border-black p-1">
      <Input type="number" className="h-8 text-center" />
    </div>
    <div className="col-span-3 border-r border-black p-1">
      <Input className="h-8" />
    </div>
    <div className="col-span-3 border-r border-black p-1">
      <Input type="number" className="h-8 text-center" />
    </div>
    <div className="col-span-2 p-1">
      <Input type="number" className="h-8 text-center" />
    </div>
  </div>
</div>

        </div>
      </CardContent>
    </Card>
  );
}

/* ---------------- Reusable Cells ---------------- */

function Row({
  sl,
  text,
  control,
  instruction,
}: {
  sl: string;
  text: string;
  control: React.ReactNode;
  instruction: string;
}) {
  return (
    <div className="grid grid-cols-12 border-b border-black">
      <div className="col-span-1 border-r border-black p-2 text-center">
        {sl}
      </div>
      <div className="col-span-7 border-r border-black p-2">
        {text}
      </div>
      <div className="col-span-2 border-r border-black p-2 flex justify-center">
        {control}
      </div>
      <div className="col-span-2 p-2">{instruction}</div>
    </div>
  );
}

function Cell({
  title,
  children,
}: {
  title?: string;
  children?: React.ReactNode;
}) {
  return (
    <div className="border-r border-black p-2">
      {title ?? children}
    </div>
  );
}
