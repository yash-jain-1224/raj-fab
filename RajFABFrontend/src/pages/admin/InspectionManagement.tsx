import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Calendar } from "@/components/ui/calendar";
import { CalendarIcon, Search, Plus, Eye, MapPin, Clock } from "lucide-react";
import { useToast } from "@/hooks/use-toast";
import { format } from "date-fns";

// Dummy inspection data
const scheduledInspections = [
  {
    id: "INS001",
    factoryName: "Kumar Textiles Pvt Ltd",
    inspectorName: "Dr. Rajesh Sharma",
    date: "2024-02-15",
    time: "10:00 AM",
    type: "Routine Inspection",
    status: "Scheduled",
    address: "Industrial Area, Jaipur"
  },
  {
    id: "INS002", 
    factoryName: "Sharma Industries",
    inspectorName: "Eng. Priya Gupta",
    date: "2024-02-16",
    time: "2:00 PM",
    type: "Safety Audit",
    status: "In Progress",
    address: "Bhiwadi Industrial Area"
  },
  {
    id: "INS003",
    factoryName: "Agarwal Manufacturing Co.",
    inspectorName: "Dr. Anil Kumar",
    date: "2024-02-12",
    time: "11:00 AM", 
    type: "Compliance Check",
    status: "Completed",
    address: "Kota Industrial Area"
  }
];

const availableInspectors = [
  { id: "1", name: "Dr. Rajesh Sharma", designation: "Senior Inspector", specialization: "Safety & Environment" },
  { id: "2", name: "Eng. Priya Gupta", designation: "Technical Inspector", specialization: "Electrical & Machinery" },
  { id: "3", name: "Dr. Anil Kumar", designation: "Lead Inspector", specialization: "Chemical & Process Safety" },
  { id: "4", name: "Mrs. Sunita Singh", designation: "Inspector", specialization: "Fire Safety & Emergency" }
];

