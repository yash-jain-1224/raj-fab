import React, { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { useMutation } from "@tanstack/react-query";
import { Plus, FileText, Award, Clock, AlertCircle } from "lucide-react";
import { useNavigate } from "react-router-dom";
import { useUsers } from "@/hooks/api";
import { useAuth } from "@/utils/AuthProvider";

const CategorySelection = () => {
  const [selectedCategory, setSelectedCategory] = useState<string | null>(null);
  const { user } = useAuth();
  const { updateUserFields } = useUsers();
  const navigate = useNavigate();

  const categories = [
    {
      title: "Registration, Renewal, Map Approval, Manufacturing",
      description:
        "Choose this section if you want to apply for registration, renewal, map approval of factories/boilers or get approvals for manufacturing.",
      category: "registration_renewal_map_approval_manufacturing",
      IsActive: true,
      routes:[
        {name:"Factory Registration",path:"/user/factory-registration"},
        {name:"Factory Renewal",path:"/user/factory-renewal"},
        {name:"Boiler Registration",path:"/user/boiler-registration"},
      ]
    },
    {
      title: "Third Party Competent Person",
      description:
        "Choose this section if you want to get certified as a competent person under the Factories Act, 1948.",
      category: "third_party_competent_person",
      IsActive: true,
    },
    // {
    //   title: "Exam Candidates",
    //   description:
    //     "Choose this section if you want to apply to appear for Boiler Attendant/ Boiler Operation Engineer Examination.",
    //   category: "exam_candidates",
    //   IsActive: false,
    // },
    {
      title: "Third Party Boiler Engineer",
      description:
        "Choose this section if you want to apply as Third Party Boiler Engineer (For Boiler Operation Engineer Only).",
      category: "third_party_boiler_engineer",
      IsActive: true,
    },
    // {
    //   title: "Hazardous Waste & E-Waste Entry",
    //   description:
    //     "Factory not covered under the Factories Act, 1948 or individual worker registration.",
    //   category: "hazardous_e_waste_entry",
    //   IsActive: false,
    // },
    {
      title: "Third Party Factory",
      description:
        "Choose this section if you want to apply as Third Party Factory under the Factories Act, 1948.",
      category: "third_party_factory",
      IsActive: true,
    },
    {
      title: "Third Party BOCW Registration",
      description:
        "Choose this section if you want to apply as Third Party BOCW Registration.",
      category: "third_party_bocw_registration",
      IsActive: true,
    },
    {
      title: "Third Party Safety Museum & Training Registration",
      description:
        "Choose this section if you want to apply as Third Party Safety Museum & Training Registration.",
      category: "third_party_safety_museum_training_registration",
      IsActive: true,
    },
  ];

  const handleCategorySelection = async (category: string, i: number) => {
    setSelectedCategory(category);
    try {
      const res = updateUserFields({
        field :"CitizenCategory",
        value: category
      });
      console.log("Category update result:", res);
      i == 1 ? navigate("/user/verify-registration", { replace: true }) : navigate("/user", { replace: true });
    } catch (error) {
      console.error("Error updating category:", error);
    }
  };

  return (
    <div className="space-y-6 container mx-auto p-4">
      <h1 className="text-3xl font-bold tracking-tight">Choose Category</h1>
      <p className="text-muted-foreground">
        Please select a category to proceed with your application.
      </p>
      {selectedCategory && (
        <div className="mt-8">
          {!selectedCategory ? (
            <div className="text-center">Processing your selection...</div>
          ) : (
            <div>
              <h2 className="text-2xl font-bold">
                You selected: {selectedCategory}
              </h2>
            </div>
          )}
        </div>
      )}

      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-2 xl:grid-cols-4">
        {categories.map((cat, i) => (
          <Card
            key={cat.category}
            className="border p-4 space-y-4 cursor-pointer hover:shadow-lg"
            onClick={() => cat.IsActive && handleCategorySelection(cat.category, i)}
          >
            <CardHeader>
              <CardTitle className="text-xl font-semibold">
                {cat.title}
              </CardTitle>
            </CardHeader>
            <CardContent>
              <p className="text-sm text-muted-foreground">{cat.description}</p>
              <Button className="mt-3" variant="outline">
                <FileText className="mr-2 h-4 w-4" />
                Select This Category
              </Button>
            </CardContent>
          </Card>
        ))}
      </div>
    </div>
  );
};

export default CategorySelection;
