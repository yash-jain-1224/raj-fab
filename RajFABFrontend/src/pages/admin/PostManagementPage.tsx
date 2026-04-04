import { useState, useMemo } from "react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Edit, Trash2, MapPin } from "lucide-react";
import { ModernDataTable } from "@/components/admin/ModernDataTable";
import { usePosts } from "@/hooks/api";
import type { Post } from "@/services/api/posts";

export default function PostManagementPage() {
  const { posts, isLoading: loading, createPost, updatePost, deletePost } = usePosts();

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editing, setEditing] = useState<Post | null>(null);
  const [form, setForm] = useState({ name: "", seniorityOrder: 0 });

  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);
  const pageSize = 10;

  const handleSave = () => {
    if (editing) {
      updatePost({ id: editing.id, data: form });
    } else {
      createPost(form);
    }
    setDialogOpen(false);
  };

  const handleDelete = (id: string) => {
    if (confirm("Delete this post?")) {
      deletePost(id);
    }
  };

  const filtered = useMemo(
    () => posts.filter((d) => d.name.toLowerCase().includes(search.toLowerCase())),
    [posts, search]
  );
  const totalPages = Math.ceil(filtered.length / pageSize);
  const paginated = filtered.slice((page - 1) * pageSize, page * pageSize);

  const columns = [
    {
      key: "index",
      header: "#",
      className: "w-16",
      render: (_: Post, index: number) => (
        <span className="font-medium text-muted-foreground">{index}</span>
      ),
    },
    {
      key: "name",
      header: "Post Name",
      render: (post: Post) => (
        <div className="flex items-center gap-3">
          <div className="h-8 w-8 rounded-full bg-primary/10 flex items-center justify-center">
            <MapPin className="h-4 w-4 text-primary" />
          </div>
          <span className="font-medium">{post.name}</span>
        </div>
      ),
    },
    {
      key: "seniorityOrder",
      header: "Seniority Order",
      className: "w-36",
      render: (post: Post) => {
        const order = post.seniorityOrder ?? 0;
        const label = order === 0 ? "Not set" : order === 10 ? `${order} (Highest)` : order === 1 ? `${order} (Lowest)` : `${order}`;
        return <span className="text-sm tabular-nums">{label}</span>;
      },
    },
    {
      key: "actions",
      header: "Actions",
      className: "text-right",
      render: (post: Post) => (
        <div className="flex items-center justify-end gap-2">
          <Button
            size="sm"
            variant="ghost"
            onClick={() => {
              setEditing(post);
              setForm({ name: post.name, seniorityOrder: post.seniorityOrder ?? 0 });
              setDialogOpen(true);
            }}
            className="h-8 w-8 p-0"
          >
            <Edit className="h-4 w-4" />
          </Button>
          <Button 
            size="sm" 
            variant="ghost" 
            onClick={() => handleDelete(post.id)}
            className="h-8 w-8 p-0 text-destructive"
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
        title="Post Management"
        description="Manage administrative posts"
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
          setForm({ name: "", seniorityOrder: 0 });
          setDialogOpen(true);
        }}
        addLabel="Add Post"
        emptyMessage="No posts found"
        pageSize={pageSize}
      />

      <Dialog open={dialogOpen} onOpenChange={setDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>{editing ? "Edit Post" : "Create Post"}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <Label>Name</Label>
              <Input value={form.name} onChange={(e) => setForm({ ...form, name: e.target.value })} />
            </div>
            <div>
              <Label>Seniority Order</Label>
              <Select
                value={form.seniorityOrder.toString()}
                onValueChange={(val) => setForm({ ...form, seniorityOrder: Number(val) })}
              >
                <SelectTrigger className="mt-1">
                  <SelectValue placeholder="Select seniority level" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="1">1 — Lowest Authority</SelectItem>
                  <SelectItem value="2">2</SelectItem>
                  <SelectItem value="3">3</SelectItem>
                  <SelectItem value="4">4</SelectItem>
                  <SelectItem value="5">5 — Mid Level</SelectItem>
                  <SelectItem value="6">6</SelectItem>
                  <SelectItem value="7">7</SelectItem>
                  <SelectItem value="8">8</SelectItem>
                  <SelectItem value="9">9</SelectItem>
                  <SelectItem value="10">10 — Highest Authority</SelectItem>
                </SelectContent>
              </Select>
              <p className="text-xs text-muted-foreground mt-1">
                Higher = more senior. Used as fallback approver when no workflow is configured.
              </p>
            </div>
            <div className="flex justify-end gap-2 mt-4">
              <Button variant="outline" onClick={() => setDialogOpen(false)}>
                Cancel
              </Button>
              <Button onClick={handleSave} disabled={!form.name}>{editing ? "Update" : "Create"}</Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
