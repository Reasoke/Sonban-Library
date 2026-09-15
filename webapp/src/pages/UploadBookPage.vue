<template>
  <div class="upload-page">
    <h1>Завантаження книги</h1>
    <div
      class="drop-zone"
      @dragover.prevent
      @dragenter.prevent
      @drop.prevent="handleDrop"
    >
      <p v-if="!selectedFile">Перетягніть файл сюди або виберіть вручну</p>
      <p v-else>📘 {{ selectedFile.name }}</p>
      <label for="fileupload">
      <input type="file" accept=".fb2" @change="handleFileChange" hidden ref="fileInput" /></label>
      <button @click="$refs.fileInput.click()" class="upload-button">Обрати файл</button>
    </div>
    <button :disabled="!selectedFile" @click="uploadBook" class="upload-button">
      Завантажити
    </button>
  </div>
</template>

<script>
import { mapActions } from 'vuex';

export default {
  data() {
    return {
      selectedFile: null,
    };
  },
  methods: {
    ...mapActions(['uploadBookFile']),

    handleDrop(event) {
      const file = event.dataTransfer.files[0];
      if (file && file.name.endsWith('.fb2')) {
        this.selectedFile = file;
      } else {
        alert('⚠️ Лише .fb2 файли підтримуються');
      }
    },

    handleFileChange(event) {
      const file = event.target.files[0];
      if (file && file.name.endsWith('.fb2')) {
        this.selectedFile = file;
      } else {
        alert('⚠️ Лише .fb2 файли підтримуються');
      }
    },

    async uploadBook() {
      await this.uploadBookFile(this.selectedFile);
    },
  },
};
</script>

<style scoped>
.upload-page {
  max-width: 500px;
  margin: 40px auto;
  text-align: center;
}

.drop-zone {
  border: 2px dashed #4f46e5;
  border-radius: 12px;
  padding: 100px;
  background-color: #f0f4ff;
  cursor: pointer;
  margin-top: 20px;
  /* margin-bottom: 20px; */
  transition: background 0.2s;
}

.drop-zone:hover {
  background-color: #e5eaff;
}

.upload-button {
  margin-top: 20px;
  padding: 10px 20px;
  background: #4f46e5;
  color: white;
  border: none;
  border-radius: 8px;
  cursor: pointer;
}
</style>
