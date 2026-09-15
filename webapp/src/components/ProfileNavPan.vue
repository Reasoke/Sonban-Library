<template>
  <nav class="nav-panel">
    <router-link to="/tokens" class="nav-item" active-class="active-link">
      Tokens
    </router-link>

    <router-link to="/profile" class="nav-item" active-class="active-link">
      Profile
    </router-link>

    <router-link to="/userBookLibrary" class="nav-item" active-class="active-link">
      User book library
    </router-link>

    <router-link to="/userAudioBookLibrary" class="nav-item" active-class="active-link">
      User audiobook library
    </router-link>
  </nav>
</template>

<script>
export default {
  methods: {
  },
};
</script>

<style scoped>
.nav-panel {
  margin-top: 10px;
  margin-bottom: 10px;
  display: flex;
  justify-content: flex-start;
  align-items: center;
  gap: 2rem;
  padding: 1rem 2rem;
  background-color: #d4d4d4;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.nav-item {
  text-decoration: none;
  color: #333;
  font-weight: 500;
  padding: 0.5rem 1rem;
  border-radius: 8px;
  transition: background-color 0.2s ease, color 0.2s ease;
}

.nav-item:hover {
  background-color: #d1e3f2;
}

.active-link {
  background-color: #B8C7D4;
  color: black;
}
</style>
