import { useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Edit, Trash2, MapPin } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { useAreas, useCities } from "@/hooks/api";
import type { Area } from "@/services/api/areas";

export default function AreaManagementPage() {
  const { areas, isLoading: loading, createArea, updateArea, deleteArea } = useAreas();
  const { cities } = useCities();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Area | null>(null);
  const [form, setForm] = useState({ name: "", cityId: "" });

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const handleSave = () => {
    if (editing) {
      updateArea({ id: editing.id, data: form });
    } else {
      createArea(form);
    }
    setDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this area?")) {
      deleteArea(id);
    }
  };

  const filtered = useMemo(
    () =>
      (areas ?? []).filter((a) =>
        `${a?.name ?? ""} ${a?.cityName ?? ""}`.toLowerCase().includes(search.toLowerCase())
      ),
    [areas, search]
  );

  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: Area, index: number) => (
        <span className="font-medium text-muted-foreground">{index}</span>
      ),
    },
    {
      key: "name",
      header: "Area Name",
      render: (area: Area) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center">
            <MapPin className="h-4 w-4 text-primary" />
          </div>
          <span className="font-medium">{area.name}</span>
        </div>
      ),
    },
    {
      key: "city",
      header: "City",
      render: (area: Area) => (
        <Badge variant="secondary" className="font-medium">
          {area.cityName}
        </Badge>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (area: Area) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            onClick={() => {
              setEditing(area);
              setForm({ name: area.name, cityId: area.cityId });
              setDialogOpen(true);
            }}
            className="h-8 w-8 p-0"
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button 
            size="sm" 
            variant="ghost" 
            onClick={() => handleDelete(area.id)}
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
        title="Area Management"
        description="Manage geographical areas within cities"
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
          setForm({ name: "", cityId: "" });
          setDialogOpen(true);
        }}
        addLabel="Add Area"
        emptyMessage="No areas found"
        pageSize={pageSize}
      />

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "Edit Area" : "Create Area"}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <Label>Name</Label>
            <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />

            <Label>City</Label>
            <Select
              value={form.cityId}
              onValueChange={(val) => setForm({ ...form, cityId: val })}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select City" />
              </SelectTrigger>
              <SelectContent>
                {(cities ?? []).length === 0 ? (
                  <div className="p-2 text-sm text-muted-foreground">No cities</div>
                ) : (
                  (cities ?? []).map((c) => (
                    <SelectItem key={c.id} value={c.id}>
                      {c.name}
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
