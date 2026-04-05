import { useState, useEffect, useCallback } from 'react';
import { useDivisions, useDistricts, useCities, useAreas } from '@/hooks/api';
import { divisionApi } from '@/services/api/divisions';
import type { Division } from '@/services/api/divisions';
import { District, districtApi } from '@/services/api/districts';
import { City, cityApi } from '@/services/api/cities';
import { Area, areaApi } from '@/services/api/areas';
import { Tehsil, tehsilApi } from '@/services/api/tehsils';

interface CascadingLocationState {
  divisions: Division[];
  districts: District[];
  cities: City[];
  areas: Area[];
  isLoadingDivisions: boolean;
  isLoadingDistricts: boolean;
  isLoadingCities: boolean;
  isLoadingAreas: boolean;
  selectedDivisionId: string;
  selectedDistrictId: string;
  selectedCityId: string;
  selectedAreaId: string;
}

export function useCascadingLocations() {
  const { divisions, isLoading: isLoadingDivisions } = useDivisions();
    const { districts, isLoading: isLoadingDistricts } = useDistricts()
  
  // const [districts, setDistricts] = useState<District[]>([]);
  const [cities, setCities] = useState<City[]>([]);
  const [tehsils, setTehsils] = useState<Tehsil[]>([]);

  // const [isLoadingDistricts, setIsLoadingDistricts] = useState(false);
  const [isLoadingCities, setIsLoadingCities] = useState(false);
  const [isLoadingTehsils, setIsLoadingTehsils] = useState(false);

  // const fetchDistrictsByDivision = async (divisionId: string) => {
  //   if (!divisionId) {
  //     setDistricts([]);
  //     return;
  //   }

  //   setIsLoadingDistricts(true);
  //   try {
  //     const data = await districtApi.getByDivision(divisionId);
  //     setDistricts(data);
  //   } finally {
  //     setIsLoadingDistricts(false);
  //   }
  // };

  const fetchCitiesByDistrict = async (districtId: string) => {
    if (!districtId) {
      setCities([]);
      return;
    }

    setIsLoadingCities(true);
    try {
      const data = await cityApi.getByDistrict(districtId);
      setCities(data);
    } finally {
      setIsLoadingCities(false);
    }
  };

  const fetchTehsilsByDistrict = async (districtId: string) => {
    if (!districtId) {
      setTehsils([]);
      return;
    }

    setIsLoadingTehsils(true);
    try {
      const data = await tehsilApi.getByDistrict(districtId);
      setTehsils(data);
    } finally {
      setIsLoadingTehsils(false);
    }
  };

  return {
    divisions,
    districts,
    cities,
    tehsils,

    isLoadingDivisions,
    isLoadingDistricts,
    isLoadingCities,
    isLoadingTehsils,

    fetchDistrictsByDivision: (_divisionId: string) => { /* districts loaded globally via useDistricts() */ },
    fetchCitiesByDistrict,
    fetchTehsilsByDistrict
  };
}

