export function validateEmail(email:string) { return !!email && /@/.test(email); }
export function validateMobile(mobile:string) { return !!mobile && /^[6-9]\d{9}$/.test(mobile); }
export function validatePincode(pin:string) { return !!pin && /^[0-9]{6}$/.test(pin); }

export function getEmailError(email:string) {
  if (!email) return "Email required";
  if (!/@/.test(email)) return "Invalid email";
  return "";
}
export function getMobileError(mobile:string) {
  if (!mobile) return "Mobile required";
  if (!/^[6-9]\d{9}$/.test(mobile)) return "Invalid mobile";
  return "";
}
export function getPincodeError(pin:string) {
  if (!pin) return "Pincode required";
  if (!/^[0-9]{6}$/.test(pin)) return "Invalid pincode";
  return "";
}
export function getPanCardError(pan:string) {
  if (!pan) return "";
  if (!/^[A-Z]{5}[0-9]{4}[A-Z]$/.test(pan)) return "Invalid PAN format";
  return "";
}
export function formatPanCard(pan:string) {
  return (pan || "").toUpperCase().replace(/[^A-Z0-9]/g,'').slice(0,10);
}
