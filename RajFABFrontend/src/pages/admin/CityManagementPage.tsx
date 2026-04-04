import { useEffect, useMemo, useState } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Edit, Trash2, Building2, MapPin } from "lucide-react";
import { useCities, useDistricts, useDivisions } from "@/hooks/api";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import { Badge } from "@/components/ui/badge";

type City = { id: string; name: string; districtId: string };

export default function CityManagementPage() {
  const { divisions } = useDivisions();
  const { districts } = useDistricts();
  const {
    cities,
    isLoading: loading,
    createCity,
    updateCity,
    deleteCity
  } = useCities();

  const [selectedDivisionId, setSelectedDivisionId] = useState<string>("");
  const [selectedDistrictId, setSelectedDistrictId] = useState<string>("");

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<City | null>(null);
  const [form, setForm] = useState<{ name: string; districtId: string }>({ name: "", districtId: "" });

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  useEffect(() => {
    // auto-select first district if available
    if (districts?.length && !selectedDistrictId) {
      setSelectedDistrictId(districts[0].id);
    }
  }, [districts, selectedDistrictId]);

  const handleOpenCreate = () => {
    setEditing(null);
    setForm({ name: "", districtId: selectedDistrictId || "" });
    setDialogOpen(true);
  };

  const handleSave = () => {
    if (!form.name.trim() || !form.districtId) {
      return;
    }

    const cityData = { name: form.name.trim(), districtId: form.districtId };

    if (editing) {
      updateCity({ id: editing.id, data: cityData });
    } else {
      createCity(cityData);
    }

    setDialogOpen(false);

    // keep selection consistent
    if (form.districtId !== selectedDistrictId) {
      setSelectedDistrictId(form.districtId);
    }
  };

  const handleDelete = (id: string) => {
    if (!confirm("Delete this city?")) return;
    deleteCity(id);
  };

  const filtered = useMemo(
    () => (cities ?? [])
      .filter((c) => selectedDistrictId ? c.districtId === selectedDistrictId : true)
      .filter((c) => (c.name ?? "").toLowerCase().includes(search.toLowerCase())),
    [cities, search, selectedDistrictId]
  );
  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  // Find district name for display  
  const getDistrictName = (districtId: string) => {
    const district = districts.find(d => d.id === districtId);
    return district?.name || districtId;
  };

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: City, index: number) => (
        <span className="font-medium text-muted-foreground">{index}</span>
      ),
    },
    {
      key: "name",
      header: "City Name",
      render: (city: City) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center">
            <Building2 className="h-4 w-4 text-primary" />
          </div>
          <span className="font-medium">{city.name}</span>
        </div>
      ),
    },
    {
      key: "district",
      header: "District",
      render: (city: City) => (
        <div className="flex items-center gap-2">
          <MapPin className="h-3 w-3 text-muted-foreground" />
          <Badge variant="secondary">{getDistrictName(city.districtId)}</Badge>
        </div>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (city: City) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            onClick={() => {
              setEditing(city);
              setForm({ name: city.name, districtId: city.districtId });
              setDialogOpen(true);
            }}
            className="h-8 w-8 p-0"
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            size="sm"
            variant="ghost"
            onClick={() => handleDelete(city.id)}
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
      {/* District Selection */}
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
        <div>
        </div>
        <Select
          value={selectedDistrictId}
          onValueChange={(val) => {
            setSelectedDistrictId(val);
            setPage(1);
          }}
        >
          <SelectTrigger className="w-[260px]">
            <SelectValue placeholder="Select District" />
          </SelectTrigger>
          <SelectContent>
            {(districts ?? []).map((d) => (
              <SelectItem key={d.id} value={d.id}>
                {d.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      {selectedDistrictId ? (
        <ModernDataTable
          title={`Cities in ${getDistrictName(selectedDistrictId)}`}
          description="Manage cities within the selected district"
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
          onAdd={handleOpenCreate}
          addLabel="Add City"
          emptyMessage="No cities found in this district"
          pageSize={pageSize}
        />
      ) : (
        <div className="flex flex-col items-center justify-center h-64 bg-muted/30 rounded-lg border-2 border-dashed">
          <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
          <h3 className="text-lg font-medium text-muted-foreground">Select a District</h3>
          <p className="text-sm text-muted-foreground mt-1">Choose a district above to view and manage its cities</p>
        </div>
      )}

      {/* Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "Edit City" : "Create City"}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <Label>City Name</Label>
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

            <Label>District</Label>
            <Select
              value={form.districtId}
              onValueChange={(val) => setForm((f) => ({ ...f, districtId: val }))}
            >
              <SelectTrigger><SelectValue placeholder="Select District" /></SelectTrigger>
              <SelectContent>
                {(districts ?? []).map((d) => (
                  <SelectItem key={d.id} value={d.id}>
                    {d.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setDialogOpen(false)}>Cancel</Button>
              <Button onClick={handleSave}>{editing ? "Update" : "Create"}</Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
