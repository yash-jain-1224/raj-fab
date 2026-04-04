import { API_BASE } from "@/config/endpoints";

export const API_BASE_URL = API_BASE;
export interface ApiResponse<T> {
  success: boolean;
  data?: T;
  message: string;
}

export class BaseApiService {
  protected async request<T>(endpoint: string, options?: RequestInit): Promise<T> {
    const token = sessionStorage.getItem("auth_token");
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      headers: {
        'Content-Type': 'application/json',
        // Add authentication header here if needed
        'Authorization': `Bearer ${token}`,
        ...options?.headers,
      },
      ...options,
    });

    if (!response.ok) {
      // Clone response so we can read it multiple times if needed
      const responseClone = response.clone();
      
      let errorMessage = `API Error: ${response.status} ${response.statusText}`;
      
      try {
        const errorData = await responseClone.json();
        // Try multiple property names due to case sensitivity
        errorMessage = 
          errorData.message || 
          errorData.Message || 
          errorData.error || 
          errorData.Error ||
          errorData.msg ||
          response.statusText;
        
        console.error('Backend Error:', errorMessage, errorData);
      } catch (jsonError) {
        // If JSON parsing fails, try to read as text
        try {
          const textBody = await response.text();
          if (textBody) {
            errorMessage = textBody;
          }
          console.error('Backend Error (text):', errorMessage);
        } catch (textError) {
          console.error('Could not parse error response:', textError);
        }
      }
      
      // Throw outside the try-catch to avoid catching our own error
      throw new Error(errorMessage);
    }

    return response.json();
  }

  protected async requestWithFormData<T>(endpoint: string, formData: FormData): Promise<ApiResponse<T>> {
    const token = sessionStorage.getItem("auth_token");
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: 'POST',
       headers: {
        Authorization: `Bearer ${token}`,
      },
      body: formData,
    });

    if (!response.ok) {
      // Clone response so we can read it multiple times if needed
      const responseClone = response.clone();
      
      let errorMessage = `API Error: ${response.status} ${response.statusText}`;
      
      try {
        const errorData = await responseClone.json();
        // Try multiple property names due to case sensitivity
        errorMessage = 
          errorData.message || 
          errorData.Message || 
          errorData.error || 
          errorData.Error ||
          errorData.msg ||
          response.statusText;
        
        console.error('Backend Error:', errorMessage, errorData);
      } catch (jsonError) {
        // If JSON parsing fails, try to read as text
        try {
          const textBody = await response.text();
          if (textBody) {
            errorMessage = textBody;
          }
          console.error('Backend Error (text):', errorMessage);
        } catch (textError) {
          console.error('Could not parse error response:', textError);
        }
      }
      
      // Throw outside the try-catch to avoid catching our own error
      throw new Error(errorMessage);
    }

    return response.json();
  }
}