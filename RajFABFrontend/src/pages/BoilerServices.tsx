import { useState } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
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
  Info
} from "lucide-react";

// Mock data - replace with actual API call
const boilerServices = [
  {
    id: 'boiler-registration',
    name: 'Boiler Registration',
    description: 'Register new boilers under Indian Boilers Act 1923 with complete technical specifications and safety compliance.',
    category: 'boiler-services',
    subcategory: 'registration',
    estimatedTime: '15-20 days',
    requiredDocuments: 6,
    isPopular: true
  },
  {
    id: 'boiler-renewal',
    name: 'Certificate Renewal',
    description: 'Renew boiler inspection certificates for continued operation compliance under IBR regulations.',
    category: 'boiler-services',
    subcategory: 'renewal',
    estimatedTime: '7-10 days',
    requiredDocuments: 4,
    isPopular: true
  },
  {
    id: 'boiler-modification',
    name: 'Boiler Modification',
    description: 'Apply for boiler modifications including pressure upgrades, capacity changes, and safety enhancements.',
    category: 'boiler-services',
    subcategory: 'modification',
    estimatedTime: '20-30 days',
    requiredDocuments: 4
  },
  {
    id: 'boiler-transfer',
    name: 'Ownership Transfer',
    description: 'Transfer boiler ownership or change installation location with proper documentation and approvals.',
    category: 'boiler-services',
    subcategory: 'transfer',
    estimatedTime: '10-15 days',
    requiredDocuments: 4
  }
];

const serviceStats = [
  {
    title: "Total Applications",
    value: "1,247",
    change: "+12%",
    icon: FileText
  },
  {
    title: "Avg. Processing Time",
    value: "14 days",
    change: "-2 days",
    icon: Clock
  },
  {
    title: "Active Boilers",
    value: "3,456",
    change: "+8%",
    icon: Factory
  },
  {
    title: "Certified Operators",
    value: "892",
    change: "+15%",
    icon: Users
  }
];

export default function BoilerServices() {
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedCategory, setSelectedCategory] = useState("all");

  const filteredServices = boilerServices.filter(service => {
    const matchesSearch = service.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
                         service.description.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesCategory = selectedCategory === "all" || service.subcategory === selectedCategory;
    return matchesSearch && matchesCategory;
  });

  return (
    <div className="space-y-6">
      <PageHeader
        title="Boiler Services"
        description="Factory and Boiler Department services under Indian Boilers Act 1923"
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
              <h3 className="font-medium text-blue-900">Important Information</h3>
              <p className="text-sm text-blue-800">
                All boiler services are governed by the Indian Boilers Act 1923 and Indian Boiler Regulations (IBR) 1950. 
                Ensure all required documents are ready before starting your application. For technical queries, 
                contact the Factory and Boiler Department at <strong>0141-2225624</strong>.
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
            Select the boiler service you need to apply for
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
              <TabsTrigger value="transfer">Transfer</TabsTrigger>
            </TabsList>

            <TabsContent value={selectedCategory} className="mt-6">
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                {filteredServices.map((service) => (
                  <BoilerServiceCard
                    key={service.id}
                    service={service}
                  />
                ))}
              </div>

              {filteredServices.length === 0 && (
                <div className="text-center py-12">
                  <Factory className="h-12 w-12 text-muted-foreground mx-auto mb-4" />
                  <h3 className="text-lg font-semibold mb-2">No services found</h3>
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