import { useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
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
import { useDistricts, useDivisions } from "@/hooks/api";
import type { District } from "@/services/api/districts";

export default function DistrictManagementPage() {
  const { districts, isLoading: loading, createDistrict, updateDistrict, deleteDistrict } = useDistricts();
  const { divisions } = useDivisions();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<District | null>(null);
  const [form, setForm] = useState({ name: "", divisionId: "" });

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const handleSave = () => {
    if (editing) {
      updateDistrict({ id: editing.id, data: form });
    } else {
      createDistrict(form);
    }
    setDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this district?")) {
      deleteDistrict(id);
    }
  };

  const filtered = useMemo(
    () =>
      (districts ?? []).filter((d) =>
        `${d.name ?? ""} ${d.divisionName ?? ""}`.toLowerCase().includes(search.toLowerCase())
      ),
    [districts, search]
  );
  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: District, index: number) => (
        <span className="font-medium text-muted-foreground">{index}</span>
      ),
    },
    {
      key: "name",
      header: "District Name",
      render: (district: District) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center">
            <Building className="h-4 w-4 text-primary" />
          </div>
          <span className="font-medium">{district.name}</span>
        </div>
      ),
    },
    {
      key: "division",
      header: "Division",
      render: (district: District) => (
        <Badge variant="secondary" className="font-medium">
          {district.divisionName}
        </Badge>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (district: District) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            onClick={() => {
              setEditing(district);
              setForm({ name: district.name, divisionId: district.divisionId });
              setDialogOpen(true);
            }}
            className="h-8 w-8 p-0"
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            size="sm"
            variant="ghost"
            onClick={() => handleDelete(district.id)}
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
        title="District Management"
        description="Manage districts within divisions"
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
        onAdd={() => {
          setEditing(null);
          setForm({ name: "", divisionId: "" });
          setDialogOpen(true);
        }}
        addLabel="Add District"
        emptyMessage="No districts found"
        pageSize={pageSize}
      />

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "Edit District" : "Create District"}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <Label>Name</Label>
            <Input
              value={form.name}
              onChange={(e) => {
                const value = e.target.value;
                if (/^[A-Za-z\s]*$/.test(value)) {
                  setForm((f) => ({ ...f, name: value }));
                }
              }}
              placeholder="e.g., Bhilwara"
            />

            <Label>Division</Label>
            <Select
              value={form.divisionId}
              onValueChange={(val) => setForm({ ...form, divisionId: val })}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select Division" />
              </SelectTrigger>
              <SelectContent>
                {(divisions ?? []).length === 0 ? (
                  <div className="p-2 text-sm text-muted-foreground">No divisions</div>
                ) : (
                  (divisions ?? []).map((d) => (
                    <SelectItem key={d.id} value={d.id}>
                      {d.name}
                    </SelectItem>
                  ))
                )}
              </SelectContent>
            </Select>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleSave}>{editing ? "Update" : "Create"}</Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
