<template>
  <div class="center-wrapper">
    <main>

      <h2>Create new password</h2>

      <!-- LOADING TOKEN CHECK -->
      <div v-if="!tokenChecked" class="loading">
        Checking link...
      </div>

      <!-- INVALID TOKEN -->
      <div v-else-if="!tokenValid" class="error-container">
        <p class="error-message">
          Reset link is invalid or has expired.
        </p>

        <router-link to="/login" class="back-link">
          Back to login
        </router-link>
      </div>

      <!-- SUCCESS STATE -->
      <div v-else-if="passwordChanged" class="success-container">
        <p class="success-message">
          Password successfully changed.
        </p>

        <router-link to="/login" class="back-link">
          Go to login
        </router-link>
      </div>

      <form v-else @submit.prevent="submitForm">
        <div class="form-group">
          <label for="password">
            New password

            <input
              id="password"
              v-model="password"
              type="password"
              required
              placeholder="Enter new password"
            />
          </label>
        </div>

        <div class="form-group">
          <label for="confirmPassword">
            Confirm password

            <input
              id="confirmPassword"
              v-model="confirmPassword"
              type="password"
              required
              placeholder="Confirm password"
            />
          </label>

          <p v-if="passwordError" class="error-message">
            {{ passwordError }}
          </p>
        </div>

        <button
          type="submit"
          class="submit-btn"
          :disabled="loading || !isFormValid"
        >
          {{ loading ? 'Saving...' : 'Save password' }}
        </button>
      </form>

    </main>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: 'ResetPasswordPage',

  props: {
    token: {
      type: String,
      required: true,
    },
  },

  data() {
    return {
      password: '',
      confirmPassword: '',
      loading: false,

      passwordChanged: false,

      tokenValid: false,
      tokenChecked: false,
    };
  },

  computed: {
    passwordError() {
      if (this.confirmPassword && this.password !== this.confirmPassword) {
        return 'Passwords do not match.';
      }
      return null;
    },

    isFormValid() {
      return (
        this.password && this.confirmPassword && !this.passwordError
      );
    },
  },

  async mounted() {
    await this.checkToken();
  },

  methods: {
    async checkToken() {
      try {
        await axios.get('/api/auth/validate-reset-token', {
          params: {
            token: this.token,
          },
        });

        this.tokenValid = true;
      } catch (error) {
        this.tokenValid = false;
      } finally {
        this.tokenChecked = true;
      }
    },

    async submitForm() {
      this.loading = true;

      try {
        await axios.post('/api/auth/reset-password', {
          token: this.token,
          newPassword: this.password,
        });

        this.passwordChanged = true;
      } catch (error) {
        console.error(error);
      } finally {
        this.loading = false;
      }
    },
  },
};
</script>

<style scoped>
.center-wrapper {
  display: flex;
  align-items: center;
  justify-content: center;
  min-height: 75vh;
}

main {
  background: #fff;
  border-radius: 8px;
  padding: 40px;
  width: 350px;
  text-align: center;
  box-shadow: 0 4px 10px rgba(0, 0, 0, 0.1);
}

h2 {
  font-size: 1.5rem;
  margin-bottom: 15px;
}

.form-group {
  margin-bottom: 20px;
  text-align: left;
}

label {
  display: block;
  margin-bottom: 5px;
  font-weight: bold;
}

input {
  width: 100%;
  padding: 10px;
  border: 2px solid #B8C7D4;
  border-radius: 12px;
  font-size: 1rem;
  box-sizing: border-box;
}

input:focus {
  border-color: #5783C8;
  outline: none;
}

.submit-btn {
  width: 100%;
  padding: 12px;
  background-color: #5783C8;
  color: #fff;
  border: none;
  border-radius: 12px;
  font-size: 1rem;
  cursor: pointer;
  transition: background-color 0.3s;
}

.submit-btn:disabled {
  background-color: #ddd;
  cursor: not-allowed;
}

.submit-btn:hover:not(:disabled) {
  background-color: #63a0e6;
}

.error-message {
  color: #f44336;
  font-size: 0.875rem;
  margin-top: 5px;
}

.success-container,
.error-container {
  display: flex;
  flex-direction: column;
  gap: 15px;
}

.success-message {
  color: #2e7d32;
  font-size: 1rem;
  line-height: 1.5;
}

.back-link {
  color: #4285f4;
  text-decoration: none;
  font-weight: 500;
}

.back-link:hover {
  text-decoration: underline;
}

.loading {
  color: #666;
  font-size: 1rem;
}
</style>
