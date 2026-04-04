import { useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Plus, Edit, ChevronLeft, ChevronRight } from "lucide-react";
import { useToast } from "@/hooks/use-toast";
import { 
  useUsers, 
  useRoles, 
  useModules, 
  useDivisions, 
  useDistricts, 
  useAreas 
} from "@/hooks/api";
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Checkbox } from "@/components/ui/checkbox";
import { Badge } from "@/components/ui/badge";
import { API_BASE_URL } from "@/services/api/base";


// ✅ Static privileges for now
const STATIC_PRIVILEGES = [
  "Factory",
  "Boiler",
  "Steam Pipeline Approval",
  "Factory Closer Form",
  "Medical Report",
  "BOE",
  "Boiler Manufacturer",
  "Boiler Manufacturer Drawing",
  "Boiler Erector",
  "Boiler Repair",
  "Boiler of Components and Fittings",
  "Approval Of Training",
];

export default function UserPrivilegesPage() {
  const { toast } = useToast();
  const { users } = useUsers();
  const { roles } = useRoles();
  const { modules } = useModules();
  const { divisions } = useDivisions();
  const { districts } = useDistricts();
  const { areas } = useAreas();

  const [userPrivileges, setUserPrivileges] = useState<Record<string, { privileges: string[]; moduleIds?: string[] }>>({});

  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedUserId, setSelectedUserId] = useState("");
  const [selectedRoleId, setSelectedRoleId] = useState("");
  // multi-select modules
  const [selectedModuleIds, setSelectedModuleIds] = useState<string[]>([]);
  const [selectedDivisionId, setSelectedDivisionId] = useState("");
  const [selectedDistrictId, setSelectedDistrictId] = useState("");
  const [selectedAreaIds, setSelectedAreaIds] = useState<string[]>([]);
  const [selectedPrivileges, setSelectedPrivileges] = useState<string[]>([]);

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;


  // toggle module id in selectedModuleIds
  const toggleModule = (id: string) => {
    setSelectedModuleIds((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]));
  };

  const handleSave = async () => {
    if (!selectedUserId) {
      toast({ title: "Error", description: "Select a user", variant: "destructive" });
      return;
    }

    // Build payload matching backend DTO exactly
    const payload = {
      UserId: selectedUserId,
      RoleId: selectedRoleId || null,
      ModuleId: selectedModuleIds.length > 0 ? selectedModuleIds[0] : null, // Backend expects single ModuleId
      DivisionId: selectedDivisionId || null,
      DistrictId: selectedDistrictId || null,
      AreaIds: selectedAreaIds.filter(id => id && id !== "none"),
      Privileges: selectedPrivileges || [],
    };

    try {
      const res = await fetch(`${API_BASE_URL}/privileges/assign`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(payload),
      });

      const json = await res.json();

      if (!res.ok || (json && json.success === false)) {
        // Try to show server message if any
        const message = (json && json.message) || "Server rejected the request";
        toast({ title: "Error", description: message, variant: "destructive" });
        return;
      }

      // Update local state only on success
      setUserPrivileges((prev) => ({
        ...prev,
        [selectedUserId]: { privileges: selectedPrivileges, moduleIds: selectedModuleIds },
      }));

      toast({ title: "Success", description: "Privileges assigned" });
      setDialogOpen(false);
    } catch (err) {
      console.error("Assign privileges failed:", err);
      toast({ title: "Network Error", description: "Could not reach server", variant: "destructive" });
    }
  };

  // --- Filtering + Pagination ---
  const filteredUsers = useMemo(
    () => users.filter((u) => `${u.username} ${u.fullName}`.toLowerCase().includes(search.toLowerCase())),
    [users, search]
  );
  const totalPages = Math.ceil(filteredUsers.length / pageSize);
  const paginatedUsers = filteredUsers.slice((page - 1) * pageSize, page * pageSize);

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <h1 className="text-3xl font-bold">User Privileges</h1>
        <Button
          onClick={() => {
            setSelectedUserId("");
            setSelectedRoleId("");
            setSelectedModuleIds([]);
            setSelectedDivisionId("");
            setSelectedDistrictId("");
            setSelectedAreaIds([]);
            setSelectedPrivileges([]);
            setDialogOpen(true);
          }}
        >
          <Plus className="h-4 w-4 mr-2" /> Assign Privileges
        </Button>
      </div>

      {/* Search */}
      <Input
        placeholder="Search users..."
        value={search}
        onChange={(e) => {
          setSearch(e.target.value);
          setPage(1);
        }}
        className="max-w-xs"
      />

      {/* Table */}
      <div className="rounded-md border bg-white shadow-sm">
        <Table>
          <TableCaption>User privilege assignments</TableCaption>
          <TableHeader>
            <TableRow>
              <TableHead>#</TableHead>
              <TableHead>Username</TableHead>
              <TableHead>Full Name</TableHead>
              <TableHead>Privileges</TableHead>
              <TableHead className="text-right">Actions</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {paginatedUsers.map((u, i) => (
              <TableRow key={u.id}>
                <TableCell>{(page - 1) * pageSize + i + 1}</TableCell>
                <TableCell>{u.username}</TableCell>
                <TableCell>{u.fullName}</TableCell>
                <TableCell>
                  {(userPrivileges[u.id]?.privileges || []).map((p) => (
                    <Badge key={p} variant="secondary" className="mr-1">{p}</Badge>
                  ))}
                  {/* show selected modules as badges too if present */}
                  {(userPrivileges[u.id]?.moduleIds || []).map((mId) => {
                    const mod = modules.find((m) => m.id === mId);
                    return mod ? <Badge key={mId} variant="outline" className="ml-1">{mod.name}</Badge> : null;
                  })}
                </TableCell>
                <TableCell className="text-right">
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={async () => {
                      setSelectedUserId(u.id);
                      // Fetch existing assignments for this user
                      try {
                        const res = await fetch(`${API_BASE_URL}/privileges/user/${u.id}`);
                        const json = await res.json();
                        if (res.ok && json.success && json.data) {
                          const { privileges = [], locationAssignments = [] } = json.data;
                          setSelectedPrivileges(privileges);
                          
                          // Load location data from first assignment (they should all be the same except areas)
                          if (locationAssignments.length > 0) {
                            const firstAssignment = locationAssignments[0];
                            setSelectedRoleId(firstAssignment.RoleId || "");
                            setSelectedModuleIds(firstAssignment.ModuleId ? [firstAssignment.ModuleId] : []);
                            setSelectedDivisionId(firstAssignment.DivisionId || "");
                            setSelectedDistrictId(firstAssignment.DistrictId || "");
                            // Collect all area IDs
                            const areaIds = locationAssignments
                              .filter(la => la.AreaId)
                              .map(la => la.AreaId);
                            setSelectedAreaIds(areaIds);
                          } else {
                            // Reset if no assignments
                            setSelectedRoleId("");
                            setSelectedModuleIds([]);
                            setSelectedDivisionId("");
                            setSelectedDistrictId("");
                            setSelectedAreaIds([]);
                          }
                        }
                      } catch (err) {
                        console.error("Failed to load user assignments:", err);
                        // Set defaults on error
                        setSelectedPrivileges([]);
                        setSelectedRoleId("");
                        setSelectedModuleIds([]);
                        setSelectedDivisionId("");
                        setSelectedDistrictId("");
                        setSelectedAreaIds([]);
                      }
                      setDialogOpen(true);
                    }}
                  >
                    <Edit className="h-4 w-4 mr-1" /> Manage
                  </Button>
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </div>

      {/* Pagination */}
      {totalPages > 1 && (
        <div className="flex justify-end gap-2">
          <Button onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1}>
            <ChevronLeft className="h-4 w-4" /> Prev
          </Button>
          <span>
            Page {page} of {totalPages}
          </span>
          <Button onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages}>
            Next <ChevronRight className="h-4 w-4" />
          </Button>
        </div>
      )}

      {/* Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-3xl">
          <DialogHeader>
            <DialogTitle>Assign Privileges</DialogTitle>
          </DialogHeader>
          <div className="grid grid-cols-2 gap-4 max-h-[70vh] overflow-y-auto">
            {/* User */}
            <div>
              <Label>User</Label>
              <Select value={selectedUserId} onValueChange={setSelectedUserId}>
                <SelectTrigger><SelectValue placeholder="Select User" /></SelectTrigger>
                <SelectContent>
                  {users.map((u) => <SelectItem key={u.id} value={u.id}>{u.username} - {u.fullName}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>

            {/* Role */}
            <div>
              <Label>Role</Label>
              <Select value={selectedRoleId} onValueChange={setSelectedRoleId}>
                <SelectTrigger><SelectValue placeholder="Select Role" /></SelectTrigger>
                <SelectContent>
                  {roles.map((r) => <SelectItem key={r.id} value={r.id}>{r.name}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>

            {/* Modules (multi-select) */}
            <div className="col-span-2">
              <Label>Modules (multi-select)</Label>
              <div className="grid grid-cols-2 gap-2 border p-3 rounded-md mt-2">
                {modules.map((mod) => (
                  <div key={mod.id} className="flex items-center gap-2">
                    <Checkbox
                      checked={selectedModuleIds.includes(mod.id)}
                      onCheckedChange={(checked) =>
                        checked ? setSelectedModuleIds((s) => [...s, mod.id]) : setSelectedModuleIds((s) => s.filter((x) => x !== mod.id))
                      }
                    />
                    <span>{mod.name}</span>
                  </div>
                ))}
              </div>
            </div>

            {/* Division */}
            <div>
              <Label>Division</Label>
              <Select value={selectedDivisionId} onValueChange={setSelectedDivisionId}>
                <SelectTrigger><SelectValue placeholder="Select Division" /></SelectTrigger>
                <SelectContent>
                  {divisions.map((d) => <SelectItem key={d.id} value={d.id}>{d.name}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>

            {/* District */}
            <div>
              <Label>District</Label>
              <Select value={selectedDistrictId} onValueChange={setSelectedDistrictId}>
                <SelectTrigger><SelectValue placeholder="Select District" /></SelectTrigger>
                <SelectContent>
                  {districts.map((d) => <SelectItem key={d.id} value={d.id}>{d.name}</SelectItem>)}
                </SelectContent>
              </Select>
            </div>

            {/* Areas (multi-select) */}
            <div className="col-span-2">
              <Label>Areas (multi-select)</Label>
              <div className="grid grid-cols-2 gap-2 border p-3 rounded-md mt-2">
                {areas.map((area) => (
                  <div key={area.id} className="flex items-center gap-2">
                    <Checkbox
                      checked={selectedAreaIds.includes(area.id)}
                      onCheckedChange={(checked) =>
                        checked ? setSelectedAreaIds((s) => [...s, area.id]) : setSelectedAreaIds((s) => s.filter((x) => x !== area.id))
                      }
                    />
                    <span>{area.name}</span>
                  </div>
                ))}
              </div>
            </div>

            {/* Privileges */}
            <div className="col-span-2">
              <Label>Privileges</Label>
              <div className="grid grid-cols-2 gap-2 border p-3 rounded-md mt-2">
                {STATIC_PRIVILEGES.map((priv) => (
                  <div key={priv} className="flex items-center gap-2">
                    <Checkbox
                      checked={selectedPrivileges.includes(priv)}
                      onCheckedChange={(checked) =>
                        checked
                          ? setSelectedPrivileges([...selectedPrivileges, priv])
                          : setSelectedPrivileges(selectedPrivileges.filter((p) => p !== priv))
                      }
                    />
                    <span>{priv}</span>
                  </div>
                ))}
              </div>
            </div>
          </div>

          <div className="flex justify-end gap-2 mt-4">
            <Button variant="outline" onClick={() => setDialogOpen(false)}>Cancel</Button>
            <Button onClick={handleSave}>Save</Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}