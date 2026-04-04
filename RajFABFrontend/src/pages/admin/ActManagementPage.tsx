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
import { Edit, Trash2, MapPin } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";

import { useActs } from "@/hooks/api";
import type { Act, CreateActRequest } from "@/services/api/acts";

import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Form, FormField, FormItem, FormMessage } from "@/components/ui/form";

const schema = z.object({
  name: z.string().min(1, "Act name is required"),
  implementationYear: z
    .number()
    .min(1900, "Year must be 1900 or later")
    .max(new Date().getFullYear(), "Year cannot exceed current year"),
});

type FormValues = z.infer<typeof schema>;

export default function ActManagementPage() {
  const currentYear = new Date().getFullYear();
  const { acts, isLoading, createAct, updateAct, deleteAct } = useActs();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Act | null>(null);

  const form = useForm<FormValues>({
    resolver: zodResolver(schema),
    defaultValues: {
      name: "",
      implementationYear: currentYear,
    },
  });

  const { handleSubmit, reset } = form;

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const filtered = useMemo(
    () =>
      acts.filter((a) => a.name.toLowerCase().includes(search.toLowerCase())),
    [acts, search]
  );

  const totalPages = Math.ceil(filtered.length / pageSize);
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const onSave = async (data: FormValues) => {
    const payload: CreateActRequest = {
      name: data.name,
      implementationYear: data.implementationYear,
    };

    if (editing) {
      await updateAct({ id: editing.id, data: payload });
    } else {
      await createAct(payload);
    }

    reset({
      name: "",
      implementationYear: currentYear,
    });

    setEditing(null);
    setDialogOpen(false);
  };

  const onDelete = async (id: string) => {
    if (confirm("Delete this act?")) {
      await deleteAct(id);
    }
  };

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: Act, index: number) => (
        <span className="font-medium text-muted-foreground">{index}</span>
      ),
    },
    {
      key: "name",
      header: "Act Name",
      render: (act: Act) => (
        <div className="flex items-center gap-3">
          <span className="font-medium">{act.name}</span>
        </div>
      ),
    },
    {
      key: "implementationYear",
      header: "Implementation Year",
      render: (act: Act) => (
        <div className="flex items-center gap-3">
          <span className="font-medium">{act.implementationYear}</span>
        </div>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (act: Act) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            variant="ghost"
            size="sm"
            className="h-8 w-8 p-0"
            onClick={() => {
              setEditing(act);
              reset({
                name: act.name,
                implementationYear: act.implementationYear,
              });
              setDialogOpen(true);
            }}
          >
            <Edit className="h-4 w-4" />
          </Button>

          <Button
            variant="ghost"
            size="sm"
            className="h-8 w-8 p-0 text-destructive"
            onClick={() => onDelete(act.id)}
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];

  return (
    <div className="space-y-6">
      <ModernDataTable
        title="Act Management"
        description="Manage administrative acts"
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
        addLabel="Add Act"
        onAdd={() => {
          setEditing(null);
          reset({ name: "", implementationYear: currentYear });
          setDialogOpen(true);
        }}
        emptyMessage="No acts found"
        pageSize={pageSize}
      />

      <Dialog
        open={dialogOpen}
        onOpenChange={(open) => {
          setDialogOpen(open);
          if (!open) {
            setEditing(null);
            reset({ name: "", implementationYear: currentYear });
          }
        }}
      >
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "Edit Act" : "Create Act"}</DialogTitle>
          </DialogHeader>

          <Form {...form}>
            <form className="space-y-4" onSubmit={handleSubmit(onSave)}>
              <FormField
                control={form.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <Label>Act Name</Label>
                    <Input {...field} placeholder="Enter act name" />
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="implementationYear"
                render={({ field }) => (
                  <FormItem>
                    <Label>Implementation Year</Label>
                    <Input
                      type="number"
                      {...field}
                      onChange={(e) => field.onChange(Number(e.target.value))}
                    />
                    <FormMessage />
                  </FormItem>
                )}
              />

              <div className="flex justify-end gap-2 mt-4">
                <Button
                  variant="outline"
                  type="button"
                  onClick={() => setDialogOpen(false)}
                >
                  Cancel
                </Button>
                <Button type="submit">{editing ? "Update" : "Create"}</Button>
              </div>
            </form>
          </Form>
        </DialogContent>
      </Dialog>
    </div>
  );
}
