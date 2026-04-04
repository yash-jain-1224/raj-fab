export interface FactoryCategory {
  id: string;
  name: string;
  factoryTypeId: string;
  workerRangeId: string;
  factoryTypeName: string;
  workerRangeLabel: string;
}

export interface CreateFactoryCategoryRequest {
  name: string;
  factoryTypeId: string;
  workerRangeId: string;
}