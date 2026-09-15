<template>
  <div class="book-details" v-if="bookDetails">
    <div class="book-header">
      <img :src="`/api/books/files/${bookDetails.id}/cover`" alt="Cover" class="cover" />

      <div class="info">
        <h1 class="title">{{ bookDetails.name }}</h1>

        <p><strong>Author(s):</strong>
          {{bookDetails.authors.map(a => a.name).join(', ') || 'Unknown'}}</p>
        <p><strong>Genre(s):</strong>
          {{bookDetails.genres.map(g => g.name).join(', ') || 'Uncategorized'}}</p>
        <p><strong>Year:</strong> {{ bookDetails.createdYear || 'Unknown' }}</p>
        <p><strong>Rating:</strong> {{ bookDetails.rating.toFixed(1) }} out of 5</p>
        <p><strong>Price:</strong>
          {{ bookDetails.price === 0 ? 'Free' : `$${bookDetails.price}` }}</p>

        <div v-if="bookDetails.isBought" class="rating-block">
          <div class="rating-header">
            <h3>Your rating</h3>

            <div class="stars">
              <span
                v-for="i in 5"
                :key="i"
                class="star"
                tabindex="0"
                role="button"
                @mousemove="handleHover($event, i)"
                @mouseleave="hoverRating = 0"
                @focus="hoverRating = i"
                @blur="hoverRating = 0"
                @click="giveRating($event, i)"
                @keydown.enter="giveRating($event, i)"
                @keydown.space.prevent="giveRating($event, i)"
              >
                <span class="star-bg">☆</span>
                <span
                  class="star-fill"
                  :style="{ width: getStarFill(i) }"
                >★</span>
              </span>
            </div>
          </div>

          <button class="btn buy" @click="submitRating" :disabled="!userRating">
            ⭐ Submit rating
          </button>
        </div>

        <div class="button-actions">
          <button v-if="bookDetails.isBought" class="btn download" @click="handleDownload"
            :disabled="isDownloading">
            <span v-if="isDownloading" class="spinner"></span>
            {{ isDownloading ? 'Downloading...' : '📥 Download' }}
          </button>
          <button v-else-if="bookDetails.price === 0" class="btn buy" @click="buyBook">
            📚 Add to library
          </button>
          <button v-else class="btn buy" @click="buyBook">🛒 Buy</button>
          <button class="btn" @click="toggleFavourite">
            {{ bookDetails.isFavourite ? '🗑 Remove from favourite' : '❤ Add to favourite' }}
          </button>
        </div>
      </div>
    </div>

    <div class="description">
      <h2>Anotation</h2>
      <p>{{ bookDetails.description }}</p>
    </div>

    <router-link class="back-link" to="/books">← Back to library</router-link>

    <div class="comments">
      <CommentsSection
        :key="bookDetails.id"
        :productId="bookDetails.id"
        :productType="1"
      />
    </div>
  </div>

  <div v-else class="loading_message">
    <p>Loading...</p>
  </div>
</template>

<script>
import { mapState, mapActions } from 'vuex';
import CommentsSection from '@/components/CommentsSection.vue';

