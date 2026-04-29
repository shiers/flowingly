export interface ParsedDataDto {
  costCentre: string;
  totalIncludingTax: number;
  totalExcludingTax: number;
  salesTax: number;
  paymentMethod: string | null;
  vendor: string | null;
  description: string | null;
  date: string | null;
}

export interface MetadataDto {
  parser: string;
  workflowClassification: string;
  aiExtensionReady: boolean;
}

export interface ValidationErrorDto {
  code: string;
  message: string;
}

export interface ParseResponse {
  success: boolean;
  data: ParsedDataDto | null;
  metadata: MetadataDto;
  errors: ValidationErrorDto[];
}
