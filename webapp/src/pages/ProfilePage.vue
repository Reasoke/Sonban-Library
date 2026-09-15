<template>
  <main>
    <div v-if="loading">
      <div class="loading_message">
        <p>Loading...</p>
      </div>
    </div>

    <div v-else-if="isAuthenticated">
      <ProfileNavPan/>

      <div class="container">

        <div class="main">
          <img src="https://cdn-icons-png.flaticon.com/512/847/847969.png" alt="Avatar" class="avatar" />

          <label for="name" class="visually-hidden">Name
            <input type="text" id="name" v-model="editedProfile.name" class="input" /></label>

          <label for="email" class="visually-hidden">Email
            <input type="email" id="email" v-model="editedProfile.email" class="input" /></label>

          <div class="subscription">
            <p class="subscription-text">
              Subscription: <strong>Normal</strong>
            </p>

            <button class="subscription-link" @click="goToSubscription">
              Change subscription
            </button>
          </div>

          <div class="buttons-row">
            <button class="save-button" @click="saveProfile">Save</button>
            <button class="logout-button" @click="logoutProfile">Log Out</button>
          </div>
        </div>
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
import ProfileNavPan from '@/components/ProfileNavPan.vue';

export default {
  components: {
    ProfileNavPan,
  },
  data() {
    return {
      editedProfile: {
        name: '',
        email: '',
      },
      loading: true,
    };
  },
  computed: {
    ...mapState(['user']),
    ...mapGetters(['isAuthenticated']),
  },
  watch: {
    user: {
      immediate: true,
      handler(newVal) {
        if (newVal) {
          this.editedProfile = { ...newVal };
        }
      },
    },
  },
  async created() {
    try {
      await this.fetchUserInfo();
    } catch (error) {
      console.error('Failed to load profile:', error);
    } finally {
      this.loading = false;
    }
  },
  methods: {
    ...mapActions(['fetchUserInfo', 'updateProfile', 'logout']),
    async saveProfile() {
      this.loading = true;
      const success = await this.updateProfile(this.editedProfile);
      await this.fetchUserInfo();
      this.loading = false;
      if (!success) {
        alert('Failed to save profile');
      }
    },
    async logoutProfile() {
      await this.logout();
    },
    goToSubscription() {
      this.$router.push('/subscriptions');
    },
  },
};
</script>

<style scoped>
.not_authorised_message{
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 75vh;
}

.not_authorised_message p {
  font-size: 1.5rem;
  font-weight: bold;
}

.loading_message{
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 75vh;
}

.loading_message p {
  font-size: 1.5rem;
  font-weight: bold;
}

.container {
  display: flex;
  align-items: center;
  justify-content: center;
}

/* Main section */
.main {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 40px 0px;
  font-size: 1.2rem;
  text-align: center;
}

.avatar {
  width: 80px;
  height: 80px;
  margin-bottom: 30px;
}

.input {
  display: block;
  margin: 1rem 0;
  padding: 0.5rem;
  width: 100%;
  border-radius: 20px;
}

.buttons-row {
  display: flex;
  flex-direction: row;
  gap: 15px;
  margin-top: 10px;
  justify-content: center;
  flex-wrap: wrap;
}

.save-button,
.logout-button {
  padding: 8px 24px;
  border: 1px solid #aaa;
  border-radius: 10px;
  cursor: pointer;
  color: #ffffff;

  font-size: 1rem;
  line-height: 1;
  height: 40px;

  display: inline-flex;
  align-items: center;
  justify-content: center;
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

.input {
  width: 300px;
  padding: 10px;
  margin-bottom: 20px;
  text-align: center;
  border: 1px solid #000000;
  background-color: #B8C7D4;
  font-family: monospace;
}

.save-button {
  padding: 8px 24px;
  background-color: #28a776;
  border: 1px solid #aaa;
  cursor: pointer;
  color: #ffffff;
}

.save-button:hover {
  background-color: #1f7855;
}

.logout-button {
  padding: 8px 24px;
  background-color: #2859a7;
  border: 1px solid #aaa;
  cursor: pointer;
  color: #ffffff;
}

.logout-button:hover {
  background-color: #163972;
}

.subscription {
  display: flex;
  flex-direction: column;
  align-items: center;
  margin-bottom: 20px;
}

.subscription-text {
  margin: 10px 0;
  font-size: 1.1rem;
}

.subscription-text strong {
  font-weight: bold;
}

.subscription-link {
  padding: 8px 24px;
  border-radius: 10px;
  border: 1px solid #aaa;
  cursor: pointer;
  background-color: #d4d4d4;
  transition: 0.2s;
}

.subscription-link:hover {
  background-color: #9e9e9e;
}
</style>
