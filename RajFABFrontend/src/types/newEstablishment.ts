// establishment form data mapping model
export interface NewEstablishmentFormData {
    establishmentDetails: {
        name: string;
        address: string;
        state: string;
        district: string;
        pincode: string;
    };
    factory: {
        factoryName: string;
        registrationNumber: string;
        address: string;
        state: string;
        district: string;
        pincode: string;
        employerDetails: {
            name: string;
            fatherName: string;
            address: string;
            state: string;
            district: string;
            pincode: string;
        };
    };
}

