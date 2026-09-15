<template>
  <div class="book-card" @click="$emit('click')" tabindex="0" @keydown.enter="$emit('click')">
    <img :src="`/api/audiobooks/files/${audiobook.id}/cover`" alt="AudioBook cover" />
    <h3 class="book-title">{{ audiobook.name }}</h3>
    <p class="authors">Author(s): {{ authorsList }}</p>
    <p class="voice-actors">Voice Actor(s): {{ voiceActorsList }}</p>
    <p class="price">{{ priceText }}</p>
  </div>
</template>

<script>
export default {
  props: {
    audiobook: { type: Object, required: true },
  },

  computed: {
    authorsList() {
      return this.audiobook.authors.map((a) => a.name).join(', ');
    },

    voiceActorsList() {
      return this.audiobook.voiceActors.map((a) => a.name).join(', ');
    },

    priceText() {
      return this.audiobook.price > 0 ? `${this.audiobook.price} $` : 'Free';
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
.voice-actors,
.price {
  margin: 0;
  font-size: 14px;
  color: #555;
  text-align: center;
}
</style>
