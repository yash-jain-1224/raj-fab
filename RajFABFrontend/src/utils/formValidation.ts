/**
 * Shared form validation utilities for all registration forms.
 */

export type ValidationErrors = Record<string, string>;

/** Check if a value is empty (null, undefined, or blank string) */
export const isEmpty = (value: unknown): boolean => {
  if (value === null || value === undefined) return true;
  if (typeof value === "string") return value.trim() === "";
  if (typeof value === "number") return false;
  return true;
};

/** Validate email format */
export const isValidEmail = (email: string): boolean =>
  /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);

/** Validate mobile number (10 digits) */
export const isValidMobile = (mobile: string): boolean =>
  /^\d{10}$/.test(mobile);

/** Validate PIN code (6 digits) */
export const isValidPinCode = (pin: string): boolean =>
  /^\d{6}$/.test(pin);

/** Validate phone number (10 digits) */
export const isValidPhone = (phone: string): boolean =>
  /^\d{10}$/.test(phone);

/**
 * Validate required fields in an object.
 * Returns a record of field -> error message for any empty fields.
 */
export const validateRequired = (
  data: Record<string, unknown>,
  requiredFields: string[],
): ValidationErrors => {
  const errors: ValidationErrors = {};
  for (const field of requiredFields) {
    if (isEmpty(data[field])) {
      errors[field] = "This field is required";
    }
  }
  return errors;
};

/**
 * Validate common contact fields (email, mobile, pinCode, telephone).
 * Only adds error if the field has a value but is invalid.
 */
export const validateContactFields = (
  data: Record<string, unknown>,
): ValidationErrors => {
  const errors: ValidationErrors = {};

  if (data.email && typeof data.email === "string" && !isValidEmail(data.email)) {
    errors.email = "Enter a valid email address";
  }
  if (data.mobile && typeof data.mobile === "string" && !isValidMobile(data.mobile)) {
    errors.mobile = "Mobile must be exactly 10 digits";
  }
  if (data.pinCode && typeof data.pinCode === "string" && !isValidPinCode(data.pinCode)) {
    errors.pinCode = "PIN Code must be exactly 6 digits";
  }
  if (data.telephone && typeof data.telephone === "string" && !isValidPhone(data.telephone)) {
    errors.telephone = "Telephone must be exactly 10 digits";
  }

  return errors;
};

/**
 * Combined validation: required fields + contact format checks.
 */
export const validateForm = (
  data: Record<string, unknown>,
  requiredFields: string[],
): ValidationErrors => {
  return {
    ...validateRequired(data, requiredFields),
    ...validateContactFields(data),
  };
};

/**
 * Check if there are any errors in a ValidationErrors object.
 */
export const hasErrors = (errors: ValidationErrors): boolean =>
  Object.keys(errors).length > 0;
