// Re-export all API services for easy importing
export * from './base';
export * from './modules';
export * from './forms';
export * from './submissions';
export * from './occupiers';
export * from './factoryMapApprovals';
export * from './factoryRegistrations';
export * from './factoryTypes';
export * from './roles';
export * from './users';
export * from './policeStations';
export * from './railwayStations';
export * from './divisions';
export * from './districts';
export * from './areas';
export * from './cities';
export * from './boilers';
export * from './feeCalculation';
export * from './factoryClosures';
export * from './factoryCommenceAndCessation';

// master data exports
export * from './acts';
export * from './rules';
export * from './posts';
export * from './workerRanges';
export * from './offices';
export * from './posts';
export * from './factoryCategories';
export * from './officeLevels';
export * from './officePostLevel';
export * from './appeal';
export * from './brnDetails';
export * from './tehsils'
export * from './eSign'
export * from './competentPerson'
export * from './boilerCategories';

// Legacy exports for backward compatibility
export { occupierApi } from './occupiers';
export { factoryMapApprovalApi as factoryMapApi } from './factoryMapApprovals';

// Factory Registration legacy exports
export const createFactoryRegistration = async (data: any) => {
  const { applicationRegistrationApi: factoryRegistrationApi } = await import('./factoryRegistrations');
  return factoryRegistrationApi.create(data);
};

export const uploadFactoryRegistrationDocument = async (registrationId: string, file: File, documentType: string) => {
  const { applicationRegistrationApi: factoryRegistrationApi } = await import('./factoryRegistrations');
  return factoryRegistrationApi.uploadDocument(registrationId, file, documentType);
};

export const getFactoryMapApprovalByAcknowledgementNumber = async (acknowledgementNumber: string) => {
  const { factoryMapApprovalApi } = await import('./factoryMapApprovals');
  return factoryMapApprovalApi.getByAcknowledgementNumber(acknowledgementNumber);
};