import { useState } from "react";

export function usePincodeLookup() {
  const [poList, setPoList] = useState([]);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState(null);

  async function lookup(pin) {
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
        setError("No post offices");
        return { postOffices: [] };
      }
      setPoList(entry.PostOffice);
      return { postOffices: entry.PostOffice };
    } catch (e) {
      setError("Lookup failed");
      return { postOffices: [] };
    } finally {
      setIsLoading(false);
    }
  }

  return { poList, isLoading, error, lookup };
}
