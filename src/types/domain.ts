export type Role = 'Admin' | 'TeamLead' | 'Worker';

export type ServiceType =
  | 'Sunday'
  | 'Friday'
  | 'Special'
  | 'Prayer'
  | 'Conference';

export type Team = {
  id: string;
  name: string;
  description?: string | null;
  isActive: boolean;
};

export type AttendanceRecord = {
  id: string;
  serviceDate: string;
  serviceType: ServiceType;
  men: number;
  women: number;
  children: number;
  visitors: number;
  notes?: string;
};

export type Worker = {
  id: string;
  fullName: string;
  email: string;
  phone: string;
  teamId: string;
  role: Role;
  startedServing: string;
  baptized: boolean;
  dateOfBirth: string;
  address: string;
  status: 'Active' | 'Inactive' | 'On Break';
};

export type FinanceEntry = {
  id: string;
  serviceDate: string;
  serviceType: ServiceType;
  category:
    | 'Tithe'
    | 'Thanksgiving'
    | 'Normal Offering'
    | 'Special Offering'
    | 'Building Fund'
    | 'Other';
  amount: number;
  paymentMethod: 'Cash' | 'EFT' | 'Card' | 'Bank Deposit';
  verified: boolean;
  correctionForEntryId?: string;
  note?: string;
};

export type InventoryItem = {
  id: string;
  name: string;
  teamId: string;
  description: string;
  quantity: number;
  condition: 'New' | 'Good' | 'Damaged' | 'Needs Repair' | 'Lost';
  imageUrl?: string;
  status: 'Pending Approval' | 'Approved' | 'Retired';
};
