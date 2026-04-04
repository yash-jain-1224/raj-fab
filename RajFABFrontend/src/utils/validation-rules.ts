// ----------------------
// Email Validation
// ----------------------
export function validateEmail(value: string): string | null {
  if (!value) return "Email is required";

  const regex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
  if (!regex.test(value)) return "Invalid email format";

  return null;
}

// ----------------------
// Mobile Validation
// ----------------------
export function validateMobile(value: string): string | null {
  if (!value) return "Mobile number is required";

  const regex = /^[6-9]\d{9}$/;
  if (!regex.test(value)) return "Enter a valid 10-digit Indian mobile number";

  return null;
}

// ----------------------
// Pincode Validation
// ----------------------
export function validatePincode(value: string): string | null {
  if (!value) return "Pincode is required";

  const regex = /^[1-9][0-9]{5}$/;
  if (!regex.test(value)) return "Invalid pincode format";

  return null;
}

// ----------------------
// PAN Card Validation
// ----------------------
export function validatePanCard(value: string): string | null {
  if (!value) return null; // optional field

  const regex = /^[A-Z]{5}[0-9]{4}[A-Z]{1}$/;
  if (!regex.test(value)) return "Invalid PAN format";

  return null;
}

// ----------------------
// Required Field
// ----------------------
export function validateRequired(value: any, message = "This field is required") {
  if (value === null || value === undefined || value === "") return message;
  return null;
}

// ----------------------
// Text-only Field
// ----------------------
export function validateText(value: string): string | null {
  if (!value) return "Required";

  if (!/^[a-zA-Z\s]+$/.test(value)) return "Only letters allowed";

  return null;
}

// ----------------------
// PAN Formatter
// ----------------------
export function formatPanCard(value: string): string {
  return value
    .toUpperCase()
    .replace(/[^A-Z0-9]/gi, "")
    .slice(0, 10);
}
