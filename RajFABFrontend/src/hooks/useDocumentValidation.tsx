/**
 * Replace factoryTypesMock with actual API integration if available.
 * For now it mirrors original expectations.
 */
const factoryTypesMock = [
  { id: "hazardous-process", documents: [{ key: "safety-plan", name: "Safety Plan", required: true }, { key: "layout", name: "Layout", required: true }] },
  { id: "default", documents: [{ key: "layout", name: "Layout", required: false }] }
];

export function useDocumentValidation() {
  function getFactoryTypeMeta(factoryTypeId: string) {
    return factoryTypesMock.find(f => f.id === factoryTypeId) || factoryTypesMock[1];
  }

  function getMissing(factoryTypeId: string, uploadedDocuments: Record<string, File[]>) {
    const meta = getFactoryTypeMeta(factoryTypeId);
    const missing: string[] = [];
    meta.documents.forEach(d => {
      if (d.required) {
        const uploaded = uploadedDocuments?.[d.key];
        if (!uploaded || uploaded.length === 0) missing.push(d.name);
      }
    });
    return missing;
  }

  return { getMissing };
}
