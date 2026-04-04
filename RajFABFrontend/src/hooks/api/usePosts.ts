import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { postApi } from '@/services/api/posts';
import { Post, CreatePostRequest } from '@/services/api/posts';
import { useToast } from '@/hooks/use-toast';

export function usePosts() {
  const { toast } = useToast();
  const queryClient = useQueryClient();

  const {
    data: posts = [],
    isLoading,
    error
  } = useQuery({
    queryKey: ['posts'],
    queryFn: () => postApi.getAll(),
  });

  const createMutation = useMutation({
    mutationFn: (data: CreatePostRequest) => postApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['posts'] });
      toast({
        title: "Success",
        description: "Post created successfully",
      });
    },
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message,
        variant: "destructive",
      });
    },
  });

  const updateMutation = useMutation({
    mutationFn: ({ id, data }: { id: string; data: CreatePostRequest }) => 
      postApi.update(id, data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['posts'] });
      toast({
        title: "Success",
        description: "Post updated successfully",
      });
    },
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message,
        variant: "destructive",
      });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: (id: string) => postApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['posts'] });
      toast({
        title: "Success",
        description: "Post deleted successfully",
      });
    },
    onError: (error: Error) => {
      toast({
        title: "Error",
        description: error.message,
        variant: "destructive",
      });
    },
  });

  return {
    posts,
    isLoading,
    error,
    createPost: createMutation.mutate,
    updatePost: updateMutation.mutate,
    deletePost: deleteMutation.mutate,
    isCreating: createMutation.isPending,
    isUpdating: updateMutation.isPending,
    isDeleting: deleteMutation.isPending,
  };
}

export function usePost(id: string) {
  return useQuery({
    queryKey: ['posts', id],
    queryFn: () => postApi.getById(id),
    enabled: !!id,
  });
}