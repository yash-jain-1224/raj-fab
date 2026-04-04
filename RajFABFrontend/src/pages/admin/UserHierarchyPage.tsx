// src/pages/UserHierarchyPage.tsx
import { useEffect, useMemo, useState } from "react";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Edit, Trash2 } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useUsers } from "@/hooks/api/useUsers";
import { useUserHierarchies } from "@/hooks/api/useUserHierarchy";

import type { UserHierarchy } from "@/services/api/userHierarchy";
import { toast } from "react-hot-toast";

type UserOption = { id: string; label: string };

export default function UserHierarchyPage() {
  const { users, isLoading: usersLoading } = useUsers();
  
  const {
    hierarchies,
    isLoading: hierarchiesLoading,
    createHierarchy,
    updateHierarchy,
    deleteHierarchy,
    createHierarchyAsync,
    updateHierarchyAsync,
    isCreating,
    isUpdating,
    isDeleting,
  } = useUserHierarchies();

  const loading = usersLoading || hierarchiesLoading;

  // dialog/form state
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<UserHierarchy | null>(null);
  
  const [form, setForm] = useState<{ userId: string; reportsToId: string; emergencyReportToId: string }>({
    userId: "none",
    reportsToId: "none",
    emergencyReportToId: "none",
  });

  // local saving flag so button reliably disables
  const [isSaving, setIsSaving] = useState(false);

  // search + paging
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  // --- SINGLE getId helper (only defined once) ---
  const getId = (u: any) => u?.id ?? u?._id ?? u?._id_str ?? u?.userId ?? u?.uid ?? "";

  // Build user options robustly with district filtering
  const userOptions = useMemo<UserOption[]>(() => {
    const list = users || [];
    return list.map((u: any) => {
      const id = getId(u);
      const username = u?.username ?? u?.userName ?? u?.user ?? (u?.email ? String(u.email).split?.("@")?.[0] : null) ?? null;
      const fullName = u?.fullName ?? u?.full_name ?? u?.name ?? null;
      const roleName = u?.roleName ?? u?.role ?? "";

      let label = "";
      if (username && fullName && username !== fullName) {
        label = `${username} (${fullName})`;
      } else if (username) {
        label = username;
      } else if (fullName) {
        label = fullName;
      } else if (id) {
        label = `#${id}`;
      } else {
        label = "—";
      }

      const additionalInfo = [] as string[];
      if (roleName) additionalInfo.push(roleName);
      if (additionalInfo.length > 0) {
        label = `${label} | ${additionalInfo.join(" | ")}`;
      }

      return { id, label };
    });
  }, [users]);

  // Available users for Employee dropdown (excludes users with existing hierarchies)
  const availableUserOptions = useMemo<UserOption[]>(() => {
    const usedUserIds = new Set((hierarchies || []).map((h: any) => h.userId));
    if (editing?.userId) usedUserIds.delete(editing.userId);
    return userOptions.filter(user => !usedUserIds.has(user.id));
  }, [userOptions, hierarchies, editing?.userId]);

  // filtering
  const filtered = useMemo(() => {
    const q = search.trim().toLowerCase();
    if (!q) return hierarchies || [];
    return (hierarchies || []).filter((h: any) => {
      const u = userOptions.find((x) => x.id === h.userId)?.label ?? "";
      const r = userOptions.find((x) => x.id === h.reportsToId)?.label ?? "";
      const e = userOptions.find((x) => x.id === h.emergencyReportToId)?.label ?? "";
      return `${u} ${r} ${e}`.toLowerCase().includes(q);
    });
  }, [hierarchies, search, userOptions]);

  const totalPages = Math.max(1, Math.ceil((filtered?.length || 0) / pageSize));
  const paginated = useMemo(() => {
    const start = (page - 1) * pageSize;
    return filtered.slice(start, start + pageSize);
  }, [filtered, page]);

  // index map to show absolute index across pages
  const indexMap = useMemo(() => {
    const m = new Map<string, number>();
    filtered.forEach((f: any, i: number) =>
      m.set(f.id ?? `${f.userId}-${f.reportsToId ?? "null"}`, (page - 1) * pageSize + i + 1)
    );
    return m;
  }, [filtered, page]);

  const openCreate = () => {
    setEditing(null);
    setForm({ userId: "none", reportsToId: "none", emergencyReportToId: "none" });
    setDialogOpen(true);
  };

  const openEdit = (h: UserHierarchy) => {
    setEditing(h);
    setForm({
      userId: h.userId ?? "none",
      reportsToId: h.reportsToId ?? "none",
      emergencyReportToId: h.emergencyReportToId ?? "none",
    });
    setDialogOpen(true);
    window.scrollTo({ top: 0, behavior: "smooth" });
  };

  const resetForm = () => {
    setEditing(null);
    setForm({ userId: "none", reportsToId: "none", emergencyReportToId: "none" });
    setDialogOpen(false);
  };

  // Ensure that when dialog is closed externally we reset editing/form
  useEffect(() => {
    if (!dialogOpen) {
      setEditing(null);
      setForm({ userId: "none", reportsToId: "none", emergencyReportToId: "none" });
      setIsSaving(false);
    }
  }, [dialogOpen]);

  // Direct API fallback helper (keeps network calls predictable)
  const directApiCall = async (method: "POST" | "PUT" | "DELETE", path: string, data?: any) => {
    const options: RequestInit = {
      method,
      headers: { "Content-Type": "application/json" },
    };
    
    if (method !== "DELETE" && data) {
      options.body = JSON.stringify(data);
    }
    
    const res = await fetch(path, options);
    const text = await res.text();
    let json = null;
    try { json = JSON.parse(text); } catch { json = text; }
    if (!res.ok) throw new Error(`Status ${res.status} - ${JSON.stringify(json)}`);
    return json;
  };

  // Validate, save and call hooks (create/update). Fixed to handle react-query mutations properly.
  const handleSave = async () => {
    console.log("handleSave called, form:", form, "editing:", editing);
    if (!form.userId || form.userId === "none") {
      toast?.error?.("Please select an employee");
      return;
    }
    if (form.reportsToId && form.reportsToId !== "none" && form.reportsToId === form.userId) {
      toast?.error?.("Employee cannot report to themselves");
      return;
    }
    if (form.emergencyReportToId && form.emergencyReportToId !== "none" && form.emergencyReportToId === form.userId) {
      toast?.error?.("Emergency report cannot be the employee themselves");
      return;
    }

    const cleanForm = {
      userId: form.userId,
      reportsToId: form.reportsToId === "none" ? null : form.reportsToId,
      emergencyReportToId: form.emergencyReportToId === "none" ? null : form.emergencyReportToId,
    };

    try {
      setIsSaving(true);

      if (editing?.id) {
        try {
          await updateHierarchyAsync({ id: editing.id, data: cleanForm } as any);
          toast.success("Hierarchy updated");
        } catch (fallbackError) {
          await directApiCall("PUT", `/user-hierarchy/${editing.id}`, cleanForm);
          toast.success("Hierarchy updated");
        }
      } else {
        try {
          await createHierarchyAsync(cleanForm as any);
          toast.success("Hierarchy created");
        } catch (fallbackError) {
          await directApiCall("POST", `/user-hierarchy`, cleanForm);
          toast.success("Hierarchy created");
        }
      }

      setPage(1);
      resetForm();
    } catch (err: any) {
      console.error("Save failed:", err);
      toast.error(err?.message || "Failed to save hierarchy");
    } finally {
      setIsSaving(false);
    }
  };

  const handleDelete = async (id?: string) => {
    if (!id) return;
    if (!confirm("Delete this mapping?")) return;
    try {
      // react-query mutations return void
      try {
        deleteHierarchy(id);
        toast.success("Deleted");
      } catch (fallbackError) {
        await directApiCall("DELETE", `/user-hierarchy/${id}`);
        toast.success("Deleted");
      }
    } catch (err: any) {
      console.error(err);
      toast.error(err?.message || "Failed to delete");
    }
  };

  const getLabel = (id?: string | null) => userOptions.find((u) => u.id === (id ?? ""))?.label ?? "-";

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (row: any, index: number) => {
        const key = row.id ?? `${row.userId}-${row.reportsToId ?? "null"}`;
        const abs = indexMap.get(key);
        return <span className="font-medium text-muted-foreground">{abs ?? index}</span>;
      },
    },
    {
      key: "employee",
      header: "Employee",
      render: (row: any) => <div className="font-medium">{getLabel(row.userId)}</div>,
    },
    {
      key: "reportsTo",
      header: "Reports To",
      render: (row: any) => <div className="text-sm text-muted-foreground">{getLabel(row.reportsToId)}</div>,
    },
    {
      key: "emergency",
      header: "Emergency Report To",
      render: (row: any) => <div className="text-sm text-muted-foreground">{getLabel(row.emergencyReportToId)}</div>,
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (row: any) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            onClick={() => openEdit(row)}
            className="h-8 w-8 p-0"
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            size="sm"
            variant="ghost"
            onClick={() => handleDelete(row.id)}
            className="h-8 w-8 p-0 text-destructive hover:text-destructive"
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
        title="User Hierarchy"
        description="Manage reporting & emergency reporting relationships"
        data={paginated}
        columns={columns}
        loading={loading}
        search={search}
        onSearchChange={(val: string) => {
          setSearch(val);
          setPage(1);
        }}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
        onAdd={openCreate}
        addLabel="Add Hierarchy"
        emptyMessage="No hierarchy mappings found"
        pageSize={pageSize}
      />

      {/* Dialog */}
      <Dialog
        open={dialogOpen}
        onOpenChange={(v) => {
          if (isSaving) {
            console.debug("Preventing dialog close while saving");
            return;
          }
          setDialogOpen(v);
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "Edit Mapping" : "Create Mapping"}</DialogTitle>
          </DialogHeader>

          <div className="space-y-4">

            <div>
              <Label>Employee</Label>
              <Select value={form.userId || "none"} onValueChange={(v) => { console.debug("Employee changed:", v); setForm((s) => ({ ...s, userId: v })); }}>
                <SelectTrigger className="w-full"><SelectValue placeholder="Select employee" /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="none">-- Select --</SelectItem>
                  {availableUserOptions.map((u) => (
                    <SelectItem key={u.id} value={u.id}>
                      {u.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label>Reports To</Label>
              <Select value={form.reportsToId || "none"} onValueChange={(v) => { console.debug("ReportsTo changed:", v); setForm((s) => ({ ...s, reportsToId: v })); }}>
                <SelectTrigger className="w-full"><SelectValue placeholder="Select reporting manager (optional)" /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="none">-- None --</SelectItem>
                  {userOptions.map((u) => (
                    <SelectItem key={u.id} value={u.id}>
                      {u.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label>Emergency Report To</Label>
              <Select
                value={form.emergencyReportToId || "none"}
                onValueChange={(v) => { console.debug("Emergency changed:", v); setForm((s) => ({ ...s, emergencyReportToId: v })); }}
              >
                <SelectTrigger className="w-full"><SelectValue placeholder="Select emergency contact (optional)" /></SelectTrigger>
                <SelectContent>
                  <SelectItem value="none">-- None --</SelectItem>
                  {userOptions.map((u) => (
                    <SelectItem key={u.id} value={u.id}>
                      {u.label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => { if (!isSaving) setDialogOpen(false); }}>Cancel</Button>
              <Button
                type="button"
                onClick={() => { console.log("Create/Update button clicked"); handleSave(); }}
                disabled={Boolean(isSaving || isCreating || isUpdating || isDeleting)}
              >
                {isSaving ? (editing ? "Updating..." : "Creating...") :
                  (editing ? (isUpdating ? "Updating..." : "Update") : (isCreating ? "Creating..." : "Create"))}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
