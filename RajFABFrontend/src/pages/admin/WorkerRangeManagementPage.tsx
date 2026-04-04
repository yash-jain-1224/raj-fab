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
import { Edit, Trash2, Users } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import { useWorkerRanges } from "@/hooks/api";
import type { WorkerRange } from "@/types/workerRanges";

export default function WorkerRangeManagementPage() {
  const {
    workerRanges,
    isLoading: loading,
    createWorkerRange,
    updateWorkerRange,
    deleteWorkerRange,
  } = useWorkerRanges();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<WorkerRange | null>(null);
  const [form, setForm] = useState({ minWorkers: 0, maxWorkers: 0 });

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const handleSave = () => {
    if (form.minWorkers < 0 || form.maxWorkers <= form.minWorkers) {
      alert("Max workers must be greater than Min workers");
      return;
    }

    if (editing) {
      updateWorkerRange({ id: editing.id, data: form });
    } else {
      createWorkerRange(form);
    }
    setDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this worker range?")) {
      deleteWorkerRange(id);
    }
  };

  const filtered = useMemo(
    () =>
      workerRanges.filter((r) =>
        `${r.minWorkers}-${r.maxWorkers}`
          .toLowerCase()
          .includes(search.toLowerCase())
      ),
    [workerRanges, search]
  );

  const totalPages = Math.ceil(filtered.length / pageSize);
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: WorkerRange, index: number) => (
        <span className="font-medium text-muted-foreground">
          {(page - 1) * pageSize + index}
        </span>
      ),
    },
    {
      key: "range",
      header: "Worker Range",
      render: (r: WorkerRange) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center">
            <Users className="h-4 w-4 text-primary" />
          </div>
          <span className="font-medium">
            {r.minWorkers} – {r.maxWorkers} Workers
          </span>
        </div>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (r: WorkerRange) => (
        <div className="flex justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            className="h-8 w-8 p-0"
            onClick={() => {
              setEditing(r);
              setForm({
                minWorkers: r.minWorkers,
                maxWorkers: r.maxWorkers,
              });
              setDialogOpen(true);
            }}
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            size="sm"
            variant="ghost"
            className="h-8 w-8 p-0 text-destructive"
            onClick={() => handleDelete(r.id)}
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
        title="Worker Range Management"
        description="Define minimum and maximum workers range"
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
          setForm({ minWorkers: 0, maxWorkers: 0 });
          setDialogOpen(true);
        }}
        addLabel="Add Worker Range"
        emptyMessage="No worker ranges found"
        pageSize={pageSize}
      />

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editing ? "Edit Worker Range" : "Create Worker Range"}
            </DialogTitle>
          </DialogHeader>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <Label>Minimum Workers</Label>
              <Input
                type="number"
                value={form.minWorkers}
                onChange={(e) =>
                  setForm({ ...form, minWorkers: Number(e.target.value) })
                }
              />
            </div>
            <div>
              <Label>Maximum Workers</Label>
              <Input
                type="number"
                value={form.maxWorkers}
                onChange={(e) =>
                  setForm({ ...form, maxWorkers: Number(e.target.value) })
                }
              />
            </div>
          </div>

          <div className="flex justify-end gap-2 mt-6">
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleSave}>
              {editing ? "Update" : "Create"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
