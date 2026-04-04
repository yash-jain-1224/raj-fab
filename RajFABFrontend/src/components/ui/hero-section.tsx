import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { 
  Building2, 
  Shield, 
  FileCheck, 
  Clock, 
  ArrowRight,
  CheckCircle 
} from "lucide-react";

const HeroSection = () => {
  return (
    <section className="bg-gradient-hero text-primary-foreground py-20 px-4">
      <div className="max-w-7xl mx-auto">
        <div className="grid lg:grid-cols-2 gap-12 items-center">
          {/* Content */}
          <div className="space-y-8">
            <div className="space-y-4">
              <h1 className="text-4xl lg:text-6xl font-bold leading-tight">
                Digital Transformation for
                <span className="block text-accent"> Factory Safety</span>
              </h1>
              <p className="text-xl text-primary-foreground/90 max-w-lg">
                Streamlined licensing, inspections, and compliance management for factories and boilers in Rajasthan
              </p>
            </div>

            {/* Features List */}
            <div className="space-y-3">
              {[
                "Online License Applications",
                "Digital Inspection Reports", 
                "Instant Certificate Generation",
                "24/7 Status Tracking"
              ].map((feature, index) => (
                <div key={index} className="flex items-center space-x-3">
                  <CheckCircle className="h-5 w-5 text-accent" />
                  <span className="text-primary-foreground/90">{feature}</span>
                </div>
              ))}
            </div>

            {/* CTA Buttons */}
            <div className="flex flex-col sm:flex-row gap-4">
              <Button size="lg" className="bg-accent hover:bg-accent/90 text-accent-foreground">
                Apply for License
                <ArrowRight className="ml-2 h-5 w-5" />
              </Button>
              <Button size="lg" variant="outline" className="border-primary-foreground/20 text-primary-foreground hover:bg-primary-foreground/10">
                Track Application
              </Button>
            </div>
          </div>

          {/* Stats Cards */}
          <div className="grid grid-cols-2 gap-4">
            <Card className="p-6 bg-background/10 backdrop-blur-sm border-primary-foreground/20">
              <div className="flex items-center space-x-3">
                <div className="p-3 bg-accent/20 rounded-lg">
                  <Building2 className="h-6 w-6 text-accent" />
                </div>
                <div>
                  <p className="text-2xl font-bold text-primary-foreground">5,000+</p>
                  <p className="text-primary-foreground/80 text-sm">Registered Factories</p>
                </div>
              </div>
            </Card>

            <Card className="p-6 bg-background/10 backdrop-blur-sm border-primary-foreground/20">
              <div className="flex items-center space-x-3">
                <div className="p-3 bg-accent/20 rounded-lg">
                  <Shield className="h-6 w-6 text-accent" />
                </div>
                <div>
                  <p className="text-2xl font-bold text-primary-foreground">98%</p>
                  <p className="text-primary-foreground/80 text-sm">Safety Compliance</p>
                </div>
              </div>
            </Card>

            <Card className="p-6 bg-background/10 backdrop-blur-sm border-primary-foreground/20">
              <div className="flex items-center space-x-3">
                <div className="p-3 bg-accent/20 rounded-lg">
                  <FileCheck className="h-6 w-6 text-accent" />
                </div>
                <div>
                  <p className="text-2xl font-bold text-primary-foreground">2,500+</p>
                  <p className="text-primary-foreground/80 text-sm">Certificates Issued</p>
                </div>
              </div>
            </Card>

            <Card className="p-6 bg-background/10 backdrop-blur-sm border-primary-foreground/20">
              <div className="flex items-center space-x-3">
                <div className="p-3 bg-accent/20 rounded-lg">
                  <Clock className="h-6 w-6 text-accent" />
                </div>
                <div>
                  <p className="text-2xl font-bold text-primary-foreground">72h</p>
                  <p className="text-primary-foreground/80 text-sm">Avg. Processing</p>
                </div>
              </div>
            </Card>
          </div>
        </div>
      </div>
    </section>
  );
};

export default HeroSection;