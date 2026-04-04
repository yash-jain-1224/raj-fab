import { BaseApiService } from "./base";
import type {
  WorkerRange,
  CreateWorkerRangeRequest,
} from "@/types/workerRanges";

export class WorkerRangeApiService extends BaseApiService {
  async getAll(): Promise<WorkerRange[]> {
    const response = await this.request<
      WorkerRange[] | { data: WorkerRange[] }
    >("/worker-range");
    return Array.isArray(response) ? response : response.data ?? [];
  }

  async create(data: CreateWorkerRangeRequest): Promise<WorkerRange> {
    return this.request<WorkerRange>("/worker-range", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async update(
    id: string,
    data: CreateWorkerRangeRequest
  ): Promise<WorkerRange> {
    return this.request<WorkerRange>(`/worker-range/${id}/update`, {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async delete(id: string): Promise<void> {
    await this.request<void>(`/worker-range/${id}/delete`, {
      method: "POST",
    });
  }
}

export const workerRangeApi = new WorkerRangeApiService();
