// src/hooks/useLocationMasters.ts
import { useState, useCallback } from "react";
import { API_BASE_URL } from "@/services/api/base";

export interface Office {
  id: string;
  name: string;
}
export interface Role {
  id: string;
  name: string;
}

export function useOfficeMasters() {
  const [offices, setOffices] = useState<Office[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);

  /**
   * Fetch offices from backend.
   * Accepts raw array (like your sample) or { success: true, data: [...] } shapes.
   */
  const fetchOffices = useCallback(async (): Promise<Office[]> => {
    const res = await fetch(`${API_BASE_URL}/offices`);
    if (!res.ok) throw new Error(`fetchOffices failed: ${res.status}`);
    const json = await res.json();

    // Accept both raw-array and wrapped forms
    const arr = Array.isArray(json) ? json : (json?.data ?? json?.result ?? []);
    const normalized: Office[] = (arr || []).map((d: any) => ({
      id: String(d.id),
      name: d.name,
    }));
    setOffices(normalized);
    return normalized;
  }, []);

  /**
   * Fetch roles for a office. Adjust query param / route if your API uses another pattern.
   */
  const fetchRoles = useCallback(async (officeId?: string): Promise<Role[]> => {
    if (!officeId) {
      setRoles([]);
      return [];
    }
    const url = `${API_BASE_URL}/roles?officeId=${encodeURIComponent(officeId)}`;
    const res = await fetch(url);
    if (!res.ok) throw new Error(`fetchRoles failed: ${res.status}`);
    const json = await res.json();
    const arr = Array.isArray(json) ? json : (json?.data ?? json?.result ?? []);
    const normalized: Role[] = (arr || []).map((c: any) => ({ id: String(c.id), name: c.name }));
    setRoles(normalized);
    return normalized;
  }, []);

  return { offices, roles, fetchOffices, fetchRoles };
}

export default useOfficeMasters;
