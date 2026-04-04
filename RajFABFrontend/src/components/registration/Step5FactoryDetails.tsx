// src/components/registration/Step5FactoryDetails.tsx
import React from "react";
import { Label } from "@/components/ui/label";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Textarea } from "@/components/ui/textarea";
import type { RegistrationData } from "@/pages/user/NewRegistration";

type Props = {
  formData: RegistrationData;
  updateFormData: (k: keyof RegistrationData, v: any) => void;
  districts: any[];
  policeStations?: any[];
  railwayStations?: any[];
  factoryTypes?: any[];
  poList: any[];
  isPinLoading: boolean;
  pinError: string | null;
  errors: Record<string, string>;
};

export default function Step5FactoryDetails({
  formData,
  updateFormData,
  districts,
  policeStations = [],
  railwayStations = [],
  factoryTypes = [],
  poList,
  isPinLoading,
  pinError,
  errors
}: Props) {
  return (
    <div className="space-y-8">
      <div>
        <h3 className="text-xl font-semibold mb-4">Factory Registration - Map Approval Form</h3>
        <p className="text-muted-foreground mb-6">Please fill out all the required details for factory map approval and registration.</p>
      </div>

      {/* Occupier Details */}
      <div className="space-y-6">
        <h4 className="text-lg font-semibold text-primary border-b pb-2">1. Details of Occupier / Applicant</h4>
        <div className="grid md:grid-cols-2 gap-6">
          <div>
            <Label htmlFor="occupierName">Full Name *</Label>
            <Input
              id="occupierName"
              value={formData.occupierName}
              onChange={(e) => updateFormData('occupierName', e.target.value)}
              className="mt-2"
              placeholder="Enter occupier name"
            />
            {errors.occupierName && <p className="text-sm text-red-500 mt-1">{errors.occupierName}</p>}
          </div>

          <div>
            <Label htmlFor="fatherNameOccupier">Father's Name *</Label>
            <Input
              id="fatherNameOccupier"
              value={formData.fatherNameOccupier}
              onChange={(e) => updateFormData('fatherNameOccupier', e.target.value)}
              className="mt-2"
              placeholder="Enter father's name"
            />
            {errors.fatherNameOccupier && <p className="text-sm text-red-500 mt-1">{errors.fatherNameOccupier}</p>}
          </div>

          <div>
            <Label htmlFor="designation">Designation *</Label>
            <Input
              id="designation"
              value={formData.designation}
              onChange={(e) => updateFormData('designation', e.target.value)}
              className="mt-2"
              placeholder="e.g., Director, Manager, Owner"
            />
            {errors.designation && <p className="text-sm text-red-500 mt-1">{errors.designation}</p>}
          </div>

          <div>
            <Label htmlFor="emailOccupier">Email *</Label>
            <Input
              id="emailOccupier"
              type="email"
              value={formData.emailOccupier}
              onChange={(e) => updateFormData('emailOccupier', e.target.value)}
              className="mt-2"
              placeholder="Enter email address"
            />
            {errors.emailOccupier && <p className="text-sm text-red-500 mt-1">{errors.emailOccupier}</p>}
          </div>

          <div>
            <Label htmlFor="mobileOccupier">Mobile Number *</Label>
            <Input
              id="mobileOccupier"
              value={formData.mobileOccupier}
              onChange={(e) => updateFormData('mobileOccupier', e.target.value)}
              className="mt-2"
              placeholder="Enter 10-digit mobile number"
              maxLength={10}
            />
            {errors.mobileOccupier && <p className="text-sm text-red-500 mt-1">{errors.mobileOccupier}</p>}
          </div>

          <div>
            <Label htmlFor="occupierPanCard">PAN Card *</Label>
            <Input
              id="occupierPanCard"
              value={formData.occupierPanCard}
              onChange={(e) => updateFormData('occupierPanCard', e.target.value.toUpperCase())}
              className="mt-2"
              placeholder="e.g., ABCDE1234F"
              maxLength={10}
            />
            {errors.occupierPanCard && <p className="text-sm text-red-500 mt-1">{errors.occupierPanCard}</p>}
          </div>
        </div>

        {/* Occupier Address */}
        <div className="grid md:grid-cols-2 gap-6">
          <div>
            <Label htmlFor="plotNoOccupier">Plot/House No. *</Label>
            <Input
              id="plotNoOccupier"
              value={formData.plotNoOccupier}
              onChange={(e) => updateFormData('plotNoOccupier', e.target.value)}
              className="mt-2"
              placeholder="Enter plot/house number"
            />
            {errors.plotNoOccupier && <p className="text-sm text-red-500 mt-1">{errors.plotNoOccupier}</p>}
          </div>

          <div>
            <Label htmlFor="streetLocalityOccupier">Street/Locality *</Label>
            <Input
              id="streetLocalityOccupier"
              value={formData.streetLocalityOccupier}
              onChange={(e) => updateFormData('streetLocalityOccupier', e.target.value)}
              className="mt-2"
              placeholder="Enter street/locality"
            />
            {errors.streetLocalityOccupier && <p className="text-sm text-red-500 mt-1">{errors.streetLocalityOccupier}</p>}
          </div>

          <div>
            <Label htmlFor="cityTownOccupier">City/Town *</Label>
            <Input
              id="cityTownOccupier"
              value={formData.cityTownOccupier}
              onChange={(e) => updateFormData('cityTownOccupier', e.target.value)}
              className="mt-2"
              placeholder="Enter city/town"
            />
            {errors.cityTownOccupier && <p className="text-sm text-red-500 mt-1">{errors.cityTownOccupier}</p>}
          </div>

          <div>
            <Label htmlFor="districtOccupier">District *</Label>
            <Select
              value={String(formData.districtOccupier)}
              onValueChange={(value) => updateFormData('districtOccupier', value)}
            >
              <SelectTrigger className="mt-2">
                <SelectValue placeholder="Select district" />
              </SelectTrigger>
              <SelectContent>
                {districts.map(d => <SelectItem key={d.id} value={String(d.id)}>{d.name}</SelectItem>)}
              </SelectContent>
            </Select>
            {errors.districtOccupier && <p className="text-sm text-red-500 mt-1">{errors.districtOccupier}</p>}
          </div>

          <div>
            <Label htmlFor="pincodeOccupier">Pincode *</Label>
            <Input
              id="pincodeOccupier"
              value={formData.pincodeOccupier}
              onChange={(e) => updateFormData('pincodeOccupier', e.target.value)}
              className="mt-2"
              placeholder="Enter 6-digit pincode"
              maxLength={6}
            />
            {errors.pincodeOccupier && <p className="text-sm text-red-500 mt-1">{errors.pincodeOccupier}</p>}
          </div>
        </div>
      </div>

      {/* Factory Details */}
      <div className="space-y-6">
        <h4 className="text-lg font-semibold text-primary border-b pb-2">2. Details of Factory</h4>
        <div className="grid md:grid-cols-2 gap-6">
          <div>
            <Label htmlFor="fullNameOfFactory">Full Name of Factory *</Label>
            <Input
              id="fullNameOfFactory"
              value={formData.fullNameOfFactory}
              onChange={(e) => updateFormData('fullNameOfFactory', e.target.value)}
              className="mt-2"
              placeholder="Enter full factory name"
            />
            {errors.fullNameOfFactory && <p className="text-sm text-red-500 mt-1">{errors.fullNameOfFactory}</p>}
          </div>

          <div>
            <Label htmlFor="plotFactory">Plot/Survey No. *</Label>
            <Input
              id="plotFactory"
              value={formData.plotFactory}
              onChange={(e) => updateFormData('plotFactory', e.target.value)}
              className="mt-2"
              placeholder="Enter plot/survey number"
            />
            {errors.plotFactory && <p className="text-sm text-red-500 mt-1">{errors.plotFactory}</p>}
          </div>

          <div>
            <Label htmlFor="streetLocalityFactory">Street/Locality *</Label>
            <Input
              id="streetLocalityFactory"
              value={formData.streetLocalityFactory}
              onChange={(e) => updateFormData('streetLocalityFactory', e.target.value)}
              className="mt-2"
              placeholder="Enter street/locality"
            />
            {errors.streetLocalityFactory && <p className="text-sm text-red-500 mt-1">{errors.streetLocalityFactory}</p>}
          </div>

          <div>
            <Label htmlFor="cityTownFactory">City/Town *</Label>
            <Input
              id="cityTownFactory"
              value={formData.cityTownFactory}
              onChange={(e) => updateFormData('cityTownFactory', e.target.value)}
              className="mt-2"
              placeholder="Enter city/town"
            />
            {errors.cityTownFactory && <p className="text-sm text-red-500 mt-1">{errors.cityTownFactory}</p>}
          </div>

          <div>
            <Label htmlFor="pincodeFactory">Pincode *</Label>
            <Input
              id="pincodeFactory"
              value={formData.pincodeFactory}
              onChange={(e) => updateFormData('pincodeFactory', e.target.value)}
              className="mt-2"
              placeholder="Enter 6-digit pincode"
              maxLength={6}
            />
            {isPinLoading && <p className="text-sm text-muted-foreground mt-1">Loading post offices...</p>}
            {pinError && <p className="text-sm text-red-500 mt-1">{pinError}</p>}
            {errors.pincodeFactory && <p className="text-sm text-red-500 mt-1">{errors.pincodeFactory}</p>}
          </div>

          <div>
            <Label htmlFor="areaFactory">Area (Post Office) *</Label>
            <Select
              value={formData.areaFactory}
              onValueChange={(value) => updateFormData('areaFactory', value)}
              disabled={!poList || poList.length === 0}
            >
              <SelectTrigger className="mt-2">
                <SelectValue placeholder={poList.length ? "Select post office" : "Enter valid pincode first"} />
              </SelectTrigger>
              <SelectContent>
                {poList.map((po: any) => (
                  <SelectItem key={po.Name} value={po.Name}>{po.Name}</SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.areaFactory && <p className="text-sm text-red-500 mt-1">{errors.areaFactory}</p>}
          </div>

          <div>
            <Label htmlFor="districtFactory">District *</Label>
            <Select
              value={String(formData.districtFactory)}
              onValueChange={(value) => updateFormData('districtFactory', value)}
            >
              <SelectTrigger className="mt-2">
                <SelectValue placeholder="Select district" />
              </SelectTrigger>
              <SelectContent>
                {districts.map(d => <SelectItem key={d.id} value={String(d.id)}>{d.name}</SelectItem>)}
              </SelectContent>
            </Select>
            {errors.districtFactory && <p className="text-sm text-red-500 mt-1">{errors.districtFactory}</p>}
          </div>

          <div>
            <Label htmlFor="policeStation">Police Station *</Label>
            <Select
              value={String(formData.policeStation)}
              onValueChange={(value) => updateFormData('policeStation', value)}
            >
              <SelectTrigger className="mt-2">
                <SelectValue placeholder="Select police station" />
              </SelectTrigger>
              <SelectContent>
                {policeStations.map((ps: any) => (
                  <SelectItem key={ps.id} value={String(ps.id)}>{ps.name}</SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.policeStation && <p className="text-sm text-red-500 mt-1">{errors.policeStation}</p>}
          </div>

          <div>
            <Label htmlFor="railwayStation">Railway Station *</Label>
            <Select
              value={String(formData.railwayStation)}
              onValueChange={(value) => updateFormData('railwayStation', value)}
            >
              <SelectTrigger className="mt-2">
                <SelectValue placeholder="Select railway station" />
              </SelectTrigger>
              <SelectContent>
                {railwayStations.map((rs: any) => (
                  <SelectItem key={rs.id} value={String(rs.id)}>{rs.name}</SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.railwayStation && <p className="text-sm text-red-500 mt-1">{errors.railwayStation}</p>}
          </div>

          <div>
            <Label htmlFor="areaInSquareMeter">Area (In Square Meter) *</Label>
            <Input
              id="areaInSquareMeter"
              type="number"
              value={formData.areaInSquareMeter}
              onChange={(e) => updateFormData('areaInSquareMeter', e.target.value)}
              className="mt-2"
              placeholder="Enter area in square meters"
            />
            {errors.areaInSquareMeter && <p className="text-sm text-red-500 mt-1">{errors.areaInSquareMeter}</p>}
          </div>

          <div>
            <Label htmlFor="businessRegistrationNumber">Business Registration Number</Label>
            <Input
              id="businessRegistrationNumber"
              value={formData.businessRegistrationNumber || ''}
              onChange={(e) => updateFormData('businessRegistrationNumber', e.target.value)}
              className="mt-2"
              placeholder="Enter business registration number (if applicable)"
            />
            {errors.businessRegistrationNumber && <p className="text-sm text-red-500 mt-1">{errors.businessRegistrationNumber}</p>}
          </div>
        </div>
      </div>

      {/* Manufacturing Process and Workers */}
      <div className="space-y-6">
        <h4 className="text-lg font-semibold text-primary border-b pb-2">3. Manufacturing Process & Workers</h4>
        <div className="grid md:grid-cols-2 gap-6">
          <div className="md:col-span-2">
            <Label htmlFor="manufacturingProcessName">Manufacturing Process Name *</Label>
            <Textarea
              id="manufacturingProcessName"
              value={formData.manufacturingProcessName}
              onChange={(e) => updateFormData('manufacturingProcessName', e.target.value)}
              className="mt-2"
              placeholder="Describe the manufacturing process"
              rows={3}
            />
            {errors.manufacturingProcessName && <p className="text-sm text-red-500 mt-1">{errors.manufacturingProcessName}</p>}
          </div>

          <div>
            <Label htmlFor="factoryType">Factory Type *</Label>
            <Select
              value={String(formData.factoryType)}
              onValueChange={(value) => updateFormData('factoryType', value)}
            >
              <SelectTrigger className="mt-2">
                <SelectValue placeholder="Select factory type" />
              </SelectTrigger>
              <SelectContent>
                {factoryTypes.map((ft: any) => (
                  <SelectItem key={ft.id} value={String(ft.id)}>{ft.name}</SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.factoryType && <p className="text-sm text-red-500 mt-1">{errors.factoryType}</p>}
          </div>

          <div>
            <Label htmlFor="totalNoOfShifts">Total Number of Shifts *</Label>
            <Input
              id="totalNoOfShifts"
              type="number"
              value={formData.totalNoOfShifts}
              onChange={(e) => updateFormData('totalNoOfShifts', e.target.value)}
              className="mt-2"
              placeholder="e.g., 1, 2, or 3"
              min="1"
              max="5"
            />
            {errors.totalNoOfShifts && <p className="text-sm text-red-500 mt-1">{errors.totalNoOfShifts}</p>}
          </div>

          <div>
            <Label htmlFor="totalNoOfWorkersMale">Male Workers *</Label>
            <Input
              id="totalNoOfWorkersMale"
              type="number"
              value={formData.totalNoOfWorkersMale}
              onChange={(e) => updateFormData('totalNoOfWorkersMale', e.target.value)}
              className="mt-2"
              placeholder="Number of male workers"
              min="0"
            />
            {errors.totalNoOfWorkersMale && <p className="text-sm text-red-500 mt-1">{errors.totalNoOfWorkersMale}</p>}
          </div>

          <div>
            <Label htmlFor="totalNoOfWorkersFemale">Female Workers *</Label>
            <Input
              id="totalNoOfWorkersFemale"
              type="number"
              value={formData.totalNoOfWorkersFemale}
              onChange={(e) => updateFormData('totalNoOfWorkersFemale', e.target.value)}
              className="mt-2"
              placeholder="Number of female workers"
              min="0"
            />
            {errors.totalNoOfWorkersFemale && <p className="text-sm text-red-500 mt-1">{errors.totalNoOfWorkersFemale}</p>}
          </div>

          <div>
            <Label htmlFor="totalNoOfWorkersTransgender">Transgender Workers *</Label>
            <Input
              id="totalNoOfWorkersTransgender"
              type="number"
              value={formData.totalNoOfWorkersTransgender}
              onChange={(e) => updateFormData('totalNoOfWorkersTransgender', e.target.value)}
              className="mt-2"
              placeholder="Number of transgender workers"
              min="0"
            />
            {errors.totalNoOfWorkersTransgender && <p className="text-sm text-red-500 mt-1">{errors.totalNoOfWorkersTransgender}</p>}
          </div>

          {formData.totalWorkers && (
            <div className="md:col-span-2">
              <Label>Total Workers</Label>
              <div className="mt-2 p-3 bg-muted rounded-md">
                <p className="text-lg font-semibold">{formData.totalWorkers}</p>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
