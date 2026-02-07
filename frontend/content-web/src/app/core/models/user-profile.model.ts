export interface UserProfile {
  id: string;
  email: string;
  displayName: string;
  profileImageUrl?: string | null;
  role: string;
  createdAtUtc: string;
  lastLoginAtUtc: string | null;
}
