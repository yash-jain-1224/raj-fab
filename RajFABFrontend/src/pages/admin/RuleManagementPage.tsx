import { useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Edit, Trash2, Building } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { useRules, useActs } from "@/hooks/api";
import type { Rule } from "@/services/api/rules";

type Errors = {
  name?: string;
  category?: string;
  actId?: string;
  implementationYear?: string;
};

export default function RuleManagementPage() {
  const { rules, isLoading, createRule, updateRule, deleteRule } = useRules();
  const { acts } = useActs();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Rule | null>(null);
  const [form, setForm] = useState({
    name: "",
    category: "",
    actId: "",
    implementationYear: "",
  });
  const [errors, setErrors] = useState<Errors>({});

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const getActName = (actId: string) =>
    acts?.find((a) => a.id === actId)?.name ?? "—";

  const validate = () => {
    const e: Errors = {};

    if (!form.name.trim()) e.name = "Name is required";
    if (!form.category.trim()) e.category = "Category is required";
    if (!form.implementationYear)
      e.implementationYear = "Implementation Year is required";
    if (!form.actId) e.actId = "Act is required";

    if (form.implementationYear) {
      const year = Number(form.implementationYear);
      if (isNaN(year) || year < 1800 || year > 9999) {
        e.implementationYear = "Year must be between 1800 and 9999";
      }
    }

    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleSave = () => {
    if (!validate()) return;

    const payload = {
      name: form.name.trim(),
      category: form.category.trim(),
      actId: form.actId,
      implementationYear: form.implementationYear
        ? Number(form.implementationYear)
        : null,
    };

    if (editing) {
      updateRule({ id: editing.id, data: payload });
    } else {
      createRule(payload);
    }

    setDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this rule?")) {
      deleteRule(id);
    }
  };

  const filtered = useMemo(
    () =>
      (rules ?? []).filter((r) =>
        `${r.name ?? ""} ${r.category ?? ""}`
          .toLowerCase()
          .includes(search.toLowerCase())
      ),
    [rules, search]
  );

  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const columns = [
    {
      key: "index",
      header: "#",
      render: (_: Rule, index: number) => index,
    },
    {
      key: "name",
      header: "Rule Name",
      render: (rule: Rule) => (
        <div className="flex items-center gap-2">
          <Building className="h-4 w-4 text-primary" />
          <span className="font-medium">{rule.name}</span>
        </div>
      ),
    },
    {
      key: "category",
      header: "Category",
      render: (rule: Rule) => <Badge variant="outline">{rule.category}</Badge>,
    },
    {
      key: "act",
      header: "Act",
      render: (rule: Rule) => (
        <Badge variant="secondary">{getActName(rule.actId)}</Badge>
      ),
    },
    {
      key: "year",
      header: "Year",
      render: (rule: Rule) => rule.implementationYear ?? "—",
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (rule: Rule) => (
        <div className="flex justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            onClick={() => {
              setEditing(rule);
              setErrors({});
              setForm({
                name: rule.name,
                category: rule.category,
                actId: rule.actId,
                implementationYear: rule.implementationYear?.toString() ?? "",
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
            className="h-8 w-8 p-0 text-destructive"
            onClick={() => handleDelete(rule.id)}
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
        title="Rule Management"
        description="Manage rules within acts"
        data={paginated}
        columns={columns}
        loading={isLoading}
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
          setErrors({});
          setForm({
            name: "",
            category: "",
            actId: "",
            implementationYear: "",
          });
          setDialogOpen(true);
        }}
        addLabel="Add Rule"
        emptyMessage="No rules found"
        pageSize={pageSize}
      />

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "Edit Rule" : "Create Rule"}</DialogTitle>
          </DialogHeader>

          <div className="space-y-3">
            <Label>Name *</Label>
            <Input
              value={form.name}
              onChange={(e) => setForm({ ...form, name: e.target.value })}
            />
            {errors.name && (
              <p className="text-sm text-destructive">{errors.name}</p>
            )}

            <Label>Category *</Label>
            <Input
              value={form.category}
              onChange={(e) => setForm({ ...form, category: e.target.value })}
            />
            {errors.category && (
              <p className="text-sm text-destructive">{errors.category}</p>
            )}

            <Label>Implementation Year *</Label>
            <Input
              type="number"
              value={form.implementationYear}
              onChange={(e) =>
                setForm({ ...form, implementationYear: e.target.value })
              }
            />
            {errors.implementationYear && (
              <p className="text-sm text-destructive">
                {errors.implementationYear}
              </p>
            )}

            <Label>Act *</Label>
            <Select
              value={form.actId}
              onValueChange={(val) => setForm({ ...form, actId: val })}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select Act" />
              </SelectTrigger>
              <SelectContent>
                {(acts ?? []).length === 0 ? (
                  <div className="px-3 py-2 text-sm text-muted-foreground">
                    No acts available
                  </div>
                ) : (
                  acts.map((a) => (
                    <SelectItem key={a.id} value={a.id}>
                      {a.name}
                    </SelectItem>
                  ))
                )}
              </SelectContent>
            </Select>
            {errors.actId && (
              <p className="text-sm text-destructive">{errors.actId}</p>
            )}

            <div className="flex justify-end gap-2 pt-2">
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
