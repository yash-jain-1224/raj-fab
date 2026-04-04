import { useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Form, FormMessage, FormField, FormItem } from "@/components/ui/form";
import { Edit, Trash2 } from "lucide-react";

import { ModernDataTable } from "@/components/admin/ModernDataTable";
import { AreaSelector } from "@/components/admin/AreaSelector";

import { useOffices } from "@/hooks/api/useOffices";
import { useDivisions } from "@/hooks/api/useDivisions";
import { useDistricts } from "@/hooks/api/useDistricts";
import { useCities } from "@/hooks/api/useCities";

import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import * as z from "zod";
import { CreateOfficeRequest } from "@/services/api/offices";
import { Checkbox } from "@/components/ui/checkbox";

export const officeSchema = z.object({
  name: z.string().min(1, "Office name is required"),
  districtId: z.string().min(1, "District is required"),
  cityId: z.string().min(1, "City is required"),
  pincode: z.string().min(1, "Pincode is required"),
  address: z.string().min(1, "Address is required"),
  isHeadOffice: z.boolean().default(false),

  applicationArea: z.object({
    divisionIds: z.array(z.string()).min(1, "Select at least one division"),
    districtIds: z.array(z.string()).min(1, "Select at least one district"),
    cityIds: z.array(z.string()).min(1, "Select at least one city"),
  }),

  inspectionArea: z.object({
    divisionIds: z.array(z.string()).min(1, "Select at least one division"),
    districtIds: z.array(z.string()).min(1, "Select at least one district"),
    cityIds: z.array(z.string()).min(1, "Select at least one city"),
  }),
});

export type OfficeFormValues = z.infer<typeof officeSchema>;

