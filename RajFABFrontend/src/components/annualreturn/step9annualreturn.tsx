import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";

export default function Step9AnnualReturn() {
  return (
    <Card>
      <CardContent className="p-4 space-y-3">
        <h2 className="font-semibold">
          I. Details of accidents, dangerous occurrence and notifiable diseases:
        </h2>

        {/* TABLE */}
        <div className="border border-black text-xs">
          {/* HEADER */}
          <div className="grid grid-cols-12 font-semibold text-center border-b border-black">
            <div className="col-span-1 border-r border-black p-2">
              Sl. No.
            </div>
            <div className="col-span-3 border-r border-black p-2">
              Total number of accidents by which a person injured
            </div>
            <div className="col-span-3 border-r border-black p-2">
              Total number of fatal accidents and names of the deceased
            </div>
            <div className="col-span-2 border-r border-black p-2">
              Total number Dangerous Occurrences
            </div>
            <div className="col-span-3 p-2">
              Total number of cases of Notifiable Diseases
            </div>
          </div>

          {/* ROW */}
          <div className="grid grid-cols-12 text-center">
            <div className="col-span-1 border-r border-black p-2">
              1
            </div>

            <div className="col-span-3 border-r border-black p-1">
              <Input
                type="number"
                min="0"
                className="h-8 text-center"
              />
            </div>

            <div className="col-span-3 border-r border-black p-1">
              <Input
                className="h-8"
                placeholder="Name(s) of deceased"
              />
            </div>

            <div className="col-span-2 border-r border-black p-1">
              <Input
                type="number"
                min="0"
                className="h-8 text-center"
              />
            </div>

            <div className="col-span-3 p-1">
              <Input
                type="number"
                min="0"
                className="h-8 text-center"
              />
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
