export interface Result<T> {
    isSuccess: boolean;
    errors: string[];
    statusCode: number;
    value: T | null;
  }
  
  export interface PaginatedList<T> {
    items: T[];
    totalCount: number;
    hasPreviousPage: boolean;
    hasNextPage: boolean;
  }
  
  // Backend'ten gelen risk DTO
  export interface RiskAssessmentDto {
    riskScore: number;
    riskLevel: string;
    reason: string;
    createdAt: string; // ISO string
  }
  
  export interface VendorWithRiskDto {
    id: string;
    name: string;
    financialHealth: number;
    slaUptime: number;
    majorIncidents: number;
    latestRisk?: RiskAssessmentDto | null;
  }
  