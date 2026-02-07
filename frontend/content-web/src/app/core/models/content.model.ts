export interface Content {
  id: string;
  authorId?: string;
  authorDisplayName?: string;
  authorProfileImageUrl?: string | null;
  title: string;
  body: string;
  status: string;
  imageUrl?: string | null;
  createdAtUtc?: string;
  publishedAtUtc?: string | null;
  rowVersion?: string;
}
