import { Routes } from '@angular/router';
import { ContentListPage } from './features/content/pages/content-list/content-list.page';
import { ContentDetailPage } from './features/content/pages/content-detail/content-detail.page';
import { CreateContentPage } from './features/content/pages/create-content/create-content.page';

export const routes: Routes = [
    { path: '', component: ContentListPage },
    { path: 'create', component: CreateContentPage },
    { path: ':id', component: ContentDetailPage }
];