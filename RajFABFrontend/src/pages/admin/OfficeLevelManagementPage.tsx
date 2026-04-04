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
import { Edit, Trash2, Layers } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import { useOfficeLevels } from "@/hooks/api";
import type { OfficeLevel } from "@/services/api/officeLevels";

export default function OfficeLevelManagementPage() {
  const {
    officeLevels,
    isLoading: loading,
    createOfficeLevel,
    updateOfficeLevel,
    deleteOfficeLevel,
  } = useOfficeLevels();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<OfficeLevel | null>(null);
  const [form, setForm] = useState({ name: "", levelOrder: 1 });

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const handleSave = () => {
    if (editing) {
      updateOfficeLevel({
        id: editing.id,
        data: form,
      });
    } else {
      createOfficeLevel(form);
    }
    setDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this office level?")) {
      deleteOfficeLevel(id);
    }
  };

  const filtered = useMemo(
    () =>
      officeLevels.filter(
        (l) =>
          l.name.toLowerCase().includes(search.toLowerCase()) ||
          l.levelOrder.toString().includes(search)
      ),
    [officeLevels, search]
  );

  const totalPages = Math.ceil(filtered.length / pageSize);
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: OfficeLevel, index: number) => (
        <span className="font-medium text-muted-foreground">
          {(page - 1) * pageSize + index}
        </span>
      ),
    },
    {
      key: "level",
      header: "Level",
      render: (level: OfficeLevel) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center">
            <Layers className="h-4 w-4 text-primary" />
          </div>
          <div>
            <div className="font-medium">{level.name}</div>
            <div className="text-xs text-muted-foreground">
              Order: {level.levelOrder}
            </div>
          </div>
        </div>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (level: OfficeLevel) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            onClick={() => {
              setEditing(level);
              setForm({
                name: level.name,
                levelOrder: level.levelOrder,
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
            onClick={() => handleDelete(level.id)}
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
        title="Office Level Management"
        description="Manage hierarchical office levels"
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
          setForm({ name: "", levelOrder: 1 });
          setDialogOpen(true);
        }}
        addLabel="Add Office Level"
        emptyMessage="No office levels found"
        pageSize={pageSize}
      />

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editing ? "Edit Office Level" : "Create Office Level"}
            </DialogTitle>
          </DialogHeader>

          <div className="space-y-4">
            <div>
              <Label>Level Name</Label>
              <Input
                value={form.name}
                onChange={(e) => setForm({ ...form, name: e.target.value })}
              />
            </div>

            <div>
              <Label>Level Order</Label>
              <Input
                type="number"
                min={1}
                value={form.levelOrder}
                onChange={(e) =>
                  setForm({
                    ...form,
                    levelOrder: Number(e.target.value),
                  })
                }
              />
            </div>

            <div className="flex justify-end gap-2 mt-4">
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
