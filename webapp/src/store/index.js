import { createStore } from 'vuex';
import axios from 'axios';

export default createStore({
  state: {
    user: null,
    books: [],
    bookDetails: null,
    audiobooks: [],
    audiobookDetails: null,
    genres: [],
    userBookLibrary: [],
    userAudioBookLibrary: [],
  },
  mutations: {
    updateUserInfo(state, user) {
      state.user = user;
    },
    updateBooks(state, books) {
      state.books = books;
    },
    setBookDetails(state, details) {
      state.bookDetails = details;
    },
    updateAudioBooks(state, audiobook) {
      state.audiobooks = audiobook;
    },
    setAudioBookDetails(state, details) {
      state.audiobookDetails = details;
    },
    updateGenres(state, genres) {
      state.genres = genres;
    },
    updateUserBookLibrary(state, library) {
      state.userBookLibrary = library;
    },
    updateUserAudioBookLibrary(state, library) {
      state.userAudioBookLibrary = library;
    },
  },
  actions: {
    // USER
    async postUserInfo({ commit }, { email, password }) {
      try {
        const response = await axios.post('api/profile/login', {
          email,
          password,
        });
        commit('updateUserInfo', response.data);
      } catch (error) {
        console.error('Login error:', error);
        if (error.response?.status === 403) {
          alert('Невірна пошта або пароль');
        }
      }
    },

    async postRegisterUser({ commit }, { name, email, password }) {
      try {
        const response = await axios.post('/api/profile/register', {
          name,
          email,
          password,
        });
        commit('updateUserInfo', response.data);
      } catch (error) {
        console.error('Register error:', error);
      }
    },

    async logout({ commit }) {
      try {
        await axios.get('/api/profile/logout');
        commit('updateUserInfo', null);
      } catch (error) {
        console.error('Logout error:', error);
      }
    },

    async fetchUserInfo({ commit }) {
      try {
        const response = await axios.get('/api/profile');
        commit('updateUserInfo', response.data);
      } catch (error) {
        console.warn('User is not authorised');
      }
    },

    async updateProfile({ commit }, userInfo) {
      try {
        const response = await axios.post('/api/profile', userInfo);
        commit('updateUserInfo', response.data);
        return true;
      } catch (error) {
        console.error('Error updating profile:', error);
        return false;
      }
    },

    async fetchGenres({ commit }) {
      const res = await axios.get('/api/genres');
      commit('updateGenres', res.data);
    },

    // BOOKS
    async fetchBooks(
      { commit },
      {
        take = 20, page = 1, name = null, author = null, genre = null,
      },
    ) {
      const skip = (page - 1) * take;
      const res = await axios.get('/api/books', {
        params: {
          take, skip, name, author, genre,
        },
      });
      commit('updateBooks', res.data);
    },

    async fetchBookDetails({ commit }, bookId) {
      const res = await axios.get(`/api/books/${bookId}`);
      commit('setBookDetails', res.data);
    },

    // AUDIOBOOKS
    async fetchAudioBooks(
      { commit },
      {
        take = 20, page = 1, name = null, author = null, voiceActor = null, genre = null,
      },
    ) {
      const skip = (page - 1) * take;
      const res = await axios.get('/api/audiobooks', {
        params: {
          take, skip, name, author, voiceActor, genre,
        },
      });
      commit('updateAudioBooks', res.data);
    },

    async fetchAudioBookDetails({ commit }, audiobookId) {
      const res = await axios.get(`/api/audiobooks/${audiobookId}`);
      commit('setAudioBookDetails', res.data);
    },

    // USERS BOOKS
    async fetchUserBookLibrary(
      { commit },
      {
        take = 20, page = 1, name = null, author = null, genre = null, isFavourite = null,
      },
    ) {
      const skip = (page - 1) * take;
      const res = await axios.get('api/user/books', {
        params: {
          take, skip, name, author, genre, isFavourite,
        },
      });
      commit('updateUserBookLibrary', res.data);
    },

    async fetchUserAudioBookLibrary(
      { commit },
      {
        take = 20,
        page = 1,
        name = null,
        author = null,
        voiceActor = null,
        isFavourite = null,
      },
    ) {
      const skip = (page - 1) * take;
      const res = await axios.get('api/user/audiobooks', {
        params: {
          take, skip, name, author, voiceActor, isFavourite,
        },
      });
      commit('updateUserAudioBookLibrary', res.data);
    },

    async addToFavourites(_, { productId, productType }) {
      try {
        await axios.post('/api/user/products/favourite/add', {
          productId,
          productType,
        }, {
          headers: { 'Content-Type': 'application/json' },
        });
      } catch (err) {
        if (err.response?.status === 401) {
          alert('⚠️ Увійдіть в акаунт для додавання до обраного.');
        } else {
          console.error(err);
          alert('⚠️ Не вдалося додати до обраного.');
        }
      }
    },

    async removeFromFavourites(_, { productId, productType }) {
      try {
        console.log(productId);
        await axios.post('/api/user/products/favourite/remove', {
          productId,
          productType,
        }, {
          headers: { 'Content-Type': 'application/json' },
        });
      } catch (err) {
        console.error(err);
        alert('⚠️ Не вдалося видалити з обраного.');
        throw err;
      }
    },

    async setRating({ dispatch }, { productId, productType, rating }) {
      try {
        await axios.post('/api/products/rate', {
          productId,
          productType,
          rating,
        }, {
          headers: { 'Content-Type': 'application/json' },
        });

        if (productType === 1) {
          await dispatch('fetchBookDetails', productId);
        }
        if (productType === 2) {
          await dispatch('fetchAudioBookDetails', productId);
        }
      } catch (err) {
        if (err.response?.status === 401) {
          alert('⚠️ Увійдіть в акаунт для оцінювання.');
        } else {
          console.error(err);
          alert('⚠️ Не вдалося поставити оцінку.');
        }
      }
    },

    // comments
    async postComment(_, {
      productId,
      productType,
      message,
      replyToUserId,
    }) {
      const response = await axios.post('/api/comments', {
        productId,
        productType,
        message,
        replyToUserId,
      });

      return response.data;
    },

    async buyBookSession(_, { productId, request }) {
      try {
        const res = await axios.post(`/api/payments/checkout-session?productId=${productId}`, request);
        const sessionUrl = res.data.url;
        if (sessionUrl && sessionUrl.trim() !== '') {
          window.location.href = sessionUrl;
        } else {
          if (request.productType === 1) {
            window.location.href = `/book/${productId}`;
          }
          if (request.productType === 2) {
            window.location.href = `/audiobook/${productId}`;
          }
        }
      } catch (err) {
        if (err.response?.status === 401) {
          alert('⚠️ Увійдіть в акаунт для покупки книги.');
        } else {
          console.error(err);
          alert('⚠️ Помилка під час створення сесії Stripe.');
        }
      }
    },

    async downloadBook(_, { productId, productType, title }) {
      try {
        const response = await axios.get('/api/files', {
          params: {
            productId,
            productType,
          },
          responseType: 'blob',
        });

        const blob = new Blob([response.data], { type: response.headers['content-type'] });
        const url = window.URL.createObjectURL(blob);

        const link = document.createElement('a');
        link.href = url;

        const disposition = response.headers['content-disposition'];
        let extension = '';
        if (disposition) {
          const match = disposition.match(/filename="?(.+?)"?$/);
          if (match?.[1]) {
            const serverFileName = match[1];
            const parts = serverFileName.split('.');
            extension = parts.length > 1 ? parts.pop() : '';
          }
        }
        const safeTitle = title || 'download';
        const fileName = extension ? `${safeTitle}.${extension}` : safeTitle;
        link.download = fileName;

        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        window.URL.revokeObjectURL(url);
      } catch (error) {
        console.error(error);
        alert(error);
      }
    },

    async uploadBookFile(_, file) {
      const formData = new FormData();
      formData.append('file', file);

      try {
        const response = await axios.post('/api/admin/uploadbook', formData, {
          headers: { 'Content-Type': 'multipart/form-data' },
        });
        // const response = await axios.post('/api/admin/uploadbook', formData);
        alert('Книгу успішно завантажено!');
        console.log(response.data);
      } catch (error) {
        // console.error(error);
        alert(error);
      }
    },
  },
  getters: {
    isAuthenticated: (state) => !!state.user,
  },
});
