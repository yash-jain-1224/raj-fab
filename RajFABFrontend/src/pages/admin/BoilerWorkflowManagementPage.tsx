import { useState } from "react";
import { Building2, ArrowRight, Info, Lock } from "lucide-react";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import { useOffices, useRolesByOffices } from "@/hooks/api";
import {
  useBoilerWorkflowManagement,
  useSaveInspectionScrutinyWorkflow,
} from "@/hooks/api/useBoilerWorkflow";

export default function BoilerWorkflowManagementPage() {
  const [selectedOfficeId, setSelectedOfficeId] = useState("");
  const [levelCount, setLevelCount] = useState<"2" | "3">("2");
  const [level2PostId, setLevel2PostId] = useState("");

  const { offices } = useOffices();

  const { data: workflowData, isLoading } = useBoilerWorkflowManagement(selectedOfficeId);

  // Load roles for the selected office (used in Level 2 dropdown for 3-level workflow)
  const rolesByOffice = useRolesByOffices(selectedOfficeId ? [selectedOfficeId] : []);
  const officeRoles = selectedOfficeId ? (rolesByOffice[selectedOfficeId] ?? []) : [];

  const { mutate: saveWorkflow, isPending: isSaving } = useSaveInspectionScrutinyWorkflow();

  // The Level 1 post from Part 1 (Application Scrutiny) — used as read-only in Part 3 Level 1
  const part1Level1PostId = workflowData?.part1?.levels?.find(
    (l) => l.levelNumber === 1
  )?.officePostId;
  const part1Level1PostName = workflowData?.part1?.levels?.find(
    (l) => l.levelNumber === 1
  )?.officePostName;

  // Level 2 dropdown: all roles for selected office, excluding Level 1 post
  const availableLevel2Roles = officeRoles.filter(
    (r) => r.id !== part1Level1PostId
  );

  const handleSave = () => {
    saveWorkflow({
      officeId: selectedOfficeId,
      levelCount: Number(levelCount),
      ...(levelCount === "3" && level2PostId ? { level2OfficePostId: level2PostId } : {}),
    });
  };

  // Pre-fill form from existing Part 3 config
  const existingPart3 = workflowData?.part3;

  return (
    <div className="space-y-6">
      {/* Office Selector */}
      {!selectedOfficeId ? (
        <div className="flex flex-col items-center justify-center h-64 bg-muted/30 rounded-lg border-2 border-dashed">
          <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
          <div className="my-3">
            <Label>Select Office</Label>
            <Select value={selectedOfficeId} onValueChange={setSelectedOfficeId}>
              <SelectTrigger className="w-72 mt-1">
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
          <p className="text-sm text-muted-foreground">
            Select an office to configure its Boiler Workflow
          </p>
        </div>
      ) : (
        <div className="space-y-6">
          {/* Header with office selector */}
          <div className="flex items-center gap-4">
            <div>
              <Label>Office</Label>
              <Select value={selectedOfficeId} onValueChange={(v) => {
                setSelectedOfficeId(v);
                setLevelCount("2");
                setLevel2PostId("");
              }}>
                <SelectTrigger className="w-72 mt-1">
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
          </div>

          {isLoading ? (
            <p className="text-muted-foreground">Loading workflow configuration...</p>
          ) : (
            <div className="space-y-6">

              {/* ── SECTION A: Part 1 — Application Scrutiny (read-only) ─── */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <span className="bg-primary text-primary-foreground rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold">
                      A
                    </span>
                    Part 1: Application Scrutiny
                  </CardTitle>
                  <CardDescription className="flex items-center gap-1">
                    <Lock className="h-3 w-3" />
                    Auto-populated from Application Workflow Management
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  {workflowData?.part1 ? (
                    <div className="space-y-3">
                      <div className="flex items-center gap-2">
                        <Badge variant="outline">
                          {workflowData.part1.applicationType ?? "Boiler"}
                        </Badge>
                        <span className="text-sm text-muted-foreground">
                          {workflowData.part1.levelCount}-level workflow
                        </span>
                      </div>

                      <div className="flex items-start gap-4 overflow-x-auto py-2">
                        {workflowData.part1.levels
                          .sort((a, b) => a.levelNumber - b.levelNumber)
                          .map((level, i, arr) => (
                            <div key={level.id} className="flex items-center gap-4">
                              <div className="min-w-[200px] border rounded-lg p-3 bg-muted/50">
                                <p className="text-xs font-semibold text-primary">
                                  Level {level.levelNumber}
                                </p>
                                <p className="text-sm mt-1">
                                  {level.officePostName ?? level.officePostId}
                                </p>
                              </div>
                              {i < arr.length - 1 && (
                                <ArrowRight className="h-5 w-5 text-muted-foreground shrink-0" />
                              )}
                            </div>
                          ))}
                      </div>

                      <p className="text-xs text-muted-foreground flex items-center gap-1">
                        <Info className="h-3 w-3" />
                        To change this workflow, go to Application Workflow Management.
                      </p>
                    </div>
                  ) : (
                    <p className="text-sm text-muted-foreground">
                      No Application Workflow configured for this office with a Boiler application
                      type. Please set it up in Application Workflow Management first.
                    </p>
                  )}
                </CardContent>
              </Card>

              {/* ── SECTION B: Part 2 — Inspection (informational) ──────── */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <span className="bg-primary text-primary-foreground rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold">
                      B
                    </span>
                    Part 2: Inspection
                  </CardTitle>
                  <CardDescription>
                    Inspector assigned to this office (managed in Office Post Management)
                  </CardDescription>
                </CardHeader>
                <CardContent>
                  {workflowData?.part2?.inspectorName ? (
                    <div className="flex items-center gap-3 p-3 bg-muted/50 rounded-lg border">
                      <div>
                        <p className="font-medium">{workflowData.part2.inspectorName}</p>
                        <p className="text-sm text-muted-foreground">
                          {workflowData.part2.inspectorPost}
                        </p>
                      </div>
                    </div>
                  ) : (
                    <p className="text-sm text-muted-foreground">
                      No Inspector found for this office. Assign an Inspector post in Office Post
                      Management.
                    </p>
                  )}
                </CardContent>
              </Card>

              {/* ── SECTION C: Part 3 — Inspection Scrutiny (configurable) */}
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <span className="bg-primary text-primary-foreground rounded-full w-6 h-6 flex items-center justify-center text-xs font-bold">
                      C
                    </span>
                    Part 3: Inspection Scrutiny
                  </CardTitle>
                  <CardDescription>
                    Configure the bidirectional inspection scrutiny workflow for this office
                  </CardDescription>
                </CardHeader>
                <CardContent className="space-y-5">
                  {existingPart3 && (
                    <div className="p-3 bg-green-50 border border-green-200 rounded-lg text-sm text-green-800">
                      An Inspection Scrutiny workflow ({existingPart3.levelCount}-level) is already
                      configured. Saving will replace it.
                    </div>
                  )}

                  {/* Number of Levels */}
                  <div className="flex items-center gap-4">
                    <div>
                      <Label>Number of Levels</Label>
                      <Select
                        value={levelCount}
                        onValueChange={(v) => {
                          setLevelCount(v as "2" | "3");
                          setLevel2PostId("");
                        }}
                      >
                        <SelectTrigger className="w-32 mt-1">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="2">2</SelectItem>
                          <SelectItem value="3">3</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                  </div>

                  {/* Level configuration grid */}
                  <div className="flex items-start gap-6 overflow-x-auto py-2">

                    {/* Level 1 — auto from Part 1 Level 1 */}
                    <div className="flex items-center gap-4">
                      <div className="min-w-[220px] border rounded-lg p-4 bg-muted/50">
                        <p className="text-xs font-semibold text-primary">Level 1</p>
                        <p className="text-sm mt-1">
                          {part1Level1PostName ?? (
                            <span className="text-muted-foreground italic">
                              (from App. Scrutiny Level 1)
                            </span>
                          )}
                        </p>
                        <Badge variant="secondary" className="mt-2 text-xs">
                          Auto-populated from Application Scrutiny
                        </Badge>
                      </div>
                      <ArrowRight className="h-5 w-5 text-muted-foreground shrink-0" />
                    </div>

                    {/* Level 2 */}
                    <div className="flex items-center gap-4">
                      <div className="min-w-[240px] border rounded-lg p-4 bg-muted/50">
                        <p className="text-xs font-semibold text-primary">Level 2</p>

                        {levelCount === "2" ? (
                          <>
                            <p className="text-sm mt-1">Chief Inspector of Factories and Boilers</p>
                            <Badge variant="secondary" className="mt-2 text-xs">
                              Auto-populated (Fixed)
                            </Badge>
                          </>
                        ) : (
                          <>
                            <Select
                              value={level2PostId}
                              onValueChange={setLevel2PostId}
                            >
                              <SelectTrigger className="mt-2">
                                <SelectValue placeholder="Select Office Post" />
                              </SelectTrigger>
                              <SelectContent>
                                {availableLevel2Roles.length === 0 ? (
                                  <SelectItem value="none" disabled>
                                    No posts available
                                  </SelectItem>
                                ) : (
                                  availableLevel2Roles.map((r) => (
                                    <SelectItem key={r.id} value={r.id}>
                                      {r.postName}
                                    </SelectItem>
                                  ))
                                )}
                              </SelectContent>
                            </Select>
                          </>
                        )}
                      </div>

                      {levelCount === "3" && (
                        <ArrowRight className="h-5 w-5 text-muted-foreground shrink-0" />
                      )}
                    </div>

                    {/* Level 3 — only for 3-level, auto Chief */}
                    {levelCount === "3" && (
                      <div className="min-w-[240px] border rounded-lg p-4 bg-muted/50">
                        <p className="text-xs font-semibold text-primary">Level 3</p>
                        <p className="text-sm mt-1">Chief Inspector of Factories and Boilers</p>
                        <Badge variant="secondary" className="mt-2 text-xs">
                          Auto-populated (Fixed)
                        </Badge>
                      </div>
                    )}
                  </div>

                  <p className="text-xs text-muted-foreground flex items-center gap-1">
                    <Info className="h-3 w-3" />
                    This workflow is bidirectional — applications can move forward and backward
                    between levels.
                  </p>

                  <div className="flex justify-end pt-2">
                    <Button
                      onClick={handleSave}
                      disabled={
                        isSaving ||
                        !selectedOfficeId ||
                        (levelCount === "3" && !level2PostId)
                      }
                    >
                      {isSaving ? "Saving..." : "Save Inspection Scrutiny Workflow"}
                    </Button>
                  </div>
                </CardContent>
              </Card>

            </div>
          )}
        </div>
      )}
    </div>
  );
}
