import { useState, useMemo } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Separator } from '@/components/ui/separator';
import { ModernDataTable } from '@/components/admin/ModernDataTable';
import { ModulePermissionMatrix } from '@/components/admin/ModulePermissionMatrix';
import { LocationAccessSelector } from '@/components/admin/LocationAccessSelector';
import { useUsersWithPrivileges, useModules } from '@/hooks/api';
import { useUserPrivileges, usePrivilegeMutations } from '@/hooks/api/usePrivileges';
import { privilegeApi } from '@/services/api/privileges';
import { useToast } from '@/hooks/use-toast';
import { UserCheck, Shield, MapPin, Settings } from 'lucide-react';
import type { FormModule } from '@/types/forms';
import type { UserWithPrivileges } from '@/services/api/users';

export default function UserPrivilegesPageEnhanced() {
  const [dialogOpen, setDialogOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<UserWithPrivileges | null>(null);
  const [selectedModules, setSelectedModules] = useState<string[]>([]);
  const [modulePermissions, setModulePermissions] = useState<Record<string, string[]>>({});
  const [selectedAreas, setSelectedAreas] = useState<string[]>([]);
  const [search, setSearch] = useState('');
  const [page, setPage] = useState(1);
  const [loadingPrivileges, setLoadingPrivileges] = useState(false);
  
  const { users, isLoading: usersLoading } = useUsersWithPrivileges();
  const { modules, isLoading: modulesLoading } = useModules();
  const { bulkAssignPrivileges, isBulkAssigning } = usePrivilegeMutations();
  const { toast } = useToast();
  
  const pageSize = 10;

  const handleSave = async () => {
    if (!selectedUser) return;

    try {
      await bulkAssignPrivileges({
        userId: selectedUser.id,
        modulePermissions: Object.entries(modulePermissions).map(([moduleId, permissions]) => ({
          moduleId,
          permissions,
        })),
        areaAssignments: [{
          areaIds: selectedAreas,
        }],
      });

      setDialogOpen(false);
      resetForm();
    } catch (error) {
      console.error('Failed to assign privileges:', error);
    }
  };

  const resetForm = () => {
    setSelectedUser(null);
    setSelectedModules([]);
    setModulePermissions({});
    setSelectedAreas([]);
  };

  const handleModuleToggle = (moduleId: string) => {
    setSelectedModules(prev => {
      const newModules = prev.includes(moduleId)
        ? prev.filter(id => id !== moduleId)
        : [...prev, moduleId];
      
      // Clean up permissions for deselected modules
      if (!newModules.includes(moduleId)) {
        setModulePermissions(prev => {
          const updated = { ...prev };
          delete updated[moduleId];
          return updated;
        });
      }
      
      return newModules;
    });
  };

  const handleModulePermissionChange = (moduleId: string, permissions: string[]) => {
    setModulePermissions(prev => ({
      ...prev,
      [moduleId]: permissions,
    }));
  };

  const openDialog = async (user?: UserWithPrivileges) => {
    console.log('[openDialog] Starting, user:', user);
    setSelectedUser(user || null);
    
    if (user) {
      setLoadingPrivileges(true);
      console.log('[openDialog] Loading privileges for userId:', user.id);
      
      try {
        const privileges = await privilegeApi.getUserPrivileges(user.id);
        console.log('[openDialog] ===== RAW API RESPONSE =====');
        console.log('[openDialog] Full response:', JSON.stringify(privileges, null, 2));
        console.log('[openDialog] Type of modulePermissions:', typeof privileges.modulePermissions);
        console.log('[openDialog] Is modulePermissions array?:', Array.isArray(privileges.modulePermissions));
        console.log('[openDialog] Number of modulePermissions:', privileges.modulePermissions.length);
        
        // Pre-populate selected modules
        const moduleIds = privileges.modulePermissions.map(mp => mp.moduleId);
        console.log('[openDialog] ===== EXTRACTED MODULE IDS =====');
        console.log('[openDialog] Module IDs:', moduleIds);
        setSelectedModules(moduleIds);
        
        // Pre-populate permissions per module
        const permissionMap: Record<string, string[]> = {};
        console.log('[openDialog] ===== PROCESSING PERMISSIONS =====');
        privileges.modulePermissions.forEach((mp, index) => {
          console.log(`[openDialog] Processing module ${index}:`, {
            moduleId: mp.moduleId,
            moduleName: mp.moduleName,
            permissions: mp.permissions,
            permissionsType: typeof mp.permissions,
            isArray: Array.isArray(mp.permissions),
            permissionsValue: JSON.stringify(mp.permissions)
          });
          permissionMap[mp.moduleId] = mp.permissions;
        });
        console.log('[openDialog] ===== FINAL PERMISSION MAP =====');
        console.log('[openDialog] Permission map:', JSON.stringify(permissionMap, null, 2));
        setModulePermissions(permissionMap);
        
        // Pre-populate area assignments
        const areaIds = privileges.areaAssignments.map(aa => aa.areaId);
        console.log('[openDialog] ===== AREA ASSIGNMENTS =====');
        console.log('[openDialog] Area IDs:', areaIds);
        setSelectedAreas(areaIds);
        
        console.log('[openDialog] ✅ All state set successfully');
      } catch (error) {
        console.error('[openDialog] ❌ Error loading privileges:', error);
        toast({
          title: "Error",
          description: "Failed to load existing privileges. You can still assign new ones.",
          variant: "destructive",
        });
      } finally {
        setLoadingPrivileges(false);
        console.log('[openDialog] Loading complete, loadingPrivileges set to false');
      }
    }
    
    console.log('[openDialog] Opening dialog now');
    setDialogOpen(true);
  };

  const filtered = useMemo(
    () => (users ?? []).filter(user => 
      `${user.username} ${user.fullName} ${user.email}`.toLowerCase().includes(search.toLowerCase())
    ),
    [users, search]
  );

  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: UserWithPrivileges, index: number) => (
        <span className="font-medium text-muted-foreground">{index}</span>
      ),
    },
    {
      key: "user",
      header: "User",
      render: (user: UserWithPrivileges) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center">
            <UserCheck className="h-4 w-4 text-primary" />
          </div>
          <div>
            <div className="font-medium">{user.fullName}</div>
            <div className="text-sm text-muted-foreground">@{user.username}</div>
          </div>
        </div>
      ),
    },
    {
      key: "contact",
      header: "Contact",
      render: (user: UserWithPrivileges) => (
        <div>
          <div className="text-sm">{user.email}</div>
          <div className="text-sm text-muted-foreground">{user.mobile}</div>
        </div>
      ),
    },
    {
      key: "privileges",
      header: "Current Privileges",
      render: (user: UserWithPrivileges) => (
        <div className="flex flex-wrap gap-1">
          {user.privilegeCount === 0 ? (
            <Badge variant="outline" className="text-xs text-muted-foreground">
              No privileges
            </Badge>
          ) : (
            <>
              <Badge variant="secondary" className="text-xs">
                {user.privilegeCount} {user.privilegeCount === 1 ? 'permission' : 'permissions'}
              </Badge>
              {user.moduleNames.slice(0, 2).map(name => (
                <Badge key={name} variant="outline" className="text-xs">
                  {name}
                </Badge>
              ))}
              {user.moduleNames.length > 2 && (
                <Badge variant="outline" className="text-xs">
                  +{user.moduleNames.length - 2} more
                </Badge>
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
      render: (user: UserWithPrivileges) => (
        <Button
          size="sm"
          variant="outline"
          onClick={() => openDialog(user)}
          className="gap-2"
        >
          <Settings className="h-4 w-4" />
          Manage Privileges
        </Button>
      ),
    },
  ];

  const selectedModuleObjects = modules.filter(m => selectedModules.includes(m.id));

  return (
    <div className="space-y-6">
      <ModernDataTable
        title="User Privilege Management"
        description="Assign module permissions and location access to users"
        data={paginated}
        columns={columns}
        loading={usersLoading}
        search={search}
        onSearchChange={(value) => {
          setSearch(value);
          setPage(1);
        }}
        page={page}
        totalPages={totalPages}
        onPageChange={setPage}
        emptyMessage="No users found"
        pageSize={pageSize}
      />

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
              Manage User Privileges
              {selectedUser && (
                <Badge variant="outline" className="ml-2">
                  {selectedUser.fullName}
                </Badge>
              )}
            </DialogTitle>
          </DialogHeader>

          {loadingPrivileges ? (
            <div className="flex items-center justify-center py-12">
              <div className="text-center space-y-3">
                <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto"></div>
                <p className="text-sm text-muted-foreground">Loading existing privileges...</p>
              </div>
            </div>
          ) : (
            <div className="space-y-6">
            {/* Module Selection */}
            <Card>
              <CardHeader>
                <CardTitle className="text-base">Module Access</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
                  {modules.map((module) => (
                    <div
                      key={module.id}
                      className={`p-3 border rounded-lg cursor-pointer transition-all ${
                        selectedModules.includes(module.id)
                          ? 'border-primary bg-primary/5'
                          : 'border-border hover:border-border/80'
                      }`}
                      onClick={() => handleModuleToggle(module.id)}
                    >
                      <div className="font-medium text-sm">{module.name}</div>
                      <div className="text-xs text-muted-foreground mt-1">{module.category}</div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            {/* Module Permissions */}
            {selectedModuleObjects.length > 0 && (
              <Card>
                <CardHeader>
                  <CardTitle className="text-base">Module Permissions</CardTitle>
                </CardHeader>
                <CardContent>
                  <ScrollArea className="max-h-[60vh] overflow-y-auto">
                    <div className="space-y-4">
                      {selectedModuleObjects.map((module) => {
                        const perms = modulePermissions[module.id] || [];
                        console.log(`[Render] ModulePermissionMatrix for ${module.name}:`, {
                          moduleId: module.id,
                          selectedPermissions: perms,
                          permType: typeof perms,
                          isArray: Array.isArray(perms),
                          length: perms.length,
                          values: JSON.stringify(perms)
                        });
                        
                        return (
                          <ModulePermissionMatrix
                            key={module.id}
                            module={module}
                            selectedPermissions={perms}
                            onChange={(permissions) => handleModulePermissionChange(module.id, permissions)}
                            disabled={isBulkAssigning || loadingPrivileges}
                          />
                        );
                      })}
                    </div>
                  </ScrollArea>
                </CardContent>
              </Card>
            )}

            {/* Location Access */}
            <LocationAccessSelector
              selectedAreas={selectedAreas}
              onChange={setSelectedAreas}
              title="Geographic Access"
              allowMultipleAreas={true}
              disabled={isBulkAssigning || loadingPrivileges}
            />

            {/* Action Buttons */}
            <div className="flex justify-end gap-3 pt-4 border-t">
              <Button
                variant="outline"
                onClick={() => setDialogOpen(false)}
                disabled={isBulkAssigning || loadingPrivileges}
              >
                Cancel
              </Button>
              <Button
                onClick={handleSave}
                disabled={isBulkAssigning || loadingPrivileges || selectedModules.length === 0}
              >
                {isBulkAssigning ? 'Assigning...' : 'Assign Privileges'}
              </Button>
            </div>
          </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}