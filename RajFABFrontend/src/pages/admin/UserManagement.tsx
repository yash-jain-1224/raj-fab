import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "@/components/ui/dialog";
import { User, Plus, Edit, Trash2, Eye, Shield, UserCheck } from "lucide-react";
import { useToast } from "@/hooks/use-toast";

// Dummy user data
const users = [
  {
    id: "U001",
    name: "Rajesh Kumar",
    email: "rajesh.kumar@email.com",
    phone: "+91 98765 43210",
    role: "Citizen",
    status: "Active",
    registrationDate: "2024-01-15",
    lastLogin: "2024-01-28",
    factoryCount: 2,
    applications: 5
  },
  {
    id: "U002",
    name: "Dr. Priya Sharma",
    email: "priya.sharma@rajfab.gov.in",
    phone: "+91 98765 43211",
    role: "Inspector",
    status: "Active",
    registrationDate: "2023-08-20",
    lastLogin: "2024-01-28",
    factoryCount: 0,
    applications: 0
  },
  {
    id: "U003",
    name: "Sunita Agarwal",
    email: "sunita.agarwal@email.com",
    phone: "+91 98765 43212",
    role: "Citizen",
    status: "Inactive",
    registrationDate: "2024-01-10",
    lastLogin: "2024-01-20",
    factoryCount: 1,
    applications: 3
  },
  {
    id: "U004",
    name: "Admin User",
    email: "admin@rajfab.gov.in",
    phone: "+91 98765 43213",
    role: "Administrator",
    status: "Active",
    registrationDate: "2023-01-01",
    lastLogin: "2024-01-28",
    factoryCount: 0,
    applications: 0
  }
];

const roles = [
  { value: "citizen", label: "Citizen" },
  { value: "inspector", label: "Inspector" },
  { value: "administrator", label: "Administrator" },
  { value: "approver", label: "Approver" }
];

