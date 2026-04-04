// src/hooks/useLocationMasters.ts
import { useState, useCallback } from "react";
import { API_BASE_URL } from "@/services/api/base";

export interface District {
  id: string;
  name: string;
  divisionId?: string;
  divisionName?: string;
}
export interface City {
  id: string;
  name: string;
}

export function useLocationMasters() {
  const [districts, setDistricts] = useState<District[]>([]);
  const [cities, setCities] = useState<City[]>([]);

  /**
   * Fetch districts from backend.
   * Accepts raw array (like your sample) or { success: true, data: [...] } shapes.
   */
  const fetchDistricts = useCallback(async (): Promise<District[]> => {
    const res = await fetch(`${API_BASE_URL}/district`);
    if (!res.ok) throw new Error(`fetchDistricts failed: ${res.status}`);
    const json = await res.json();

    // Accept both raw-array and wrapped forms
    const arr = Array.isArray(json) ? json : (json?.data ?? json?.result ?? []);
    const normalized: District[] = (arr || []).map((d: any) => ({
      id: String(d.id),
      name: d.name,
      divisionId: d.divisionId ? String(d.divisionId) : undefined,
      divisionName: d.divisionName,
    }));
    setDistricts(normalized);
    return normalized;
  }, []);

  /**
   * Fetch cities for a district. Adjust query param / route if your API uses another pattern.
   */
  const fetchCities = useCallback(async (districtId?: string): Promise<City[]> => {
    if (!districtId) {
      setCities([]);
      return [];
    }
    const url = `${API_BASE_URL}/cities?districtId=${encodeURIComponent(districtId)}`;
    const res = await fetch(url);
    if (!res.ok) throw new Error(`fetchCities failed: ${res.status}`);
    const json = await res.json();
    const arr = Array.isArray(json) ? json : (json?.data ?? json?.result ?? []);
    const normalized: City[] = (arr || []).map((c: any) => ({ id: String(c.id), name: c.name }));
    setCities(normalized);
    return normalized;
  }, []);

  return { districts, cities, fetchDistricts, fetchCities };
}

export default useLocationMasters;
