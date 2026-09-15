<template>
  <div class="center-wrapper">
    <main>
      <h2>Create an account</h2>
      <form @submit.prevent="submitForm">

        <div class="form-group">
          <label for="username">
            Username
            <input type="text" id="username" v-model="username"
            required placeholder="Enter your username"
              @blur="validateUsername" />
          </label>
          <p v-if="usernameError" class="error-message">{{ usernameError }}</p>
        </div>

        <div class="form-group">
          <label for="email">
            Email
            <input type="email" id="email" v-model="email" required placeholder="Enter your email"
              @blur="validateEmail" />
          </label>
          <p v-if="emailError" class="error-message">{{ emailError }}</p>
        </div>

        <div class="form-group">
          <label for="password">
            Password
            <input type="password" id="password" v-model="password"
            required placeholder="Enter your password"
              @blur="validatePassword" />
          </label>
          <p v-if="passwordError" class="error-message">{{ passwordError }}</p>
        </div>

        <div class="form-group">
          <label for="confirmPassword">
            Confirm password
            <input type="password" id="confirmPassword" v-model="confirmPassword" required
              placeholder="Confirm your password" @blur="validateConfirmPassword" />
          </label>
          <p v-if="confirmPasswordError" class="error-message">{{ confirmPasswordError }}</p>
        </div>

        <div class="extra-options">
          <router-link to="/login">Already have an account?</router-link>
        </div>

        <button type="submit" class="submit-btn" :disabled="!isFormValid">
          Continue
        </button>
      </form>
    </main>
  </div>
</template>

<script>
export default {
  props: {
    isVisible: {
      type: Boolean,
      default: false,
    },
  },
  data() {
    return {
      username: '',
      email: '',
      password: '',
      confirmPassword: '',
      usernameError: null,
      emailError: null,
      passwordError: null,
      confirmPasswordError: null,
    };
  },
  computed: {
    isFormValid() {
      return (
        !this.usernameError
        && !this.emailError
        && !this.passwordError
        && !this.confirmPasswordError
        && this.username
        && this.email
        && this.password
        && this.confirmPassword
      );
    },
  },
  methods: {
    validateUsername() {
      if (!this.username) {
        this.usernameError = 'Login is a required field.';
      } else if (this.username.length < 3) {
        this.usernameError = 'Login must be at least 3 characters long';
      } else {
        this.usernameError = null;
      }
    },
    validateEmail() {
      const emailPattern = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
      if (!this.email) {
        this.emailError = 'Email is a required field.';
      } else if (!emailPattern.test(this.email)) {
        this.emailError = 'Incorrect Email format';
      } else {
        this.emailError = null;
      }
    },
    validatePassword() {
      if (!this.password) {
        this.passwordError = 'Password is a required field.';
      } else if (this.password.length < 6) {
        this.passwordError = 'Password must be at least 6 characters long.';
      } else {
        this.passwordError = null;
      }
    },
    validateConfirmPassword() {
      if (!this.confirmPassword) {
        this.confirmPasswordError = 'Password confirmation is a required field.';
      } else if (this.confirmPassword !== this.password) {
        this.confirmPasswordError = 'Passwords do not match.';
      } else {
        this.confirmPasswordError = null;
      }
    },
    async submitForm() {
      try {
        await this.$store.dispatch('postRegisterUser', {
          name: this.username,
          email: this.email,
          password: this.password,
        });

        if (this.$store.state.user) {
          this.$router.push({ name: 'BooksLibraryPage' });
        }
      } catch (error) {
        this.usernameError = 'Authorization error. Check the entered data.';
        this.emailError = 'Authorization error. Check the entered data.';
        this.passwordError = 'Authorization error. Check the entered data.';
      }
    },
    closeModal() {
      this.$emit('close');
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
  position: relative;
}

h2 {
  font-size: 1.5rem;
  margin-bottom: 20px;
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

button.submit-btn {
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

button.submit-btn:disabled {
  background-color: #ddd;
}

button.submit-btn:hover:not(:disabled) {
  background-color: #63a0e6;
}

.error-message {
  color: #f44336;
  font-size: 0.875rem;
  margin-top: 5px;
}

.extra-options {
  margin-bottom: 20px;
  display: flex;
  flex-direction: column;
  align-items: flex-start;
  text-align: left;
}

.extra-options a {
  color: #4285f4;
  text-decoration: none;
  margin-bottom: 10px;
}

.extra-options a:hover {
  text-decoration: underline;
}
</style>
