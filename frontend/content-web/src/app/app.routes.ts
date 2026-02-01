import { Routes } from '@angular/router';
import { ContentListPage } from './features/content/pages/content-list/content-list.page';
import { ContentDetailPage } from './features/content/pages/content-detail/content-detail.page';
import { CreateContentPage } from './features/content/pages/create-content/create-content.page';
import { LoginPage } from './features/auth/pages/login/login.page';
import { RegisterPage } from './features/auth/pages/register/register.page';
import { VerifyEmailPage } from './features/auth/pages/verify-email/verify-email.page';
import { VerifyEmailSentPage } from './features/auth/pages/verify-email-sent/verify-email-sent.page';
import { ForgotPasswordPage } from './features/auth/pages/forgot-password/forgot-password.page';
import { ResetPasswordPage } from './features/auth/pages/reset-password/reset-password.page';
import { authGuard } from './core/guards/auth.guard';
import { ContentDraftsPage } from './features/content/pages/drafts/content-drafts.page';
import { ContentArchivedPage } from './features/content/pages/archived/content-archived.page';
import { EditContentPage } from './features/content/pages/edit-content/edit-content.page';
import { ContentPublishedPage } from './features/content/pages/published/content-published.page';
import { AccountSettingsPage } from './features/auth/pages/account-settings/account-settings.page';

export const routes: Routes = [
    { path: '', component: ContentListPage },
    { path: 'login', component: LoginPage },
    { path: 'register', component: RegisterPage },
    { path: 'verify-email', component: VerifyEmailPage },
    { path: 'verify-email-sent', component: VerifyEmailSentPage },
    { path: 'forgot-password', component: ForgotPasswordPage },
    { path: 'reset-password', component: ResetPasswordPage },
    { path: 'account', component: AccountSettingsPage, canActivate: [authGuard] },
    { path: 'create', component: CreateContentPage, canActivate: [authGuard] },
    { path: 'drafts', component: ContentDraftsPage, canActivate: [authGuard] },
    { path: 'published', component: ContentPublishedPage, canActivate: [authGuard] },
    { path: 'archived', component: ContentArchivedPage, canActivate: [authGuard] },
    { path: 'edit/:id', component: EditContentPage, canActivate: [authGuard] },
    { path: ':id', component: ContentDetailPage }
];
