export interface ModulePermission {
  id: string;
  moduleId: string;
  moduleName: string;
  permissionName: string;
  permissionCode: string;
  description?: string;
}

export interface RoleModulePermission {
  moduleId: string;
  moduleName: string;
  actId: string;
  ruleId: string;
  permissions: string[];
}

export interface RolePrivilegeData {
  roleId: string;
  postName: string;
  modulePermissions: RoleModulePermission[];
}

export interface AssignRolePrivilegesRequest {
  roleId: string;
  modulePermissions: {
    moduleId: string;
    permissions: string[];
  }[];
}

export const PERMISSION_TYPES = {
  VIEW: { code: "VIEW", name: "View", description: "Can view records" },
  EDIT: { code: "EDIT", name: "Edit", description: "Can modify records" },
  FORWARD: {
    code: "FORWARD",
    name: "Forward",
    description: "Can forward applications",
  },
  FORWARD_TO_APPLIER: {
    code: "FORWARD_TO_APPLIER",
    name: "Forward to Applier",
    description: "Can send back to applicant",
  },
  APPROVE: {
    code: "APPROVE",
    name: "Approve",
    description: "Can approve applications",
  },
  REJECT: {
    code: "REJECT",
    name: "Reject",
    description: "Can reject applications",
  },
  SEND_BACK: {
    code: "SEND_BACK",
    name: "Send Back",
    description: "Can send back to a previous workflow level",
  },
} as const;

export type PermissionCode = keyof typeof PERMISSION_TYPES;
