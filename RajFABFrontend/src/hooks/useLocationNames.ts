import { useMemo } from 'react';
import type { District } from '@/services/api/districts';
import type { Area } from '@/services/api/areas';
import type { PoliceStation } from '@/services/api/policeStations';
import type { RailwayStation } from '@/services/api/railwayStations';

export function useLocationNames(
  districts: District[] | undefined,
  areas: Area[] | undefined,
  policeStations?: PoliceStation[] | undefined,
  railwayStations?: RailwayStation[] | undefined
) {
  return useMemo(() => {
    const getDistrictName = (id: string | undefined | null) => {
      if (!id || !districts) return id || '';
      const district = districts.find(d => d.id === id);
      return district?.name || id;
    };

    const getAreaName = (id: string | undefined | null) => {
      if (!id || !areas) return id || '';
      const area = areas.find(a => a.id === id);
      return area?.name || id;
    };

    const getPoliceStationName = (id: string | undefined | null) => {
      if (!id || !policeStations) return id || '';
      const station = policeStations.find(p => p.id === id);
      return station?.name || id;
    };

    const getRailwayStationName = (id: string | undefined | null) => {
      if (!id || !railwayStations) return id || '';
      const station = railwayStations.find(r => r.id === id);
      return station?.name || id;
    };

    return {
      getDistrictName,
      getAreaName,
      getPoliceStationName,
      getRailwayStationName,
    };
  }, [districts, areas, policeStations, railwayStations]);
}
