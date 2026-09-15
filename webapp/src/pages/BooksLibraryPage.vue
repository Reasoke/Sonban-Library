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
          <label for="name-search">Search by name:</label>
          <input id="name-search" type="text" v-model="nameQuery" placeholder="Books name" />
        </div>

        <div class="search-group">
          <label for="genre-select">Genres:</label>
          <select id="genre-select" v-model="selectedGenre">
            <option value="">All genres</option>
            <option v-for="genre in genres" :key="genre.id" :value="genre.name">
              {{ genre.name }}
            </option>
          </select>
        </div>
      </div>
    </header>

    <div class="books-grid">
      <BookCard v-for="book in books" :key="book.id" :book="book"
      @click="openBookDetails(book.id)" />
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
import BookCard from '@/components/BookCard.vue';

export default {
  components: { BookCard },

  data() {
    return {
      selectedGenre: '',
      authorQuery: '',
      nameQuery: '',
      page: 1,
      take: 20,
    };
  },

  computed: {
    ...mapState(['books', 'genres']),
  },

  methods: {
    ...mapActions([
      'fetchBooks',
    ]),

    openBookDetails(bookId) {
      this.$router.push(`/book/${bookId}`);
    },

    changePage(newPage) {
      this.page = newPage;
      this.fetchBooksData();
    },

    async fetchBooksData() {
      const {
        selectedGenre, authorQuery, nameQuery, page, take,
      } = this;
      await this.fetchBooks({
        take, page, name: nameQuery, author: authorQuery, genre: selectedGenre,
      });
    },
  },

  watch: {
    selectedGenre() {
      this.page = 1;
      this.fetchBooksData();
    },
    authorQuery() {
      this.page = 1;
      this.fetchBooksData();
    },
    nameQuery() {
      this.page = 1;
      this.fetchBooksData();
    },
  },

  created() {
    this.$store.dispatch('fetchGenres');
    this.fetchBooksData();
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

.books-grid {
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
