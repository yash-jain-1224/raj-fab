import { useEffect, useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Edit, Trash2, User as UserIcon, Mail, Phone } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import { Badge } from "@/components/ui/badge";
import { Switch } from "@/components/ui/switch";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useUsers } from "@/hooks/api";
import type { CreateUserRequest, User as UserDto } from "@/services/api/users";
import useOfficeMasters from "@/hooks/useOfficeMasters";
import { useToast } from "@/hooks/use-toast";
import { useSsoDetails } from "@/hooks/api/useSsoDetails";

export default function UserManagementPage() {
  const { toast } = useToast();
  const { roles, offices, fetchOffices, fetchRoles } = useOfficeMasters();
  const {
    users,
    isLoading: loading,
    createUser,
    updateUser,
    deleteUser,
  } = useUsers();
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<any>(null);
  const [form, setForm] = useState<CreateUserRequest>({
    username: "",
    fullName: "",
    email: "",
    mobile: "",
    gender: "",
    userType: "department",
    isActive: true,
  });
  const {
    data: ssoDetails,
    isLoading: ssoDataLoading,
    error,
    refetch,
  } = useSsoDetails(form.username);

  const [loadingRoles, setLoadingRoles] = useState({
    offices: false,
    roles: false,
  });

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const handleSave = async () => {
    if (!form.fullName?.trim()) {
      return;
    }

    const data = { ...form, userType: form.userType || "department" };
    if (editing) {
      updateUser({ id: editing.id, data });
    } else {
      createUser(data);
    }
    setDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this user?")) {
      deleteUser(id);
    }
  };

  const handleToggleStatus = (user: any, newStatus: boolean) => {
    const updated = { ...user, isActive: newStatus };
    updateUser({ id: user.id, data: updated });
  };

  const handleSSOLogin = async () => {
    try {
      if (!form.username?.trim()) {
        toast({
          title: "Validation Error",
          description: "Please enter SSO ID",
          variant: "destructive",
        });
        return;
      }
      setForm((prev) => ({
        ...prev,
        fullName: "",
        email: "",
        mobile: "",
        gender: "",
        userType: "department",
        isActive: true,
      }));

      const result = await refetch();
      const { data: user }: any = result?.data;

      if (!user) {
        toast({
          title: "Not Found",
          description: "User data not found.",
          variant: "destructive",
        });
        return;
      }

      setForm((prev) => ({
        ...prev,
        fullName: user.displayName || "",
        email: user.mailPersonal?.toLowerCase() || "",
        mobile: user.mobile || "",
        gender: user.gender?.toLowerCase() || "",
      }));

      console.log("SSO Details:", user);
    } catch (err) {
      const message =
        err instanceof Error ? err.message : "Something went wrong";

      toast({
        title: "Error",
        description: message,
        variant: "destructive",
      });
    }
  };


  // --- Filtering + Pagination ---
  const filteredUsers = useMemo(() => {
    return users.filter((u) =>
      `${u.username} ${u.fullName} ${u.email} ${u.mobile} ${
        u.officeName
      } ${u.roles?.map((r) => r.name).join(" ")}`
        .toLowerCase()
        .includes(search.toLowerCase())
    );
  }, [users, search]);

  const totalPages = Math.ceil(filteredUsers.length / pageSize);

  const paginatedUsers = useMemo(() => {
    const start = (page - 1) * pageSize;
    return filteredUsers.slice(start, start + pageSize);
  }, [filteredUsers, page]);

  useEffect(() => {
    // fetch offices on mount
    const init = async () => {
      try {
        setLoadingRoles((s) => ({ ...s, offices: true }));
        await fetchOffices();
        setLoadingRoles((s) => ({ ...s, offices: false }));
      } catch (err) {
        console.error("Initial load failed:", err);
        setLoadingRoles((s) => ({ ...s, offices: false }));
      }
    };
    init();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: UserDto, index: number) => (
        <span className="font-medium text-muted-foreground">{index}</span>
      ),
    },
    {
      key: "user",
      header: "User",
      render: (user: UserDto) => (
        <div className="flex items-center gap-3">
          <div className="h-10 w-10 rounded-full bg-primary/10 flex items-center justify-center">
            <UserIcon className="h-5 w-5 text-primary" />
          </div>
          <div>
            <div className="font-medium">{user.fullName}</div>
            <div className="text-sm text-muted-foreground">
              @{user.username}
            </div>
          </div>
        </div>
      ),
    },
    {
      key: "contact",
      header: "Contact",
      render: (user: UserDto) => (
        <div className="space-y-1">
          <div className="flex items-center gap-2 text-sm">
            <Mail className="h-3 w-3 text-muted-foreground" />
            <span>{user.email}</span>
          </div>
          <div className="flex items-center gap-2 text-sm">
            <Phone className="h-3 w-3 text-muted-foreground" />
            <span>{user.mobile}</span>
          </div>
        </div>
      ),
    },
    {
      key: "userType",
      header: "User Type",
      render: (user: UserDto) => (
        <Badge variant="secondary" className="font-medium">
          {user.userType.charAt(0).toUpperCase() + user.userType.slice(1)}
        </Badge>
      ),
    },
    {
      key: "status",
      header: "Status",
      render: (user: UserDto) => (
        <div className="flex items-center gap-3">
          <Badge variant={user.isActive ? "default" : "outline"}>
            {user.isActive ? "Active" : "Inactive"}
          </Badge>
          {user?.userType !== "superadmin" && (
            <Switch
              checked={user.isActive}
              onCheckedChange={(checked) => handleToggleStatus(user, checked)}
            />
          )}
        </div>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (user: UserDto) => {
        return (
          user?.userType !== "superadmin" && (
            <div className="flex items-center justify-end gap-2">
              <Button
                size="sm"
                variant="ghost"
                onClick={async () => {
                  setEditing(user);
                  setForm({
                    username: user.username,
                    fullName: user.fullName,
                    email: user.email,
                    mobile: user.mobile,
                    gender: user.gender,
                    userType: user.userType || "department",
                    isActive: user.isActive,
                  });
                  setDialogOpen(true);
                }}
                className="h-8 w-8 p-0"
              >
                <Edit className="h-4 w-4" />
              </Button>

              <Button
                size="sm"
                variant="ghost"
                onClick={() => handleDelete(user.id)}
                className="h-8 w-8 p-0 text-destructive"
              >
                <Trash2 className="h-4 w-4" />
              </Button>
            </div>
          )
        );
      },
    },
  ];

  return (
    <div className="space-y-6">
      <ModernDataTable
        title="User Management"
        description="Create and manage users with roles and privileges"
        data={paginatedUsers}
        columns={columns}
        loading={loading}
        search={search}
        onSearchChange={(value) => {
          setSearch(value);
          setPage(1);
        }}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
        onAdd={() => {
          setEditing(null);
          setForm({
            username: "",
            fullName: "",
            email: "",
            mobile: "",
            gender: "",
            userType: "department",
            isActive: true,
          });
          if (offices.length === 0) {
            setLoadingRoles((s) => ({ ...s, offices: true }));
            fetchOffices().finally(() => {
              setLoadingRoles((s) => ({ ...s, offices: false }));
            });
          }
          setDialogOpen(true);
        }}
        addLabel="Add User"
        emptyMessage="No users found"
        pageSize={pageSize}
      />

      {/* Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-lg max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>{editing ? "Edit User" : "Create User"}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <Label>Username</Label>
              <div className="flex gap-3">
                <Input
                  value={form.username}
                  onChange={(e) =>
                    setForm({ ...form, username: e.target.value })
                  }
                />
                <Button onClick={handleSSOLogin} disabled={ssoDataLoading}>
                  {ssoDataLoading ? "Fetching" : "Fetch from SSO"}
                </Button>
              </div>
            </div>
            <div>
              <Label>Full Name</Label>
              <Input
                value={form.fullName}
                onChange={(e) => setForm({ ...form, fullName: e.target.value })}
              />
            </div>
            <div>
              <Label>Email</Label>
              <Input
                type="email"
                value={form.email}
                onChange={(e) => setForm({ ...form, email: e.target.value })}
              />
            </div>
            <div>
              <Label>Mobile</Label>
              <Input
                value={form.mobile}
                onChange={(e) => setForm({ ...form, mobile: e.target.value })}
              />
            </div>
            <div>
              <Label htmlFor="gender">Gender</Label>
              <Select
                value={form.gender}
                onValueChange={(value) => setForm({ ...form, gender: value })}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select gender" />
                </SelectTrigger>

                <SelectContent>
                  <SelectItem value="male">Male</SelectItem>
                  <SelectItem value="female">Female</SelectItem>
                </SelectContent>
              </Select>
            </div>
            <div>
              <Label>User Type</Label>
              <Select
                value={form.userType}
                onValueChange={async (val) => {
                  setForm((f) => ({ ...f, userType: val }));
                }}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select User Type" />
                </SelectTrigger>
                <SelectContent>
                  {["department", "citizen"].map((r) => (
                    <SelectItem key={r} value={r}>
                      {r.charAt(0).toUpperCase() + r.slice(1)}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            {editing && (
              <div className="flex items-center gap-2">
                <Switch
                  checked={form.isActive}
                  onCheckedChange={(checked) =>
                    setForm({ ...form, isActive: checked })
                  }
                />
                <Label>{form.isActive ? "Active" : "Inactive"}</Label>
              </div>
            )}
            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleSave}>
                {editing ? "Update" : "Create"}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
