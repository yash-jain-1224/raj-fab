import { BaseApiService } from './base';

export interface Post {
  id: string;
  name: string;
  seniorityOrder: number;
}

export interface CreatePostRequest {
  name: string;
  seniorityOrder: number;
}

export class PostApiService extends BaseApiService {
  async getAll(): Promise<Post[]> {
    const response = await this.request<Post[] | { data: Post[] }>('/post');
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async getById(id: string): Promise<Post> {
    return this.request<Post>(`/post/${id}`);
  }

  async create(post: CreatePostRequest): Promise<Post> {
    return this.request<Post>('/post', {
      method: 'POST',
      body: JSON.stringify(post),
    });
  }

  async update(id: string, post: CreatePostRequest): Promise<Post> {
    return this.request<Post>(`/post/${id}/update`, {
      method: 'POST',
      body: JSON.stringify(post),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/post/${id}/delete`, {
      method: 'POST',
    });
  }
}

export const postApi = new PostApiService();