export default function InspectionManagement() {
  const { toast } = useToast();
  const [selectedInspection, setSelectedInspection] = useState(scheduledInspections[0]);
  const [newInspection, setNewInspection] = useState({
    factoryName: "",
    inspectorId: "",
    date: undefined as Date | undefined,
    time: "",
    type: "",
    priority: "",
    notes: ""
  });

  const handleScheduleInspection = () => {
    toast({
      title: "Inspection Scheduled",
      description: "New inspection has been scheduled successfully!",
    });
  };

  const getStatusBadge = (status: string) => {
    const variants = {
      "Scheduled": "default",
      "In Progress": "secondary",
      "Completed": "outline",
      "Cancelled": "destructive"
    } as const;
    
    return <Badge variant={variants[status as keyof typeof variants] || "secondary"}>{status}</Badge>;
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Inspection Management</h1>
          <p className="text-muted-foreground">Schedule and manage factory inspections</p>
        </div>
        <Button>
          <Plus className="h-4 w-4 mr-2" />
          Schedule New Inspection
        </Button>
      </div>

      <Tabs defaultValue="scheduled">
        <TabsList>
          <TabsTrigger value="scheduled">Scheduled Inspections</TabsTrigger>
          <TabsTrigger value="new">Schedule New</TabsTrigger>
          <TabsTrigger value="reports">Inspection Reports</TabsTrigger>
        </TabsList>

        <TabsContent value="scheduled" className="space-y-4">
          <div className="flex gap-4">
            <div className="flex-1">
              <Input placeholder="Search inspections..." className="max-w-sm" />
            </div>
            <Select>
              <SelectTrigger className="w-48">
                <SelectValue placeholder="Filter by status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="scheduled">Scheduled</SelectItem>
                <SelectItem value="in-progress">In Progress</SelectItem>
                <SelectItem value="completed">Completed</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-4">
            {scheduledInspections.map((inspection) => (
              <Card key={inspection.id} className="cursor-pointer hover:shadow-md transition-shadow">
                <CardHeader className="pb-3">
                  <div className="flex justify-between items-start">
                    <div>
                      <CardTitle className="text-lg">{inspection.factoryName}</CardTitle>
                      <p className="text-sm text-muted-foreground">{inspection.id}</p>
                    </div>
                    {getStatusBadge(inspection.status)}
                  </div>
                </CardHeader>
                <CardContent className="space-y-3">
                  <div className="flex items-center gap-2 text-sm">
                    <Clock className="h-4 w-4" />
                    <span>{inspection.date} at {inspection.time}</span>
                  </div>
                  <div className="flex items-center gap-2 text-sm">
                    <MapPin className="h-4 w-4" />
                    <span>{inspection.address}</span>
                  </div>
                  <div>
                    <p className="text-sm"><strong>Inspector:</strong> {inspection.inspectorName}</p>
                    <p className="text-sm"><strong>Type:</strong> {inspection.type}</p>
                  </div>
                  <div className="flex gap-2 pt-2">
                    <Button size="sm" variant="outline" className="flex-1">
                      <Eye className="h-4 w-4 mr-2" />
                      View Details
                    </Button>
                    {inspection.status === "Scheduled" && (
                      <Button size="sm" variant="secondary">Reschedule</Button>
                    )}
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="new" className="space-y-6">
          <Card>
            <CardHeader>
              <CardTitle>Schedule New Inspection</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-4">
                  <div>
                    <Label htmlFor="factoryName">Factory/Company Name</Label>
                    <Input
                      id="factoryName"
                      value={newInspection.factoryName}
                      onChange={(e) => setNewInspection({...newInspection, factoryName: e.target.value})}
                      placeholder="Enter factory name"
                    />
                  </div>

                  <div>
                    <Label htmlFor="inspectorId">Assign Inspector</Label>
                    <Select value={newInspection.inspectorId} onValueChange={(value) => setNewInspection({...newInspection, inspectorId: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select inspector" />
                      </SelectTrigger>
                      <SelectContent>
                        {availableInspectors.map(inspector => (
                          <SelectItem key={inspector.id} value={inspector.id}>
                            <div>
                              <div className="font-medium">{inspector.name}</div>
                              <div className="text-xs text-muted-foreground">{inspector.designation} - {inspector.specialization}</div>
                            </div>
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                  </div>

                  <div>
                    <Label htmlFor="type">Inspection Type</Label>
                    <Select value={newInspection.type} onValueChange={(value) => setNewInspection({...newInspection, type: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select inspection type" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="routine">Routine Inspection</SelectItem>
                        <SelectItem value="safety">Safety Audit</SelectItem>
                        <SelectItem value="compliance">Compliance Check</SelectItem>
                        <SelectItem value="environmental">Environmental Assessment</SelectItem>
                        <SelectItem value="fire-safety">Fire Safety Inspection</SelectItem>
                        <SelectItem value="follow-up">Follow-up Inspection</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div>
                    <Label htmlFor="priority">Priority Level</Label>
                    <Select value={newInspection.priority} onValueChange={(value) => setNewInspection({...newInspection, priority: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select priority" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="low">Low</SelectItem>
                        <SelectItem value="medium">Medium</SelectItem>
                        <SelectItem value="high">High</SelectItem>
                        <SelectItem value="urgent">Urgent</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>

                  <div>
                    <Label htmlFor="time">Preferred Time</Label>
                    <Select value={newInspection.time} onValueChange={(value) => setNewInspection({...newInspection, time: value})}>
                      <SelectTrigger>
                        <SelectValue placeholder="Select time slot" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="09:00">09:00 AM</SelectItem>
                        <SelectItem value="10:00">10:00 AM</SelectItem>
                        <SelectItem value="11:00">11:00 AM</SelectItem>
                        <SelectItem value="14:00">02:00 PM</SelectItem>
                        <SelectItem value="15:00">03:00 PM</SelectItem>
                        <SelectItem value="16:00">04:00 PM</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                <div className="space-y-4">
                  <div>
                    <Label>Select Inspection Date</Label>
                    <Calendar
                      mode="single"
                      selected={newInspection.date}
                      onSelect={(date) => setNewInspection({...newInspection, date})}
                      className="rounded-md border"
                      disabled={(date) => date < new Date()}
                    />
                  </div>
                </div>
              </div>

              <div className="mt-6">
                <Label htmlFor="notes">Special Notes/Requirements</Label>
                <Textarea
                  id="notes"
                  value={newInspection.notes}
                  onChange={(e) => setNewInspection({...newInspection, notes: e.target.value})}
                  placeholder="Enter any special requirements or notes for this inspection"
                  rows={4}
                />
              </div>

              <div className="flex justify-end gap-3 mt-6">
                <Button variant="outline">Save as Draft</Button>
                <Button onClick={handleScheduleInspection}>Schedule Inspection</Button>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="reports" className="space-y-4">
          <Card>
            <CardHeader>
              <CardTitle>Inspection Reports</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {scheduledInspections
                  .filter(inspection => inspection.status === "Completed")
                  .map(inspection => (
                  <div key={inspection.id} className="p-4 border rounded-lg">
                    <div className="flex justify-between items-start">
                      <div>
                        <h4 className="font-semibold">{inspection.factoryName}</h4>
                        <p className="text-sm text-muted-foreground">
                          Inspected by {inspection.inspectorName} on {inspection.date}
                        </p>
                        <p className="text-sm">Type: {inspection.type}</p>
                      </div>
                      <div className="flex gap-2">
                        <Button size="sm" variant="outline">
                          View Report
                        </Button>
                        <Button size="sm" variant="outline">
                          Download PDF
                        </Button>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </div>
  );
}