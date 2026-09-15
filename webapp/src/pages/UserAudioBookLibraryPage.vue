<template>
  <main>
    <div v-if="isAuthenticated">
      <ProfileNavPan />

      <div class="container">

        <div class="main-page">
          <header>
            <div class="search-controls">
              <div class="search-group">
                <label for="author-search">Search by author:</label>
                <input id="author-search" type="text" v-model="authorQuery" placeholder="Author" />
              </div>

              <div class="search-group">
                <label for="voiceActor-search">Search by voice actor:</label>
                <input id="voiceActor-search" type="text" v-model="voiceActorQuery"
                       placeholder="Voice actors name" />
              </div>

              <div class="search-group">
                <label for="name-search">Search by name:</label>
                <input id="name-search" type="text" v-model="nameQuery" placeholder="Books name" />
              </div>

              <div class="search-group">
                <label for="favourite-select">Favourite:</label>
                <select id="favourite-select" v-model="selectedFavourite">
                  <option value="">All</option>
                  <option :value="true">Favourite</option>
                  <option :value="false">Not Favourite</option>
                </select>
              </div>
            </div>
          </header>

          <div class="books-grid">
            <AudioBookCard v-for="audiobook in userAudioBookLibrary" :key="audiobook.id"
                      :audiobook="audiobook" @click="openAudioBookDetails(audiobook.id)" />
          </div>

          <footer>
            <button :disabled="page === 1" @click="changePage(page - 1)">Previous</button>
            <span>Page {{ page }}</span>
            <button @click="changePage(page + 1)">Next</button>
          </footer>
        </div>

      </div>
    </div>

    <div v-else-if="loading">
      <div class="loading_message">
        <p>Loading...</p>
      </div>
    </div>

    <div v-else>
      <div class="not_authorised_message">
        <router-link to="/login">Please log in to access this page</router-link>
      </div>
    </div>
  </main>
</template>

<script>
import { mapState, mapGetters, mapActions } from 'vuex';
import AudioBookCard from '@/components/AudioBookCard.vue';
import ProfileNavPan from '@/components/ProfileNavPan.vue';

export default {
  components: {
    AudioBookCard,
    ProfileNavPan,
  },
  data() {
    return {
      selectedFavourite: '',
      authorQuery: '',
      voiceActorQuery: '',
      nameQuery: '',
      page: 1,
      take: 20,
      loading: true,
    };
  },

  computed: {
    ...mapState(['user', 'userAudioBookLibrary', 'genres']),
    ...mapGetters(['isAuthenticated']),
  },
  methods: {
    ...mapActions([
      'fetchUserAudioBookLibrary',
      'fetchUserInfo',
    ]),

    openAudioBookDetails(audiobookId) {
      this.$router.push({ name: 'AudioBookDetails', params: { id: audiobookId } });
    },

    changePage(newPage) {
      this.page = newPage;
      this.fetchAudioBooksData();
    },

    async fetchAudioBooksData() {
      const {
        authorQuery, voiceActorQuery, nameQuery, page, take, selectedFavourite,
      } = this;
      await this.fetchUserAudioBookLibrary({
        take,
        page,
        name: nameQuery,
        author: authorQuery,
        voiceActor: voiceActorQuery,
        isFavourite: selectedFavourite === '' ? null : selectedFavourite,
      });
    },
  },

  watch: {
    selectedFavourite() {
      this.page = 1;
      this.fetchAudioBooksData();
    },
    authorQuery() {
      this.page = 1;
      this.fetchAudioBooksData();
    },
    voiceActorQuery() {
      this.page = 1;
      this.fetchAudioBooksData();
    },
    nameQuery() {
      this.page = 1;
      this.fetchAudioBooksData();
    },
  },

  created() {
    if (!this.user) {
      this.fetchUserInfo();
    }
    this.$store.dispatch('fetchGenres');
    this.fetchAudioBooksData();
  },
};
</script>

<style scoped>
.not_authorised_message {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 75vh;
}

.not_authorised_message p {
  font-size: 1.5rem;
  font-weight: bold;
}

.not_authorised_message,
.loading_message {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 75vh;
  font-size: 1.3rem;
  font-weight: bold;
}

.loading_message {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 75vh;
}

.loading_message p {
  font-size: 1.5rem;
  font-weight: bold;
}

.main-page {
  padding: 30px;
}

header {
  margin-bottom: 20px;
}

.search-controls {
  display: flex;
  flex-wrap: wrap;
  gap: 20px;

  margin-bottom: 24px;
  padding: 20px;
  border-radius: 12px;
  background: #d4d4d4;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.05);
}

.search-group {
  display: flex;
  flex-direction: column;
}

.search-group input,
.search-group select {
  padding: 8px 12px;
  font-size: 16px;
  border-radius: 10px;
  border: 1px solid #ccc;
  transition: border-color 0.3s, box-shadow 0.3s;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.search-group input:focus,
.search-group select:focus {
  border-color: #d4d4d4;
  outline: none;
  box-shadow: 0 0 0 3px rgba(79, 70, 229, 0.2);
}

.search-group label {
  margin-bottom: 5px;
  font-size: 17px;
  /* color: #555; */
}

.books-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 20px;
}

footer {
  margin-top: 20px;
  text-align: center;
}

footer button {
  margin-left: 10px;
  margin-right: 10px;
  padding: 5px;
  /* border: none; */
  border-radius: 10px;
}
</style>
