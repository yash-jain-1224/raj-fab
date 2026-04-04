// utils/validation.ts

// ----------------------
// BASIC VALIDATORS
// ----------------------

// ==========================
// FILE VALIDATION (CLIENT-SIDE)
// ==========================
export function validateFilesClient(
  files: File[] | FileList | null | undefined,
  options?: {
    required?: boolean;
    maxSizeMB?: number;
    allowedTypes?: string[];
  }
): string | null {
  if (!files || (Array.isArray(files) && files.length === 0)) {
    if (options?.required) return "This file is required";
    return null;
  }

  // Convert FileList to File[]
  const fileArray: File[] = Array.from(files as FileList);

  const maxSizeBytes = (options?.maxSizeMB || 25) * 1024 * 1024;

  for (const file of fileArray) {
    // file is now strongly typed as File
    if (file.size > maxSizeBytes) {
      return `File size must be less than ${options?.maxSizeMB || 25} MB`;
    }

    if (options?.allowedTypes && !options.allowedTypes.includes(file.type)) {
      return `Invalid file type: ${file.type}`;
    }
  }

  return null; // valid
}



export function validatePositiveNumber(
  value: string,
  fieldName = "This field"
) {
  if (!value || value.trim() === "") {
    return `${fieldName} is required`;
  }

  const num = parseFloat(value);
  if (isNaN(num) || num <= 0) {
    return `${fieldName} must be a positive number`;
  }

  return null;
}

// ==========================
// INTEGER VALIDATION (UPGRADED)
// ==========================
export function validateInteger(
  value: string,
  fieldName = "This field",
  options?: { allowZero?: boolean }
) {
  if (!value || value.trim() === "") {
    return `${fieldName} is required`;
  }

  if (!/^\d+$/.test(value)) {
    return `${fieldName} must be a valid integer`;
  }

  const num = parseInt(value, 10);

  if (!options?.allowZero && num === 0) {
    return `${fieldName} cannot be zero`;
  }

  return null;
}
export const validateRequired = (value: any, msg: string) =>
  !value || String(value).trim() === "" ? msg : null;

export const validateEmail = (value: string) => {
  if (!value) return "Email is required";
  const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return re.test(value.trim()) ? null : "Enter a valid email";
};

export const validateMobile = (value: string) => {
  if (!value) return "Mobile number is required";
  const v = value.replace(/\D/g, "");
  return /^[6-9]\d{9}$/.test(v) ? null : "Enter a valid 10-digit mobile number";
};

export const validatePincode = (value: string) => {
  if (!value) return "Pincode is required";
  return /^\d{6}$/.test(value) ? null : "Enter a valid 6-digit pincode";
};

export const validateWebsiteUrl = (value: string) => {
  if (!value) return "Website URL is required";

  const urlPattern = /^(https?:\/\/)?([\w-]+\.)+[\w-]{2,}(\/[\w-./?%&=]*)?$/i;

  return urlPattern.test(value) ? null : "Enter a valid website URL";
};

export const validatePanCard = (value: string) => {
  if (!value) return null;
  const re = /^[A-Z]{5}[0-9]{4}[A-Z]$/;
  return re.test(value.toUpperCase()) ? null : "Invalid PAN format (ABCDE1234F)";
};

export const formatPanCard = (v: string) =>
  v ? v.toUpperCase().replace(/[^A-Z0-9]/g, "").slice(0, 10) : "";

export const validateText = (value: string, msg: string, max = 1000) => {
  if (!value) return msg;
  if (value.length > max) return `Maximum ${max} characters allowed.`;
  return null;
};

