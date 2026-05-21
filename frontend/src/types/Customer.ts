export interface CustomerDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  signatureBase64: string;
  dateCreated: string;
}

export interface CreateCustomerRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  signatureBase64: string;
}

export interface ApiError {
  status: number;
  title: string;
  detail: string;
}
