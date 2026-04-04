import { useState, useMemo } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { ScrollArea } from '@/components/ui/scroll-area';
import { ModernDataTable } from '@/components/admin/ModernDataTable';
import { ModulePermissionMatrix } from '@/components/admin/ModulePermissionMatrix';
import { LocationAccessSelector } from '@/components/admin/LocationAccessSelector';

import { useRolesWithDetails } from '@/hooks/api/useRolePrivileges';
import { useModules } from '@/hooks/api/useModules';
import { rolePrivilegeApi } from '@/services/api/rolePrivileges';
import { useRolePrivilegeMutations } from '@/hooks/api/useRolePrivileges';
import type { RoleWithDetails } from '@/services/api/rolePrivileges';
import { Shield } from 'lucide-react';

export default function RolePrivilegesPageEnhanced() {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedRole, setSelectedRole] = useState<RoleWithDetails | null>(null);
  const [selectedModules, setSelectedModules] = useState<string[]>([]);
  const [modulePermissions, setModulePermissions] = useState<Record<string, string[]>>({});
  const [selectedAreas, setSelectedAreas] = useState<string[]>([]);
  const [selectedDivisions, setSelectedDivisions] = useState<string[]>([]);
  const [selectedDistricts, setSelectedDistricts] = useState<string[]>([]);
  const [locationAccess, setLocationAccess] = useState({
    divisionIds: [] as string[],
    districtIds: [] as string[],
    areaIds: [] as string[]
  });

  const [loadingPrivileges, setLoadingPrivileges] = useState(false);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);

  const { data: roles = [], isLoading: rolesLoading } = useRolesWithDetails();
  const { modules } = useModules();
  const { assignPrivileges, isAssigning } = useRolePrivilegeMutations();

  const pageSize = 10;

  const resetForm = () => {
    setSelectedModules([]);
    setModulePermissions({});
    setSelectedAreas([]);
    setLocationAccess({
      divisionIds: [],
      districtIds: [],
      areaIds: []
    });
  };


  // Toggle Module Selection
  const handleModuleToggle = (moduleId: string) => {
    setSelectedModules(prev => {
      const updated = prev.includes(moduleId)
        ? prev.filter(id => id !== moduleId)
        : [...prev, moduleId];

      if (!updated.includes(moduleId)) {
        setModulePermissions(prev => {
          const newMap = { ...prev };
          delete newMap[moduleId];
          return newMap;
        });
      }

      return updated;
    });
  };

  // Change Module Permissions
  const handleModulePermissionChange = (moduleId: string, permissions: string[]) => {
    setModulePermissions(prev => ({
      ...prev,
      [moduleId]: permissions,
    }));
  };

  // -------------------------
  // LOAD EXISTING PRIVILEGES
  // -------------------------
  const openDialog = async (role: RoleWithDetails) => {
    setSelectedRole(role);
    setDialogOpen(true);
    setLoadingPrivileges(true);

    try {
      const result = await rolePrivilegeApi.getRolePrivileges(role.id);

      // -----------------------------
      // 1. Convert privileges → modulePermissions
      // -----------------------------
      const permissionMap: Record<string, string[]> = {};

      result.privileges.forEach((p: any) => {
        const moduleName = p.module;
        const action = p.action;

        const moduleObj = modules.find(m => m.name === moduleName);
        if (!moduleObj) return;

        if (!permissionMap[moduleObj.id]) {
          permissionMap[moduleObj.id] = [];
        }

        permissionMap[moduleObj.id].push(action);
      });

      setModulePermissions(permissionMap);
      setSelectedModules(Object.keys(permissionMap));

      // -----------------------------
      // 2. Load Areas + Division + District
      // -----------------------------
      const areaIds = result.locationAssignments.map((x: any) => x.areaId);
      const divisionIds = result.locationAssignments.map((x: any) => x.divisionId);
      const districtIds = result.locationAssignments.map((x: any) => x.districtId);

      setSelectedAreas(areaIds);

      if (result.locationAssignments.length > 0) {
        const first = result.locationAssignments[0];

        setLocationAccess({
          divisionIds,
          districtIds,
          areaIds
        });
      }

    } catch (error) {
      console.error("Failed to load office post privileges:", error);
    }

    setLoadingPrivileges(false);
  };

  // -------------------------
  // SAVE HANDLER
  // -------------------------
  const handleSave = async () => {
    if (!selectedRole) return;

    await assignPrivileges({
      roleId: selectedRole.id,
      modulePermissions: Object.entries(modulePermissions).map(([moduleId, permissions]) => ({
        moduleId,
        permissions,
      })),
      areaAssignments: [
        {
          divisionIds: selectedDivisions,
          districtIds: selectedDistricts,
          areaIds: selectedAreas
        }
      ]
    });

    setDialogOpen(false);
    resetForm();
  };

  // -------------------------
  // TABLE FILTER + PAGINATION
  // -------------------------
  const filtered = useMemo(
    () => roles.filter(r => r.name.toLowerCase().includes(search.toLowerCase())),
    [roles, search]
  );

  const totalPages = Math.ceil(filtered.length / pageSize);
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const columns = [
    {
      key: "name",
      header: "Office Post Name",
      render: (role: RoleWithDetails) => (
        <div className="font-medium">{role.name}</div>
      ),
    },
    {
      key: "privileges",
      header: "Privileges",
      render: (role: RoleWithDetails) => (
        <div className="flex flex-wrap gap-1">
          {role.privilegeCount === 0 ? (
            <Badge variant="outline">No privileges</Badge>
          ) : (
            <>
              <Badge variant="secondary">{role.privilegeCount} permissions</Badge>
              {role.moduleNames.slice(0, 2).map(name => (
                <Badge key={name} variant="outline">{name}</Badge>
              ))}
              {role.moduleNames.length > 2 && (
                <Badge variant="outline">+{role.moduleNames.length - 2} more</Badge>
              )}
            </>
          )}
        </div>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (role: RoleWithDetails) => (
        <Button variant="outline" size="sm" onClick={() => openDialog(role)}>
          Manage Privileges
        </Button>
      ),
    },
  ];

  const selectedModuleObjects = modules.filter(m => selectedModules.includes(m.id));

  return (
    <div className="space-y-6">

      <ModernDataTable
        title="Office Post Privilege Management"
        description="Assign module permissions and location access to office posts"
        data={paginated}
        columns={columns}
        loading={rolesLoading}
        search={search}
        onSearchChange={setSearch}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
        emptyMessage="No roles found"
        pageSize={pageSize}
      />

      {/* DIALOG */}
      <Dialog open={dialogOpen} onOpenChange={(open) => {
        if (!open && !loadingPrivileges) {
          setDialogOpen(open);
          resetForm();
        }
      }}>
        <DialogContent className="max-w-6xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Shield className="h-5 w-5" />
              Manage Office Post Privileges
              {selectedRole && <Badge variant="outline" className="ml-2">{selectedRole.name}</Badge>}
            </DialogTitle>
          </DialogHeader>

          {loadingPrivileges ? (
            <div className="flex items-center justify-center py-12">
              <p>Loading existing privileges...</p>
            </div>
          ) : (
            <div className="space-y-6">

              {/* MODULE ACCESS */}
              <Card>
                <CardHeader>
                  <CardTitle>Module Access</CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="grid grid-cols-2 md:grid-cols-4 gap-3">
                    {modules.map(module => (
                      <div
                        key={module.id}
                        className={`p-3 border rounded-lg cursor-pointer ${selectedModules.includes(module.id)
                          ? "bg-primary/5 border-primary"
                          : "hover:border-border"
                          }`}
                        onClick={() => handleModuleToggle(module.id)}
                      >
                        <div className="font-medium">{module.name}</div>
                        <div className="text-xs text-muted-foreground">{module.category}</div>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>

              {/* PERMISSIONS */}
              {selectedModuleObjects.length > 0 && (
                <Card>
                  <CardHeader>
                    <CardTitle>Module Permissions</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <ScrollArea className="">
                      <div className="space-y-4">
                        {selectedModuleObjects.map(module => (
                          <ModulePermissionMatrix
                            key={module.id}
                            module={module}
                            selectedPermissions={modulePermissions[module.id] || []}
                            onChange={(permissions) =>
                              handleModulePermissionChange(module.id, permissions)
                            }
                            disabled={isAssigning}
                          />
                        ))}
                      </div>
                    </ScrollArea>
                  </CardContent>
                </Card>
              )}

              {/* LOCATION ACCESS */}
              {/* <LocationAccessSelector
                selectedAreas={locationAccess.areaIds}
                selectedDivisions={locationAccess.divisionIds}
                selectedDistricts={locationAccess.districtIds}
                onChange={(areas, divisions, districts) => {
                  setLocationAccess({
                    divisionIds: divisions,
                    districtIds: districts,
                    areaIds: areas
                  });
                  setSelectedDivisions(divisions);
                  setSelectedDistricts(districts);
                  setSelectedAreas(areas);
                }}
              /> */}


              {/* ACTION BUTTONS */}
              <div className="flex justify-end gap-3">
                <Button variant="outline" onClick={() => setDialogOpen(false)}>
                  Cancel
                </Button>
                <Button onClick={handleSave} disabled={isAssigning}>
                  {isAssigning ? "Saving..." : "Save"}
                </Button>
              </div>

            </div>
          )}
        </DialogContent>
      </Dialog>

    </div>
  );
}
