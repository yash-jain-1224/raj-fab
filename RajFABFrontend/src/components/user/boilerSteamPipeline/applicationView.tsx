import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { boilerSteamPipelinesInfo } from "@/hooks/api/useBoilers";

export default function BoilerSteamPipelineDetails({ formId }: { formId: string }) {
  const { data, isLoading, error } = boilerSteamPipelinesInfo(formId || "skip");
  const appData = (data as any)?.data || data || {};

  if (isLoading) {
    return <div className="text-sm text-muted-foreground">Loading steam pipeline details...</div>;
  }

  if (error) {
    return <div className="text-sm text-destructive">Failed to load steam pipeline details.</div>;
  }

  return (
    <Card>
      <CardHeader>
        <CardTitle>Steam Pipeline Details</CardTitle>
      </CardHeader>
      <CardContent>
        <table className="w-full border border-gray-300 text-sm">
          <tbody>
            {Object.entries(appData).map(([key, value]) => (
              <tr key={key}>
                <td className="bg-gray-100 px-2 py-1 border">{labelize(key)}</td>
                <td className="px-2 py-1 border">{String(value ?? "-")}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </CardContent>
    </Card>
  );
}

function labelize(text: string) {
  return text.replace(/([A-Z])/g, " $1").replace(/^./, (s) => s.toUpperCase());
}

