import { FormModule } from '@/types/forms';
import { BOILER_PERMISSIONS } from '@/types/boiler';

export const BOILER_MODULES: FormModule[] = [
  {
    id: 'boiler-registration',
    name: 'Boiler Registration',
    description: 'Register new boilers under Indian Boilers Act 1923',
    category: 'boiler-services',
    subcategory: 'registration',
    isActive: true,
    requiresAuth: true,
    allowedRoles: ['applicant', 'inspector', 'admin'],
    permissions: [
      BOILER_PERMISSIONS.VIEW_BOILER_RECORDS.code,
      BOILER_PERMISSIONS.REGISTER_BOILER.code,
    ],
    fields: [
      // Applicant Information Section
      {
        id: 'applicant-section',
        type: 'section',
        label: 'Applicant Information',
        required: true,
        order: 1,
        config: {
          collapsible: false,
          description: 'Owner/Applicant details as per IBR Form III'
        }
      },
      {
        id: 'owner-name',
        type: 'text',
        label: 'Owner/Organization Name',
        required: true,
        order: 2,
        validation: { minLength: 2, maxLength: 100 }
      },
      {
        id: 'contact-person',
        type: 'text',
        label: 'Contact Person',
        required: true,
        order: 3,
        validation: { minLength: 2, maxLength: 50 }
      },
      {
        id: 'mobile',
        type: 'tel',
        label: 'Mobile Number',
        required: true,
        order: 4,
        validation: { pattern: '^[6-9]\\d{9}$' }
      },
      {
        id: 'email',
        type: 'email',
        label: 'Email Address',
        required: true,
        order: 5
      },
      {
        id: 'address',
        type: 'textarea',
        label: 'Complete Address',
        required: true,
        order: 6,
        validation: { minLength: 10, maxLength: 300 }
      },
      
      // Boiler Specifications Section
      {
        id: 'specifications-section',
        type: 'section',
        label: 'Boiler Technical Specifications',
        required: true,
        order: 10,
        config: {
          collapsible: false,
          description: 'Technical details as per manufacturer specifications'
        }
      },
      {
        id: 'boiler-type',
        type: 'select',
        label: 'Boiler Type',
        required: true,
        order: 11,
        config: {
          options: [
            { value: 'fire-tube', label: 'Fire Tube Boiler' },
            { value: 'water-tube', label: 'Water Tube Boiler' },
            { value: 'electric', label: 'Electric Boiler' },
            { value: 'waste-heat', label: 'Waste Heat Boiler' },
            { value: 'other', label: 'Other (Specify in remarks)' }
          ]
        }
      },
      {
        id: 'manufacturer',
        type: 'text',
        label: 'Manufacturer Name',
        required: true,
        order: 12,
        validation: { minLength: 2, maxLength: 100 }
      },
      {
        id: 'serial-number',
        type: 'text',
        label: 'Manufacturer Serial Number',
        required: true,
        order: 13,
        validation: { minLength: 3, maxLength: 50 }
      },
      {
        id: 'year-of-manufacture',
        type: 'number',
        label: 'Year of Manufacture',
        required: true,
        order: 14,
        validation: { min: 1990, max: new Date().getFullYear() }
      },
      {
        id: 'working-pressure',
        type: 'number',
        label: 'Working Pressure (kg/cm²)',
        required: true,
        order: 15,
        validation: { min: 0.1, max: 100, step: 0.1 }
      },
      {
        id: 'design-pressure',
        type: 'number',
        label: 'Design Pressure (kg/cm²)',
        required: true,
        order: 16,
        validation: { min: 0.1, max: 150, step: 0.1 }
      },
      {
        id: 'steam-capacity',
        type: 'number',
        label: 'Steam Generation Capacity (tonnes/hour)',
        required: true,
        order: 17,
        validation: { min: 0.1, max: 1000, step: 0.1 }
      },
      {
        id: 'fuel-type',
        type: 'select',
        label: 'Fuel Type',
        required: true,
        order: 18,
        config: {
          options: [
            { value: 'coal', label: 'Coal' },
            { value: 'oil', label: 'Oil/Furnace Oil' },
            { value: 'gas', label: 'Natural Gas/LPG' },
            { value: 'biomass', label: 'Biomass/Agricultural Waste' },
            { value: 'electric', label: 'Electric' },
            { value: 'multi-fuel', label: 'Multi-fuel' }
          ]
        }
      },
      {
        id: 'heating-area',
        type: 'number',
        label: 'Heating Surface Area (m²)',
        required: true,
        order: 19,
        validation: { min: 1, max: 10000, step: 0.1 }
      },
      
      // Installation Location Section
      {
        id: 'location-section',
        type: 'section',
        label: 'Installation Location',
        required: true,
        order: 30,
        config: {
          collapsible: false,
          description: 'Boiler installation location details'
        }
      },
      {
        id: 'factory-name',
        type: 'text',
        label: 'Factory/Establishment Name',
        required: true,
        order: 31,
        validation: { minLength: 2, maxLength: 100 }
      },
      {
        id: 'factory-license',
        type: 'text',
        label: 'Factory License Number (if applicable)',
        required: false,
        order: 32,
        validation: { maxLength: 50 }
      },
      {
        id: 'plot-number',
        type: 'text',
        label: 'Plot/Survey Number',
        required: true,
        order: 33,
        validation: { maxLength: 50 }
      },
      {
        id: 'street',
        type: 'text',
        label: 'Street/Road',
        required: true,
        order: 34,
        validation: { maxLength: 100 }
      },
      {
        id: 'locality',
        type: 'text',
        label: 'Locality/Village',
        required: true,
        order: 35,
        validation: { maxLength: 100 }
      },
      {
        id: 'pincode',
        type: 'text',
        label: 'PIN Code',
        required: true,
        order: 36,
        validation: { pattern: '^[1-9][0-9]{5}$' }
      },
      {
        id: 'area-id',
        type: 'area-select',
        label: 'Area',
        required: true,
        order: 37
      },
      
      // Safety Features Section
      {
        id: 'safety-section',
        type: 'section',
        label: 'Safety Features & Equipment',
        required: true,
        order: 50,
        config: {
          collapsible: false,
          description: 'Boiler safety devices as per IBR requirements'
        }
      },
      {
        id: 'safety-valves-count',
        type: 'number',
        label: 'Number of Safety Valves',
        required: true,
        order: 51,
        validation: { min: 1, max: 10 }
      },
      {
        id: 'safety-valve-pressure',
        type: 'number',
        label: 'Safety Valve Setting Pressure (kg/cm²)',
        required: true,
        order: 52,
        validation: { min: 0.1, max: 150, step: 0.1 }
      },
      {
        id: 'water-gauges',
        type: 'number',
        label: 'Number of Water Level Gauges',
        required: true,
        order: 53,
        validation: { min: 1, max: 5 }
      },
      {
        id: 'pressure-gauges',
        type: 'number',
        label: 'Number of Pressure Gauges',
        required: true,
        order: 54,
        validation: { min: 1, max: 5 }
      },
      {
        id: 'blowdown-valves',
        type: 'number',
        label: 'Number of Blowdown Valves',
        required: true,
        order: 55,
        validation: { min: 1, max: 5 }
      },
      {
        id: 'feedwater-system',
        type: 'text',
        label: 'Feedwater System Description',
        required: true,
        order: 56,
        validation: { minLength: 5, maxLength: 200 }
      },
      {
        id: 'emergency-shutoff',
        type: 'checkbox',
        label: 'Emergency Shutoff System Provided',
        required: true,
        order: 57
      },
      
      // Operator Details Section
      {
        id: 'operator-section',
        type: 'section',
        label: 'Boiler Operator Details',
        required: true,
        order: 70,
        config: {
          collapsible: false,
          description: 'Certified boiler operator information'
        }
      },
      {
        id: 'operator-name',
        type: 'text',
        label: 'Operator Name',
        required: true,
        order: 71,
        validation: { minLength: 2, maxLength: 50 }
      },
      {
        id: 'operator-certificate',
        type: 'text',
        label: 'Operator Certificate Number',
        required: true,
        order: 72,
        validation: { minLength: 5, maxLength: 50 }
      },
      {
        id: 'operator-certificate-expiry',
        type: 'date',
        label: 'Operator Certificate Expiry Date',
        required: true,
        order: 73
      },
      
      // Document Upload Section
      {
        id: 'documents-section',
        type: 'section',
        label: 'Required Documents',
        required: true,
        order: 90,
        config: {
          collapsible: false,
          description: 'Upload all required documents for boiler registration'
        }
      },
      {
        id: 'manufacturing-certificate',
        type: 'file',
        label: 'Manufacturing Certificate',
        required: true,
        order: 91,
        config: {
          accept: '.pdf,.jpg,.jpeg,.png',
          maxSize: 5242880, // 5MB
          multiple: true,
          maxFiles: 3
        }
      },
      {
        id: 'technical-drawings',
        type: 'file',
        label: 'Technical Drawings',
        required: true,
        order: 92,
        config: {
          accept: '.pdf,.jpg,.jpeg,.png,.dwg',
          maxSize: 10485760, // 10MB
          multiple: true,
          maxFiles: 5
        }
      },
      {
        id: 'safety-valve-certificates',
        type: 'file',
        label: 'Safety Valve Certificates',
        required: true,
        order: 93,
        config: {
          accept: '.pdf,.jpg,.jpeg,.png',
          maxSize: 5242880,
          multiple: true,
          maxFiles: 3
        }
      },
      {
        id: 'operator-competency-certificate',
        type: 'file',
        label: 'Operator Competency Certificate',
        required: true,
        order: 94,
        config: {
          accept: '.pdf,.jpg,.jpeg,.png',
          maxSize: 5242880,
          multiple: false
        }
      },
      {
        id: 'location-approval',
        type: 'file',
        label: 'Location Approval from Local Authority',
        required: true,
        order: 95,
        config: {
          accept: '.pdf,.jpg,.jpeg,.png',
          maxSize: 5242880,
          multiple: true,
          maxFiles: 2
        }
      },
      {
        id: 'environmental-clearance',
        type: 'file',
        label: 'Environmental Clearance (if applicable)',
        required: false,
        order: 96,
        config: {
          accept: '.pdf,.jpg,.jpeg,.png',
          maxSize: 5242880,
          multiple: true,
          maxFiles: 2
        }
      },
      
      // Remarks Section
      {
        id: 'remarks-section',
        type: 'section',
        label: 'Additional Information',
        required: false,
        order: 100,
        config: { collapsible: true }
      },
      {
        id: 'remarks',
        type: 'textarea',
        label: 'Remarks/Additional Information',
        required: false,
        order: 101,
        validation: { maxLength: 500 }
      }
    ],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString()
  },
  
  {
    id: 'boiler-renewal',
    name: 'Boiler Certificate Renewal',
    description: 'Renew boiler inspection certificates under IBR',
    category: 'boiler-services',
    subcategory: 'renewal',
    isActive: true,
    requiresAuth: true,
    allowedRoles: ['applicant', 'inspector', 'admin'],
    permissions: [
      BOILER_PERMISSIONS.VIEW_BOILER_RECORDS.code,
      BOILER_PERMISSIONS.RENEW_CERTIFICATE.code,
    ],
    fields: [
      // Boiler Selection Section
      {
        id: 'boiler-selection-section',
        type: 'section',
        label: 'Boiler Identification',
        required: true,
        order: 1,
        config: {
          collapsible: false,
          description: 'Select the boiler for certificate renewal'
        }
      },
      {
        id: 'boiler-registration-number',
        type: 'text',
        label: 'Boiler Registration Number',
        required: true,
        order: 2,
        validation: { minLength: 5, maxLength: 50 }
      },
      {
        id: 'current-certificate-number',
        type: 'text',
        label: 'Current Certificate Number',
        required: true,
        order: 3,
        validation: { minLength: 5, maxLength: 50 }
      },
      {
        id: 'last-inspection-date',
        type: 'date',
        label: 'Last Inspection Date',
        required: true,
        order: 4
      },
      
      // Renewal Details Section
      {
        id: 'renewal-details-section',
        type: 'section',
        label: 'Renewal Details',
        required: true,
        order: 10,
        config: {
          collapsible: false,
          description: 'Provide details for certificate renewal'
        }
      },
      {
        id: 'renewal-reason',
        type: 'select',
        label: 'Reason for Renewal',
        required: true,
        order: 11,
        config: {
          options: [
            { value: 'annual', label: 'Annual Renewal' },
            { value: 'biennial', label: 'Biennial Renewal' },
            { value: 'expired', label: 'Expired Certificate' },
            { value: 'damage', label: 'Certificate Lost/Damaged' }
          ]
        }
      },
      {
        id: 'changes-from-last-inspection',
        type: 'textarea',
        label: 'Changes Since Last Inspection',
        required: true,
        order: 12,
        validation: { minLength: 10, maxLength: 500 }
      },
      
      // Updated Operator Details
      {
        id: 'operator-section',
        type: 'section',
        label: 'Current Boiler Operator',
        required: true,
        order: 20,
        config: {
          collapsible: false,
          description: 'Current certified boiler operator information'
        }
      },
      {
        id: 'operator-name',
        type: 'text',
        label: 'Operator Name',
        required: true,
        order: 21,
        validation: { minLength: 2, maxLength: 50 }
      },
      {
        id: 'operator-certificate',
        type: 'text',
        label: 'Operator Certificate Number',
        required: true,
        order: 22,
        validation: { minLength: 5, maxLength: 50 }
      },
      {
        id: 'operator-certificate-expiry',
        type: 'date',
        label: 'Operator Certificate Expiry Date',
        required: true,
        order: 23
      },
      
      // Document Upload Section
      {
        id: 'documents-section',
        type: 'section',
        label: 'Required Documents',
        required: true,
        order: 30,
        config: {
          collapsible: false,
          description: 'Upload required documents for renewal'
        }
      },
      {
        id: 'last-inspection-report',
        type: 'file',
        label: 'Last Inspection Report',
        required: true,
        order: 31,
        config: {
          accept: '.pdf,.jpg,.jpeg,.png',
          maxSize: 10485760,
          multiple: true,
          maxFiles: 3
        }
      },
      {
        id: 'current-boiler-photos',
        type: 'file',
        label: 'Current Boiler Photographs',
        required: true,
        order: 32,
        config: {
          accept: '.jpg,.jpeg,.png',
          maxSize: 5242880,
          multiple: true,
          maxFiles: 5
        }
      },
      {
        id: 'maintenance-records',
        type: 'file',
        label: 'Maintenance Records',
        required: true,
        order: 33,
        config: {
          accept: '.pdf,.jpg,.jpeg,.png',
          maxSize: 10485760,
          multiple: true,
          maxFiles: 5
        }
      },
      {
        id: 'operator-competency-certificate',
        type: 'file',
        label: 'Updated Operator Competency Certificate',
        required: true,
        order: 34,
        config: {
          accept: '.pdf,.jpg,.jpeg,.png',
          maxSize: 5242880,
          multiple: false
        }
      }
    ],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString()
  },
  
  {
    id: 'boiler-modification',
    name: 'Boiler Modification Application',
    description: 'Apply for boiler modifications under IBR Form V',
    category: 'boiler-services',
    subcategory: 'modification',
    isActive: true,
    requiresAuth: true,
    allowedRoles: ['applicant', 'inspector', 'admin'],
    permissions: [
      BOILER_PERMISSIONS.VIEW_BOILER_RECORDS.code,
      BOILER_PERMISSIONS.MODIFY_BOILER.code,
    ],
    fields: [
      // Boiler Selection
      {
        id: 'boiler-identification-section',
        type: 'section',
        label: 'Boiler Identification',
        required: true,
        order: 1,
        config: { collapsible: false }
      },
      {
        id: 'boiler-registration-number',
        type: 'text',
        label: 'Boiler Registration Number',
        required: true,
        order: 2,
        validation: { minLength: 5, maxLength: 50 }
      },
      
      // Modification Details
      {
        id: 'modification-section',
        type: 'section',
        label: 'Modification Details',
        required: true,
        order: 10,
        config: { collapsible: false }
      },
      {
        id: 'modification-type',
        type: 'select',
        label: 'Type of Modification',
        required: true,
        order: 11,
        config: {
          options: [
            { value: 'pressure-increase', label: 'Working Pressure Increase' },
            { value: 'capacity-increase', label: 'Steam Capacity Increase' },
            { value: 'fuel-change', label: 'Fuel Type Change' },
            { value: 'safety-upgrade', label: 'Safety System Upgrade' },
            { value: 'location-change', label: 'Location Change' },
            { value: 'other', label: 'Other Modification' }
          ]
        }
      },
      {
        id: 'modification-details',
        type: 'textarea',
        label: 'Detailed Description of Modification',
        required: true,
        order: 12,
        validation: { minLength: 20, maxLength: 1000 }
      },
      {
        id: 'engineering-justification',
        type: 'textarea',
        label: 'Engineering Justification',
        required: true,
        order: 13,
        validation: { minLength: 20, maxLength: 1000 }
      },
      {
        id: 'safety-impact-assessment',
        type: 'textarea',
        label: 'Safety Impact Assessment',
        required: true,
        order: 14,
        validation: { minLength: 20, maxLength: 1000 }
      },
      
      // Document Upload
      {
        id: 'documents-section',
        type: 'section',
        label: 'Required Documents',
        required: true,
        order: 20,
        config: { collapsible: false }
      },
      {
        id: 'engineering-drawings',
        type: 'file',
        label: 'Engineering Drawings',
        required: true,
        order: 21,
        config: {
          accept: '.pdf,.dwg,.jpg,.jpeg,.png',
          maxSize: 20971520, // 20MB
          multiple: true,
          maxFiles: 10
        }
      },
      {
        id: 'calculation-sheets',
        type: 'file',
        label: 'Calculation Sheets',
        required: true,
        order: 22,
        config: {
          accept: '.pdf,.xls,.xlsx,.jpg,.jpeg,.png',
          maxSize: 10485760,
          multiple: true,
          maxFiles: 5
        }
      },
      {
        id: 'material-certificates',
        type: 'file',
        label: 'Material Certificates',
        required: true,
        order: 23,
        config: {
          accept: '.pdf,.jpg,.jpeg,.png',
          maxSize: 10485760,
          multiple: true,
          maxFiles: 5
        }
      },
      {
        id: 'safety-analysis',
        type: 'file',
        label: 'Safety Analysis Report',
        required: true,
        order: 24,
        config: {
          accept: '.pdf,.doc,.docx',
          maxSize: 10485760,
          multiple: true,
          maxFiles: 3
        }
      }
    ],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString()
  },
  
  {
    id: 'boiler-transfer',
    name: 'Boiler Transfer Application',
    description: 'Transfer boiler ownership or location under IBR Form IV',
    category: 'boiler-services',
    subcategory: 'transfer',
    isActive: true,
    requiresAuth: true,
    allowedRoles: ['applicant', 'inspector', 'admin'],
    permissions: [
      BOILER_PERMISSIONS.VIEW_BOILER_RECORDS.code,
      BOILER_PERMISSIONS.TRANSFER_BOILER.code,
    ],
    fields: [
      // Boiler Identification
      {
        id: 'boiler-identification-section',
        type: 'section',
        label: 'Boiler Identification',
        required: true,
        order: 1,
        config: { collapsible: false }
      },
      {
        id: 'boiler-registration-number',
        type: 'text',
        label: 'Boiler Registration Number',
        required: true,
        order: 2,
        validation: { minLength: 5, maxLength: 50 }
      },
      
      // Transfer Type
      {
        id: 'transfer-type-section',
        type: 'section',
        label: 'Transfer Details',
        required: true,
        order: 10,
        config: { collapsible: false }
      },
      {
        id: 'transfer-type',
        type: 'select',
        label: 'Type of Transfer',
        required: true,
        order: 11,
        config: {
          options: [
            { value: 'ownership', label: 'Ownership Transfer Only' },
            { value: 'location', label: 'Location Transfer Only' },
            { value: 'both', label: 'Both Ownership and Location' }
          ]
        }
      },
      {
        id: 'transfer-reason',
        type: 'textarea',
        label: 'Reason for Transfer',
        required: true,
        order: 12,
        validation: { minLength: 10, maxLength: 500 }
      },
      
      // Current Owner Details
      {
        id: 'current-owner-section',
        type: 'section',
        label: 'Current Owner Details',
        required: true,
        order: 20,
        config: { collapsible: false }
      },
      {
        id: 'current-owner-name',
        type: 'text',
        label: 'Current Owner Name',
        required: true,
        order: 21,
        validation: { minLength: 2, maxLength: 100 }
      },
      {
        id: 'current-organization-name',
        type: 'text',
        label: 'Current Organization Name',
        required: true,
        order: 22,
        validation: { minLength: 2, maxLength: 100 }
      },
      
      // New Owner Details  
      {
        id: 'new-owner-section',
        type: 'section',
        label: 'New Owner Details',
        required: true,
        order: 30,
        config: { collapsible: false }
      },
      {
        id: 'new-owner-name',
        type: 'text',
        label: 'New Owner Name',
        required: true,
        order: 31,
        validation: { minLength: 2, maxLength: 100 }
      },
      {
        id: 'new-organization-name',
        type: 'text',
        label: 'New Organization Name',
        required: true,
        order: 32,
        validation: { minLength: 2, maxLength: 100 }
      },
      {
        id: 'new-contact-person',
        type: 'text',
        label: 'New Owner Contact Person',
        required: true,
        order: 33,
        validation: { minLength: 2, maxLength: 50 }
      },
      {
        id: 'new-mobile',
        type: 'tel',
        label: 'New Owner Mobile',
        required: true,
        order: 34,
        validation: { pattern: '^[6-9]\\d{9}$' }
      },
      {
        id: 'new-email',
        type: 'email',
        label: 'New Owner Email',
        required: true,
        order: 35
      },
      {
        id: 'new-address',
        type: 'textarea',
        label: 'New Owner Complete Address',
        required: true,
        order: 36,
        validation: { minLength: 10, maxLength: 300 }
      },
      
      // New Location (conditional)
      {
        id: 'new-location-section',
        type: 'section',
        label: 'New Installation Location (if applicable)',
        required: false,
        order: 40,
        config: { 
          collapsible: true,
          description: 'Fill this section only if boiler location is changing'
        }
      },
      {
        id: 'new-factory-name',
        type: 'text',
        label: 'New Factory/Establishment Name',
        required: false,
        order: 41,
        validation: { maxLength: 100 }
      },
      {
        id: 'new-plot-number',
        type: 'text',
        label: 'New Plot/Survey Number',
        required: false,
        order: 42,
        validation: { maxLength: 50 }
      },
      {
        id: 'new-complete-address',
        type: 'textarea',
        label: 'New Complete Installation Address',
        required: false,
        order: 43,
        validation: { maxLength: 300 }
      },
      {
        id: 'new-area-id',
        type: 'area-select',
        label: 'New Area',
        required: false,
        order: 44
      },
      
      // Operator Details
      {
        id: 'operator-section',
        type: 'section',
        label: 'Boiler Operator Details',
        required: true,
        order: 50,
        config: { collapsible: false }
      },
      {
        id: 'operator-name',
        type: 'text',
        label: 'Operator Name',
        required: true,
        order: 51,
        validation: { minLength: 2, maxLength: 50 }
      },
      {
        id: 'operator-certificate',
        type: 'text',
        label: 'Operator Certificate Number',
        required: true,
        order: 52,
        validation: { minLength: 5, maxLength: 50 }
      },
      {
        id: 'operator-certificate-expiry',
        type: 'date',
        label: 'Operator Certificate Expiry Date',
        required: true,
        order: 53
      },
      
      // Document Upload
      {
        id: 'documents-section',
        type: 'section',
        label: 'Required Documents',
        required: true,
        order: 60,
        config: { collapsible: false }
      },
      {
        id: 'ownership-transfer-deed',
        type: 'file',
        label: 'Ownership Transfer Deed/Agreement',
        required: true,
        order: 61,
        config: {
          accept: '.pdf,.jpg,.jpeg,.png',
          maxSize: 10485760,
          multiple: true,
          maxFiles: 3
        }
      },
      {
        id: 'current-certificate',
        type: 'file',
        label: 'Current Boiler Certificate',
        required: true,
        order: 62,
        config: {
          accept: '.pdf,.jpg,.jpeg,.png',
          maxSize: 5242880,
          multiple: false
        }
      },
      {
        id: 'location-approval',
        type: 'file',
        label: 'New Location Approval (if location changing)',
        required: false,
        order: 63,
        config: {
          accept: '.pdf,.jpg,.jpeg,.png',
          maxSize: 5242880,
          multiple: true,
          maxFiles: 2
        }
      },
      {
        id: 'operator-competency-certificate',
        type: 'file',
        label: 'Operator Competency Certificate',
        required: true,
        order: 64,
        config: {
          accept: '.pdf,.jpg,.jpeg,.png',
          maxSize: 5242880,
          multiple: false
        }
      }
    ],
    createdAt: new Date().toISOString(),
    updatedAt: new Date().toISOString()
  }
];

export const BOILER_MODULE_CATEGORIES = [
  {
    id: 'boiler-services',
    name: 'Boiler Services',
    description: 'Factory and Boiler Department services under Indian Boilers Act 1923',
    icon: 'factory',
    subcategories: [
      {
        id: 'registration',
        name: 'Registration',
        description: 'New boiler registration services'
      },
      {
        id: 'renewal',
        name: 'Renewal',
        description: 'Certificate renewal services'
      },
      {
        id: 'modification',
        name: 'Modification',
        description: 'Boiler modification applications'
      },
      {
        id: 'transfer',
        name: 'Transfer',
        description: 'Ownership and location transfer'
      },
      {
        id: 'inspection',
        name: 'Inspection',
        description: 'Boiler inspection services'
      }
    ]
  }
];