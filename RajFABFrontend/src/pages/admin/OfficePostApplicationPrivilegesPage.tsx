import { useState, useMemo } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
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
  useActs,
  useModuleByRule,
  useOffices,
  useRolesWithPrivileges,
  useRulesByAct,
} from "@/hooks/api";
import { privilegeApi } from "@/services/api/privileges";
import { useToast } from "@/hooks/use-toast";
import { Shield, Building2, Settings, Eye } from "lucide-react";
import type { RoleWithPrivileges } from "@/services/api/roles";
import { Link } from "react-router-dom";
import { Label } from "@/components/ui/label";
import { ModulePermissionTable } from "@/components/admin/ModulePermissionTable";
import { PERMISSION_TYPES } from "@/types/privileges";

type DialogMode = "view" | "manage";

export default function OfficePostApplicationPrivilegesPage() {
  const { toast } = useToast();

  const [selectedOfficeId, setSelectedOfficeId] = useState("");
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogMode, setDialogMode] = useState<DialogMode>("manage");
  const [selectedRole, setSelectedRole] = useState<RoleWithPrivileges | null>(
    null
  );

  const [selectedActId, setSelectedActId] = useState("");
  const [selectedRuleId, setSelectedRuleId] = useState("");

  const [selectedModules, setSelectedModules] = useState<string[]>([]);
  const [modulePermissions, setModulePermissions] = useState<
    Record<string, string[]>
  >({});

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const [loadingPrivileges, setLoadingPrivileges] = useState(false);

  const { offices = [] } = useOffices();
  const { roles = [], isLoading: rolesLoading } =
    useRolesWithPrivileges(selectedOfficeId);
  const { acts = [] } = useActs();
  const { data: rules = [] } = useRulesByAct(selectedActId);
  const { data: modules = [] } = useModuleByRule(selectedRuleId);

  const pageSize = 10;

  const resetForm = () => {
    setSelectedRole(null);
    setSelectedModules([]);
    setModulePermissions({});
    setSelectedActId("");
    setSelectedRuleId("");
  };

  const openDialog = async (role: RoleWithPrivileges) => {
    setSelectedRole(role);
    setLoadingPrivileges(true);

    try {
      const res = await privilegeApi.getRolePrivileges(role.id);

      const modIds = res.modulePermissions.map((m) => m.moduleId);
      const map: Record<string, string[]> = {};
      setSelectedActId(
        res.modulePermissions.length > 0 ? res.modulePermissions[0].actId : ""
      );
      setSelectedRuleId(
        res.modulePermissions.length > 0 ? res.modulePermissions[0].ruleId : ""
      );

      res.modulePermissions.forEach((m) => {
        map[m.moduleId] = m.permissions;
      });
      setSelectedModules(modIds);
      setModulePermissions(map);
    } catch {
      toast({
        title: "Error",
        description: "Failed to load privileges",
        variant: "destructive",
      });
    } finally {
      setLoadingPrivileges(false);
      setDialogOpen(true);
    }
  };

  const handleSave = async () => {
    if (!selectedRole) return;

    await privilegeApi.assignRolePrivileges({
      roleId: selectedRole.id,
      modulePermissions: Object.entries(modulePermissions).map(
        ([moduleId, permissions]) => ({
          moduleId,
          permissions,
        })
      ),
    });

    toast({ title: "Success", description: "Privileges updated successfully" });
    setDialogOpen(false);
    resetForm();
  };

  const handleModuleToggle = (moduleId: string) => {
    if (dialogMode === "view") return;

    setSelectedModules((prev) => {
      const exists = prev.includes(moduleId);

      setModulePermissions((curr) => {
        const copy = { ...curr };
        if (!exists) copy[moduleId] = Object.keys(PERMISSION_TYPES);
        else delete copy[moduleId];
        return copy;
      });

      return exists
        ? prev.filter((id) => id !== moduleId)
        : [...prev, moduleId];
    });
  };

  const handlePermissionToggle = (moduleId: string, permission: string) => {
    setModulePermissions((prev) => {
      const current = prev[moduleId] || [];
      const updated = current.includes(permission)
        ? current.filter((p) => p !== permission)
        : [...current, permission];
      return { ...prev, [moduleId]: updated };
    });
  };

  const handlePermissionSelectAll = (permission: string) => {
    if (dialogMode === "view") return;

    setModulePermissions((prev) => {
      const updated = { ...prev };

      const allHavePermission = modules.every((m) =>
        (updated[m.id] || []).includes(permission)
      );

      modules.forEach((m) => {
        const current = updated[m.id] || [];

        if (allHavePermission) {
          updated[m.id] = current.filter((p) => p !== permission);
        } else {
          if (!current.includes(permission)) {
            updated[m.id] = [...current, permission];
          }
        }
      });

      return updated;
    });

    setSelectedModules(modules.map((m) => m.id));
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
      render: (_: RoleWithPrivileges, i: number) => i,
    },
    {
      key: "postName",
      header: "Post",
      render: (r: RoleWithPrivileges) => (
        <div className="font-medium">
          {r.postName}, {r.officeCityName}
        </div>
      ),
    },
    {
      key: "officeName",
      header: "Office",
      render: (r: RoleWithPrivileges) => r.officeName,
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (r: RoleWithPrivileges) => (
        <div className="flex gap-3 justify-end">
          <Button
            size="sm"
            variant="outline"
            onClick={() => {
              setDialogMode("view");
              openDialog(r);
            }}
          >
            <Eye className="h-4 w-4" /> View
          </Button>
          <Button
            size="sm"
            variant="outline"
            onClick={() => {
              setDialogMode("manage");
              openDialog(r);
            }}
          >
            <Settings className="h-4 w-4" /> Manage
          </Button>
        </div>
      ),
    },
  ];

  const officeComponent = (
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
          {(offices ?? []).map((o) => (
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
          title="Office Post Privilege Management"
          description="Manage privileges for office posts"
          data={paginated}
          columns={columns}
          loading={rolesLoading}
          search={search}
          onSearchChange={(value) => {
            setSearch(value);
            setPage(1);
          }}
          page={page}
          totalPages={totalPages}
          onPageChange={setPage}
          pageSize={pageSize}
          emptyMessage="No office posts found"
          filterComponent={officeComponent}
        />
      ) : (
        <div className="flex flex-col items-center justify-center h-64 bg-muted/30 rounded-lg border-2 border-dashed">
          <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
          {officeComponent}
          <p className="text-sm text-muted-foreground my-1">
            Choose an office above to view and manage application privileges.
          </p>
        </div>
      )}

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-6xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Shield className="h-5 w-5" />
              Manage Post Privileges
              {selectedRole && (
                <Badge className="ml-2">
                  {selectedRole.postName} – {selectedRole.officeName}
                </Badge>
              )}
            </DialogTitle>
          </DialogHeader>

          {dialogMode === "manage" && (
            <Card>
              <CardContent className="grid grid-cols-2 md:grid-cols-3 gap-3 mt-3">
                <div>
                  <Label>Act *</Label>
                  <Select
                    value={selectedActId}
                    onValueChange={setSelectedActId}
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
                </div>
                <div>
                  <Label>Rule *</Label>
                  <Select
                    value={selectedRuleId}
                    disabled={!selectedActId}
                    onValueChange={setSelectedRuleId}
                  >
                    {selectedActId ? (
                      <SelectTrigger>
                        <SelectValue placeholder="Select Rule" />
                      </SelectTrigger>
                    ) : (
                      <SelectTrigger>
                        <SelectValue placeholder="Select Act First" />
                      </SelectTrigger>
                    )}

                    <SelectContent>
                      {(rules ?? []).length === 0 ? (
                        <div className="px-3 py-2 text-sm text-muted-foreground">
                          No rules available
                        </div>
                      ) : (
                        rules.map((r) => (
                          <SelectItem key={r.id} value={r.id}>
                            {r.name}
                          </SelectItem>
                        ))
                      )}
                    </SelectContent>
                  </Select>
                </div>
              </CardContent>
            </Card>
          )}

          {modules.length > 0 ? (
            <ModulePermissionTable
              modules={modules}
              selectedModules={selectedModules}
              modulePermissions={modulePermissions}
              onModuleToggle={handleModuleToggle}
              onPermissionToggle={handlePermissionToggle}
              onPermissionSelectAll={handlePermissionSelectAll}
              readOnly={dialogMode === "view"}
            />
          ) : (
            dialogMode === "view" &&
            `Privilege not assigned to "${selectedRole.postName} – ${selectedRole.officeName}".`
          )}

          <div className="flex justify-end gap-3 pt-4">
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Close
            </Button>
            {dialogMode === "manage" && (
              <Button onClick={handleSave}>Save</Button>
            )}
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