export default {
  data() {
    return {
      userRating: 0,
      hoverRating: 0,
      isDownloading: false,
    };
  },
  computed: {
    ...mapState(['bookDetails']),
  },
  components: {
    CommentsSection,
  },

  methods: {
    ...mapActions(['fetchBookDetails', 'addToFavourites', 'removeFromFavourites', 'setRating', 'buyBookSession', 'downloadBook']),

    getStarFill(index) {
      const rating = this.hoverRating > 0 ? this.hoverRating : this.userRating;

      if (rating >= index) return '100%';
      if (rating >= index - 0.5) return '50%';
      return '0%';
    },

    handleHover(event, index) {
      const { left, width } = event.currentTarget.getBoundingClientRect();
      const x = event.clientX - left;

      this.hoverRating = x < width / 2 ? index - 0.5 : index;
    },

    giveRating(event, index) {
      const { left, width } = event.currentTarget.getBoundingClientRect();
      const x = event.clientX - left;

      this.userRating = x < width / 2 ? index - 0.5 : index;
      this.hoverRating = 0;
    },

    async submitRating() {
      try {
        await this.setRating({
          productId: this.bookDetails.id,
          productType: 1,
          rating: this.userRating,
        });
      } catch (e) {
        console.error(e);
      }
    },

    async toggleFavourite() {
      try {
        if (this.bookDetails.isFavourite) {
          await this.removeFromFavourites({ productId: this.bookDetails.id, productType: 1 });
          this.bookDetails.isFavourite = false;
        } else {
          await this.addToFavourites({ productId: this.bookDetails.id, productType: 1 });
          this.bookDetails.isFavourite = true;
        }
      } catch (err) {
        alert(err);
      }
    },

    async buyBook() {
      const request = {
        productName: this.bookDetails.name,
        productType: 1,
        unitAmount: Math.round(this.bookDetails.price * 100), // в копійки
        quantity: 1,
        currency: 'usd',
        successUrl: `${window.location.origin}/book/${this.bookDetails.id}`,
        cancelUrl: `${window.location.origin}/book/${this.bookDetails.id}`,
      };
      this.buyBookSession({ productId: this.bookDetails.id, request });
    },

    async handleDownload() {
      try {
        this.isDownloading = true;

        await this.downloadBook({
          productId: this.bookDetails.id,
          productType: 1,
          title: this.bookDetails.name,
        });
      } finally {
        this.isDownloading = false;
      }
    },
  },
  watch: {
    '$route.params.id': {
      immediate: true,
      handler(newId) {
        if (newId) {
          // this.fetchBookDetails(newId);
          this.fetchBookDetails(newId).then(() => {
            this.userRating = this.bookDetails.personalRating || 0;
          });
        }
      },
    },
  },
};
</script>

<style scoped>
.book-details {
  max-width: 900px;
  margin: 0 auto;
  padding: 30px;
  font-family: Arial, sans-serif;
}

.book-header {
  display: flex;
  gap: 30px;
  align-items: flex-start;
  margin-bottom: 30px;
}

.cover {
  width: 200px;
  height: auto;
  border-radius: 12px;
  object-fit: cover;
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
}

.info {
  flex: 1;
}

.info p{
  margin-top: 10px;
}

.title {
  font-size: 2rem;
  margin-bottom: 10px;
}

.button-actions {
  display: flex;
  align-items: center;
  gap: 15px;
  margin-top: 15px;
}

.btn {
  padding: 8px 16px;
  background-color: #d4d4d4;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: 0.2s;
}

.btn:hover {
  background-color: #9e9e9e;
}

.buy {
  background-color: #4caf50;
  color: white;
}

.buy:hover {
  background-color: #429344;
}

.description {
  margin-top: 20px;
}

.description h2 {
  font-size: 1.5rem;
  margin-bottom: 10px;
}

.back-link {
  display: inline-block;
  margin-top: 20px;
  text-decoration: underline;
  color: #007bff;
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

.rating-header {
  display: flex;
  align-items: center;
  gap: 15px;
}

.rating-header h3 {
  margin: 0;
}

/*.rating-block {
  margin-top: 10px;
}*/

.stars {
  display: flex;
  gap: 5px;
  cursor: pointer;
  font-size: 35px;
}

.star {
  position: relative;
  display: inline-block;
}

.star-bg {
  color: #ccc;
}

.star-fill {
  position: absolute;
  top: 0;
  left: 0;
  overflow: hidden;
  color: gold;
  white-space: nowrap;
  pointer-events: none;
  transition: width 0.15s ease;
}

.btn:disabled {
  opacity: 0.7;
  cursor: not-allowed;
}

.spinner {
  display: inline-block;
  width: 14px;
  height: 14px;
  margin-right: 8px;

  border: 2px solid transparent;
  border-top: 2px solid currentColor;
  border-radius: 50%;

  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0);
  }

  to {
    transform: rotate(360deg);
  }
}
</style>
