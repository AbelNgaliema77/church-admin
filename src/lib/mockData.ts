import {
  AttendanceRecord,
  FinanceEntry,
  InventoryItem,
  Team,
  Worker
} from '../types/domain';

export const teams: Team[] = [
  {
    id: 'team-worship',
    name: 'Worship',
    description: 'Music and praise team',
    isActive: true
  },
  {
    id: 'team-media',
    name: 'Media',
    description: 'Sound, camera and livestream',
    isActive: true
  },
  {
    id: 'team-children',
    name: 'Children',
    description: 'Children ministry',
    isActive: true
  },
  {
    id: 'team-ushering',
    name: 'Ushering',
    description: 'Welcoming and seating',
    isActive: true
  },
  {
    id: 'team-finance',
    name: 'Finance',
    description: 'Finance and treasury',
    isActive: true
  }
];

export const attendanceRecords: AttendanceRecord[] = [
  {
    id: 'attendance-1',
    serviceDate: '2026-04-05',
    serviceType: 'Sunday',
    men: 42,
    women: 58,
    children: 31,
    visitors: 8,
    notes: 'Good attendance'
  },
  {
    id: 'attendance-2',
    serviceDate: '2026-04-12',
    serviceType: 'Sunday',
    men: 46,
    women: 62,
    children: 28,
    visitors: 12,
    notes: 'More visitors'
  }
];

export const workers: Worker[] = [
  {
    id: 'worker-1',
    fullName: 'Grace Mokoena',
    email: 'grace@example.com',
    phone: '+27 71 000 0001',
    teamId: 'team-worship',
    role: 'TeamLead',
    startedServing: '2023-04-01',
    baptized: true,
    dateOfBirth: '1992-08-10',
    address: 'Cape Town',
    status: 'Active'
  },
  {
    id: 'worker-2',
    fullName: 'Daniel Nkosi',
    email: 'daniel@example.com',
    phone: '+27 72 000 0002',
    teamId: 'team-media',
    role: 'Worker',
    startedServing: '2024-01-14',
    baptized: true,
    dateOfBirth: '1996-05-22',
    address: 'Bellville',
    status: 'Active'
  }
];

export const financeEntries: FinanceEntry[] = [
  {
    id: 'finance-1',
    serviceDate: '2026-04-05',
    serviceType: 'Sunday',
    category: 'Tithe',
    amount: 12450,
    paymentMethod: 'EFT',
    verified: true,
    note: 'Verified Sunday tithe'
  },
  {
    id: 'finance-2',
    serviceDate: '2026-04-05',
    serviceType: 'Sunday',
    category: 'Normal Offering',
    amount: 3900,
    paymentMethod: 'Cash',
    verified: false,
    note: 'Waiting for verification'
  }
];

export const inventoryItems: InventoryItem[] = [
  {
    id: 'inventory-1',
    name: 'Wireless Microphone',
    teamId: 'team-media',
    description: 'Dual wireless mic set',
    quantity: 2,
    condition: 'Good',
    status: 'Approved'
  },
  {
    id: 'inventory-2',
    name: 'Children Chairs',
    teamId: 'team-children',
    description: 'Plastic chairs for children ministry',
    quantity: 20,
    condition: 'Good',
    status: 'Pending Approval'
  }
];
