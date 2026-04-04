/**
 * Simple document validation helper
 * Expects factoryType metadata shape:
 * { id: string, documents: [{ key, name, required: boolean }] }
 *
 * For this example we mock factoryTypes but integration point is clear.
 */
const factoryTypesMock = [
  { id: "hazardous-process", documents: [{ key: "safety-plan", name: "Safety Plan", required: true }, { key: "layout", name: "Layout", required: true }] },
  { id: "default", documents: [{ key: "layout", name: "Layout", required: false }] }
];

export function useDocumentValidation() {
  function getFactoryTypeMeta(factoryTypeId) {
    return factoryTypesMock.find(f => f.id === factoryTypeId) || factoryTypesMock[1];
  }

  function getMissing(factoryTypeId, uploadedDocuments) {
    const meta = getFactoryTypeMeta(factoryTypeId);
    const missing = [];
    meta.documents.forEach(d => {
      if (d.required) {
        const uploaded = uploadedDocuments?.[d.key];
        if (!uploaded || uploaded.length === 0) missing.push(d.name);
      }
    });
    return missing;
  }

  return { validateDocuments: getMissing, getMissing };
}
