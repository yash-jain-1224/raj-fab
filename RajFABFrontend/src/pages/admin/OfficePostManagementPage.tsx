import { useEffect, useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Edit, MapPin, Trash2, Building2 } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import { Badge } from "@/components/ui/badge";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { useOffices, usePosts, useRoles } from "@/hooks/api";
import type { CreateRoleRequest, Role } from "@/services/api/roles";
import { Link } from "react-router-dom";
import { Checkbox } from "@/components/ui/checkbox";

export default function RoleManagementPage() {
  const {
    roles,
    isLoading: loading,
    createRole,
    updateRole,
    deleteRole,
    isCreating,
    isUpdating,
  } = useRoles();

  const { offices } = useOffices();
  const { posts } = usePosts();
  // const lettersOnly = (v: string) => /^[A-Za-z\s]*$/.test(v);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<any>(null);
  const [form, setForm] = useState({
    postId: "",
    officeId: "",
    isAuthorise: false,
  });
  const [selectedOfficeId, setSelectedOfficeId] = useState<string>("");

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const handleSave = async () => {
    const data: CreateRoleRequest = {
      postId: form.postId,
      officeId: form.officeId,
    };
    if (editing) {
      updateRole({ id: editing.id, data });
    } else {
      createRole(data);
    }
    setDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this post?")) {
      deleteRole(id);
    }
  };

  // --- Filtering + Pagination ---
  const filtered = useMemo(() => {
    return (roles ?? [])
      .filter((r) =>
        selectedOfficeId ? r.officeId === selectedOfficeId : true
      )
      .filter((r) => r.postName.toLowerCase().includes(search.toLowerCase()));
  }, [roles, selectedOfficeId, search]);

  const totalPages = Math.ceil(filtered.length / pageSize);

  const paginated = useMemo(() => {
    const start = (page - 1) * pageSize;
    return filtered.slice(start, start + pageSize);
  }, [filtered, page]);

  // useEffect(() => {
  //   if (offices?.length && !selectedOfficeId) {
  //     setSelectedOfficeId(offices[0].id);
  //     setForm((prev) => ({ ...prev, officeId: offices[0].id }));
  //   }
  // }, [offices, selectedOfficeId]);

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: Role, index: number) => (
        <span className="font-medium text-muted-foreground">{index}</span>
      ),
    },
    {
      key: "name",
      header: "Post Name",
      render: (role: Role) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center">
            <span className="text-sm font-medium text-primary">
              {role.postName.charAt(0).toUpperCase()}
            </span>
          </div>
          <span className="font-medium">
            {role.postName},{" "}
            {offices.find((i) => i.id === selectedOfficeId)?.cityName}
          </span>
        </div>
      ),
    },
    {
      key: "Office",
      header: "Office",
      render: (role: Role) => (
        <div className="space-y-1">
          <div className="flex items-center gap-2 text-sm">
            <MapPin className="h-3 w-3 text-muted-foreground" />
            <span>{role?.officeName || role.officeId}</span>
          </div>
          {/* <Badge variant="outline" className="text-xs">
                {office.districtName || office.districtId}
              </Badge> */}
        </div>
      ),
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (role: any) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            onClick={() => {
              setEditing(role);
              setForm({
                postId: role.postId,
                officeId: role.officeId,
                isAuthorise: role.isAuthorise,
              });
              setDialogOpen(true);
            }}
            className="h-8 w-8 p-0"
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button
            size="sm"
            variant="ghost"
            onClick={() => handleDelete(role.id)}
            className="h-8 w-8 p-0 text-destructive"
          >
            <Trash2 className="h-4 w-4" />
          </Button>
        </div>
      ),
    },
  ];
  const officeComponent = (
    <div className="my-3">
      <Label>Select an Office</Label>
      <Select
        value={selectedOfficeId}
        onValueChange={(val) => {
          setSelectedOfficeId(val);
          setPage(1);
        }}
      >
        <SelectTrigger className="w-72">
          <SelectValue placeholder="Select Office" />
        </SelectTrigger>
        <SelectContent>
          {(offices ?? []).map((o) => (
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
      {offices.length === 0 ? (
        <div className="flex flex-col items-center justify-center h-96 bg-muted/30 rounded-lg border-2 border-dashed">
          <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
          <h3 className="text-lg font-medium text-muted-foreground">
            No offices found
          </h3>
          <p className="text-sm text-muted-foreground mt-1">
            Please add an office first to manage posts
          </p>
          <Link to="/admin/officemanagement">
            <Button className="mt-4">Go to Office Management</Button>
          </Link>
        </div>
      ) : selectedOfficeId ? (
        <ModernDataTable
          title="Office Post Management"
          description="Create and manage office posts"
          data={paginated}
          columns={columns}
          loading={loading}
          search={search}
          onSearchChange={(value) => {
            setSearch(value);
            setPage(1);
          }}
          page={page}
          totalPages={totalPages}
          onPageChange={setPage}
          onAdd={() => {
            setEditing(null);
            setForm({
              postId: "none",
              officeId: selectedOfficeId,
              isAuthorise: false,
            });
            setDialogOpen(true);
          }}
          addLabel="Add Post"
          emptyMessage="No posts found"
          pageSize={pageSize}
          filterComponent={officeComponent}
        />
      ) : (
        <div className="flex flex-col items-center justify-center h-64 bg-muted/30 rounded-lg border-2 border-dashed">
          <Building2 className="h-12 w-12 text-muted-foreground mb-4" />
          {/* <h3 className="text-lg font-medium text-muted-foreground">
            Select an Office
          </h3> */}
          {officeComponent}
          <p className="text-sm text-muted-foreground my-1">
            Choose an office above to view and manage its posts
          </p>
        </div>
      )}

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "Edit Post" : "Create Post"}</DialogTitle>
          </DialogHeader>

          <div className="space-y-4">
            <div>
              <Label htmlFor="post-select">Post Name</Label>
              <Select
                value={form.postId}
                onValueChange={(value) =>
                  setForm({ ...form, postId: value })
                }
              >
                <SelectTrigger id="post-select">
                  <SelectValue placeholder="Select post..." />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="none" disabled>None</SelectItem>
                  {posts.map((post) => (
                    <SelectItem key={post.id} value={post.id}>
                      {post.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div>
              <Label htmlFor="office-select">Office</Label>
              <Select value={form.officeId || "none"} disabled>
                <SelectTrigger id="office-select">
                  <SelectValue placeholder="Select office..." />
                </SelectTrigger>
                <SelectContent>
                  {offices.map((office) => (
                    <SelectItem key={office.id} value={office.id}>
                      {office.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div>
              <div className="flex items-center space-x-2 mt-2">
                <Checkbox
                  id={"Certificate"}
                  checked={form.isAuthorise}
                  onCheckedChange={() =>
                    setForm({ ...form, isAuthorise: !form.isAuthorise })
                  }
                />
                <label
                  htmlFor={`Certificate`}
                  className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
                >
                  Can Certificate Generate
                </label>
              </div>
            </div>

            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setDialogOpen(false)}>
                Cancel
              </Button>
              <Button
                onClick={handleSave}
                disabled={
                  !form.postId || !form.officeId || isCreating || isUpdating
                }
              >
                {editing ? "Update" : "Create"}
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
