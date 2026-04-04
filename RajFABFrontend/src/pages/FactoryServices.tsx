import { useState } from "react";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Input } from "@/components/ui/input";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { BoilerServiceCard } from "@/components/boiler/BoilerServiceCard";
import { PageHeader } from "@/components/layout/PageHeader";
import {
  Factory,
  Search,
  Filter,
  FileText,
  Clock,
  Users,
  TrendingUp,
  Info,
} from "lucide-react";

// Mock data - replace with actual API call
const factoryServices = [
  {
    id: "factory-registration",
    name: "New Establishment",
    description:
      "Application for Registration for existing establishment or factory /New Establishment or factory / Amendment to certificate of Registration.",
    category: "factory-services",
    subcategory: "registration",
    estimatedTime: "15-20 days",
    requiredDocuments: 6,
    isPopular: true,
    url: "new-establishment",
  },
  {
    id: "licence-renewal",
    name: "Licence Renewal",
    description:
      "Application for licence / Renewal of licence / Amendment to licence / Transfer of licence of Factory.",
    category: "factory-services",
    subcategory: "renewal",
    estimatedTime: "7-10 days",
    requiredDocuments: 4,
    isPopular: true,
    url: "factory-registration",
  },
  {
    id: "map-approval",
    name: "Map Approval",
    description:
      "Application for permission for the site on which the factory is to be situated and for the construction or extension thereof.",
    category: "factory-services",
    subcategory: "registration",
    estimatedTime: "20-30 days",
    requiredDocuments: 4,
    url: "map-approval",
  },
  {
    id: "manager-change",
    name: "Manager Change",
    description: "Notice of Change of Manager.",
    category: "factory-services",
    subcategory: "modification",
    estimatedTime: "10-15 days",
    requiredDocuments: 4,
    url: "manager-change",
  },
  {
    id: "commencement-cessation",
    name: "Commencement/Cessation",
    description: "Notice of Commencement/Cessation of Establishment.",
    category: "factory-services",
    subcategory: "modification",
    estimatedTime: "10-15 days",
    requiredDocuments: 4,
    url: "Factory-CommenceAndCessation",
  },
  {
    id: "non-hazardous",
    name: "Factories Non-Hazardous",
    description:
      "Application for factroies involving non-hazardous process and employing upto 50 Workers.",
    category: "factory-services",
    subcategory: "registration",
    estimatedTime: "10-15 days",
    requiredDocuments: 4,
    url: "Factory-Form7",
  },
  {
    id: "annual-return",
    name: "Annunal Return",
    description:
      "Application for factroies involving non-hazardous process and employing upto 50 Workers.",
    category: "factory-services",
    subcategory: "other",
    estimatedTime: "10-15 days",
    requiredDocuments: 4,
    url: "AnnualReturn-Form25",
  },
  {
    id: "appeal",
    name: "Appeal",
    description:
      "Application for appeal against any order or decision of the Factory and Boiler Department.",
    category: "factory-services",
    subcategory: "other",
    estimatedTime: "10-15 days",
    requiredDocuments: 4,
    url: "Appeal-Form38",
  },
];

const serviceStats = [
  {
    title: "Total Applications",
    value: "1,247",
    change: "+12%",
    icon: FileText,
  },
  {
    title: "Avg. Processing Time",
    value: "14 days",
    change: "-2 days",
    icon: Clock,
  },
  {
    title: "Active Boilers",
    value: "3,456",
    change: "+8%",
    icon: Factory,
  },
  {
    title: "Certified Operators",
    value: "892",
    change: "+15%",
    icon: Users,
  },
];

