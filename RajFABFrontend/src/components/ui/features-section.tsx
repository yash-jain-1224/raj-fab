import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { 
  Smartphone, 
  Globe, 
  Shield, 
  Zap, 
  FileCheck, 
  Bell,
  MapPin,
  BarChart3,
  Clock,
  CheckCircle2
} from "lucide-react";

const FeaturesSection = () => {
  const features = [
    {
      icon: Smartphone,
      title: "Mobile-First Design",
      description: "Responsive interface optimized for mobile inspections and field operations"
    },
    {
      icon: Globe,
      title: "Multi-Language Support", 
      description: "Available in Hindi and English with Unicode support for accessibility"
    },
    {
      icon: Shield,
      title: "Secure & Compliant",
      description: "Enterprise-grade security with digital signatures and audit trails"
    },
    {
      icon: Zap,
      title: "Real-Time Processing",
      description: "Instant status updates and notifications throughout the application lifecycle"
    },
    {
      icon: FileCheck,
      title: "Digital Certificates",
      description: "Auto-generated certificates with eSign integration and verification portal"
    },
    {
      icon: Bell,
      title: "Smart Notifications",
      description: "SMS and email alerts for application updates and compliance reminders"
    },
    {
      icon: MapPin,
      title: "Geo-Tagged Inspections",
      description: "Location-verified inspection reports with photo and video evidence"
    },
    {
      icon: BarChart3,
      title: "Advanced Analytics",
      description: "Comprehensive dashboards and reporting for performance monitoring"
    },
    {
      icon: Clock,
      title: "24/7 Availability",
      description: "Round-the-clock access to services with minimal downtime"
    }
  ];

  const stats = [
    { value: "72h", label: "Average Processing Time", trend: "↓ 60% faster" },
    { value: "99.5%", label: "System Uptime", trend: "↑ Best in class" },
    { value: "15+", label: "Integrated Services", trend: "↑ All in one platform" },
    { value: "24/7", label: "Support Available", trend: "Always accessible" }
  ];

  return (
    <section className="py-20 px-4">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="text-center mb-16">
          <Badge variant="outline" className="mb-4">
            Platform Features
          </Badge>
          <h2 className="text-3xl lg:text-5xl font-bold text-foreground mb-6">
            Built for the Digital Age
          </h2>
          <p className="text-xl text-muted-foreground max-w-3xl mx-auto">
            Modern technology stack ensuring seamless user experience and regulatory compliance
          </p>
        </div>

        {/* Features Grid */}
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6 mb-16">
          {features.map((feature, index) => (
            <Card key={index} className="group hover:shadow-medium transition-all duration-300 border-0 bg-card/50 backdrop-blur-sm">
              <CardContent className="p-6">
                <div className="flex items-start space-x-4">
                  <div className="p-2 bg-primary/10 rounded-lg group-hover:bg-primary/20 transition-colors">
                    <feature.icon className="h-5 w-5 text-primary" />
                  </div>
                  <div className="flex-1">
                    <h3 className="font-semibold text-foreground mb-2 group-hover:text-primary transition-colors">
                      {feature.title}
                    </h3>
                    <p className="text-sm text-muted-foreground leading-relaxed">
                      {feature.description}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
        </div>

        {/* Stats Section */}
        <div className="bg-muted/30 rounded-2xl p-8">
          <div className="text-center mb-8">
            <h3 className="text-2xl font-bold text-foreground mb-2">
              Platform Performance
            </h3>
            <p className="text-muted-foreground">
              Real-time metrics showcasing our commitment to excellence
            </p>
          </div>

          <div className="grid grid-cols-2 lg:grid-cols-4 gap-6">
            {stats.map((stat, index) => (
              <div key={index} className="text-center space-y-2">
                <div className="text-3xl font-bold text-primary">
                  {stat.value}
                </div>
                <div className="text-sm font-medium text-foreground">
                  {stat.label}
                </div>
                <div className="flex items-center justify-center space-x-1">
                  <CheckCircle2 className="h-3 w-3 text-success" />
                  <span className="text-xs text-success">
                    {stat.trend}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
};

export default FeaturesSection;