export const validateName = (value: string, fieldName = "Name") => {
  if (!value || value.trim() === "") return `${fieldName} is required`;
  if (value.length < 2) return `${fieldName} must be at least 2 characters`;
  if (value.length > 100) return `${fieldName} must be less than 100 characters`;
  if (!/^[a-zA-Z\s.'-]+$/.test(value)) return `${fieldName} can only contain letters, spaces, and basic punctuation`;
  return null;
};

export const validateDate = (value: string, fieldName = "Date") => {
  if (!value) return `${fieldName} is required`;
  const date = new Date(value);
  if (isNaN(date.getTime())) return `Enter a valid ${fieldName.toLowerCase()}`;
  return null;
};

export const validateDateOfBirth = (value: string) => {
  if (!value) return "Date of birth is required";
  const date = new Date(value);
  if (isNaN(date.getTime())) return "Enter a valid date of birth";

  const today = new Date();
  const age = today.getFullYear() - date.getFullYear();
  const monthDiff = today.getMonth() - date.getMonth();
  const adjustedAge = monthDiff < 0 || (monthDiff === 0 && today.getDate() < date.getDate()) ? age - 1 : age;

  if (adjustedAge < 18) return "Must be at least 18 years old";
  if (adjustedAge > 100) return "Please enter a valid date of birth";

  return null;
};

export const validateGender = (value: string) => {
  if (!value || value.trim() === "") return "Gender is required";
  return null;
};

export const validateSelect = (value: string | number, fieldName = "This field") => {
  if (!value || value === "" || value === "0") return `${fieldName} is required`;
  return null;
};

export const validateArea = (value: string, fieldName = "Area") => {
  if (!value) return `${fieldName} is required`;
  const num = parseFloat(value);
  if (isNaN(num) || num <= 0) return `${fieldName} must be a positive number`;
  if (num > 1000000) return `${fieldName} seems unreasonably large`;
  return null;
};

export const validateDesignation = (value: string) => {
  if (!value || value.trim() === "") return "Designation is required";
  if (value.length < 2) return "Designation must be at least 2 characters";
  if (value.length > 100) return "Designation must be less than 100 characters";
  return null;
};

export const validateAddress = (value: string, fieldName = "Address") => {
  if (!value || value.trim() === "") return `${fieldName} is required`;
  if (value.length < 3) return `${fieldName} must be at least 3 characters`;
  if (value.length > 200) return `${fieldName} must be less than 200 characters`;
  return null;
};

export const validateShifts = (value: string) => {
  if (!value || value.trim() === "") return "Number of shifts is required";
  const num = parseInt(value, 10);
  if (isNaN(num) || num < 1) return "Number of shifts must be at least 1";
  if (num > 5) return "Number of shifts cannot exceed 5";
  return null;
};

export const validateWorkerCount = (value: string, fieldName = "Worker count") => {
  if (!value || value.trim() === "") return `${fieldName} is required`;
  const num = parseInt(value, 10);
  if (isNaN(num) || num < 0) return `${fieldName} must be 0 or greater`;
  if (num > 100000) return `${fieldName} seems unreasonably large`;
  return null;
};

export const validateMaterials = (materials: any[]) => {
  if (!materials || materials.length === 0) return "At least one raw material is required";
  return null;
};

export const validateProducts = (products: any[]) => {
  if (!products || products.length === 0) return "At least one product is required";
  return null;
};

export const validateBusinessRegistration = (value: string, required: boolean = false) => {
  if (!required && !value) return null;
  if (required && (!value || value.trim() === "")) return "Business registration number is required";
  if (value && value.length < 5) return "Business registration number must be at least 5 characters";
  if (value && value.length > 50) return "Business registration number must be less than 50 characters";
  return null;
};

export const validateFactoryRegistration = (value: string, required: boolean = false) => {
  if (!required && !value) return null;
  if (required && (!value || value.trim() === "")) return "Factory registration number is required";
  if (value && value.length < 5) return "Factory registration number must be at least 5 characters";
  if (value && value.length > 50) return "Factory registration number must be less than 50 characters";
  return null;
};

// Single utility to run validators:
export const runValidators = (...validators: (string | null)[]) => {
  for (const v of validators) if (v) return v;
  return null;
};
