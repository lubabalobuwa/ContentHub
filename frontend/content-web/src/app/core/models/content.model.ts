export interface Content {
  id: string;
  authorId?: string;
  title: string;
  body: string;
  status: string;
  rowVersion?: string;
}
