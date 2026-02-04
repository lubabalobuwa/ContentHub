export interface AdminUser {
  id: string;
  email: string;
  displayName: string;
  role: string;
  emailConfirmed: boolean;
  isDisabled: boolean;
  createdAtUtc: string;
  lastLoginAtUtc: string | null;
}
