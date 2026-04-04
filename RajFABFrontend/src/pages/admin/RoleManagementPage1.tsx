import { useEffect, useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import {
  Accordion,
  AccordionContent,
  AccordionItem,
  AccordionTrigger,
} from "@/components/ui/accordion";
import { Edit, Trash2, Plus } from "lucide-react";

import { useRoles } from "@/hooks/api";
import { useOffices } from "@/hooks/api/useOffices";
import type { CreateRoleRequest, Role } from "@/services/api/roles";

export default function RoleManagementAccordion() {
  const { roles, createRole, updateRole, deleteRole, isCreating, isUpdating } = useRoles();
  const { offices } = useOffices();

  const lettersOnly = (v: string) => /^[A-Za-z\s]*$/.test(v);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Role | null>(null);

  const [form, setForm] = useState({ name: "", officeId: "" });

  const [search, setSearch] = useState("");
  const [activeOffice, setActiveOffice] = useState<string | undefined>(undefined);

  const filteredRoles = useMemo(() => {
    return (roles ?? []).filter(
      (r) =>
        r.officeId === activeOffice &&
        r.name.toLowerCase().includes(search.toLowerCase())
    );
  }, [roles, activeOffice, search]);

  const openCreateDialog = (officeId: string) => {
    setEditing(null);
    setForm({ name: "", officeId });
    setDialogOpen(true);
  };

  const openEditDialog = (role: Role) => {
    setEditing(role);
    setForm({ name: role.name, officeId: role.officeId! });
    setDialogOpen(true);
  };

  const handleSave = async () => {
    const data: CreateRoleRequest = {
      name: form.name.trim(),
      officeId: form.officeId,
    };

    if (!data.name || !data.officeId) return;

    if (editing) updateRole({ id: editing.id, data });
    else createRole(data);

    setDialogOpen(false);
  };

  return (
    <div className="p-4 space-y-6">
      <h2 className="text-xl font-semibold">Office & Post Management</h2>

      {/* ACCORDION */}
      <Accordion
        type="single"
        collapsible
        onValueChange={(val) => {
          setActiveOffice(val);
          setSearch("");
        }}
      >
        {(offices ?? []).map((office) => (
          <AccordionItem key={office.id} value={office.id}>
            <AccordionTrigger className="text-lg font-semibold">
              {office.name}
            </AccordionTrigger>

            <AccordionContent>
              <div className="p-4 border rounded-md bg-muted/30 space-y-4">

                {/* SEARCH + ADD BUTTON */}
                <div className="flex items-center justify-between gap-3">
                  <Input
                    placeholder="Search post..."
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    className="max-w-xs"
                  />

                  <Button onClick={() => openCreateDialog(office.id)}>
                    <Plus className="h-4 w-4 mr-2" /> Add Post
                  </Button>
                </div>

                {/* POSTS LIST */}
                <div className="space-y-2">
                  {filteredRoles.length === 0 ? (
                    <p className="text-sm text-muted-foreground pl-1">
                      No posts found
                    </p>
                  ) : (
                    filteredRoles.map((role) => (
                      <div
                        key={role.id}
                        className="flex items-center justify-between p-3 rounded-md border bg-background"
                      >
                        <div>
                          <p className="font-medium">{role.name}</p>
                        </div>

                        <div className="flex gap-2">
                          <Button
                            size="sm"
                            variant="ghost"
                            onClick={() => openEditDialog(role)}
                          >
                            <Edit className="h-4 w-4" />
                          </Button>
                          <Button
                            size="sm"
                            variant="ghost"
                            className="text-destructive"
                            onClick={() => deleteRole(role.id)}
                          >
                            <Trash2 className="h-4 w-4" />
                          </Button>
                        </div>
                      </div>
                    ))
                  )}
                </div>
              </div>
            </AccordionContent>
          </AccordionItem>
        ))}
      </Accordion>

      {/* CREATE / EDIT DIALOG */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>
              {editing ? "Edit Post" : "Create Post"}
            </DialogTitle>
          </DialogHeader>

          <div className="space-y-4">
            {/* Post Name */}
            <div>
              <Label>Post Name</Label>
              <Input
                value={form.name}
                onChange={(e) => {
                  const v = e.target.value;
                  if (lettersOnly(v)) setForm({ ...form, name: v });
                }}
                placeholder="e.g., Admin, Clerk"
              />
            </div>

            {/* Office fixed selection */}
            <div>
              <Label>Office</Label>
              <Select value={form.officeId} disabled>
                <SelectTrigger>
                  <SelectValue placeholder="Select office" />
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

            {/* Actions */}
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
