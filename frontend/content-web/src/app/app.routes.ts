import { Routes } from '@angular/router';
import { ContentListPage } from './features/content/pages/content-list/content-list.page';
import { ContentDetailPage } from './features/content/pages/content-detail/content-detail.page';

export const routes: Routes = [
    { path: '', component: ContentListPage },
    { path: ':id', component: ContentDetailPage }
];