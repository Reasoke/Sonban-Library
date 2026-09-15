<template>
  <div class="center-wrapper">
    <main>
      <h2>Forgot password</h2>

      <p class="description">
        Enter your email address and we'll send you a password reset link.
      </p>

      <form
        v-if="!emailSent"
        @submit.prevent="submitForm"
      >
        <div class="form-group">
          <label for="email">
            Email

            <input
              id="email"
              v-model="email"
              type="email"
              placeholder="Enter your email"
              required
              @blur="validateEmail"
            />
          </label>

          <p
            v-if="emailError"
            class="error-message"
          >
            {{ emailError }}
          </p>
        </div>

        <div class="actions">
          <router-link
            to="/login"
            class="back-btn"
          >
            Back
          </router-link>

          <button
            type="submit"
            class="submit-btn"
            :disabled="!isFormValid || loading"
          >
            {{ loading ? 'Sending...' : 'Send reset link' }}
          </button>
        </div>
      </form>

      <div
        v-else
        class="success-container"
      >
        <p class="success-message">
          Password reset instructions have been sent to your email.
        </p>

        <router-link
          to="/login"
          class="back-link"
        >
          Back to login
        </router-link>
      </div>
    </main>
  </div>
</template>

<script>
import axios from 'axios';

export default {
  name: 'ForgotPasswordPage',

  data() {
    return {
      email: '',
      emailError: null,
      loading: false,
      emailSent: false,
    };
  },

  computed: {
    isFormValid() {
      return this.email && !this.emailError;
    },
  },

  methods: {
    validateEmail() {
      const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

      if (!this.email) {
        this.emailError = 'Email is required.';
      } else if (!emailPattern.test(this.email)) {
        this.emailError = 'Invalid email format.';
      } else {
        this.emailError = null;
      }
    },

    async submitForm() {
      axios.post('/api/auth/forgot-password', {
        email: this.email,
      }).catch(console.error);

      this.emailSent = true;
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

.description {
  margin-bottom: 25px;
  color: #666;
  font-size: 0.95rem;
  line-height: 1.4;
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

.actions {
  display: flex;
  gap: 10px;
  margin-top: 20px;
}

.back-btn {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;

  padding: 12px;
  border: 2px solid #B8C7D4;
  border-radius: 12px;

  color: #3d4d5f;
  text-decoration: none;

  transition: 0.2s;
}

.back-btn:hover {
  background-color: #f4f7fa;
}

.submit-btn {
  flex: 2;
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

.success-container {
  display: flex;
  flex-direction: column;
  gap: 20px;
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
</style>
