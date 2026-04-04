import { useEffect, useMemo, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Edit, Trash2, Package, CheckCircle, XCircle } from "lucide-react";
import { useModules, useRulesByAct, useActs } from "@/hooks/api";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import { Badge } from "@/components/ui/badge";
import { FormModule } from "@/types/forms";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

export default function ModuleManagementPage() {
  const { acts } = useActs();

  const {
    modules,
    isLoading: loading,
    createModule,
    updateModule,
    deleteModule,
  } = useModules();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<FormModule | null>(null);

  const [form, setForm] = useState({
    name: "",
    category: "",
    description: "",
    actId: "",
    ruleId: "",
  });

  const [errors, setErrors] = useState({
    name: "",
    actId: "",
    ruleId: "",
    category: "",
  });

  const { data: rules = [] } = useRulesByAct(form.actId);

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const resetForm = () => {
    setForm({
      name: "",
      category: "",
      description: "",
      actId: "",
      ruleId: "",
    });

    setErrors({
      name: "",
      actId: "",
      ruleId: "",
      category: "",
    });
  };

  const validate = () => {
    const newErrors: any = {};

    if (!form.name.trim()) newErrors.name = "Name is required";
    if (!form.actId) newErrors.actId = "Act is required";
    if (!form.ruleId) newErrors.ruleId = "Rule is required";
    if (!form.category) newErrors.category = "Category is required";

    setErrors(newErrors);

    return Object.keys(newErrors).length === 0;
  };

  const openCreate = () => {
    setEditing(null);
    resetForm();
    setDialogOpen(true);
  };

  const openEdit = (m: FormModule) => {
    setEditing(m);

    setForm({
      name: m.name ?? "",
      category: m.category ?? "",
      description: m.description ?? "",
      actId: m.actId ?? "",
      ruleId: m.ruleId ?? "",
    });

    setDialogOpen(true);
  };

  const handleSave = () => {
    if (!validate()) return;

    if (editing) {
      updateModule({ id: editing.id, data: form });
    } else {
      createModule(form);
    }

    setDialogOpen(false);
    setEditing(null);
    resetForm();
  };

  const handleDelete = (id: string) => {
    if (!confirm("Delete this Application?")) return;
    deleteModule(id);
  };

  const filtered = useMemo(() => {
    const q = search.toLowerCase();

    return modules.filter(
      (m) =>
        (m.name || "").toLowerCase().includes(q) ||
        (m.category || "").toLowerCase().includes(q) ||
        (m.description || "").toLowerCase().includes(q)
    );
  }, [modules, search]);

  const totalPages = Math.ceil(filtered.length / pageSize) || 1;

  const paginated = useMemo(() => {
    const start = (page - 1) * pageSize;
    return filtered.slice(start, start + pageSize);
  }, [filtered, page]);

  useEffect(() => {
    if (page > totalPages) setPage(totalPages);
  }, [totalPages, page]);

  if (loading) {
    return (
      <div className="flex items-center justify-center h-64 text-lg">
        Loading...
      </div>
    );
  }

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: FormModule, index: number) => (
        <span className="font-medium text-muted-foreground">{index}</span>
      ),
    },
    {
      key: "name",
      header: "Application Name",
      render: (module: FormModule) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center">
            <Package className="h-4 w-4 text-primary" />
          </div>
          <span className="font-medium">{module.name}</span>
        </div>
      ),
    },
    {
      key: "category",
      header: "Category",
      render: (module: FormModule) => (
        <Badge variant="secondary">{module.category}</Badge>
      ),
    },
    {
      key: "actName",
      header: "Act",
      render: (module: FormModule) => (
        <span className="max-w-xs truncate">{module.actName}</span>
      ),
    },
    {
      key: "ruleName",
      header: "Rule",
      render: (module: FormModule) => (
        <span className="max-w-xs truncate">{module.ruleName}</span>
      ),
    },
    {
      key: "status",
      header: "Status",
      render: (module: FormModule) => (
        <div className="flex items-center gap-2">
          {module.isActive ? (
            <CheckCircle className="h-4 w-4 text-green-500" />
          ) : (
            <XCircle className="h-4 w-4 text-red-500" />
          )}
          <Badge variant={module.isActive ? "default" : "outline"}>
            {module.isActive ? "Active" : "Inactive"}
          </Badge>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <ModernDataTable
        title="Application Management"
        description="Create and manage application for the regulatory system"
        data={paginated}
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
        onAdd={openCreate}
        addLabel="Add Application"
        emptyMessage="No applications found"
        pageSize={pageSize}
      />

      <Dialog
        open={dialogOpen}
        onOpenChange={(open) => {
          setDialogOpen(open);
          if (!open) {
            setEditing(null);
            resetForm();
          }
        }}
      >
        <DialogContent className="max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              {editing ? "Edit Application" : "Create Application"}
            </DialogTitle>
          </DialogHeader>

          <div className="space-y-4">
            {/* Name */}
            <div>
              <Label>
                Name <span className="text-destructive">*</span>
              </Label>

              <Input
                value={form.name}
                onChange={(e) => {
                  setForm((f) => ({ ...f, name: e.target.value }));
                  setErrors((prev) => ({ ...prev, name: "" }));
                }}
                placeholder="e.g. Factory Registration"
              />

              {errors.name && (
                <p className="text-sm text-destructive mt-1">{errors.name}</p>
              )}
            </div>

            {/* Act */}
            <div>
              <Label>
                Act <span className="text-destructive">*</span>
              </Label>

              <Select
                value={form.actId}
                onValueChange={(val) => {
                  setForm({ ...form, actId: val, ruleId: "" });
                  setErrors((prev) => ({ ...prev, actId: "" }));
                }}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select Act" />
                </SelectTrigger>

                <SelectContent>
                  {acts?.map((a) => (
                    <SelectItem key={a.id} value={a.id}>
                      {a.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              {errors.actId && (
                <p className="text-sm text-destructive mt-1">{errors.actId}</p>
              )}
            </div>

            {/* Rule */}
            <div>
              <Label>
                Rule <span className="text-destructive">*</span>
              </Label>

              <Select
                disabled={!form.actId}
                value={form.ruleId}
                onValueChange={(val) => {
                  setForm({ ...form, ruleId: val });
                  setErrors((prev) => ({ ...prev, ruleId: "" }));
                }}
              >
                <SelectTrigger>
                  <SelectValue
                    placeholder={
                      form.actId ? "Select Rule" : "Select Act First"
                    }
                  />
                </SelectTrigger>

                <SelectContent>
                  {rules.map((r) => (
                    <SelectItem key={r.id} value={r.id}>
                      {r.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              {errors.ruleId && (
                <p className="text-sm text-destructive mt-1">{errors.ruleId}</p>
              )}
            </div>

            {/* Category */}
            <div>
              <Label>
                Category <span className="text-destructive">*</span>
              </Label>

              <Select
                value={form.category}
                onValueChange={(val) => {
                  setForm((f) => ({ ...f, category: val }));
                  setErrors((prev) => ({ ...prev, category: "" }));
                }}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select Category" />
                </SelectTrigger>

                <SelectContent>
                  <SelectItem value="factory">Factory</SelectItem>
                  <SelectItem value="boiler">Boiler</SelectItem>
                </SelectContent>
              </Select>

              {errors.category && (
                <p className="text-sm text-destructive mt-1">
                  {errors.category}
                </p>
              )}
            </div>

            {/* Description */}
            <div>
              <Label>Description</Label>

              <Textarea
                value={form.description}
                onChange={(e) =>
                  setForm((f) => ({ ...f, description: e.target.value }))
                }
                placeholder="Describe what this application is for…"
              />
            </div>

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