export default function OfficeManagementPage() {
  const { offices, createOffice, updateOffice, deleteOffice, isLoading } =
    useOffices();
  const { divisions } = useDivisions();
  const { districts } = useDistricts();
  const { cities } = useCities();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<any | null>(null);
  const [step, setStep] = useState(1);
  const form = useForm<CreateOfficeRequest>({
    resolver: zodResolver(officeSchema),
    defaultValues: {
      name: "",
      districtId: "",
      cityId: "",
      pincode: "",
      address: "",
      isHeadOffice: false,
      applicationArea: { divisionIds: [], districtIds: [], cityIds: [] },
      inspectionArea: { divisionIds: [], districtIds: [], cityIds: [] },
    },
  });

  const { watch, setValue, handleSubmit, reset, trigger } = form;

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const filtered = useMemo(() => {
    return offices.filter((o) =>
      o.name.toLowerCase().includes(search.toLowerCase())
    );
  }, [offices, search]);

  const totalPages = Math.ceil(filtered.length / pageSize);
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const resetForm = () => {
    reset({
      name: "",
      districtId: "",
      cityId: "",
      pincode: "",
      isHeadOffice: false,
      address: "",
      applicationArea: { divisionIds: [], districtIds: [], cityIds: [] },
      inspectionArea: { divisionIds: [], districtIds: [], cityIds: [] },
    });
    setStep(1);
    setEditing(null);
  };

  function getDivisionDistrict(cityIds: string[]) {
    const districtSet = new Set<string>();
    cityIds.forEach((id) => {
      const c = cities.find((x) => x.id === id);
      if (c) districtSet.add(c.districtId);
    });

    const divisionSet = new Set<string>();
    Array.from(districtSet).forEach((d) => {
      const dist = districts.find((x) => x.id === d);
      if (dist) divisionSet.add(dist.divisionId);
    });

    return {
      divisionIds: Array.from(divisionSet),
      districtIds: Array.from(districtSet),
    };
  }

  const onSave = async (data: CreateOfficeRequest) => {
    if (editing) {
      await updateOffice({ id: editing.id, data });
    } else {
      await createOffice(data);
    }

    resetForm();
    setDialogOpen(false);
  };

  const handleEdit = (office: any) => {
    setEditing(office);

    const appCities = office.officeApplicationArea || [];
    const insCities = office.officeInspectionArea || [];

    const app = getDivisionDistrict(appCities);
    const ins = getDivisionDistrict(insCities);

    reset({
      name: office.name,
      districtId: office.districtId,
      cityId: office.cityId,
      pincode: office.pincode,
      address: office.address,
      isHeadOffice: office.isHeadOffice,
      applicationArea: {
        divisionIds: app.divisionIds,
        districtIds: app.districtIds,
        cityIds: appCities,
      },
      inspectionArea: {
        divisionIds: ins.divisionIds,
        districtIds: ins.districtIds,
        cityIds: insCities,
      },
    });

    setStep(1);
    setDialogOpen(true);
  };

  const handleDelete = async (id: string) => {
    if (confirm("Delete office?")) await deleteOffice(id);
  };
  const columns = [
    {
      key: "index",
      header: "#",
      render: (_: any, index: number) => <span>{index}</span>,
    },
    { key: "name", header: "Office Name" },
    { key: "districtName", header: "District" },
    { key: "cityName", header: "Tehsil" },
    { key: "address", header: "Address" },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (office: any) => (
        <div className="flex justify-end gap-2">
          <Button variant="ghost" size="sm" onClick={() => handleEdit(office)}>
            <Edit className="w-4 h-4" />
          </Button>

          <Button
            variant="ghost"
            size="sm"
            className="text-destructive"
            onClick={() => handleDelete(office.id)}
          >
            <Trash2 className="w-4 h-4" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <ModernDataTable
        title="Office Management"
        description="Manage office records"
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
        pageSize={pageSize}
        emptyMessage="No offices found"
        onAdd={() => {
          resetForm();
          setDialogOpen(true);
        }}
        addLabel="Add Office"
      />

      <Dialog
        open={dialogOpen}
        onOpenChange={(state) => {
          setDialogOpen(state);
          if (!state) resetForm();
        }}
      >
        <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              {editing ? "Edit Office" : "Create Office"}
            </DialogTitle>
          </DialogHeader>

          <Form {...form}>
            <form onSubmit={handleSubmit(onSave)}>
              <Tabs
                value={
                  ["details", "application", "inspection", "preview"][step - 1]
                }
              >
                <TabsList className="grid grid-cols-4 w-full mb-4">
                  <TabsTrigger value="details">Details</TabsTrigger>
                  <TabsTrigger value="application">
                    Application Jurisdiction
                  </TabsTrigger>
                  <TabsTrigger value="inspection">
                    Inspection Jurisdiction
                  </TabsTrigger>
                  <TabsTrigger value="preview">Preview</TabsTrigger>
                </TabsList>

                {/* STEP 1 */}
                {step === 1 && (
                  <TabsContent value="details" className="space-y-4">
                    <FormField
                      control={form.control}
                      name="name"
                      render={({ field }) => (
                        <FormItem>
                          <Label>Office Name</Label>
                          <Input {...field} />
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <Label>District</Label>
                    <Select
                      value={watch("districtId")}
                      onValueChange={(v) => setValue("districtId", v)}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select district" />
                      </SelectTrigger>
                      <SelectContent>
                        {districts.map((d) => (
                          <SelectItem key={d.id} value={d.id}>
                            {d.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>

                    <Label>City</Label>
                    <Select
                      value={watch("cityId")}
                      onValueChange={(v) => setValue("cityId", v)}
                    >
                      <SelectTrigger>
                        <SelectValue placeholder="Select city" />
                      </SelectTrigger>
                      <SelectContent>
                        {cities
                          .filter((c) => c.districtId === watch("districtId"))
                          .map((c) => (
                            <SelectItem key={c.id} value={c.id}>
                              {c.name}
                            </SelectItem>
                          ))}
                      </SelectContent>
                    </Select>

                    <FormField
                      control={form.control}
                      name="pincode"
                      render={({ field }) => (
                        <FormItem>
                          <Label>Pincode</Label>
                          <Input {...field} />
                          <FormMessage />
                        </FormItem>
                      )}
                    />

                    <FormField
                      control={form.control}
                      name="address"
                      render={({ field }) => (
                        <FormItem>
                          <Label>Address</Label>
                          <Input {...field} />
                          <FormMessage />
                        </FormItem>
                      )}
                    />
                    <div className="flex items-center space-x-2">
                      <Checkbox
                        id="isHeadOffice"
                        checked={watch("isHeadOffice")}
                        onCheckedChange={(checked) =>
                          setValue("isHeadOffice", !!checked)
                        }
                      />
                      <Label htmlFor="isHeadOffice">Is Head Office</Label>
                    </div>
                  </TabsContent>
                )}

                {/* STEP 2 */}
                {step === 2 && (
                  <TabsContent value="application">
                    <AreaSelector
                      form={form}
                      fieldPrefix="applicationArea"
                      divisions={divisions}
                      districts={districts}
                      cities={cities}
                    />
                  </TabsContent>
                )}

                {/* STEP 3 */}
                {step === 3 && (
                  <TabsContent value="inspection">
                    <AreaSelector
                      form={form}
                      fieldPrefix="inspectionArea"
                      divisions={divisions}
                      districts={districts}
                      cities={cities}
                    />
                  </TabsContent>
                )}

                {/* STEP 4 */}
                {step === 4 && (
                  <TabsContent value="preview">
                    <div className="space-y-6 p-4 border rounded-md bg-muted/30">
                      <h2 className="text-lg font-semibold">
                        Preview Office Details
                      </h2>

                      <div className="grid grid-cols-2 gap-4">
                        <p>
                          <strong>Name:</strong> {watch("name")}
                        </p>
                        <p>
                          <strong>Pincode:</strong> {watch("pincode")}
                        </p>
                        <p>
                          <strong>District:</strong>{" "}
                          {
                            districts.find((d) => d.id === watch("districtId"))
                              ?.name
                          }
                        </p>
                        <p>
                          <strong>City:</strong>{" "}
                          {cities.find((c) => c.id === watch("cityId"))?.name}
                        </p>
                        <p className="col-span-2">
                          <strong>Address:</strong> {watch("address")}
                        </p>
                        <p>
                          <strong>Head Office:</strong>{" "}
                          {watch("isHeadOffice") ? "Yes" : "No"}
                        </p>
                      </div>

                      <div>
                        <h3 className="font-medium text-primary">
                          Application Jurisdiction
                        </h3>
                        <div className="grid grid-cols-3 gap-4 text-sm">
                          <div>
                            <p className="font-medium">Divisions:</p>
                            <ul className="list-disc ml-4">
                              {watch("applicationArea.divisionIds").map(
                                (id) => (
                                  <li key={id}>
                                    {divisions.find((d) => d.id === id)?.name}
                                  </li>
                                )
                              )}
                            </ul>
                          </div>

                          <div>
                            <p className="font-medium">Districts:</p>
                            <ul className="list-disc ml-4">
                              {watch("applicationArea.districtIds").map(
                                (id) => (
                                  <li key={id}>
                                    {districts.find((d) => d.id === id)?.name}
                                  </li>
                                )
                              )}
                            </ul>
                          </div>

                          <div>
                            <p className="font-medium">Cities:</p>
                            <ul className="list-disc ml-4">
                              {watch("applicationArea.cityIds").map((id) => (
                                <li key={id}>
                                  {cities.find((c) => c.id === id)?.name}
                                </li>
                              ))}
                            </ul>
                          </div>
                        </div>
                      </div>

                      <div>
                        <h3 className="font-medium text-primary">
                          Inspection Jurisdiction
                        </h3>
                        <div className="grid grid-cols-3 gap-4 text-sm">
                          <div>
                            <p className="font-medium">Divisions:</p>
                            <ul className="list-disc ml-4">
                              {watch("inspectionArea.divisionIds").map((id) => (
                                <li key={id}>
                                  {divisions.find((d) => d.id === id)?.name}
                                </li>
                              ))}
                            </ul>
                          </div>

                          <div>
                            <p className="font-medium">Districts:</p>
                            <ul className="list-disc ml-4">
                              {watch("inspectionArea.districtIds").map((id) => (
                                <li key={id}>
                                  {districts.find((d) => d.id === id)?.name}
                                </li>
                              ))}
                            </ul>
                          </div>

                          <div>
                            <p className="font-medium">Cities:</p>
                            <ul className="list-disc ml-4">
                              {watch("inspectionArea.cityIds").map((id) => (
                                <li key={id}>
                                  {cities.find((c) => c.id === id)?.name}
                                </li>
                              ))}
                            </ul>
                          </div>
                        </div>
                      </div>
                    </div>
                  </TabsContent>
                )}
              </Tabs>

              <div className="flex justify-end gap-2 mt-4">
                {step > 1 && (
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => setStep(step - 1)}
                  >
                    Back
                  </Button>
                )}

                {step < 4 && (
                  <Button
                    type="button"
                    onClick={async () => {
                      let valid = false;

                      if (step === 1) {
                        valid = await trigger([
                          "name",
                          "districtId",
                          "cityId",
                          "pincode",
                          "address",
                        ]);
                      }

                      if (step === 2) {
                        valid = await trigger([
                          "applicationArea.divisionIds",
                          "applicationArea.districtIds",
                          "applicationArea.cityIds",
                        ]);
                      }

                      if (step === 3) {
                        valid = await trigger([
                          "inspectionArea.divisionIds",
                          "inspectionArea.districtIds",
                          "inspectionArea.cityIds",
                        ]);
                      }

                      if (valid) setStep(step + 1);
                    }}
                  >
                    Next
                  </Button>
                )}

                {step === 4 && (
                  <Button type="submit">
                    {editing ? "Update Office" : "Create Office"}
                  </Button>
                )}
              </div>
            </form>
          </Form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
