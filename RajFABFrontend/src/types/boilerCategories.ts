export interface BoilerCategory {
  id: number;
  name: string;
  heatingSurfaceArea: number;
  isActive: boolean;
  createdAt: string; // ISO date string
  updatedAt: string; // ISO date string
}

export interface CreateBoilerCategoryRequest {
  name: string;
  heatingSurfaceArea: number;
  isActive?: boolean;
}

export interface UpdateBoilerCategoryRequest {
  id: number;
  name: string;
  heatingSurfaceArea: number;
  isActive: boolean;
}