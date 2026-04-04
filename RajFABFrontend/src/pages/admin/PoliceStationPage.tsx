import { useState, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Edit, Trash2, Shield, MapPin } from "lucide-react";
import useLocationMasters from "@/hooks/useLocationMasters";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import { Badge } from "@/components/ui/badge";
import { usePoliceStations } from "@/hooks/api";
import type { CreatePoliceStationRequest, PoliceStation } from "@/services/api/policeStations";

export default function PoliceStationPage() {
  const { cities, districts, fetchDistricts, fetchCities } = useLocationMasters();
  const { 
    policeStations, 
    isLoading: loading, 
    createPoliceStation, 
    updatePoliceStation, 
    deletePoliceStation,
    isCreating,
    isUpdating 
  } = usePoliceStations();

  // BLOCK numbers + special characters
  const allowedLettersOnly = (value: string) => /^[A-Za-z\s]*$/.test(value);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<PoliceStation | null>(null);
  const [form, setForm] = useState<CreatePoliceStationRequest>({
    name: "",
    address: "",
    districtId: "",
    cityId: "",
  });

  const [loadingLocations, setLoadingLocations] = useState({ districts: false, cities: false });
  const saving = isCreating || isUpdating;

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const filtered = policeStations.filter(ps => 
    ps.name.toLowerCase().includes(search.toLowerCase()) ||
    (ps.address?.toLowerCase() || "").includes(search.toLowerCase()) ||
    (ps.cityName?.toLowerCase() || "").includes(search.toLowerCase()) ||
    (ps.districtName?.toLowerCase() || "").includes(search.toLowerCase())
  );
  const totalPages = Math.ceil(filtered.length / pageSize);
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  useEffect(() => {
    const init = async () => {
      try {
        setLoadingLocations((s) => ({ ...s, districts: true }));
        await fetchDistricts();
        setLoadingLocations((s) => ({ ...s, districts: false }));
      } catch (err) {
        console.error("Initial load failed:", err);
        setLoadingLocations((s) => ({ ...s, districts: false }));
      }
    };
    init();
  }, []);

  const handleSave = async () => {
    if (!form.name?.trim() || !form.districtId || !form.cityId) return;

    const data = { ...form, districtId: String(form.districtId), cityId: String(form.cityId) };

    if (editing) updatePoliceStation({ id: editing.id, data });
    else createPoliceStation(data);

    setDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this police station?")) deletePoliceStation(id);
  };

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: PoliceStation, index: number) => (
        <span className="font-medium text-muted-foreground">{index}</span>
      ),
    },
    {
      key: "name",
      header: "Police Station",
      render: (station: PoliceStation) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center">
            <Shield className="h-4 w-4 text-primary" />
          </div>
          <div>
            <div className="font-medium">{station.name}</div>
            {station.address && (
              <div className="text-sm text-muted-foreground">{station.address}</div>
            )}
          </div>
        </div>
      ),
    },
    {
      key: "location",
      header: "Location",
      render: (station: PoliceStation) => (
        <div className="space-y-1">
          <div className="flex items-center gap-2 text-sm">
            <MapPin className="h-3 w-3 text-muted-foreground" />
            <span>{station.cityName || station.cityId}</span>
          </div>
          <Badge variant="outline" className="text-xs">
            {station.districtName || station.districtId}
          </Badge>
        </div>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (station: PoliceStation) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            onClick={async () => {
              setEditing(station);
              setForm({ 
                name: station.name, 
                address: station.address ?? "", 
                districtId: String(station.districtId), 
                cityId: String(station.cityId) 
              });

              try {
                setLoadingLocations((s) => ({ ...s, cities: true }));
                await fetchCities(String(station.districtId));
                setLoadingLocations((s) => ({ ...s, cities: false }));
              } finally {
                setDialogOpen(true);
              }
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
        title="Police Stations"
        description="Manage police stations across districts and cities"
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
          setForm({ name: "", address: "", districtId: "", cityId: "" });

          if (districts.length === 0) {
            setLoadingLocations((s) => ({ ...s, districts: true }));
            fetchDistricts().finally(() => {
              setLoadingLocations((s) => ({ ...s, districts: false }));
            });
          }
          setDialogOpen(true);
        }}
        addLabel="Add Police Station"
        emptyMessage="No police stations found"
        pageSize={pageSize}
      />

      {/* Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "Edit Police Station" : "Create Police Station"}</DialogTitle>
          </DialogHeader>

          <div className="space-y-4">

            {/* NAME */}
            <div>
              <Label htmlFor="ps-name">Station Name</Label>
              <Input
                id="ps-name"
                value={form.name}
                onChange={(e) => {
                  const v = e.target.value;
                  if (allowedLettersOnly(v)) {
                    setForm((f) => ({ ...f, name: v }));
                  }
                }}
                placeholder="e.g., Kotwali Police Station"
              />
            </div>

            {/* ADDRESS */}
            <div>
              <Label htmlFor="ps-address">Address (Letters Only)</Label>
              <Input
                id="ps-address"
                value={form.address}
                onChange={(e) => {
                  const v = e.target.value;
                  if (allowedLettersOnly(v)) {
                    setForm((f) => ({ ...f, address: v }));
                  }
                }}
                placeholder="e.g., Near Main Road"
              />
            </div>

            {/* DISTRICT */}
            <div>
              <Label htmlFor="ps-district">District</Label>
              <Select
                value={form.districtId}
                onValueChange={async (val) => {
                  setForm((f) => ({ ...f, districtId: val, cityId: "" }));
                  setLoadingLocations((s) => ({ ...s, cities: true }));
                  await fetchCities(val);
                  setLoadingLocations((s) => ({ ...s, cities: false }));
                }}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select District" />
                </SelectTrigger>
                <SelectContent>
                  {districts.map((d) => (
                    <SelectItem key={d.id} value={d.id}>
                      {d.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* CITY */}
            <div>
              <Label htmlFor="ps-city">City</Label>
              <Select
                value={form.cityId}
                onValueChange={(val) => setForm((f) => ({ ...f, cityId: val }))}
                disabled={!form.districtId}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select City" />
                </SelectTrigger>
                <SelectContent>
                  {cities.map((c) => (
                    <SelectItem key={c.id} value={c.id}>
                      {c.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setDialogOpen(false)}>Cancel</Button>
              <Button onClick={handleSave} disabled={saving}>
                {editing ? "Update" : "Create"}
              </Button>
            </div>

          </div>

        </DialogContent>
      </Dialog>
    </div>
  );
}
