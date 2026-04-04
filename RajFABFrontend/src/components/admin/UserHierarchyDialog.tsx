// components/UserHierarchyDialog.tsx
import React, { useEffect, useState } from "react";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { userApi, User } from "@/services/api/users";
import {
  userHierarchyApi,
  UserHierarchy,
  CreateUserHierarchyRequest,
} from "@/services/api/userHierarchy";

interface Props {
  open: boolean;
  onClose: () => void;
  editing?: UserHierarchy | null;
  onSaved?: (saved: UserHierarchy) => void;
}

const UserHierarchyDialog: React.FC<Props> = ({
  open,
  onClose,
  editing = null,
  onSaved,
}) => {
  const [users, setUsers] = useState<User[]>([]);
  const [userId, setUserId] = useState<string>("");
  const [reportsToId, setReportsToId] = useState<string | null>(null);
  const [emergencyReportToId, setEmergencyReportToId] = useState<string | null>(
    null
  );
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  // load users when dialog opens
  useEffect(() => {
    if (!open) return;
    setError(null);
    userApi
      .getAll()
      .then((list) => setUsers(list))
      .catch((err) => {
        console.error("Failed to load users", err);
        setError("Failed to load users");
      });
  }, [open]);

  // populate fields when editing
  useEffect(() => {
    if (editing) {
      setUserId(editing.userId);
      setReportsToId(editing.reportsToId ?? null);
      setEmergencyReportToId(editing.emergencyReportToId ?? null);
    } else if (open) {
      // reset when opening for create
      setUserId("");
      setReportsToId(null);
      setEmergencyReportToId(null);
      setError(null);
    }
  }, [editing, open]);

  const validate = (): string | null => {
    if (!userId) return "Please select a user.";
    if (reportsToId && reportsToId === userId)
      return "A user cannot report to themself (Reports To).";
    if (emergencyReportToId && emergencyReportToId === userId)
      return "A user cannot be emergency contact of themself.";
    if (reportsToId && emergencyReportToId && reportsToId === emergencyReportToId)
      return "Manager and Emergency contact cannot be the same user.";
    return null;
  };

  const handleSave = async () => {
    setError(null);
    const v = validate();
    if (v) {
      setError(v);
      return;
    }

    const payload: CreateUserHierarchyRequest = {
      userId,
      reportsToId: reportsToId ?? null,
      emergencyReportToId: emergencyReportToId ?? null,
    };

    try {
      setLoading(true);
      let saved: UserHierarchy;
      if (editing && editing.id) {
        saved = await userHierarchyApi.update(editing.id, payload);
      } else {
        saved = await userHierarchyApi.create(payload);
      }
      onSaved?.(saved);
      onClose();
    } catch (err: any) {
      console.error(err);
      setError(err?.message || "Failed to save user hierarchy");
    } finally {
      setLoading(false);
    }
  };

  return (
    <Dialog open={open} onOpenChange={(val) => { if (!val && !loading) onClose(); }}>
      <DialogContent className="max-w-lg w-full">
        <DialogHeader>
          <DialogTitle>
            {editing ? "Edit User Hierarchy" : "Create User Hierarchy"}
          </DialogTitle>
        </DialogHeader>

        <div className="mt-2">
          {error && <div className="mb-3 text-sm text-destructive">{error}</div>}

          <div className="space-y-4">
            <div>
              <label className="block text-sm font-medium mb-1">User</label>
              <select
                value={userId}
                onChange={(e) => setUserId(e.target.value)}
                className="w-full border rounded p-2"
                disabled={loading}
              >
                <option value="">-- Select user --</option>
                {users.map((u) => (
                  <option key={u.id} value={u.id}>
                    {u.username}
                    {u.fullName ? ` (${u.fullName})` : ""}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium mb-1">
                Reports To (Manager)
              </label>
              <select
                value={reportsToId ?? ""}
                onChange={(e) => setReportsToId(e.target.value || null)}
                className="w-full border rounded p-2"
                disabled={loading}
              >
                <option value="">-- None --</option>
                {users.map((u) => (
                  <option key={u.id} value={u.id}>
                    {u.username}
                    {u.fullName ? ` (${u.fullName})` : ""}
                  </option>
                ))}
              </select>
            </div>

            <div>
              <label className="block text-sm font-medium mb-1">
                Emergency Report To
              </label>
              <select
                value={emergencyReportToId ?? ""}
                onChange={(e) => setEmergencyReportToId(e.target.value || null)}
                className="w-full border rounded p-2"
                disabled={loading}
              >
                <option value="">-- None --</option>
                {users.map((u) => (
                  <option key={u.id} value={u.id}>
                    {u.username}
                    {u.fullName ? ` (${u.fullName})` : ""}
                  </option>
                ))}
              </select>
            </div>
          </div>

          <div className="flex justify-end gap-2 mt-6">
            <button
              className="px-4 py-2 rounded border bg-white"
              onClick={() => !loading && onClose()}
              disabled={loading}
            >
              Cancel
            </button>

            <button
              className="px-4 py-2 rounded bg-primary text-white"
              onClick={handleSave}
              disabled={loading}
            >
              {loading ? (editing ? "Updating..." : "Saving...") : editing ? "Update" : "Save"}
            </button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default UserHierarchyDialog;
