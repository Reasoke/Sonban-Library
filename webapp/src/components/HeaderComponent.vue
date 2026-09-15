<template>
  <header>
    <nav>
      <RouterLink  to="/books" class="logo">
        <img class="logo_img" alt="logo" src="../assets/logo.png" />
      </RouterLink >

      <div class="menu">
        <RouterLink  to="/books" class="link nav-link">
          Books library
        </RouterLink >

        <RouterLink  to="/audiobooks" class="link nav-link">
          Audiobooks library
        </RouterLink >

        <button class="profile_button" @click="goToProfile" aria-label="Profile">
          <img class="profile_img" alt="Profile" src="../assets/profile.svg">
        </button>
      </div>
    </nav>
  </header>
</template>

<script>
export default {
  methods: {
    goToProfile() {
      this.$router.push('/profile');
    },
  },
};
</script>

<style scoped>
nav {
  top: 0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 0rem 2rem;
  background-color: #d4d4d4;
  box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.logo_img {
  height: 6.5rem;
  font-size: 1.5rem;
  font-weight: bold;
}

.menu {
  display: flex;
  align-items: center;
  gap: 1.5rem;
}

.menu a {
  text-decoration: none;
  color: #000;
  font-weight: 700;
  font-size: 1.47rem;
}

.nav-link.router-link-exact-active {
  border-bottom: 2px solid #270082;
  opacity: 1;
}

.nav-link.router-link-active {
  opacity: 0.7;
}

.profile_button {
  background: none;
  border: none;
  cursor: pointer;
  font-size: 2rem;
}

.profile_img {
  height: 2.5rem;
  font-size: 1.5rem;
  font-weight: bold;
}
</style>
