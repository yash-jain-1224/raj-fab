export const APPLICATION_STATUS = {
  PENDING: 'Pending',
  UNDER_REVIEW: 'Under Review',
  APPROVED: 'Approved',
  REJECTED: 'Rejected',
  RETURNED_TO_APPLICANT: 'Returned to Applicant',
  OBJECTION_RAISED: 'Objection Raised',
  SUBMITTED: 'Submitted'
} as const;

 export const getStatusColor = (status: string) => {
    if (!status) return "outline";
    const normalized = normalizeStatus(status);
    switch (normalized) {
      case APPLICATION_STATUS.APPROVED:
        return "default";
      case APPLICATION_STATUS.RETURNED_TO_APPLICANT:
      case APPLICATION_STATUS.OBJECTION_RAISED:
      case APPLICATION_STATUS.REJECTED:
        return "destructive";
      case APPLICATION_STATUS.UNDER_REVIEW:
      case APPLICATION_STATUS.SUBMITTED:
        return "secondary";
      default:
        return "outline";
    }
  };

/**
 * Normalize various status strings to standardized values
 * This handles backend inconsistencies like "Returned" vs "Returned to Applicant"
 */
export function normalizeStatus(status: string | undefined | null): string {
  if (!status) return APPLICATION_STATUS.UNDER_REVIEW;
  
  const normalized = status.toLowerCase().trim();
  
  // Handle "Returned" or "Returned to Applicant"
  if (normalized.includes('return')) {
    return APPLICATION_STATUS.RETURNED_TO_APPLICANT;
  }
  
  if (normalized.includes('reject')) {
    return APPLICATION_STATUS.REJECTED;
  }
  
  if (normalized.includes('approv')) {
    return APPLICATION_STATUS.APPROVED;
  }
  
  if (normalized.includes('objection')) {
    return APPLICATION_STATUS.OBJECTION_RAISED;
  }
  
  if (normalized.includes('review')) {
    return APPLICATION_STATUS.UNDER_REVIEW;
  }
  
  // if (normalized.includes('pending')) {
  //   return APPLICATION_STATUS.PENDING;
  // }
  if (normalized.includes('pending')) {
    return APPLICATION_STATUS.SUBMITTED;
}
  if (normalized.includes('submit')) {
    return APPLICATION_STATUS.SUBMITTED;
  }
  
  // Return original if no match found
  return status;
}
