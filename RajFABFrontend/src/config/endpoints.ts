export const API_BASE =
  import.meta.env.VITE_ESTABLISHMENT_API_BASE ?? "http://localhost:5000/api";
  // import.meta.env.VITE_ESTABLISHMENT_API_BASE ?? "http://10.70.234.214:5000/api";

export const ESTABLISHMENT_CREATE_PATH = "/establishment/create";
export const FACTORY_MAP_APPROVAL_PATH = "/FactoryMapApprovals";
export const COMMENCEMENT_CESSATIONS_PATH = "/CommencementCessations";
export const ESTABLISHMENT_FETCH_PATH = "/establishment";
export const NON_HAZARDOUS_FACTORY_REGISTRATION_PATH = "/NonHazardousFactoryRegistration";
export const APPLICATION__REGISTRATION_PATH = "/ApplicationRegistration";
export const APPLICATIONS_BY_USER_PATH = "/byuser";