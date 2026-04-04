import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { 
  Building2, 
  Cog, 
  FileText, 
  Search, 
  GraduationCap, 
  MessageSquare,
  ArrowRight,
  Star,
  Clock,
  Shield
} from "lucide-react";

const ServicesSection = () => {
  const services = [
    {
      icon: Building2,
      title: "Factory Licensing",
      description: "Complete factory registration, license applications, and renewals with digital document management",
      features: ["New Registration", "License Renewal", "Amendment Applications", "Closure Applications"],
      status: "Available",
      timeline: "2-5 days"
    },
    {
      icon: Cog,
      title: "Boiler Registration",
      description: "Comprehensive boiler safety registration, inspection scheduling, and compliance tracking",
      features: ["New Boiler Registration", "Renewal Applications", "Modification Requests", "Safety Inspections"],
      status: "Available", 
      timeline: "3-7 days"
    },
    {
      icon: Search,
      title: "Inspections & Audits",
      description: "Digital inspection processes with geo-tagged reporting and real-time compliance monitoring",
      features: ["Scheduled Inspections", "Digital Reports", "Compliance Tracking", "Risk Assessment"],
      status: "Available",
      timeline: "48 hours"
    },
    {
      icon: FileText,
      title: "Certification Services",
      description: "Automated certificate generation with digital signatures and verification systems",
      features: ["Digital Certificates", "eSign Integration", "Verification Portal", "Document Archive"],
      status: "Available",
      timeline: "24 hours"
    },
    {
      icon: GraduationCap,
      title: "Safety Training (SMTC)",
      description: "Professional safety training programs for workers, supervisors, and managers",
      features: ["Training Programs", "Certification Courses", "Attendance Tracking", "Digital Certificates"],
      status: "Available",
      timeline: "Ongoing"
    },
    {
      icon: MessageSquare,
      title: "Grievance Portal",
      description: "Transparent grievance redressal system with tracking and feedback mechanisms",
      features: ["Online Grievance Filing", "Status Tracking", "Resolution Timeline", "Feedback System"],
      status: "Available",
      timeline: "7-15 days"
    }
  ];

  return (
    <section id="services" className="py-20 px-4 bg-muted/30">
      <div className="max-w-7xl mx-auto">
        {/* Header */}
        <div className="text-center mb-16">
          <Badge variant="secondary" className="mb-4">
            Digital Services
          </Badge>
          <h2 className="text-3xl lg:text-5xl font-bold text-foreground mb-6">
            Comprehensive Factory & Boiler Services
          </h2>
          <p className="text-xl text-muted-foreground max-w-3xl mx-auto">
            Streamlined digital platform for all your factory and boiler regulatory needs in Rajasthan
          </p>
        </div>

        {/* Services Grid */}
        <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-8 mb-12">
          {services.map((service, index) => (
            <Card key={index} className="group hover:shadow-medium transition-all duration-300 hover:scale-105">
              <CardHeader>
                <div className="flex items-center justify-between mb-4">
                  <div className="p-3 bg-primary/10 rounded-lg group-hover:bg-primary/20 transition-colors">
                    <service.icon className="h-6 w-6 text-primary" />
                  </div>
                  <Badge variant={service.status === "Available" ? "default" : "secondary"}>
                    {service.status}
                  </Badge>
                </div>
                <CardTitle className="text-xl group-hover:text-primary transition-colors">
                  {service.title}
                </CardTitle>
                <CardDescription className="text-base">
                  {service.description}
                </CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                {/* Features */}
                <div className="space-y-2">
                  {service.features.map((feature, idx) => (
                    <div key={idx} className="flex items-center space-x-2 text-sm">
                      <div className="w-1.5 h-1.5 bg-primary rounded-full" />
                      <span className="text-muted-foreground">{feature}</span>
                    </div>
                  ))}
                </div>

                {/* Timeline */}
                <div className="flex items-center space-x-2 pt-2 border-t border-border">
                  <Clock className="h-4 w-4 text-muted-foreground" />
                  <span className="text-sm text-muted-foreground">Timeline: {service.timeline}</span>
                </div>

                {/* Action Button */}
                <Button className="w-full group-hover:bg-primary/90 transition-colors">
                  Get Started
                  <ArrowRight className="ml-2 h-4 w-4" />
                </Button>
              </CardContent>
            </Card>
          ))}
        </div>

        {/* Quick Actions */}
        <div className="bg-gradient-primary rounded-2xl p-8 text-center">
          <h3 className="text-2xl font-bold text-primary-foreground mb-4">
            Need Immediate Assistance?
          </h3>
          <p className="text-primary-foreground/90 mb-6 max-w-2xl mx-auto">
            Our expert team is available to help you navigate the application process and answer your questions
          </p>
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Button size="lg" variant="secondary">
              <Shield className="mr-2 h-5 w-5" />
              Emergency Hotline
            </Button>
            <Button size="lg" variant="outline" className="border-primary-foreground/20 text-primary-foreground hover:bg-primary-foreground/10">
              <MessageSquare className="mr-2 h-5 w-5" />
              Live Chat Support
            </Button>
          </div>
        </div>
      </div>
    </section>
  );
};

export default ServicesSection;