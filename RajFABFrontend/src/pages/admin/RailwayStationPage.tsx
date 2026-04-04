import { useState, useEffect, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Edit, Trash2, Train, MapPin } from "lucide-react";
import { useLocationMasters } from "@/hooks/useLocationMasters";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import { Badge } from "@/components/ui/badge";
import { useRailwayStations } from "@/hooks/api";
import type { CreateRailwayStationRequest, RailwayStation } from "@/services/api/railwayStations";
import { API_BASE_URL } from "@/services/api/base";

export default function RailwayStationPage() {
  const { cities, fetchCities } = useLocationMasters();
  const { 
    railwayStations, 
    isLoading: loading, 
    createRailwayStation, 
    updateRailwayStation, 
    deleteRailwayStation,
    isCreating,
    isUpdating 
  } = useRailwayStations();

  const [districtOptions, setDistrictOptions] = useState<any[]>([]);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<RailwayStation | null>(null);

  // VALIDATION
  const lettersOnly = (v: string) => /^[A-Za-z\s]*$/.test(v);       // name
  const alphaNumericOnly = (v: string) => /^[A-Za-z0-9]*$/.test(v); // code

  const [form, setForm] = useState<CreateRailwayStationRequest>({
    name: "",
    code: "",
    districtId: "",
    cityId: "",
  });

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  useEffect(() => {
    fetchDistricts();
  }, []);

  const fetchDistricts = async () => {
    try {
      const res = await fetch(`${API_BASE_URL}/district`);
      const json = await res.json();
      const list: any[] = Array.isArray(json) ? json : (json?.data ?? []);
      setDistrictOptions(Array.isArray(list) ? list : []);
    } catch {
      setDistrictOptions([]);
    }
  };

  const handleSave = async () => {
    const data = { ...form };
    if (editing) updateRailwayStation({ id: editing.id, data });
    else createRailwayStation(data);
    setDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this railway station?")) deleteRailwayStation(id);
  };

  const filtered = useMemo(
    () =>
      (railwayStations ?? []).filter((r) =>
        `${r?.name ?? ""} ${r?.code ?? ""} ${r?.cityName ?? ""} ${r?.districtName ?? ""}`
          .toLowerCase()
          .includes(search.toLowerCase())
      ),
    [railwayStations, search]
  );

  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  if (loading) return <div className="flex items-center justify-center h-64">Loading...</div>;

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: RailwayStation, index: number) => (
        <span className="font-medium text-muted-foreground">{index}</span>
      ),
    },
    {
      key: "name",
      header: "Station Name",
      render: (station: RailwayStation) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center">
            <Train className="h-4 w-4 text-primary" />
          </div>
          <div>
            <div className="font-medium">{station.name}</div>
            {station.code && (
              <div className="text-xs text-muted-foreground">{station.code}</div>
            )}
          </div>
        </div>
      ),
    },
    {
      key: "location",
      header: "Location",
      render: (station: RailwayStation) => (
        <div className="space-y-1">
          <div className="flex items-center gap-2 text-sm">
            <MapPin className="h-3 w-3 text-muted-foreground" />
            <span>{station.cityName || "—"}</span>
          </div>
          <Badge variant="outline" className="text-xs">
            {station.districtName || "—"}
          </Badge>
        </div>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (station: RailwayStation) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            onClick={() => {
              setEditing(station);
              setForm({
                name: station.name,
                code: station.code || "",
                districtId: station.districtId,
                cityId: station.cityId,
              });
              fetchCities(station.districtId);
              setDialogOpen(true);
            }}
            className="h-8 w-8 p-0"
          >
            <Edit className="h-4 w-4" />
          </Button>

          <Button 
            size="sm" 
            variant="ghost" 
            onClick={() => handleDelete(station.id)}
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
        title="Railway Stations"
        description="Manage railway stations"
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
          setForm({ name: "", code: "", districtId: "", cityId: "" });
          if (districtOptions.length === 0) fetchDistricts();
          setDialogOpen(true);
        }}
        addLabel="Add Railway Station"
        emptyMessage="No railway stations found"
        pageSize={pageSize}
      />

      {/* Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "Edit Railway Station" : "Create Railway Station"}</DialogTitle>
          </DialogHeader>

          <div className="space-y-4">

            {/* NAME */}
            <Label>Name</Label>
            <Input
              value={form.name}
              onChange={(e) => {
                const v = e.target.value;
                if (lettersOnly(v)) {
                  setForm({ ...form, name: v });
                }
              }}
              placeholder="e.g., Jaipur Junction"
            />

            {/* CODE (letters + numbers) */}
            <Label>Code</Label>
            <Input
              value={form.code}
              onChange={(e) => {
                const v = e.target.value;
                if (alphaNumericOnly(v)) {
                  setForm({ ...form, code: v });
                }
              }}
              placeholder="e.g., JP, JP1"
            />

            {/* DISTRICT */}
            <Label>District</Label>
            <Select
              value={form.districtId}
              onValueChange={(val) => {
                setForm({ ...form, districtId: val, cityId: "" });
                fetchCities(val);
              }}
            >
              <SelectTrigger><SelectValue placeholder="Select District" /></SelectTrigger>
              <SelectContent>
                {districtOptions.map((d) => (
                  <SelectItem key={d.id} value={d.id}>
                    {d.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            {/* CITY */}
            <Label>City / Town</Label>
            <Select
              value={form.cityId}
              onValueChange={(val) => setForm({ ...form, cityId: val })}
              disabled={!form.districtId}
            >
              <SelectTrigger><SelectValue placeholder="Select City/Town" /></SelectTrigger>
              <SelectContent>
                {cities.map((c) => (
                  <SelectItem key={c.id} value={c.id}>
                    {c.name}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleSave} disabled={isCreating || isUpdating}>
                {editing ? "Update" : "Create"}
              </Button>
            </div>

          </div>

        </DialogContent>
      </Dialog>
    </div>
  );
}
