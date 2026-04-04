import { useState } from "react";

export function usePincodeLookup() {
  const [poList, setPoList] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function lookup(pin: string) {
    if (!/^[0-9]{6}$/.test(pin)) {
      setPoList([]);
      return { postOffices: [] };
    }
    setIsLoading(true);
    try {
      const res = await fetch(`https://api.postalpincode.in/pincode/${pin}`);
      const data = await res.json();
      const entry = Array.isArray(data) ? data[0] : null;
      if (!entry || entry.Status !== "Success" || !Array.isArray(entry.PostOffice)) {
        setPoList([]);
        setError("No Post Offices found");
        return { postOffices: [] };
      }
      setPoList(entry.PostOffice);
      return { postOffices: entry.PostOffice };
    } catch (e) {
      setPoList([]);
      setError("Unable to lookup pincode");
      return { postOffices: [] };
    } finally {
      setIsLoading(false);
    }
  }

  return { poList, isLoading, error, lookup };
}
