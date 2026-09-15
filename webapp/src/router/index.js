import {
  createRouter,
  createWebHistory,
} from 'vue-router';
import BooksLibraryPage from '@/pages/BooksLibraryPage.vue';
import LoginPage from '@/pages/LoginPage.vue';
import RegisterPage from '@/pages/RegisterPage.vue';
import ProfilePage from '@/pages/ProfilePage.vue';
import UserBookLibraryPage from '@/pages/UserBookLibraryPage.vue';
import BookPage from '@/pages/BookPage.vue';
import UploadBookPage from '@/pages/UploadBookPage.vue';
import AudioBooksLibraryPage from '@/pages/AudioBooksLibraryPage.vue';
import UserAudioBookLibraryPage from '@/pages/UserAudioBookLibraryPage.vue';
import AudioBookPage from '@/pages/AudioBookPage.vue';
import SubscriptionsPage from '@/pages/SubscriptionsPage.vue';
import TokensPage from '@/pages/TokensPage.vue';
import ForgotPasswordPage from '@/pages/ForgotPasswordPage.vue';
import ResetPasswordPage from '@/pages/ResetPasswordPage.vue';

export default createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      redirect: '/books',
    },
    {
      path: '/books',
      name: 'BooksLibraryPage',
      components: {
        default: BooksLibraryPage,
      },
    },
    {
      path: '/login',
      name: 'LoginPage',
      props: true,
      component: LoginPage,
    },
    {
      path: '/register',
      name: 'RegisterPage',
      props: true,
      component: RegisterPage,
    },
    {
      path: '/profile',
      name: 'ProfilePage',
      props: true,
      component: ProfilePage,
    },
    {
      path: '/tokens',
      name: 'TokensPage',
      props: true,
      component: TokensPage,
    },
    {
      path: '/userBookLibrary',
      name: 'UserBookLibrary',
      props: true,
      component: UserBookLibraryPage,
    },
    {
      path: '/userAudioBookLibrary',
      name: 'UserAudioBookLibrary',
      props: true,
      component: UserAudioBookLibraryPage,
    },
    {
      path: '/book/:id',
      name: 'BookDetails',
      props: true,
      component: BookPage,
    },
    {
      path: '/audiobooks',
      name: 'AudioBooksLibraryPage',
      props: true,
      component: AudioBooksLibraryPage,
    },
    {
      path: '/audiobook/:id',
      name: 'AudioBookDetails',
      props: true,
      component: AudioBookPage,
    },
    {
      path: '/admin/uploadbook',
      name: 'UploadBook',
      props: true,
      component: UploadBookPage,
    },
    {
      path: '/subscriptions',
      name: 'Subscriptions',
      props: true,
      component: SubscriptionsPage,
    },
    {
      path: '/forgot-password',
      name: 'ForgotPassword',
      component: ForgotPasswordPage,
    },
    {
      path: '/reset-password/:token',
      name: 'ResetPassword',
      component: ResetPasswordPage,
      props: true,
    },
  ],
});
