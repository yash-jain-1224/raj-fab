import React, { createContext, useContext } from 'react';
import { useCascadingLocations } from '@/hooks/useCascadingLocations';
import type { Division } from '@/services/api/divisions';
import type { District } from '@/services/api/districts';
import type { City } from '@/services/api/cities';
import type { Area } from '@/services/api/areas';
import { Tehsil } from '@/services/api/tehsils';

interface LocationContextType {
  divisions: Division[];
  districts: District[];
  cities: City[];
  tehsils: Tehsil[];

  isLoadingDivisions: boolean;
  isLoadingDistricts: boolean;
  isLoadingCities: boolean;
  isLoadingTehsils: boolean;

  fetchDistrictsByDivision: (divisionId: string) => void;
  fetchCitiesByDistrict: (districtId: string) => void;
  fetchTehsilsByDistrict: (districtId: string) => void;
}

const LocationContext = createContext<LocationContextType | undefined>(undefined);

export function LocationProvider({ children }: { children: React.ReactNode }) {
  const value = useCascadingLocations();

  return (
    <LocationContext.Provider value={value}>
      {children}
    </LocationContext.Provider>
  );
}

export function useLocationContext() {
  const ctx = useContext(LocationContext);
  if (!ctx) {
    throw new Error("useLocationContext must be used within LocationProvider");
  }
  return ctx;
}

