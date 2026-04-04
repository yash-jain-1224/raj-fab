/* ============================================
   FIELD VALIDATION UTILITIES
   ============================================ */

export const isValidEmail = (email: string): boolean => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  return emailRegex.test(email);
};

export const isValidPhone = (phone: string): boolean => {
  const cleanPhone = phone.replace(/\D/g, "");
  return cleanPhone.length === 10;
};

export const isEmpty = (val: any): boolean => {
  return val === null || val === undefined || val === "" || val === 0;
};

export const getFieldError = (
  fieldName: string,
  value: any,
  fieldType: "text" | "email" | "phone" | "number" | "required" = "required"
): string | null => {
  if (isEmpty(value)) {
    return `${fieldName} is required`;
  }

  if (fieldType === "email" && value && !isValidEmail(value)) {
    return `Please enter a valid ${fieldName.toLowerCase()}`;
  }

  if (fieldType === "phone" && value && !isValidPhone(value)) {
    return `${fieldName} must be 10 digits`;
  }

  if (fieldType === "number" && value < 0) {
    return `${fieldName} must be a positive number`;
  }

  return null;
};

export const validateEmail = (email: string): string | null => {
  if (isEmpty(email)) {
    return "Email is required";
  }
  if (!isValidEmail(email)) {
    return "Please enter a valid email";
  }
  return null;
};

export const validatePhone = (phone: string): string | null => {
  if (isEmpty(phone)) {
    return "Phone number is required";
  }
  if (!isValidPhone(phone)) {
    return "Phone must be 10 digits";
  }
  return null;
};

export const validateRequired = (value: any, fieldName: string): string | null => {
  if (isEmpty(value)) {
    return `${fieldName} is required`;
  }
  return null;
};
