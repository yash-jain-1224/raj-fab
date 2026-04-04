import { Checkbox } from "@/components/ui/checkbox";
import { FormModule } from "@/types/forms";
import { PERMISSION_TYPES } from "@/types/privileges";

interface ModulePermissionTableProps {
  modules: FormModule[];
  selectedModules: string[];
  modulePermissions: Record<string, string[]>;
  onModuleToggle: (moduleId: string) => void;
  onPermissionToggle: (moduleId: string, permission: string) => void;
  onPermissionSelectAll: (permission: string) => void;
  readOnly?: boolean;
}

export function ModulePermissionTable({
  modules,
  selectedModules,
  modulePermissions,
  onModuleToggle,
  onPermissionToggle,
  onPermissionSelectAll,
  readOnly = false,
}: ModulePermissionTableProps) {
  const permissionKeys = Object.keys(PERMISSION_TYPES);

  const isPermissionChecked = (permission: string) =>
    modules.length > 0 &&
    modules.every((m) => (modulePermissions[m.id] || []).includes(permission));

  const isModuleChecked = (moduleId: string) =>
    permissionKeys.every((p) =>
      (modulePermissions[moduleId] || []).includes(p)
    );

  return (
    <div className="overflow-x-auto border rounded-lg">
      <table className="min-w-full text-sm border-collapse">
        <thead className="bg-muted">
          <tr>
            <th className="p-3 border text-left">Application Type</th>

            {permissionKeys.map((p) => (
              <th key={p} className="p-3 border">
                <div className="flex items-center gap-2">
                  <Checkbox
                    checked={isPermissionChecked(p)}
                    disabled={readOnly}
                    onCheckedChange={() => onPermissionSelectAll(p)}
                  />
                  <span className="text-xs font-medium">
                    {PERMISSION_TYPES[p].name}
                  </span>
                </div>
              </th>
            ))}
          </tr>
        </thead>

        <tbody>
          {modules.map((m) => {
            const perms = modulePermissions[m.id] || [];

            return (
              <tr key={m.id} className="hover:bg-muted/50">
                <td className="p-3 border">
                  <div className="flex items-center gap-2">
                    <Checkbox
                      checked={isModuleChecked(m.id)}
                      onCheckedChange={() => onModuleToggle(m.id)}
                      disabled={readOnly}
                    />
                    <span className="font-medium">{m.name}</span>
                  </div>
                </td>

                {permissionKeys.map((p) => (
                  <td key={p} className="p-3 border">
                    <Checkbox
                      checked={perms.includes(p)}
                      disabled={readOnly}
                      onCheckedChange={() => onPermissionToggle(m.id, p)}
                    />
                  </td>
                ))}
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}
