import { FC } from "react";
import {
  Popover,
  PopoverTrigger,
  PopoverContent,
} from "@/components/ui/popover";
import { Command, CommandGroup, CommandItem } from "@/components/ui/command";
import { Badge } from "@/components/ui/badge";
import { ChevronsUpDown, Check, LucideIcon } from "lucide-react";
import { cn } from "@/lib/utils";

export interface MultiSelectOption {
  id: string | number;
  name: string;
}

interface MultiSelectProps {
  label?: string;
  icon?: LucideIcon;
  options: MultiSelectOption[];
  value: (string | number)[];
  onChange: (selected: (string | number)[]) => void;
  placeholder?: string;
  disabled?: boolean;
}

export const MultiSelect: FC<MultiSelectProps> = ({
  label,
  icon: Icon,
  options = [],
  value = [],
  onChange,
  placeholder = "Select",
  disabled = false,
}) => {

  const toggleSelect = (id: string | number) => {
    let newValues: (string | number)[] = [];

    if (value.includes(id)) {
      newValues = value.filter((v) => v !== id);
    } else {
      newValues = [...value, id];
    }

    onChange(newValues);
  };

  return (
    <div className="space-y-2 mb-3 w-full">
      {label && (
        <label className="text-sm font-medium flex items-center gap-2">
          {Icon && <Icon className="h-4 w-4" />} {label}
        </label>
      )}

      <Popover>
        <PopoverTrigger asChild disabled={disabled}>
          <button
            type="button"
            className={cn(
              "flex min-h-[38px] w-full justify-between items-center rounded-md border px-3 py-2 text-sm cursor-pointer bg-background",
              disabled && "opacity-50 cursor-not-allowed"
            )}
          >
            <div className="flex flex-wrap gap-1 items-center">
              {value.length === 0 && (
                <span className="opacity-50">{placeholder}</span>
              )}

              {value.map((id) => {
                const item = options.find((opt) => opt.id === id);
                return (
                  <Badge
                    key={id}
                    variant="secondary"
                    className="rounded-sm px-2 py-0.5"
                  >
                    {item?.name}
                  </Badge>
                );
              })}
            </div>

            <ChevronsUpDown className="h-4 w-4 opacity-50 shrink-0" />
          </button>
        </PopoverTrigger>

        <PopoverContent className="w-[300px] p-0">
          <Command>
            <CommandGroup>
              {options.map((opt) => (
                <CommandItem
                  key={opt.id}
                  value={String(opt.id)}
                  onSelect={() => toggleSelect(opt.id)}
                  className="flex items-center"
                >
                  <Check
                    className={cn(
                      "mr-2 h-4 w-4",
                      value.includes(opt.id) ? "opacity-100" : "opacity-0"
                    )}
                  />
                  {opt.name}
                </CommandItem>
              ))}
            </CommandGroup>
          </Command>
        </PopoverContent>
      </Popover>
    </div>
  );
};
