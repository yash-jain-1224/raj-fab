import { useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectTrigger,
  SelectValue,
  SelectContent,
  SelectItem,
} from "@/components/ui/select";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Card } from "@/components/ui/card";
import { Plus, Trash2, Layers, Pencil } from "lucide-react";

import { useOffices, useRolesByOffice } from "@/hooks/api";
import { useOfficeLevels } from "@/hooks/api/useOfficeLevels";
import { useOfficePostLevels } from "@/hooks/api/useOfficePostLevels";

export default function OfficeLevelRoleManagementPage() {
  const { offices = [], updateOfficeLevelCount } = useOffices();
  const { officeLevels = [] } = useOfficeLevels();

  const [officeId, setOfficeId] = useState("");
  const [levelCount, setLevelCount] = useState<number | null>(null);
  const [isEditingLevelCount, setIsEditingLevelCount] = useState(false);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [activeLevelId, setActiveLevelId] = useState<string | null>(null);
  const [selectedRoleId, setSelectedRoleId] = useState("");

  const { data: roles = [] } = useRolesByOffice(officeId);

  const {
    posts: assignments = [],
    assignPost,
    removePost,
  } = useOfficePostLevels(officeId);

  const activeLevels = useMemo(() => {
    if (!levelCount) return [];
    return [...officeLevels]
      .sort((a, b) => a.levelOrder - b.levelOrder)
      .slice(0, levelCount);
  }, [officeLevels, levelCount]);

  const handleOfficeChange = (id: string) => {
    const office = offices.find((o) => o.id === id);
    setOfficeId(id);
    setLevelCount(office?.levelCount ?? 0);
    setIsEditingLevelCount(false);
    setActiveLevelId(null);
    setSelectedRoleId("");
  };

  const handleLevelCountChange = (value: string) => {
    const count = Number(value);
    const office = offices.find((o) => o.id === officeId);
    if (!office) return;

    if (count < office.levelCount) {
      const ok = confirm(
        "Reducing levels may affect higher-level office post assignments. Continue?"
      );
      if (!ok) return;
    }

    setLevelCount(count);
    updateOfficeLevelCount({ officeId, levelCount: count });
    setIsEditingLevelCount(false);
  };

  return (
    <div className="space-y-8">
      {/* Office & Level Setup */}
      <Card className="p-6 max-w-xl space-y-6">
        <div className="flex items-center gap-2">
          <Layers className="h-6 w-6" />
          <h2 className="text-xl font-semibold">
            Office Post Level Management
          </h2>
        </div>

        <div className="space-y-2">
          <Label>Select Office</Label>
          <Select value={officeId} onValueChange={handleOfficeChange}>
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

        {officeId && (
          <div className="space-y-2">
            <Label>Number of Approval Levels</Label>

            <div className="flex items-center gap-2">
              <Select
                value={levelCount !== null ? String(levelCount) : ""}
                disabled={levelCount > 0 && !isEditingLevelCount}
                onValueChange={handleLevelCountChange}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select count" />
                </SelectTrigger>
                <SelectContent>
                  {officeLevels.map((l) => (
                    <SelectItem key={l.levelOrder} value={String(l.levelOrder)}>
                      {l.levelOrder}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>

              {levelCount > 0 && !isEditingLevelCount && (
                <Button
                  size="icon"
                  variant="outline"
                  onClick={() => setIsEditingLevelCount(true)}
                >
                  <Pencil className="h-4 w-4" />
                </Button>
              )}
            </div>
          </div>
        )}
      </Card>

      {/* Levels & Roles */}
      {activeLevels.length > 0 && (
        <div className="grid gap-6">
          {activeLevels.map((level) => {
            const levelRoles = assignments.filter(
              (a) => a.officeLevelId === level.id
            );

            return (
              <Card key={level.id} className="p-5 space-y-4">
                <div className="flex justify-between items-center">
                  <div>
                    <div className="font-semibold">{level.name}</div>
                    <div className="text-xs text-muted-foreground">
                      Approval Level {level.levelOrder}
                    </div>
                  </div>

                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => {
                      setActiveLevelId(level.id);
                      setSelectedRoleId("");
                      setDialogOpen(true);
                    }}
                  >
                    <Plus className="h-4 w-4 mr-1" />
                    Assign Office Post
                  </Button>
                </div>

                {levelRoles.length === 0 ? (
                  <div className="text-sm text-muted-foreground border border-dashed rounded-md p-3">
                    No office post assigned
                  </div>
                ) : (
                  <div className="flex flex-wrap gap-2">
                    {levelRoles.map((r) => (
                      <div
                        key={r.id}
                        className="flex items-center gap-3 border rounded-md px-3 py-2 text-sm bg-muted/30"
                      >
                        <span>{r.roleName}</span>
                        <Button
                          size="sm"
                          variant="ghost"
                          className="text-destructive"
                          onClick={() => removePost(r.id)}
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    ))}
                  </div>
                )}
              </Card>
            );
          })}
        </div>
      )}

      {/* Assign Dialog */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Assign Office Post to Level</DialogTitle>
          </DialogHeader>

          <div className="space-y-4">
            <div>
              <Label>Select Office Post</Label>
              <Select value={selectedRoleId} onValueChange={setSelectedRoleId}>
                <SelectTrigger>
                  <SelectValue placeholder="Select Office Post" />
                </SelectTrigger>
                <SelectContent>
                  {roles.map((r) => (
                    <SelectItem key={r.id} value={r.id}>
                      {r.postName}, {r.officeCityName}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setDialogOpen(false)}>
                Cancel
              </Button>
              <Button
                disabled={!selectedRoleId}
                onClick={() => {
                  if (!activeLevelId) return;
                  assignPost({
                    officeId,
                    roleId: selectedRoleId,
                    officeLevelId: activeLevelId,
                  });
                  setDialogOpen(false);
                }}
              >
                Assign
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
