<template>
  <div class="book-card" @click="$emit('click')" tabindex="0" @keydown.enter="$emit('click')">
    <img :src="`/api/books/files/${book.id}/cover`" alt="Book cover" />
    <h3 class="book-title">{{ book.name }}</h3>
    <p class="authors">Author(s): {{ authorsList }}</p>
    <p class="price">{{ priceText }}</p>
  </div>
</template>

<script>

export default {
  props: {
    book: { type: Object, required: true },
  },

  computed: {
    authorsList() {
      return this.book.authors.map((a) => a.name).join(', ');
    },

    priceText() {
      return this.book.price > 0 ? `${this.book.price} $` : 'Free';
    },
  },
};
</script>

<style scoped>
.book-card {
  border: 1px solid #ccc;
  padding: 10px;
  cursor: pointer;
  outline: none;
  display: flex;
  flex-direction: column;
  align-items: center;
}

.book-card:hover {
  box-shadow: 0 0 10px rgba(0, 0, 0, 0.15);
}

.book-card img {
  /* width: 200px; */
  height: 200px;
  object-fit: cover;
  margin-bottom: 10px;
}

.book-title {
  text-align: center;
}

.authors,
.price {
  margin: 0;
  font-size: 14px;
  color: #555;
  text-align: center;
}
</style>
