import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";

export default function Step10AnnualReturn() {
  return (
    <Card>
      <CardContent className="p-4 space-y-3">
        <h2 className="font-semibold">
          J. Mandays and Production Lost due to accidents / dangerous occurrence:
        </h2>

        {/* TABLE */}
        <div className="border border-black text-xs">
          {/* HEADER */}
          <div className="grid grid-cols-12 font-semibold text-center border-b border-black">
            <div className="col-span-1 border-r border-black p-2">
              Sl. No.
            </div>
            <div className="col-span-6 border-r border-black p-2">
              Accident / Dangerous Occurrence
            </div>
            <div className="col-span-3 border-r border-black p-2">
              Mandays lost
            </div>
            <div className="col-span-2 p-2">
              Production Lost
            </div>
          </div>

          {/* ROW */}
          <div className="grid grid-cols-12 text-center">
            <div className="col-span-1 border-r border-black p-2">
              1
            </div>

            <div className="col-span-6 border-r border-black p-1">
              <Input
                className="h-8"
                placeholder="Describe accident / occurrence"
              />
            </div>

            <div className="col-span-3 border-r border-black p-1">
              <Input
                type="number"
                min="0"
                className="h-8 text-center"
              />
            </div>

            <div className="col-span-2 p-1">
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
