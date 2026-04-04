interface RowProps {
  label: string;
  value?: any;
}

export function PreviewRow({ label, value }: RowProps) {
  return (
    <div className="grid grid-cols-3 border-b text-sm">
      <div className="p-2 font-semibold border-r bg-gray-100">
        {label}
      </div>
      <div className="p-2 col-span-2">
        {value || "-"}
      </div>
    </div>
  );
}
