import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { useNavigate } from "react-router-dom";
import {
  Factory,
  FileText,
  RefreshCw,
  Settings,
  ArrowRightLeft,
  Clock,
  Shield,
} from "lucide-react";

interface BoilerService {
  id: string;
  name: string;
  description: string;
  category: string;
  subcategory: string;
  estimatedTime?: string;
  requiredDocuments?: number;
  isPopular?: boolean;
  url?: string;
}

interface BoilerServiceCardProps {
  service: BoilerService;
  className?: string;
  IsBoilerService?: boolean;
  Act?: string;
}

const getServiceIcon = (subcategory: string) => {
  switch (subcategory) {
    case "registration":
      return <Factory className="h-6 w-6" />;
    case "renewal":
      return <RefreshCw className="h-6 w-6" />;
    case "modification":
      return <Settings className="h-6 w-6" />;
    case "transfer":
      return <ArrowRightLeft className="h-6 w-6" />;
    default:
      return <FileText className="h-6 w-6" />;
  }
};

const getServiceColor = (subcategory: string) => {
  switch (subcategory) {
    case "registration":
      return "text-blue-600 bg-blue-50 border-blue-200";
    case "renewal":
      return "text-green-600 bg-green-50 border-green-200";
    case "modification":
      return "text-orange-600 bg-orange-50 border-orange-200";
    case "transfer":
      return "text-purple-600 bg-purple-50 border-purple-200";
    default:
      return "text-gray-600 bg-gray-50 border-gray-200";
  }
};

export function BoilerServiceCard({
  service,
  className,
  IsBoilerService = true,
  Act = "Indian Boilers Act 1923 • IBR 1950",
}: BoilerServiceCardProps) {
  const navigate = useNavigate();

  const handleServiceClick = () => {
    if (IsBoilerService)
      navigate(`/user/boiler-services/${service.subcategory}`);
    else navigate(`/user/${service.url}`);
  };

  const iconColorClass = getServiceColor(service.subcategory);

  return (
    <Card
      className={`group hover:shadow-lg transition-all duration-300 cursor-pointer border hover:border-primary/20 ${className}`}
    >
      <CardHeader className="pb-3">
        <div className="flex items-start justify-between">
          <div
            className={`rounded-lg p-3 ${iconColorClass} group-hover:scale-105 transition-transform duration-200`}
          >
            {getServiceIcon(service.subcategory)}
          </div>
          <div className="flex flex-col items-end gap-2">
            {service.isPopular && (
              <Badge
                variant="secondary"
                className="bg-yellow-100 text-yellow-800 border-yellow-200"
              >
                Popular
              </Badge>
            )}
            <Badge variant="outline" className="text-xs">
              {IsBoilerService ? "IBR Compliant" : "OSH Compliant"}
            </Badge>
          </div>
        </div>

        <div className="space-y-1">
          <CardTitle className="text-lg group-hover:text-primary transition-colors">
            {service.name}
          </CardTitle>
          <CardDescription className="text-sm line-clamp-2">
            {service.description}
          </CardDescription>
        </div>
      </CardHeader>

      <CardContent className="pt-0 space-y-4">
        <div className="flex items-center justify-between text-sm text-muted-foreground">
          {service.estimatedTime && (
            <div className="flex items-center gap-1">
              <Clock className="h-4 w-4" />
              <span>{service.estimatedTime}</span>
            </div>
          )}
          {service.requiredDocuments && (
            <div className="flex items-center gap-1">
              <FileText className="h-4 w-4" />
              <span>{service.requiredDocuments} docs</span>
            </div>
          )}
        </div>

        <div className="flex items-center gap-2 text-xs text-muted-foreground">
          <Shield className="h-3 w-3" />
          <span>{Act}</span>
        </div>

        <Button
          onClick={handleServiceClick}
          className="w-full group-hover:bg-primary group-hover:text-primary-foreground transition-colors"
          variant="outline"
        >
          Start Application
        </Button>
      </CardContent>
    </Card>
  );
}
