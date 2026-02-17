export interface Comment {
  id: string;
  contentItemId: string;
  userId: string;
  userDisplayName: string;
  userProfileImageUrl?: string | null;
  text: string;
  createdAtUtc: string;
}