export default function UserManagement() {
  const { toast } = useToast();

  // NEW USER FORM STATE
  const [newUser, setNewUser] = useState({
    name: "",
    email: "",
    phone: "+91 ",
    role: "",
    department: "",
  });

  const [filterRole, setFilterRole] = useState("all");
  const [filterStatus, setFilterStatus] = useState("all");
  const [searchTerm, setSearchTerm] = useState("");

  // ❗ VALIDATION FUNCTIONS
  const lettersOnly = (v: string) => /^[A-Za-z\s]*$/.test(v);
  const emailValid = (v: string) =>
    /^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$/.test(v);
  const numbersOnly = (v: string) => /^[0-9]*$/.test(v);

  const filteredUsers = users.filter(user => {
    const matchesSearch =
      user.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      user.email.toLowerCase().includes(searchTerm.toLowerCase());

    const matchesRole = filterRole === "all" || user.role.toLowerCase() === filterRole;
    const matchesStatus = filterStatus === "all" || user.status.toLowerCase() === filterStatus;

    return matchesSearch && matchesRole && matchesStatus;
  });

  const handleCreateUser = () => {
    toast({
      title: "User Created",
      description: `New user ${newUser.name} has been created successfully!`,
    });
  };

  const handleDeleteUser = (id: string) => {
    toast({
      title: "User Deleted",
      description: `User has been deleted successfully!`,
      variant: "destructive"
    });
  };

  const getStatusBadge = (status: string) => {
    const variants = {
      "Active": "default",
      "Inactive": "secondary",
      "Suspended": "destructive"
    } as const;

    return <Badge variant={variants[status as keyof typeof variants] || "secondary"}>{status}</Badge>;
  };

  const getRoleBadge = (role: string) => {
    const colors = {
      "Citizen": "bg-blue-100 text-blue-800",
      "Inspector": "bg-green-100 text-green-800",
      "Administrator": "bg-purple-100 text-purple-800",
      "Approver": "bg-orange-100 text-orange-800"
    };

    return (
      <Badge className={colors[role as keyof typeof colors] || "bg-gray-100 text-gray-800"}>
        {role}
      </Badge>
    );
  };

  return (
    <div className="space-y-6">
      {/* HEADER + CREATE USER BUTTON */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">User Management</h1>
          <p className="text-muted-foreground">Manage system users and permissions</p>
        </div>

        {/* CREATE USER DIALOG */}
        <Dialog>
          <DialogTrigger asChild>
            <Button>
              <Plus className="h-4 w-4 mr-2" />
              Add New User
            </Button>
          </DialogTrigger>

          <DialogContent className="max-w-2xl">
            <DialogHeader>
              <DialogTitle>Create New User</DialogTitle>
            </DialogHeader>

            <div className="grid grid-cols-2 gap-4">

              {/* FULL NAME */}
              <div>
                <Label>Full Name</Label>
                <Input
                  value={newUser.name}
                  onChange={(e) => {
                    const v = e.target.value;
                    if (lettersOnly(v)) {
                      setNewUser({ ...newUser, name: v });
                    }
                  }}
                  placeholder="Enter full name"
                />
              </div>

              {/* EMAIL */}
              <div>
                <Label>Email Address</Label>
                <Input
                  type="text"
                  value={newUser.email}
                  onChange={(e) => {
                    const v = e.target.value;
                    setNewUser({ ...newUser, email: v });
                  }}
                  className={
                    newUser.email.length === 0
                      ? ""
                      : emailValid(newUser.email)
                      ? "border-green-500"
                      : "border-red-500"
                  }
                  placeholder="Enter email address"
                />
                {newUser.email && !emailValid(newUser.email) && (
                  <p className="text-xs text-red-500 mt-1">Invalid email format</p>
                )}
              </div>

              {/* PHONE NUMBER */}
              <div>
                <Label>Phone Number</Label>
                <Input
                  value={newUser.phone}
                  onChange={(e) => {
                    let v = e.target.value.replace(/\D/g, ""); // Only digits

                    if (v.startsWith("91")) v = v.substring(2);
                    if (v.length > 10) v = v.substring(0, 10);

                    const formatted =
                      v.length > 5
                        ? `+91 ${v.substring(0, 5)} ${v.substring(5)}`
                        : v.length > 0
                        ? `+91 ${v}`
                        : "+91 ";

                    setNewUser({ ...newUser, phone: formatted });
                  }}
                  placeholder="+91 XXXXX XXXXX"
                />
              </div>

              {/* ROLE */}
              <div>
                <Label>Role</Label>
                <Select
                  value={newUser.role}
                  onValueChange={(value) => setNewUser({ ...newUser, role: value })}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select role" />
                  </SelectTrigger>
                  <SelectContent>
                    {roles.map((role) => (
                      <SelectItem key={role.value} value={role.value}>
                        {role.label}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              {/* DEPARTMENT */}
              <div className="col-span-2">
                <Label>Department (for staff)</Label>
                <Input
                  value={newUser.department}
                  onChange={(e) => {
                    const v = e.target.value;
                    if (lettersOnly(v)) {
                      setNewUser({ ...newUser, department: v });
                    }
                  }}
                  placeholder="Enter department name"
                />
              </div>
            </div>

            <div className="flex justify-end gap-3 mt-6">
              <Button variant="outline">Cancel</Button>
              <Button
                disabled={
                  !newUser.name ||
                  !emailValid(newUser.email) ||
                  newUser.phone.length < 5
                }
                onClick={handleCreateUser}
              >
                Create User
              </Button>
            </div>
          </DialogContent>
        </Dialog>
      </div>

      {/* SEARCH + FILTERS */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex gap-4">
            <Input
              placeholder="Search users..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="max-w-sm"
            />
            <Select value={filterRole} onValueChange={setFilterRole}>
              <SelectTrigger className="w-48">
                <SelectValue placeholder="Filter by role" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Roles</SelectItem>
                <SelectItem value="citizen">Citizens</SelectItem>
                <SelectItem value="inspector">Inspectors</SelectItem>
                <SelectItem value="administrator">Administrators</SelectItem>
                <SelectItem value="approver">Approvers</SelectItem>
              </SelectContent>
            </Select>

            <Select value={filterStatus} onValueChange={setFilterStatus}>
              <SelectTrigger className="w-48">
                <SelectValue placeholder="Filter by status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="active">Active</SelectItem>
                <SelectItem value="inactive">Inactive</SelectItem>
                <SelectItem value="suspended">Suspended</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* USERS LIST */}
      <Card>
        <CardHeader>
          <CardTitle>Users ({filteredUsers.length})</CardTitle>
        </CardHeader>

        <CardContent>
          <div className="space-y-4">
            {filteredUsers.map((user) => (
              <div key={user.id} className="p-4 border rounded-lg hover:bg-muted/30 transition-colors">
                <div className="flex items-center justify-between">

                  {/* USER INFO */}
                  <div className="flex items-center gap-4">
                    <div className="w-10 h-10 bg-primary/10 rounded-full flex items-center justify-center">
                      <User className="h-5 w-5 text-primary" />
                    </div>

                    <div>
                      <h4 className="font-semibold">{user.name}</h4>
                      <p className="text-sm text-muted-foreground">{user.email}</p>
                      <p className="text-sm text-muted-foreground">{user.phone}</p>
                    </div>
                  </div>

                  {/* BADGES + ACTIONS */}
                  <div className="flex items-center gap-4">
                    <div className="text-right">
                      <div className="flex gap-2 mb-1">
                        {getRoleBadge(user.role)}
                        {getStatusBadge(user.status)}
                      </div>
                      <p className="text-sm text-muted-foreground">
                        Last login: {user.lastLogin}
                      </p>

                      {user.role === "Citizen" && (
                        <p className="text-sm text-muted-foreground">
                          {user.factoryCount} factories, {user.applications} applications
                        </p>
                      )}
                    </div>

                    <div className="flex gap-2">
                      <Button size="sm" variant="outline"><Eye className="h-4 w-4" /></Button>
                      <Button size="sm" variant="outline"><Edit className="h-4 w-4" /></Button>
                      <Button size="sm" variant="outline">
                        {user.status === "Active" ? <UserCheck className="h-4 w-4" /> : <Shield className="h-4 w-4" />}
                      </Button>

                      <Button
                        size="sm"
                        variant="outline"
                        onClick={() => handleDeleteUser(user.id)}
                        className="text-destructive hover:text-destructive"
                      >
                        <Trash2 className="h-4 w-4" />
                      </Button>
                    </div>

                  </div>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
