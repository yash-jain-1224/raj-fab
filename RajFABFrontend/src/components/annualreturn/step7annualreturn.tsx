import { Card, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";

export default function Step7AnnualReturn() {
  return (
    <Card>
      <CardContent className="p-4 space-y-2">
        <h2 className="font-semibold">
          G. Details pertaining to maternity benefit:
        </h2>

        <div className="grid grid-cols-4 border border-black text-xs">
          <Header title="No. of female employees" />
          <Header title="No. of female employees availed maternity leave" />
          <Header title="No. of female employees paid medical bonus" />
          <Header title="No. of deduction of wages, if any made from female employees" />

          {[1, 2, 3, 4].map((i) => (
            <div key={i} className="border-t border-black p-1">
              <Input type="number" className="h-8 text-center" />
            </div>
          ))}
        </div>
      </CardContent>
    </Card>
  );
}

const Header = ({ title }: { title: string }) => (
  <div className="border-r border-black p-2 font-semibold text-center">
    {title}
  </div>
);
