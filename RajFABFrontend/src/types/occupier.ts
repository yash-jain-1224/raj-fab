export interface Occupier {
  id: string;
  firstName: string;
  lastName: string;
  fatherName: string;
  dateOfBirth: string;
  gender: string;
  email: string;
  mobileNo: string;
  panCard?: string;
  plotNo: string;
  streetLocality: string;
  villageTownCity: string;
  district: string;
  pincode: string;
  designation?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOccupierRequest {
  firstName: string;
  lastName: string;
  fatherName: string;
  dateOfBirth: string;
  gender: string;
  email: string;
  mobileNo: string;
  panCard?: string;
  plotNo: string;
  streetLocality: string;
  villageTownCity: string;
  district: string;
  pincode: string;
  designation?: string;
}