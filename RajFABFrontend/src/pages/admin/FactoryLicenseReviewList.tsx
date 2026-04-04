import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Badge } from "@/components/ui/badge";
import { useFactoryLicenseList } from "@/hooks/api/useFactoryLicense";
import { Search, Eye } from "lucide-react";

export default function FactoryLicenseReviewList() {
  const navigate = useNavigate();
  const { data: licenses, isLoading } = useFactoryLicenseList();
  const [searchTerm, setSearchTerm] = useState("");

  const filtered = licenses?.filter((l) =>
    l.factoryRegistrationNumber.toLowerCase().includes(searchTerm.toLowerCase())
  );

  if (isLoading) return <div className="container mx-auto p-6">Loading...</div>;

  return (
    <div className="container mx-auto p-6 space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Factory Licenses</h1>
        <p className="text-muted-foreground">View submitted factory licenses</p>
      </div>

      <Card>
        <CardHeader>
          <div className="flex items-center gap-4">
            <div className="flex-1 relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input placeholder="Search by registration number..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)} className="pl-10" />
            </div>
          </div>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {filtered?.map((l) => (
              <div key={l.id} className="p-4 border rounded-lg hover:bg-accent cursor-pointer transition-colors" onClick={() => navigate(`/admin/factory-license-review/${l.id}`)}>
                <div className="flex justify-between items-start">
                  <div className="space-y-1 flex-1">
                    <div className="flex items-center gap-3">
                      <h3 className="font-semibold">{l.factoryRegistrationNumber}</h3>
                      <Badge variant="secondary">Active</Badge>
                    </div>
                    <p className="text-sm font-medium text-muted-foreground">Valid: {new Date(l.validFrom).toLocaleDateString()} - {new Date(l.validTo).toLocaleDateString()}</p>
                  </div>
                  <Button variant="outline" size="sm">
                    <Eye className="h-4 w-4 mr-2" />
                    View Details
                  </Button>
                </div>
              </div>
            ))}
            {filtered?.length === 0 && <div className="text-center py-8 text-muted-foreground">No licenses found</div>}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
