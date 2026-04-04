import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Factory, Calendar, MapPin, ChevronRight } from "lucide-react";
import { FactoryRegistration } from "@/types/factoryRegistration";
import { format } from "date-fns";

interface FactorySelectionCardProps {
  factory: FactoryRegistration;
  onSelect: () => void;
}

export function FactorySelectionCard({ factory, onSelect }: FactorySelectionCardProps) {
  return (
    <Card className="hover:shadow-lg transition-shadow cursor-pointer border-l-4 border-l-primary">
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div className="flex items-center gap-3">
            <div className="p-2 bg-primary/10 rounded-lg">
              <Factory className="h-5 w-5 text-primary" />
            </div>
            <div>
              <CardTitle className="text-lg">{factory.factoryName}</CardTitle>
              <p className="text-sm text-muted-foreground mt-1">
                Registration: {factory.registrationNumber}
              </p>
            </div>
          </div>
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        <div className="flex items-center gap-2 text-sm">
          <MapPin className="h-4 w-4 text-muted-foreground" />
          <span className="text-muted-foreground">
            {factory.cityTown}, {factory.district}
          </span>
        </div>
        <div className="flex items-center gap-2 text-sm">
          <Calendar className="h-4 w-4 text-muted-foreground" />
          <span className="text-muted-foreground">
            License Valid: {format(new Date(factory.licenseFromDate), "dd/MM/yyyy")} - {format(new Date(factory.licenseToDate), "dd/MM/yyyy")}
          </span>
        </div>
        <Button 
          onClick={onSelect}
          className="w-full mt-4"
        >
          Apply for Renewal
          <ChevronRight className="h-4 w-4 ml-2" />
        </Button>
      </CardContent>
    </Card>
  );
}
