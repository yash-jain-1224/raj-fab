import { useMemo, useState } from "react";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import {
  useOffices,
  useRolesWithPrivileges,
  useFactoryCategories,
} from "@/hooks/api";
import { useRoleInspectionPrivileges } from "@/hooks/api/useRoleInspectionPrivileges";
import { Shield, Building2, Settings, Eye, Layers } from "lucide-react";
import type { RoleWithPrivileges } from "@/services/api/roles";
import { Label } from "@/components/ui/label";
import { Link } from "react-router-dom";

type DialogMode = "view" | "manage";

export default function OfficePostInspectionPrivilegesPage() {
  const [selectedOfficeId, setSelectedOfficeId] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogMode, setDialogMode] = useState<DialogMode>("manage");
  const [selectedRole, setSelectedRole] = useState<RoleWithPrivileges | null>(
    null
  );
  const [selectedFactoryCategoryId, setSelectedFactoryCategoryId] =
    useState("");
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const { offices = [] } = useOffices();
  const { roles = [], isLoading: rolesLoading } =
    useRolesWithPrivileges(selectedOfficeId);
  const { factoryCategories = [] } = useFactoryCategories();

  const {
    inspectionPrivileges,
    assignFactoryCategory,
    removeFactoryCategory,
    isLoading: loadingPrivileges,
  } = useRoleInspectionPrivileges(selectedRole?.id);

  const openDialog = (role: RoleWithPrivileges, mode: DialogMode) => {
    setSelectedRole(role);
    setSelectedFactoryCategoryId("");
    setDialogMode(mode);
    setDialogOpen(true);
  };

  const filtered = useMemo(
    () =>
      roles.filter((r) =>
        `${r.postName} ${r.officeName}`
          .toLowerCase()
          .includes(search.toLowerCase())
      ),
    [roles, search]
  );

  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const columns = [
    {
      key: "index",
      header: "#",
      render: (_: RoleWithPrivileges, i: number) => (page - 1) * pageSize + i,
    },
    {
      key: "post",
      header: "Post",
      render: (r: RoleWithPrivileges) => (
        <div className="font-medium">
          {r.postName}, {r.officeCityName}
        </div>
      ),
    },
    {
      key: "office",
      header: "Office",
      render: (r: RoleWithPrivileges) => r.officeName,
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (r: RoleWithPrivileges) => (
        <div className="flex gap-2 justify-end">
          <Button
            size="sm"
            variant="outline"
            onClick={() => openDialog(r, "view")}
          >
            <Eye className="h-4 w-4 mr-1" /> View
          </Button>
          <Button
            size="sm"
            variant="outline"
            onClick={() => openDialog(r, "manage")}
          >
            <Settings className="h-4 w-4 mr-1" /> Manage
          </Button>
        </div>
      ),
    },
  ];

  const officeSelector = (
    <div className="my-3">
      <Label>Select an Office</Label>
      <Select
        value={selectedOfficeId}
        onValueChange={(val) => {
          setSelectedOfficeId(val);
          setPage(1);
        }}
      >
        <SelectTrigger className="w-72">
          <SelectValue placeholder="Select Office" />
        </SelectTrigger>
        <SelectContent>
          {offices.map((o) => (
            <SelectItem key={o.id} value={o.id}>
              {o.name}
            </SelectItem>
          ))}
        </SelectContent>
      </Select>
    </div>
  );

  return (
    <div className="space-y-6">
      {offices.length === 0 ? (
        <div className="flex flex-col items-center justify-center h-96 bg-muted/30 rounded-lg border-2 border-dashed">
          <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
          <h3 className="text-lg font-medium text-muted-foreground">
            No offices found
          </h3>
          <Link to="/admin/officemanagement">
            <Button className="mt-4">Go to Office Management</Button>
          </Link>
        </div>
      ) : selectedOfficeId ? (
        <ModernDataTable
          title="Office Post Inspection Privileges"
          description="Assign factory categories to posts for inspection"
          data={paginated}
          columns={columns}
          loading={rolesLoading}
          search={search}
          onSearchChange={(v) => {
            setSearch(v);
            setPage(1);
          }}
          page={page}
          totalPages={totalPages}
          onPageChange={setPage}
          pageSize={pageSize}
          emptyMessage="No posts found"
          filterComponent={officeSelector}
        />
      ) : (
        <div className="flex flex-col items-center justify-center h-64 bg-muted/30 rounded-lg border-2 border-dashed">
          <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
          {officeSelector}
          <p className="text-sm text-muted-foreground mt-2">
            Select an office to manage inspection privileges
          </p>
        </div>
      )}

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-3xl">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Shield className="h-5 w-5" />
              {dialogMode === "manage"
                ? "Manage Inspection Privileges"
                : "View Inspection Privileges"}
              {selectedRole && (
                <Badge className="ml-2">
                  {selectedRole.postName} – {selectedRole.officeName}
                </Badge>
              )}
            </DialogTitle>
          </DialogHeader>

          {dialogMode === "manage" && (
            <div className="space-y-4">
              <div>
                <Label>Factory Category</Label>
                <Select
                  value={selectedFactoryCategoryId}
                  onValueChange={setSelectedFactoryCategoryId}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Select Factory Category" />
                  </SelectTrigger>
                  <SelectContent>
                    {factoryCategories.map((fc) => (
                      <SelectItem key={fc.id} value={fc.id}>
                        {fc.name}
                      </SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>

              <Button
                disabled={!selectedFactoryCategoryId}
                onClick={() =>
                  assignFactoryCategory({
                    roleId: selectedRole!.id,
                    factoryCategoryId: selectedFactoryCategoryId,
                  })
                }
              >
                Assign Category
              </Button>
            </div>
          )}

          <div className="border-t pt-4 mt-4 space-y-2">
            <Label>Assigned Factory Categories</Label>

            {loadingPrivileges ? (
              <p className="text-sm text-muted-foreground">Loading…</p>
            ) : inspectionPrivileges.length === 0 ? (
              <p className="text-sm text-muted-foreground">
                No factory categories assigned
              </p>
            ) : (
              inspectionPrivileges.map((p) => (
                <div
                  key={p.id}
                  className="flex justify-between items-center border rounded px-3 py-2"
                >
                  <div className="flex items-center gap-2">
                    <Layers className="h-4 w-4 text-primary" />
                    <span>{p.factoryCategoryName}</span>
                  </div>

                  {dialogMode === "manage" && (
                    <Button
                      size="sm"
                      variant="destructive"
                      onClick={() => removeFactoryCategory(p.id)}
                    >
                      Remove
                    </Button>
                  )}
                </div>
              ))
            )}
          </div>

          <div className="flex justify-end pt-4">
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Close
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
