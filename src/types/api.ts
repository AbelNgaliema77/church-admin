export type UserRole = 'Pending' | 'Worker' | 'TeamLead' | 'Admin';

export type ChurchBranding = {
  id: string;
  name: string;
  slug: string;
  logoUrl?: string | null;
  primaryColor: string;
  secondaryColor: string;
  welcomeText?: string | null;
};

export type AuthResponse = {
  token: string;
  userId: string;
  email: string;
  displayName: string;
  role: UserRole;
  isActive: boolean;
  churchId?: string;
  churchSlug?: string;
  churchName?: string;
};

export type LoginState = {
  token: string;
  user: {
    id: string;
    email: string;
    displayName: string;
    role: UserRole;
    isActive: boolean;
    churchId?: string;
    churchSlug?: string;
    churchName?: string;
  };
};