export default function FactoryServices() {
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedCategory, setSelectedCategory] = useState("all");

  const filteredServices = factoryServices.filter((service) => {
    const matchesSearch =
      service.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
      service.description.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesCategory =
      selectedCategory === "all" || service.subcategory === selectedCategory;
    return matchesSearch && matchesCategory;
  });

  return (
    <div className="space-y-6">
      <PageHeader
        title="Factory Services"
        description="Occupational Safety, Health and Working Conditions (Central) Rules, 2020"
        icon={<Factory className="h-8 w-8" />}
      />

      {/* Service Statistics */}
      {/* <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
        {serviceStats.map((stat, index) => {
          const Icon = stat.icon;
          return (
            <Card key={index} className="border-l-4 border-l-primary">
              <CardContent className="p-4">
                <div className="flex items-center justify-between">
                  <div>
                    <p className="text-sm font-medium text-muted-foreground">{stat.title}</p>
                    <p className="text-2xl font-bold">{stat.value}</p>
                    <p className="text-xs text-green-600 flex items-center gap-1">
                      <TrendingUp className="h-3 w-3" />
                      {stat.change} from last month
                    </p>
                  </div>
                  <Icon className="h-8 w-8 text-muted-foreground" />
                </div>
              </CardContent>
            </Card>
          );
        })}
      </div> */}

      {/* Important Notice */}
      <Card className="border-blue-200 bg-blue-50/50">
        <CardContent className="p-4">
          <div className="flex items-start gap-3">
            <Info className="h-5 w-5 text-blue-600 mt-0.5 flex-shrink-0" />
            <div className="space-y-1">
              <h3 className="font-medium text-blue-900">
                Important Information
              </h3>
              <p className="text-sm text-blue-800">
                All Factory services are governed by the Occupational Safety, Health and Working Conditions (Central)
                Rules, 2020 (OSH) and Rajasthan Occupational Safety, Health and Working Conditions Rules, 2023 . Ensure all required documents are ready
                before starting your application. For technical queries, contact
                the Factory and Boiler Department at{" "}
                <strong>0141-2225624</strong>.
              </p>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Search and Filter */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Factory className="h-5 w-5" />
            Available Services
          </CardTitle>
          <CardDescription>
            Select the Factory service you need to apply for
          </CardDescription>
        </CardHeader>
        <CardContent className="space-y-4">
          <div className="flex flex-col sm:flex-row gap-4">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-3 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search services..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-9"
              />
            </div>
            <Button variant="outline" className="flex items-center gap-2">
              <Filter className="h-4 w-4" />
              Filter
            </Button>
          </div>

          <Tabs value={selectedCategory} onValueChange={setSelectedCategory}>
            <TabsList className="grid w-full grid-cols-5">
              <TabsTrigger value="all">All Services</TabsTrigger>
              <TabsTrigger value="registration">Registration</TabsTrigger>
              <TabsTrigger value="renewal">Renewal</TabsTrigger>
              <TabsTrigger value="modification">Modification</TabsTrigger>
              <TabsTrigger value="other">Other</TabsTrigger>
            </TabsList>

            <TabsContent value={selectedCategory} className="mt-6">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {filteredServices.map((service) => (
                  <BoilerServiceCard
                    key={service.id}
                    service={service}
                    IsBoilerService={false}
                    Act="OSH 2020"
                  />
                ))}
              </div>

              {filteredServices.length === 0 && (
                <div className="text-center py-12">
                  <Factory className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                  <h3 className="text-lg font-semibold mb-2">
                    No services found
                  </h3>
                  <p className="text-muted-foreground">
                    Try adjusting your search or filter criteria
                  </p>
                </div>
              )}
            </TabsContent>
          </Tabs>
        </CardContent>
      </Card>

      {/* Additional Resources */}
      {/* <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Regulatory Guidelines</CardTitle>
            <CardDescription>Important documents and regulations</CardDescription>
          </CardHeader>
          <CardContent className="space-y-3">
            <div className="flex items-center justify-between py-2 border-b">
              <span className="text-sm">Indian Boilers Act 1923</span>
              <Button variant="outline" size="sm">Download</Button>
            </div>
            <div className="flex items-center justify-between py-2 border-b">
              <span className="text-sm">Indian Boiler Regulations 1950</span>
              <Button variant="outline" size="sm">Download</Button>
            </div>
            <div className="flex items-center justify-between py-2 border-b">
              <span className="text-sm">IBR Forms Collection</span>
              <Button variant="outline" size="sm">Download</Button>
            </div>
            <div className="flex items-center justify-between py-2">
              <span className="text-sm">Fee Structure 2024</span>
              <Button variant="outline" size="sm">Download</Button>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Contact Information</CardTitle>
            <CardDescription>Get help with your boiler services</CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="space-y-2">
              <h4 className="font-medium">Factory and Boiler Department</h4>
              <p className="text-sm text-muted-foreground">
                Udyog Bhawan, Tilak Marg<br />
                Jaipur, Rajasthan - 302005
              </p>
            </div>
            <div className="space-y-1">
              <p className="text-sm"><strong>Phone:</strong> 0141-2225624</p>
              <p className="text-sm"><strong>Email:</strong> factory.raj@nic.in</p>
              <p className="text-sm"><strong>Website:</strong> rajfab.rajasthan.gov.in</p>
            </div>
            <div className="space-y-1">
              <p className="text-sm font-medium">Office Hours:</p>
              <p className="text-sm text-muted-foreground">
                Monday to Friday: 10:00 AM - 5:00 PM<br />
                Saturday: 10:00 AM - 2:00 PM
              </p>
            </div>
          </CardContent>
        </Card>
      </div> */}
    </div>
  );
}
