import { useFactoryClosuresByRegistration } from './api/useFactoryClosures';

/**
 * Hook to check if a factory is closed
 * Returns true if factory has an approved or closed closure record
 */
export function useFactoryClosureStatus(factoryRegistrationId: string) {
  const { data: closures, isLoading } = useFactoryClosuresByRegistration(factoryRegistrationId);
  
  const isClosed = closures?.some(
    closure => 
      closure.status?.toLowerCase() === 'approved' || 
      closure.status?.toLowerCase() === 'closed'
  ) || false;
  
  return { isClosed, isLoading, closures };
}

/**
 * Get closure info for multiple factories
 */
export function useMultipleFactoriesClosureStatus(factoryIds: string[]) {
  const closureStatuses = factoryIds.map(id => ({
    factoryId: id,
    ...useFactoryClosureStatus(id)
  }));
  
  return closureStatuses;
}
