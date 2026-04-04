import { Button } from "@/components/ui/button";
import { Separator } from "@/components/ui/separator";
import { 
  Building2, 
  Mail, 
  Phone, 
  MapPin, 
  ExternalLink,
  Facebook,
  Twitter,
  Youtube,
  Globe
} from "lucide-react";

const Footer = () => {
  const quickLinks = [
    { name: "Factory License", href: "#" },
    { name: "Boiler Registration", href: "#" },
    { name: "Track Application", href: "#" },
    { name: "Download Forms", href: "#" },
    { name: "Pay Fees", href: "#" },
    { name: "Training Programs", href: "#" }
  ];

  const supportLinks = [
    { name: "Help Center", href: "#" },
    { name: "User Manual", href: "#" },
    { name: "Video Tutorials", href: "#" },
    { name: "FAQ", href: "#" },
    { name: "Technical Support", href: "#" },
    { name: "Contact Us", href: "#" }
  ];

  const legalLinks = [
    { name: "Privacy Policy", href: "#" },
    { name: "Terms of Service", href: "#" },
    { name: "Accessibility", href: "#" },
    { name: "RTI", href: "#" },
    { name: "Grievance Portal", href: "#" },
    { name: "Citizen Charter", href: "#" }
  ];

  return (
    <footer className="bg-foreground text-background">
      <div className="max-w-7xl mx-auto px-4 py-12">
        {/* Main Footer Content */}
        <div className="grid lg:grid-cols-4 gap-8 mb-8">
          {/* Brand Section */}
          <div className="space-y-4">
            <div className="flex items-center space-x-2">
              <Building2 className="h-8 w-8 text-accent" />
              <div>
                <h3 className="text-xl font-bold">RajFAB</h3>
                <p className="text-sm text-background/70">Factories & Boilers</p>
              </div>
            </div>
            <p className="text-background/80 text-sm leading-relaxed">
              Digital platform for factory and boiler licensing, safety inspections, 
              and compliance management in Rajasthan.
            </p>
            <div className="flex space-x-3">
              <Button size="sm" variant="ghost" className="text-background hover:bg-background/10">
                <Facebook className="h-4 w-4" />
              </Button>
              <Button size="sm" variant="ghost" className="text-background hover:bg-background/10">
                <Twitter className="h-4 w-4" />
              </Button>
              <Button size="sm" variant="ghost" className="text-background hover:bg-background/10">
                <Youtube className="h-4 w-4" />
              </Button>
              <Button size="sm" variant="ghost" className="text-background hover:bg-background/10">
                <Globe className="h-4 w-4" />
              </Button>
            </div>
          </div>

          {/* Quick Links */}
          <div>
            <h4 className="font-semibold mb-4 text-background">Quick Links</h4>
            <ul className="space-y-2">
              {quickLinks.map((link, index) => (
                <li key={index}>
                  <a href={link.href} className="text-background/70 hover:text-accent text-sm transition-colors">
                    {link.name}
                  </a>
                </li>
              ))}
            </ul>
          </div>

          {/* Support */}
          <div>
            <h4 className="font-semibold mb-4 text-background">Support</h4>
            <ul className="space-y-2">
              {supportLinks.map((link, index) => (
                <li key={index}>
                  <a href={link.href} className="text-background/70 hover:text-accent text-sm transition-colors">
                    {link.name}
                  </a>
                </li>
              ))}
            </ul>
          </div>

          {/* Contact Info */}
          <div>
            <h4 className="font-semibold mb-4 text-background">Contact Information</h4>
            <div className="space-y-3">
              <div className="flex items-start space-x-3">
                <MapPin className="h-4 w-4 text-accent mt-0.5" />
                <div className="text-sm text-background/70">
                  <p>Department of Factories & Boilers</p>
                  <p>Government of Rajasthan</p>
                  <p>Jaipur, Rajasthan</p>
                </div>
              </div>
              <div className="flex items-center space-x-3">
                <Phone className="h-4 w-4 text-accent" />
                <span className="text-sm text-background/70">+91-141-XXXXXXX</span>
              </div>
              <div className="flex items-center space-x-3">
                <Mail className="h-4 w-4 text-accent" />
                <span className="text-sm text-background/70">support@rajfab.gov.in</span>
              </div>
            </div>
          </div>
        </div>

        <Separator className="bg-background/20 mb-8" />

        {/* Legal Links */}
        <div className="flex flex-wrap gap-6 mb-6">
          {legalLinks.map((link, index) => (
            <a key={index} href={link.href} className="text-background/70 hover:text-accent text-sm transition-colors">
              {link.name}
            </a>
          ))}
        </div>

        <Separator className="bg-background/20 mb-6" />

        {/* Bottom Section */}
        <div className="flex flex-col md:flex-row justify-between items-center space-y-4 md:space-y-0">
          <div className="text-sm text-background/70">
            © 2024 Government of Rajasthan. All rights reserved.
          </div>
          <div className="flex items-center space-x-4 text-sm text-background/70">
            <span>Last updated: January 2024</span>
            <span>•</span>
            <span>Version 1.0</span>
            <Button variant="ghost" size="sm" className="text-background/70 hover:text-accent">
              <ExternalLink className="h-3 w-3 mr-1" />
              Accessibility
            </Button>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;