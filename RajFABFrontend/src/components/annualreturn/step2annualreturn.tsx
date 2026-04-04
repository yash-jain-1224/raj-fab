import React from "react";
import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";

/**
 * Step B(a) & B(b) – Hours of Work and Number of Shifts
 * As per Unified Annual Return (Form-25 style)
 */
export default function Step2annualreturn({ formData, updateFormData, errors }) {
  return (
    <Card className="shadow-md">
      <CardContent className="p-6 space-y-6">
        <h2 className="text-xl font-semibold">B. Hours of Work</h2>

        {/* B(a) Hours of Work in a Day */}
        <div className="border rounded-md p-4 space-y-3">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-center">
            <Label className="font-medium">
              B(a). Hours of Work in a Day
            </Label>

            <div className="flex gap-2">
              <Input
                type="number"
                min={1}
                max={24}
                placeholder="Total working hours"
                value={formData.hoursPerDay || ""}
                onChange={(e) => updateFormData("hoursPerDay", e.target.value)}
              />
              <span className="self-center text-sm text-muted-foreground">
                Hours
              </span>
            </div>

            <p className="text-sm text-muted-foreground">
              Excluding rest interval / overtime
            </p>
          </div>

          {errors?.hoursPerDay && (
            <p className="text-red-600 text-sm">{errors.hoursPerDay}</p>
          )}
        </div>

        {/* B(b) Number of Shifts */}
        <div className="border rounded-md p-4 space-y-3">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4 items-center">
            <Label className="font-medium">
              B(b). Number of Shifts
            </Label>

            <Input
              type="number"
              min={1}
              max={5}
              placeholder="Enter number of shifts"
              value={formData.numberOfShifts || ""}
              onChange={(e) => updateFormData("numberOfShifts", e.target.value)}
            />

            <p className="text-sm text-muted-foreground">
              Day / Night / Rotational shifts
            </p>
          </div>

          {errors?.numberOfShifts && (
            <p className="text-red-600 text-sm">{errors.numberOfShifts}</p>
          )}
        </div>

        {/* Optional info note */}
        <p className="text-sm text-muted-foreground">
          Note: Details should comply with provisions of applicable labour laws
          and rules.
        </p>
      </CardContent>
    </Card>
  );
}
