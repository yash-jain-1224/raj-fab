import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { 
  Building, 
  UserCheck, 
  Users, 
  Shield, 
  FileSearch,
  ChevronRight,
  ExternalLink
} from "lucide-react";

const UserPortals = () => {
  const portals = [
    {
      icon: Building,
      title: "Industry Portal",
      description: "For factory owners, manufacturers, and industrial units",
      features: [
        "Submit license applications",
        "Track application status", 
        "Download certificates",
        "Schedule inspections",
        "Pay fees online"
      ],
      userTypes: ["Factory Owners", "Manufacturers", "Industrial Units"],
      bgColor: "bg-primary/5",
      iconBg: "bg-primary/10",
      iconColor: "text-primary"
    },
    {
      icon: UserCheck,
      title: "Inspector Portal",
      description: "For government inspectors and field officers",
      features: [
        "View assigned inspections",
        "Submit digital reports",
        "Upload geo-tagged media",
        "Access compliance checklists",
        "Generate inspection certificates"
      ],
      userTypes: ["Government Inspectors", "Field Officers", "Safety Officers"],
      bgColor: "bg-success/5",
      iconBg: "bg-success/10", 
      iconColor: "text-success"
    },
    {
      icon: Shield,
      title: "Third-Party Portal",
      description: "For competent persons and safety engineers",
      features: [
        "Register for empanelment",
        "Upload verification reports",
        "Track report submissions",
        "Access training materials",
        "Update certifications"
      ],
      userTypes: ["Competent Persons", "Safety Engineers", "Boiler Engineers"],
      bgColor: "bg-warning/5",
      iconBg: "bg-warning/10",
      iconColor: "text-warning"
    },
    {
      icon: Users,
      title: "Department Portal",
      description: "For government officers and administrators",
      features: [
        "Process applications",
        "Review inspection reports",
        "Generate analytics",
        "Manage workflows",
        "Configure system settings"
      ],
      userTypes: ["Department Officers", "Administrators", "Policy Makers"],
      bgColor: "bg-info/5",
      iconBg: "bg-info/10",
      iconColor: "text-info"
    }
  ];

  return (
    <section className="py-20 px-4">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="text-center mb-16">
          <Badge variant="outline" className="mb-4">
            User Portals
          </Badge>
          <h2 className="text-3xl lg:text-5xl font-bold text-foreground mb-6">
            Tailored Access for Every User
          </h2>
          <p className="text-xl text-muted-foreground max-w-3xl mx-auto">
            Role-based portals designed for different stakeholders in the factory and boiler ecosystem
          </p>
        </div>

        {/* Portals Grid */}
        <div className="grid lg:grid-cols-2 gap-8 mb-12">
          {portals.map((portal, index) => (
            <Card key={index} className={`group hover:shadow-medium transition-all duration-300 ${portal.bgColor} border-0`}>
              <CardHeader className="pb-4">
                <div className="flex items-start justify-between">
                  <div className={`p-3 ${portal.iconBg} rounded-lg group-hover:scale-110 transition-transform`}>
                    <portal.icon className={`h-6 w-6 ${portal.iconColor}`} />
                  </div>
                  <Button variant="ghost" size="sm" className="group-hover:bg-background/20">
                    <ExternalLink className="h-4 w-4" />
                  </Button>
                </div>
                <div>
                  <CardTitle className="text-xl mb-2 group-hover:text-primary transition-colors">
                    {portal.title}
                  </CardTitle>
                  <CardDescription className="text-base">
                    {portal.description}
                  </CardDescription>
                </div>
              </CardHeader>
              
              <CardContent className="space-y-6">
                {/* Features */}
                <div className="space-y-3">
                  <h4 className="font-semibold text-foreground">Key Features:</h4>
                  <div className="space-y-2">
                    {portal.features.map((feature, idx) => (
                      <div key={idx} className="flex items-center space-x-3">
                        <ChevronRight className="h-4 w-4 text-muted-foreground" />
                        <span className="text-sm text-muted-foreground">{feature}</span>
                      </div>
                    ))}
                  </div>
                </div>

                {/* User Types */}
                <div className="space-y-3">
                  <h4 className="font-semibold text-foreground">User Types:</h4>
                  <div className="flex flex-wrap gap-2">
                    {portal.userTypes.map((type, idx) => (
                      <Badge key={idx} variant="secondary" className="text-xs">
                        {type}
                      </Badge>
                    ))}
                  </div>
                </div>

                {/* Access Button */}
                <Button className="w-full group-hover:bg-primary/90 transition-colors">
                  Access Portal
                  <ChevronRight className="ml-2 h-4 w-4" />
                </Button>
              </CardContent>
            </Card>
          ))}
        </div>

        {/* Public Services */}
        <div className="bg-gradient-accent rounded-2xl p-8 text-center">
          <div className="flex items-center justify-center mb-4">
            <div className="p-3 bg-accent-foreground/10 rounded-lg">
              <FileSearch className="h-8 w-8 text-accent-foreground" />
            </div>
          </div>
          <h3 className="text-2xl font-bold text-accent-foreground mb-4">
            Public Information Portal
          </h3>
          <p className="text-accent-foreground/90 mb-6 max-w-2xl mx-auto">
            Access public information, file grievances, and track resolution status without requiring login
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Button size="lg" variant="secondary">
              Search Factory Records
            </Button>
            <Button size="lg" variant="outline" className="border-accent-foreground/20 text-accent-foreground hover:bg-accent-foreground/10">
              File Grievance
            </Button>
          </div>
        </div>
      </div>
    </section>
  );
};

export default UserPortals;