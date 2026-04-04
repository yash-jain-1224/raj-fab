import { Form, FormField, FormItem, FormLabel } from "@/components/ui/form";
import { useForm } from "react-hook-form";
import * as z from "zod";
import { officeSchema } from "@/pages/admin/OfficeManagemnetPage";
import { Checkbox } from "../ui/checkbox";

type OfficeFormValues = z.infer<typeof officeSchema>;

interface AreaSelectorProps {
  form: ReturnType<typeof useForm<OfficeFormValues>>;
  fieldPrefix: "applicationArea" | "inspectionArea";
  divisions: { id: string; name: string }[];
  districts: { id: string; name: string; divisionId: string }[];
  cities: { id: string; name: string; districtId: string }[];
}

export const AreaSelector: React.FC<AreaSelectorProps> = ({
  form,
  fieldPrefix,
  divisions,
  districts,
  cities,
}) => {
  const selectedDivisionIds = form.watch(`${fieldPrefix}.divisionIds`) ?? [];
  const selectedDistrictIds = form.watch(`${fieldPrefix}.districtIds`) ?? [];
  const selectedCityIds = form.watch(`${fieldPrefix}.cityIds`) ?? [];

  return (
    <FormField
      control={form.control}
      name={`${fieldPrefix}.divisionIds`}
      render={({ field }) => (
        <FormItem>
          <FormLabel>Divisions -&gt; Districts -&gt; Areas</FormLabel>
          <div className="space-y-3 max-h-[60vh] overflow-y-auto border p-3 rounded-md">
            {divisions.map((div) => {
              const divisionChecked = selectedDivisionIds.includes(div.id);
              const divDistricts = districts.filter(
                (d) => d.divisionId === div.id
              );
              const divDistrictIds = divDistricts.map((d) => d.id);
              const divCities = cities.filter((c) =>
                divDistrictIds.includes(c.districtId)
              );
              const divCityIds = divCities.map((c) => c.id);

              return (
                <div key={div.id}>
                  {/* Division */}
                  <div className="flex items-center gap-2">
                    <Checkbox
                      checked={divisionChecked}
                      onCheckedChange={(checked) => {
                        const isChecked = Boolean(checked);

                        const updatedDivisions = isChecked
                          ? [...selectedDivisionIds, div.id]
                          : selectedDivisionIds.filter((x) => x !== div.id);
                        field.onChange(updatedDivisions);

                        // Update districts
                        form.setValue(
                          `${fieldPrefix}.districtIds`,
                          isChecked
                            ? Array.from(
                                new Set([
                                  ...selectedDistrictIds,
                                  ...divDistrictIds,
                                ])
                              )
                            : selectedDistrictIds.filter(
                                (id) => !divDistrictIds.includes(id)
                              )
                        );
                        // Update cities
                        form.setValue(
                          `${fieldPrefix}.cityIds`,
                          isChecked
                            ? Array.from(
                                new Set([...selectedCityIds, ...divCityIds])
                              )
                            : selectedCityIds.filter(
                                (id) => !divCityIds.includes(id)
                              )
                        );
                      }}
                    />
                    <span>{div.name}</span>
                  </div>

                  {/* Districts */}
                  {divisionChecked && (
                    <div className="ml-6 mt-2 space-y-2">
                      {divDistricts.map((d) => {
                        const districtChecked = selectedDistrictIds.includes(
                          d.id
                        );
                        const districtCities = cities.filter(
                          (c) => c.districtId === d.id
                        );

                        return (
                          <div key={d.id}>
                            <div className="flex items-center gap-2">
                              <Checkbox
                                checked={districtChecked}
                                onCheckedChange={(checked) => {
                                  const isChecked = Boolean(checked);
                                  const updatedDistricts = isChecked
                                    ? [...selectedDistrictIds, d.id]
                                    : selectedDistrictIds.filter(
                                        (x) => x !== d.id
                                      );
                                  form.setValue(
                                    `${fieldPrefix}.districtIds`,
                                    updatedDistricts
                                  );
                                  const districtCityIds = districtCities.map(
                                    (c) => c.id
                                  );
                                  form.setValue(
                                    `${fieldPrefix}.cityIds`,
                                    isChecked
                                      ? Array.from(
                                          new Set([
                                            ...selectedCityIds,
                                            ...districtCityIds,
                                          ])
                                        )
                                      : selectedCityIds.filter(
                                          (id) => !districtCityIds.includes(id)
                                        )
                                  );
                                }}
                              />
                              <span>{d.name}</span>
                            </div>

                            {/* Cities */}
                            {districtChecked && (
                              <div className="ml-6 mt-1 space-y-1">
                                {districtCities.map((city) => {
                                  const cityChecked = selectedCityIds.includes(
                                    city.id
                                  );
                                  return (
                                    <div
                                      key={city.id}
                                      className="flex items-center gap-2"
                                    >
                                      <Checkbox
                                        checked={cityChecked}
                                        onCheckedChange={(checked) => {
                                          const isChecked = Boolean(checked);
                                          const updatedCities = isChecked
                                            ? [...selectedCityIds, city.id]
                                            : selectedCityIds.filter(
                                                (x) => x !== city.id
                                              );
                                          form.setValue(
                                            `${fieldPrefix}.cityIds`,
                                            updatedCities
                                          );
                                        }}
                                      />
                                      <span>{city.name}</span>
                                    </div>
                                  );
                                })}
                              </div>
                            )}
                          </div>
                        );
                      })}
                    </div>
                  )}
                </div>
              );
            })}
          </div>
        </FormItem>
      )}
    />
  );
};
