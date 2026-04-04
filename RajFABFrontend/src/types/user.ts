// src/types/user.ts
export type User = {
  id?: string;
  _id?: string;
  name: string;
  designation?: string;
  department?: string;
  epc?: string;
};

export type UserHierarchyDto = {
  id?: string;
  userId: string;
  reportsToId?: string | null;
  emergencyReportToId?: string | null;
  createdAt?: string;
  updatedAt?: string;
};
