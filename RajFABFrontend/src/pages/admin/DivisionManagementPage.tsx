import { useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Edit, Trash2, MapPin } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import { useDivisions } from "@/hooks/api";
import type { Division } from "@/services/api/divisions";

export default function DivisionManagementPage() {
  const { divisions, isLoading: loading, createDivision, updateDivision, deleteDivision } = useDivisions();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Division | null>(null);
  const [form, setForm] = useState({ name: "" });

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const handleSave = () => {
    if (editing) {
      updateDivision({ id: editing.id, data: form });
    } else {
      createDivision(form);
    }
    setDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this division?")) {
      deleteDivision(id);
    }
  };

  const filtered = useMemo(
    () => divisions.filter((d) => d.name.toLowerCase().includes(search.toLowerCase())),
    [divisions, search]
  );
  const totalPages = Math.ceil(filtered.length / pageSize);
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: Division, index: number) => (
        <span className="font-medium text-muted-foreground">{index}</span>
      ),
    },
    {
      key: "name",
      header: "Division Name",
      render: (division: Division) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center">
            <MapPin className="h-4 w-4 text-primary" />
          </div>
          <span className="font-medium">{division.name}</span>
        </div>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (division: Division) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            onClick={() => {
              setEditing(division);
              setForm({ name: division.name });
              setDialogOpen(true);
            }}
            className="h-8 w-8 p-0"
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            size="sm"
            variant="ghost"
            onClick={() => handleDelete(division.id)}
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
        title="Division Management"
        description="Manage administrative divisions"
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
          setForm({ name: "" });
          setDialogOpen(true);
        }}
        addLabel="Add Division"
        emptyMessage="No divisions found"
        pageSize={pageSize}
      />

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "Edit Division" : "Create Division"}</DialogTitle>
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
            <div className="flex justify-end gap-2 mt-4">
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
