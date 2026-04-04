import { useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Plus, Edit } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import { useRolesWithDetails, useRolePrivileges, useRolePrivilegeMutations } from "@/hooks/api/useRolePrivileges";
import { useModules } from "@/hooks/api/useModules";
import { LocationAccessSelector } from "@/components/admin/LocationAccessSelector";
import { rolePrivilegeApi } from "@/services/api/rolePrivileges";
import type { RoleWithDetails, AssignRolePrivilegeRequest } from "@/services/api/rolePrivileges";

export default function RolePrivilegesPage() {
  const { data: roles = [], isLoading } = useRolesWithDetails();
  const { modules } = useModules();
  const { assignPrivileges, isAssigning } = useRolePrivilegeMutations();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedRole, setSelectedRole] = useState<RoleWithDetails | null>(null);
  const [selectedPrivileges, setSelectedPrivileges] = useState<string[]>([]);
  const [selectedModuleIds, setSelectedModuleIds] = useState<string[]>([]);
  const [selectedAreas, setSelectedAreas] = useState<string[]>([]);
  const [locationData, setLocationData] = useState({
    divisionId: "",
    districtId: "",
  });

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const { data: rolePrivileges, isLoading: loadingPrivileges } = useRolePrivileges(
    selectedRole?.id || ""
  );

  const openDialog = async (role: RoleWithDetails) => {
    setSelectedRole(role);
    setDialogOpen(true);
    
    // Reset form
    setSelectedPrivileges([]);
    setSelectedModuleIds([]);
    setSelectedAreas([]);
    setLocationData({ divisionId: "", districtId: "" });

    // Load existing privileges if any
    try {
      const privileges = await rolePrivilegeApi.getRolePrivileges(role.id);
      if (privileges) {
        setSelectedPrivileges(privileges.privileges || []);
        
        const locationAssignments = privileges.locationAssignments || [];
        if (locationAssignments.length > 0) {
          const first = locationAssignments[0];
          setLocationData({
            divisionId: first.divisionId || "",
            districtId: first.districtId || "",
          });
          
          const moduleIds = locationAssignments
            .map((la: any) => la.moduleId)
            .filter((id: string) => id);
          setSelectedModuleIds(Array.from(new Set(moduleIds)));
          
          const areaIds = locationAssignments
            .map((la: any) => la.areaId)
            .filter((id: string) => id);
          setSelectedAreas(areaIds);
        }
      }
    } catch (error) {
      console.error("Failed to load role privileges:", error);
    }
  };

  const handleSave = () => {
    if (!selectedRole) return;

    const data: AssignRolePrivilegeRequest = {
      roleId: selectedRole.id,
      modulePermissions: selectedModuleIds.map(moduleId => ({
        moduleId,
        permissions: selectedPrivileges
      })),
      areaAssignments: [{
        areaIds: selectedAreas,
        divisionId: locationData.divisionId || undefined,
        districtId: locationData.districtId || undefined,
      }],
    };

    assignPrivileges(data);
    setDialogOpen(false);
  };

  const togglePrivilege = (moduleName: string) => {
    setSelectedPrivileges((prev) =>
      prev.includes(moduleName)
        ? prev.filter((p) => p !== moduleName)
        : [...prev, moduleName]
    );
  };

  const toggleModule = (moduleId: string) => {
    setSelectedModuleIds((prev) =>
      prev.includes(moduleId)
        ? prev.filter((id) => id !== moduleId)
        : [...prev, moduleId]
    );
  };

  const filteredRoles = useMemo(() => {
    return roles.filter((r) =>
      r.name.toLowerCase().includes(search.toLowerCase())
    );
  }, [roles, search]);

  const totalPages = Math.ceil(filteredRoles.length / pageSize);

  const paginatedRoles = useMemo(() => {
    const start = (page - 1) * pageSize;
    return filteredRoles.slice(start, start + pageSize);
  }, [filteredRoles, page]);

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: RoleWithDetails, index: number) => (
        <span className="font-medium text-muted-foreground">{index + 1}</span>
      ),
    },
    {
      key: "name",
      header: "Role Name",
      render: (role: RoleWithDetails) => (
        <div>
          <div className="font-medium">{role.name}</div>
          {role.officeName && (
            <div className="text-sm text-muted-foreground">Office: {role.officeName}</div>
          )}
        </div>
      ),
    },
    {
      key: "privileges",
      header: "Privileges",
      render: (role: RoleWithDetails) => (
        <div>
          <div className="font-medium">{role.privilegeCount} privileges</div>
          {role.moduleNames.length > 0 && (
            <div className="text-sm text-muted-foreground">
              {role.moduleNames.join(", ")}
            </div>
          )}
        </div>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (role: RoleWithDetails) => (
        <Button
          size="sm"
          variant="ghost"
          onClick={() => openDialog(role)}
          className="h-8 w-8 p-0"
        >
          <Edit className="h-4 w-4" />
        </Button>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <ModernDataTable
        title="Role Privileges"
        description="Assign privileges to roles"
        data={paginatedRoles}
        columns={columns}
        loading={isLoading}
        search={search}
        onSearchChange={(value) => {
          setSearch(value);
          setPage(1);
        }}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
        emptyMessage="No roles found"
        pageSize={pageSize}
      />

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-3xl max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              Assign Privileges - {selectedRole?.name}
            </DialogTitle>
          </DialogHeader>

          <div className="space-y-6">
            {/* Modules */}
            <div>
              <h3 className="font-medium mb-3">Select Modules</h3>
              <div className="grid grid-cols-2 gap-2">
                {modules.map((module) => (
                  <label
                    key={module.id}
                    className="flex items-center gap-2 p-2 border rounded cursor-pointer hover:bg-muted"
                  >
                    <input
                      type="checkbox"
                      checked={selectedModuleIds.includes(module.id)}
                      onChange={() => toggleModule(module.id)}
                    />
                    <span>{module.name}</span>
                  </label>
                ))}
              </div>
            </div>

            {/* Privileges */}
            <div>
              <h3 className="font-medium mb-3">Select Privileges</h3>
              <div className="grid grid-cols-2 gap-2">
                {modules.map((module) => (
                  <label
                    key={module.name}
                    className="flex items-center gap-2 p-2 border rounded cursor-pointer hover:bg-muted"
                  >
                    <input
                      type="checkbox"
                      checked={selectedPrivileges.includes(module.name)}
                      onChange={() => togglePrivilege(module.name)}
                    />
                    <span>{module.name}</span>
                  </label>
                ))}
              </div>
            </div>

            {/* Location Selector */}
            <LocationAccessSelector
              selectedAreas={selectedAreas}
              onChange={setSelectedAreas}
              title="Geographic Access"
              allowMultipleAreas={true}
              disabled={isAssigning || loadingPrivileges}
            />

            {/* Actions */}
            <div className="flex justify-end gap-2">
              <Button
                variant="outline"
                onClick={() => setDialogOpen(false)}
                disabled={isAssigning || loadingPrivileges}
              >
                Cancel
              </Button>
              <Button
                onClick={handleSave}
                disabled={isAssigning || loadingPrivileges || selectedModuleIds.length === 0}
              >
                {isAssigning ? "Assigning..." : "Assign Privileges"}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
