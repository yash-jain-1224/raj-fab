import { useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Checkbox } from "@/components/ui/checkbox";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Edit, Trash2 } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

import { useUsers } from "@/hooks/api/useUsers";
import { useRoles, useRolesByOffice } from "@/hooks/api/useRoles";
import { useUserRoles } from "@/hooks/api/useUserRoles";
import type { AssignRole } from "@/services/api/assignRolesToUser";
import { useOffices } from "@/hooks/api";

export default function AssignRolesToUserManagementPage() {
  const [form, setForm] = useState({
    userId: "",
    officeId: "",
    roleId: "",
    joiningDate: "",
    joiningType: "Government",
    joiningDetail: "New Appointment",
    isInspector: false,
  });
  const { users } = useUsers();
  const { offices } = useOffices();
  const { data: roles = [] } = useRolesByOffice(form.officeId);

  const {
    userRolesList,
    assignRole,
    updateRole,
    removeRole,
    userRolesListLoading,
  } = useUserRoles();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<AssignRole | null>(null);


  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const filtered = userRolesList.filter(
    (r) =>
      r.username.toLowerCase().includes(search.toLowerCase()) ||
      r.roleName.toLowerCase().includes(search.toLowerCase())
  );

  const totalPages = Math.ceil(filtered.length / pageSize);
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const handleSave = () => {
    if (!form.userId || !form.roleId || !form.joiningDate) return;

    const payload = {
      userId: form.userId,
      roleId: form.roleId,
      joiningDate: form.joiningDate,
      joiningType: form.joiningType,
      joiningDetail: form.joiningDetail,
      isInspector: form.isInspector,
    };

    if (editing) {
      updateRole(editing.id, payload);
    } else {
      assignRole(payload);
    }

    setDialogOpen(false);
    setForm({ userId: "", roleId: "", officeId: "", joiningDate: "", joiningType: "Government", joiningDetail: "New Appointment", isInspector: false });
  };

  const handleDelete = (id: string) => {
    if (!confirm("Remove assigned Office Post?")) return;
    removeRole(id);
  };

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: any, index: number) => (
        <span className="font-medium text-muted-foreground">{index}</span>
      ),
    },
    {
      key: "username",
      header: "User Name",
      render: (row: AssignRole) => <span>{row.username}</span>,
    },
    {
      key: "roleName",
      header: "Assigned Office Post",
      render: (row: AssignRole) => (
        <span>
          {row.roleName}, {row.officeCityName}
        </span>
      ),
    },
    {
      key: "officeName",
      header: "Assigned Office",
      render: (row: AssignRole) => <span>{row.officeName}</span>,
    },
    {
      key: "joiningDate",
      header: "Joining Date",
      render: (row: AssignRole) => (
        <span>{new Date(row.joiningDate).toLocaleDateString()}</span>
      ),
    },
    {
      key: "joiningType",
      header: "Type",
      render: (row: AssignRole) => (
        <span className="px-2 py-1 text-xs rounded bg-blue-100 text-blue-700">
          {row.joiningType}
        </span>
      ),
    },
    {
      key: "joiningDetail",
      header: "Detail",
      render: (row: AssignRole) => (
        <span className="text-gray-700">{row.joiningDetail || "—"}</span>
      ),
    },
    {
      key: "isInspector",
      header: "Inspector",
      render: (row: AssignRole) => (
        <span className={`px-2 py-1 text-xs rounded ${row.isInspector ? "bg-orange-100 text-orange-700" : "bg-gray-100 text-gray-500"}`}>
          {row.isInspector ? "Yes" : "No"}
        </span>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (row: AssignRole) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            onClick={() => {
              setEditing(row);
              setForm({
                userId: row.userId,
                roleId: row.roleId,
                officeId: row.officeId,
                joiningDate: row.joiningDate.split("T")[0],
                joiningType: row.joiningType,
                joiningDetail: row.joiningDetail,
                isInspector: row.isInspector,
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
            onClick={() => handleDelete(row.id)}
            className="h-8 w-8 p-0 text-destructive"
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <ModernDataTable
        title="Assign Office Post to Users"
        description="Manage single Office Post assignment per user"
        data={paginated}
        columns={columns}
        loading={userRolesListLoading}
        search={search}
        onSearchChange={(v) => {
          setSearch(v);
          setPage(1);
        }}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
        onAdd={() => {
          setEditing(null);
          setForm({
            userId: "",
            roleId: "",
            officeId: "",
            joiningDate: "",
            joiningType: "Government",
            joiningDetail: "New Appointment",
            isInspector: false,
          });
          setDialogOpen(true);
        }}
        addLabel="Assign Office Post"
        pageSize={pageSize}
      />

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              {editing ? "Edit Assignment" : "Assign Office Post"}
            </DialogTitle>
          </DialogHeader>

          <div className="space-y-4">
            <div>
              <Label>User</Label>
              <Select
                value={form.userId}
                onValueChange={(val) => setForm({ ...form, userId: val })}
                disabled={!!editing}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select Department User" />
                </SelectTrigger>
                <SelectContent>
                  {users
                    .filter((i) => !i.userType.includes("admin"))
                    .map((u) => (
                      <SelectItem key={u.id} value={u.id}>
                        {u.fullName} ({u.email})
                      </SelectItem>
                    ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label>Office</Label>
              <Select
                value={form.officeId}
                onValueChange={(val) => {
                  setForm({ ...form, officeId: val, roleId: "" });
                }}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select Office" />
                </SelectTrigger>
                <SelectContent>
                  {offices.map((r) => (
                    <SelectItem key={r.id} value={r.id}>
                      {r.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label>Office Post</Label>
              <Select
                disabled={!form.officeId}
                value={form.roleId}
                onValueChange={(val) => setForm({ ...form, roleId: val })}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select Office Post" />
                </SelectTrigger>
                <SelectContent>
                  {roles.map((r) => (
                    <SelectItem key={r.id} value={r.id}>
                      {r.postName}, {r.officeCityName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label>Joining Date</Label>
              <Input
                type="date"
                value={form.joiningDate}
                onChange={(e) =>
                  setForm({ ...form, joiningDate: e.target.value })
                }
              />
            </div>

            <div>
              <Label>Joining Type</Label>
              <Select
                value={form.joiningType}
                onValueChange={(val) => setForm({ ...form, joiningType: val })}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select Joining Type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Government">Government</SelectItem>
                  <SelectItem value="Private">
                    Private(Competent Person for Boilers)
                  </SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label>Joining Detail</Label>
              <Select
                value={form.joiningDetail}
                onValueChange={(val) =>
                  setForm({ ...form, joiningDetail: val })
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select Detail" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="New Appointment">
                    New Appointment
                  </SelectItem>
                  <SelectItem value="Additional Charge">
                    Additional Charge
                  </SelectItem>
                  <SelectItem value="Transfer">Transfer</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="flex items-center gap-3 py-2">
              <Checkbox
                id="isInspector"
                checked={form.isInspector}
                onCheckedChange={(checked) =>
                  setForm({ ...form, isInspector: checked === true })
                }
              />
              <Label htmlFor="isInspector" className="cursor-pointer">
                Mark as Inspector (can be assigned boiler applications)
              </Label>
            </div>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleSave}>
                {editing ? "Update" : "Assign"}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
