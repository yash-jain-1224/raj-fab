import * as LucideIcons from "lucide-react";
import { Card, CardHeader, CardTitle } from "@/components/ui/card";
import { Building2 } from "lucide-react";
import type { LucideIcon } from "lucide-react";


type FormCardProps = {
  title: string;
  subTitle?: string;
  ruleText?: string;
  description?: string;
  icon?: keyof typeof LucideIcons; // 👈 string-based icon name
};


const FormCard = ({
  title,
  subTitle,
  ruleText,
  description,
  icon,
}: FormCardProps) => {
  const IconComponent: LucideIcon =
    (icon && LucideIcons[icon] as LucideIcon);

  return (
    <Card className="bg-primary text-primary-foreground">
      <CardHeader className="text-center space-y-1">
        <div className="flex items-center gap-3">
          {/* <IconComponent className="h-8 w-8" /> */}

          <div className="flex flex-col items-center text-center w-full">
            <CardTitle className="text-3xl font-semibold">{title}</CardTitle>

            {subTitle && <p className="text-xl font-semibold">{subTitle}</p>}
            {ruleText && <p className="text-blue-100">{ruleText}</p>}
            {description && (
              <p className="text-sm text-primary-foreground">{description}</p>
            )}
          </div>
        </div>
      </CardHeader>
    </Card>
  );
};

export default FormCard;

