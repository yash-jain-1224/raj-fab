import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";

export default function Step8AnnualReturn() {
  return (
    <Card>
      <CardContent className="p-4 space-y-3">
        <h2 className="font-semibold">
          H. Details of payment of bonus:
        </h2>

        {/* TABLE */}
        <div className="border border-black text-xs">
          {/* HEADER */}
          <div className="grid grid-cols-12 font-semibold text-center border-b border-black">
            <div className="col-span-1 border-r border-black p-2">
              Sl. No.
            </div>
            <div className="col-span-4 border-r border-black p-2">
              No. of employees covered under the Bonus provision
            </div>
            <div className="col-span-4 border-r border-black p-2">
              Total amount of bonus actually paid
            </div>
            <div className="col-span-3 p-2">
              Date on which the Bonus paid
            </div>
          </div>

          {/* ROW */}
          <div className="grid grid-cols-12 text-center">
            <div className="col-span-1 border-r border-black p-2">
              1
            </div>

            <div className="col-span-4 border-r border-black p-1">
              <Input
                type="number"
                min="0"
                className="h-8 text-center"
              />
            </div>

            <div className="col-span-4 border-r border-black p-1">
              <Input
                type="number"
                min="0"
                className="h-8 text-center"
              />
            </div>

            <div className="col-span-3 p-1">
              <Input type="date" className="h-8" />
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
