export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  role: string;
  createdAtUtc: string;
  lastLoginAtUtc: string | null;
}
