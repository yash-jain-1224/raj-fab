// src/hooks/useValidation.ts
import { useState } from "react";

export type ValidatorFn = () => string | null | undefined;
export type ValidatorMap = Record<string, ValidatorFn | undefined>;

export default function useValidation<T extends Record<string, any>>(opts: {
  values: T;
  setValues: React.Dispatch<React.SetStateAction<T>>;
}) {
  const { values, setValues } = opts;
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [touched, setTouched] = useState<Record<string, boolean>>({});

  const setField = <K extends keyof T>(field: K, value: T[K]) => {
    setValues(prev => ({ ...prev, [String(field)]: value }));
    // clear touched? keep touched true on change
    setTouched(prev => ({ ...prev, [String(field)]: true }));
  };

  const handleBlur = (field: string) => {
    setTouched(prev => ({ ...prev, [field]: true }));
  };
const setError = (field: string, message: string | null) => {
  setErrors((prev) => {
    const next = { ...prev };
    if (message) next[field] = message;
    else delete next[field];
    return next;
  });
};

  const clearFieldError = (field: string) => {
    setErrors(prev => {
      if (!(field in prev)) return prev;
      const next = { ...prev };
      delete next[field];
      return next;
    });
  };

  const validateFields = (validators: ValidatorMap): boolean => {
    const nextErrors: Record<string, string> = {};
    for (const key of Object.keys(validators)) {
      const fn = validators[key];
      if (!fn) continue;
      const result = fn();
      if (result) nextErrors[key] = result;
    }
    setErrors(nextErrors);
    // mark touched for all validated fields
    const newTouched = { ...touched };
    for (const k of Object.keys(validators)) newTouched[k] = true;
    setTouched(newTouched);
    return Object.keys(nextErrors).length === 0;
  };

  const validateField = (field: string, fn?: ValidatorFn) => {
    if (!fn) return true;
    const r = fn();
    setErrors(prev => {
      const next = { ...prev };
      if (r) next[field] = r;
      else delete next[field];
      return next;
    });
    setTouched(prev => ({ ...prev, [field]: true }));
    return !r;
  };

  const validateAll = (validators: ValidatorMap) => validateFields(validators);

  return {
    values,
    setValues,
    setField,
    errors,
    setErrors,
    touched,
    setTouched,
    handleBlur,
    clearFieldError,
    validateFields,
    validateField,
    validateAll,
    setError,
  };
}
