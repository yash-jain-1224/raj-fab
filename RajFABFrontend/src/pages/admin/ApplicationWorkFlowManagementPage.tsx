import { useState, useMemo, useEffect } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Edit, Trash2, Layers, ArrowRight, Building2 } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";

import { useApplicationWorkFlows } from "@/hooks/api/useApplicationWorkFlows";
import {
  useOffices,
  useActs,
  useRulesByAct,
  useFactoryCategories,
  useRolesByOffices,
} from "@/hooks/api";
import { useModuleByRule } from "@/hooks/api/useModules";

import type {
  ApplicationWorkFlow,
  CreateApplicationWorkFlowRequest,
} from "@/services/api/applicationWorkflows";
import { Checkbox } from "@/components/ui/checkbox";
import { v4 as uuidv4 } from 'uuid';

type WorkflowLevel = {
  id?: string;
  levelNumber: number;
  roleId: string;
  useOtherOffice: boolean;
  officeId: string;
};

type ApplicationRow = {
  id: string;
  moduleId: string;
  moduleName: string;
  factoryCategoryId: string;
  count: number;
  levels: WorkflowLevel[];
};

type FormState = {
  officeId: string;
  actId: string;
  ruleId: string;
  applicationRows: ApplicationRow[];
};

export default function ApplicationWorkFlowManagementPage() {
  const {
    workflows,
    isLoading,
    createWorkflow,
    updateWorkflow,
    deleteWorkflow,
  } = useApplicationWorkFlows();

  const { offices } = useOffices();
  const { acts } = useActs();
  const { factoryCategories } = useFactoryCategories();

  const [selectedOfficeId, setSelectedOfficeId] = useState("");

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<ApplicationWorkFlow | null>(null);

  const [form, setForm] = useState<FormState>({
    officeId: selectedOfficeId,
    actId: "",
    ruleId: "",
    applicationRows: [],
  });

  const { data: rules = [] } = useRulesByAct(form.actId);
  const { data: modules = [] } = useModuleByRule(form.ruleId);

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;
  const syncRowLevels = (rowIndex: number, newCount: number) => {
    if (newCount < 1) return;

    setForm((prev) => {
      const rows = [...prev.applicationRows];
      const row = rows[rowIndex];

      const updatedLevels = Array.from({ length: newCount }, (_, i) => {
        return (
          row.levels[i] ?? {
            levelNumber: i + 1,
            roleId: "",
            useOtherOffice: false,
            officeId: "",
          }
        );
      }).map((l, i) => ({ ...l, levelNumber: i + 1 }));

      rows[rowIndex] = {
        ...row,
        count: newCount,
        levels: updatedLevels,
      };

      return { ...prev, applicationRows: rows };
    });
  };
  const officeIdsForRoles = useMemo(() => {
    const ids = new Set<string>();
    if (selectedOfficeId) ids.add(selectedOfficeId);

    form.applicationRows.forEach((row) => {
      row.levels.forEach((l) => {
        if (l.useOtherOffice && l.officeId) ids.add(l.officeId);
      });
    });

    return Array.from(ids);
  }, [selectedOfficeId, form.applicationRows]);

  const rolesByOffice = useRolesByOffices(officeIdsForRoles);

  useEffect(() => {
    if (!form.ruleId || editing) return;

    const rows: ApplicationRow[] = modules.map((m) => ({
      id: uuidv4(),
      moduleId: m.id,
      moduleName: m.name,
      factoryCategoryId: "",
      count: 1,
      levels: [
        {
          levelNumber: 1,
          roleId: "",
          useOtherOffice: false,
          officeId: "",
        },
      ],
    }));

    setForm((p) => ({ ...p, applicationRows: rows }));
  }, [form.ruleId, modules, editing]);

  const handleSave = () => {
    const payload: CreateApplicationWorkFlowRequest = {
      officeId: selectedOfficeId,
      applications: form.applicationRows.map((r) => ({
        moduleId: r.moduleId,
        factoryCategoryId: r.factoryCategoryId,
        levelCount: r.count,
        levels: r.levels.map((l) => ({
          levelNumber: l.levelNumber,
          roleId: l.roleId,
          useOtherOffice: l.useOtherOffice,
          ...(l.useOtherOffice && l.officeId ? { officeId: l.officeId } : {}),
        })),
      })),
    };

    if (editing) {
      const row = form.applicationRows[0];

      updateWorkflow({
        id: editing.id,
        data: {
          levelCount: row.count,
          factoryCategoryId: row.factoryCategoryId,
          isActive: true,
          levels: row.levels.map((l) => ({
            id: l.id,
            levelNumber: l.levelNumber,
            roleId: l.roleId,
            isActive: true,
          })),
        },
      });
    } else createWorkflow(payload);
    setDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this workflow?")) deleteWorkflow(id);
  };

  const filtered = useMemo(() => {
    if (!selectedOfficeId) return [];
    return (workflows ?? [])
      .filter((w) => w.officeId === selectedOfficeId)
      .filter((w) =>
        `${w.actName} ${w.ruleName} ${w.moduleName} ${w.factoryCategoryName}`
          .toLowerCase()
          .includes(search.toLowerCase())
      );
  }, [workflows, selectedOfficeId, search]);

  const totalPages = Math.max(1, Math.ceil(filtered.length / pageSize));
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const columns = [
    {
      key: "index",
      header: "#",
      render: (_: any, i: number) => (page - 1) * pageSize + i,
    },
    {
      key: "applicationType",
      header: "Application Type",
      render: (w: ApplicationWorkFlow) => (
        <Badge variant="outline">{w.moduleName}</Badge>
      ),
    },
    {
      key: "factoryCategory",
      header: "Factory Category",
      render: (w: ApplicationWorkFlow) => w.factoryCategoryName,
    },
    {
      key: "levels",
      header: "Levels",
      render: (w: ApplicationWorkFlow) => (
        <div className="flex items-center gap-2">
          <Layers className="h-4 w-4 text-primary" />
          <span className="font-medium">{w.levelCount}</span>
        </div>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (w: ApplicationWorkFlow) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            className="h-8 w-8 p-0"
            onClick={() => {
              setEditing(w);
              setForm({
                officeId: w.officeId,
                actId: w.actId,
                ruleId: w.ruleId,
                applicationRows: [
                  {
                    id: w.id,
                    moduleId: w.moduleId,
                    moduleName: w.moduleName,
                    factoryCategoryId: w.factoryCategoryId,
                    count: w.levelCount,
                    levels: w.levels
                      .sort((a, b) => a.levelNumber - b.levelNumber)
                      .map((l) => ({
                        levelNumber: l.levelNumber,
                        roleId: l.roleId,
                        useOtherOffice: !!l.officeId,
                        officeId: l.officeId ?? "",
                        id: l.id,
                      })),
                  },
                ],
              });
              setDialogOpen(true);
            }}
          >
            <Edit className="h-4 w-4" />
          </Button>

          <Button
            size="sm"
            variant="ghost"
            className="h-8 w-8 p-0 text-destructive"
            onClick={() => handleDelete(w.id)}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  const officeSelector = (
    <div className="my-3">
      <Label>Select Office</Label>
      <Select value={selectedOfficeId} onValueChange={setSelectedOfficeId}>
        <SelectTrigger className="w-72">
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
  );

  return (
    <div className="space-y-6">
      {!selectedOfficeId ? (
        <div className="flex flex-col items-center justify-center h-64 bg-muted/30 rounded-lg border-2 border-dashed">
          <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
          {officeSelector}
          <p className="text-sm text-muted-foreground">
            Please select an office to manage workflows
          </p>
        </div>
      ) : (
        <ModernDataTable
          title="Application Workflow Management"
          description="Office based application workflows"
          data={paginated}
          columns={columns}
          loading={isLoading}
          search={search}
          onSearchChange={(v) => {
            setSearch(v);
            setPage(1);
          }}
          page={page}
          totalPages={totalPages}
          onPageChange={setPage}
          onAdd={() => {
            setEditing(null);
            setForm({
              officeId: selectedOfficeId,
              actId: "",
              ruleId: "",
              applicationRows: [],
            });
            setDialogOpen(true);
          }}
          addLabel="Add Workflow"
          emptyMessage="No workflows found"
          pageSize={pageSize}
          filterComponent={officeSelector}
        />
      )}

      {/* MODAL */}
      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent className="max-w-7xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              {editing
                ? "Edit Application Workflow"
                : "Create Application Workflow"}
            </DialogTitle>
          </DialogHeader>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <Label>Act</Label>
              <Select
                value={form.actId}
                disabled={!!editing}
                onValueChange={(v) =>
                  setForm({
                    ...form,
                    actId: v,
                    ruleId: "",
                    applicationRows: [],
                  })
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select Act" />
                </SelectTrigger>
                <SelectContent>
                  {acts.map((a) => (
                    <SelectItem key={a.id} value={a.id}>
                      {a.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label>Rule</Label>
              <Select
                value={form.ruleId}
                disabled={!!editing || !form.actId}
                onValueChange={(v) =>
                  setForm({ ...form, ruleId: v, applicationRows: [] })
                }
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select Rule" />
                </SelectTrigger>
                <SelectContent>
                  {rules?.map((r) => (
                    <SelectItem key={r.id} value={r.id}>
                      {r.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          {/* APPLICATION TABLE */}
          {form.applicationRows.length > 0 && (
            <div className="[&_td]:whitespace-nowrap [&_td]:w-fit [&_th]:whitespace-nowrap [&_th]:w-fit">
              <table className="w-full table-auto border rounded-md mt-6">
                <thead className="bg-muted">
                  <tr>
                    {!editing && <th className="p-2">Action</th>}
                    <th className="p-2 text-left">Application Type</th>
                    <th className="p-2">Factory Category</th>
                    <th className="p-2">Level Count</th>
                    <th className="p-2">Work Flow</th>
                  </tr>
                </thead>

                <tbody>
                  {form.applicationRows.map((row, index) => {
                    const moduleRows = form.applicationRows.filter(
                      (r) => r.moduleId === row.moduleId
                    );

                    const usedCategoriesForModule = new Set(
                      moduleRows
                        .filter((r) => r.factoryCategoryId)
                        .map((r) => r.factoryCategoryId)
                    );

                    const canAddMoreForModule =
                      moduleRows.length < factoryCategories.length;

                    const isLastRowForModule =
                      moduleRows[moduleRows.length - 1].id === row.id;

                    const availableCategories = factoryCategories.filter(
                      (fc) =>
                        !usedCategoriesForModule.has(fc.id) ||
                        fc.id === row.factoryCategoryId
                    );

                    return (
                      <tr key={row.id} className="border-t">
                        {!editing && (
                          <td className="p-2 text-center">
                            {isLastRowForModule && canAddMoreForModule && (
                              <Button
                                variant="outline"
                                size="sm"
                                onClick={() => {
                                  setForm((prev) => {
                                    const moduleRows =
                                      prev.applicationRows.filter(
                                        (r) => r.moduleId === row.moduleId
                                      );

                                    const used = new Set(
                                      moduleRows.map((r) => r.factoryCategoryId)
                                    );

                                    const nextCategory = factoryCategories.find(
                                      (fc) => !used.has(fc.id)
                                    );

                                    if (!nextCategory) return prev;

                                    const newRow: ApplicationRow = {
                                      id: uuidv4(),
                                      moduleId: row.moduleId,
                                      moduleName: row.moduleName,
                                      factoryCategoryId: "",
                                      count: 1,
                                      levels: [
                                        {
                                          levelNumber: 1,
                                          roleId: "",
                                          useOtherOffice: false,
                                          officeId: "",
                                        },
                                      ],
                                    };

                                    const rows = [...prev.applicationRows];

                                    const lastIndexOfModule = rows
                                      .map((r, i) => ({ r, i }))
                                      .filter(
                                        (x) => x.r.moduleId === row.moduleId
                                      )
                                      .slice(-1)[0].i;

                                    rows.splice(
                                      lastIndexOfModule + 1,
                                      0,
                                      newRow
                                    );

                                    return {
                                      ...prev,
                                      applicationRows: rows,
                                    };
                                  });
                                }}
                              >
                                + Add
                              </Button>
                            )}

                            <Button
                              variant="outline"
                              size="sm"
                              className="text-destructive ms-2"
                              onClick={() => {
                                setForm((prev) => ({
                                  ...prev,
                                  applicationRows: prev.applicationRows.filter(
                                    (r) => r.id !== row.id
                                  ),
                                }));
                              }}
                            >
                              Remove
                            </Button>
                          </td>
                        )}

                        {/* Application Type */}
                        <td className="p-2 font-medium">{row.moduleName}</td>

                        {/* Factory Category */}
                        <td className="p-2">
                          <Select
                            value={row.factoryCategoryId}
                            onValueChange={(v) => {
                              const rows = [...form.applicationRows];
                              rows[index].factoryCategoryId = v;
                              setForm({ ...form, applicationRows: rows });
                            }}
                          >
                            <SelectTrigger>
                              <SelectValue placeholder="Select Category" />
                            </SelectTrigger>
                            <SelectContent>
                              {availableCategories.map((fc) => (
                                <SelectItem key={fc.id} value={fc.id}>
                                  {fc.name}
                                </SelectItem>
                              ))}
                            </SelectContent>
                          </Select>
                        </td>

                        {/* LEVEL COUNT */}
                        <td className="p-2">
                          <Input
                            type="number"
                            min={1}
                            max={5}
                            value={row.count}
                            onChange={(e) => {
                              let val = Number(e.target.value);
                              if (Number.isNaN(val)) return;
                              val = Math.max(1, Math.min(5, val));
                              syncRowLevels(index, val);
                            }}
                            onBlur={(e) => {
                              if (!e.target.value) syncRowLevels(index, 1);
                            }}
                            className="w-16"
                            inputMode="numeric"
                          />
                        </td>

                        <td className="p-2 align-top w-fit">
                          <div className="w-[675px] overflow-x-auto overflow-y-hidden">
                            <div className="flex items-start gap-6 py-2 min-w-max">
                              {row.levels
                                .sort((a, b) => a.levelNumber - b.levelNumber)
                                .map((l, i) => {
                                  const levelRoles = l.useOtherOffice
                                    ? rolesByOffice[l.officeId] || []
                                    : rolesByOffice[selectedOfficeId] || [];

                                  const usedRoles = row.levels
                                    .slice(0, i)
                                    .map((x) => x.roleId)
                                    .filter(Boolean);

                                  const filteredLevelRoles = levelRoles.filter(
                                    (r) => !usedRoles.includes(r.id)
                                  );

                                  return (
                                    <div
                                      key={i}
                                      className="flex items-center gap-4"
                                    >
                                      <div className="min-w-[300px] border rounded-lg p-4 bg-muted">
                                        <span className="text-sm font-semibold text-primary">
                                          Level {l.levelNumber}
                                        </span>

                                        <Label className="flex items-center gap-2 my-2">
                                          <Checkbox
                                            checked={l.useOtherOffice}
                                            onCheckedChange={(checked) => {
                                              const rows = [
                                                ...form.applicationRows,
                                              ];
                                              const levels = [...row.levels];
                                              levels[i] = {
                                                ...levels[i],
                                                useOtherOffice:
                                                  Boolean(checked),
                                                officeId: "",
                                                roleId: "",
                                              };
                                              rows[index] = {
                                                ...row,
                                                levels,
                                              };
                                              setForm({
                                                ...form,
                                                applicationRows: rows,
                                              });
                                            }}
                                          />
                                          Allow other office
                                        </Label>

                                        {l.useOtherOffice && (
                                          <Select
                                            value={l.officeId}
                                            onValueChange={(v) => {
                                              const rows = [
                                                ...form.applicationRows,
                                              ];
                                              const levels = [...row.levels];
                                              levels[i].officeId = v;
                                              levels[i].roleId = "";
                                              rows[index] = {
                                                ...row,
                                                levels,
                                              };
                                              setForm({
                                                ...form,
                                                applicationRows: rows,
                                              });
                                            }}
                                          >
                                            <SelectTrigger>
                                              <SelectValue placeholder="Select Office" />
                                            </SelectTrigger>
                                            <SelectContent>
                                              {offices
                                                .filter(
                                                  (o) =>
                                                    o.id !== selectedOfficeId
                                                )
                                                .map((o) => (
                                                  <SelectItem
                                                    key={o.id}
                                                    value={o.id}
                                                  >
                                                    {o.name}
                                                  </SelectItem>
                                                ))}
                                            </SelectContent>
                                          </Select>
                                        )}

                                        <Select
                                          value={l.roleId}
                                          onValueChange={(v) => {
                                            const rows = [
                                              ...form.applicationRows,
                                            ];
                                            const levels = [...row.levels];
                                            levels[i].roleId = v;
                                            rows[index] = { ...row, levels };
                                            setForm({
                                              ...form,
                                              applicationRows: rows,
                                            });
                                          }}
                                        >
                                          <SelectTrigger className="mt-2">
                                            <SelectValue placeholder="Select Role" />
                                          </SelectTrigger>
                                          <SelectContent>
                                            {filteredLevelRoles.length === 0 ? (
                                              <SelectItem value="none" disabled>
                                                No roles available
                                              </SelectItem>
                                            ) : (
                                              filteredLevelRoles.map((r) => (
                                                <SelectItem
                                                  key={r.id}
                                                  value={r.id}
                                                >
                                                  {r.postName}
                                                  {r.officeCityName && (
                                                    <span className="text-xs ml-1">
                                                      , {r.officeCityName}
                                                    </span>
                                                  )}
                                                </SelectItem>
                                              ))
                                            )}
                                          </SelectContent>
                                        </Select>
                                      </div>

                                      {i < row.levels.length - 1 && (
                                        <ArrowRight className="h-6 w-6 text-muted-foreground shrink-0" />
                                      )}
                                    </div>
                                  );
                                })}
                            </div>
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}

          <div className="flex justify-end gap-2 pt-4">
            <Button variant="outline" onClick={() => setDialogOpen(false)}>
              Cancel
            </Button>
            <Button onClick={handleSave}>
              {editing ? "Update" : "Create"}
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
