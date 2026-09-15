<template>
  <div class="main-page">
    <header>
      <!-- <h1>Book library</h1> -->

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
          <input id="name-search" type="text" v-model="nameQuery" placeholder="AudionBooks name" />
        </div>
      </div>
    </header>

    <div class="audiobooks-grid">
      <AudioBookCard v-for="audiobook in audiobooks" :key="audiobook.id" :audiobook="audiobook"
      @click="openBookDetails(audiobook.id)" />
    </div>

    <footer>
      <button :disabled="page === 1" @click="changePage(page - 1)">Previous</button>
      <span>Page {{ page }}</span>
      <button @click="changePage(page + 1)">Next</button>
    </footer>
  </div>
</template>

<script>
import { mapState, mapActions } from 'vuex';
import AudioBookCard from '@/components/AudioBookCard.vue';

export default {
  components: { AudioBookCard },

  data() {
    return {
      authorQuery: '',
      voiceActorQuery: '',
      nameQuery: '',
      page: 1,
      take: 20,
    };
  },

  computed: {
    ...mapState(['audiobooks', 'genres']),
  },

  methods: {
    ...mapActions([
      'fetchAudioBooks',
    ]),

    openBookDetails(audiobookId) {
      this.$router.push(`/audiobook/${audiobookId}`);
    },

    changePage(newPage) {
      this.page = newPage;
      this.fetchAudioBooksData();
    },

    async fetchAudioBooksData() {
      const {
        authorQuery, voiceActorQuery, nameQuery, page, take,
      } = this;
      await this.fetchAudioBooks({
        take,
        page,
        name: nameQuery,
        author: authorQuery,
        voiceActor: voiceActorQuery,
      });
    },
  },

  watch: {
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
    this.$store.dispatch('fetchGenres');
    this.fetchAudioBooksData();
  },
};
</script>

<style scoped>
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

.audiobooks-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 20px;
}

footer {
  margin-top: 20px;
  text-align: center;
}

footer button{
  margin-left: 10px;
  margin-right: 10px;
  padding: 5px;
  /* border: none; */
  border-radius: 10px;
}
</style>
