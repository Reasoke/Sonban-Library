<template>
  <main>
    <div v-if="isAuthenticated">
      <ProfileNavPan />

      <div class="container">
        <div class="main-page tokens-page">

          <div class="tokens-card">
            <div class="header">
              <div>
                <h1>MSP Server Tokens</h1>

                <p>
                  Tokens used for authorization in your MSP server.
                  Keep them secure and never share them publicly.
                </p>
                <div class="server-info">
                  <span class="server-label">MSP Server URL</span>

                  <div class="server-url-container">
                    <code class="server-url">
                      https://localhost:8888/mcp
                    </code>

                    <button
                      class="copy-url-btn"
                      @click="copyServerUrl"
                    >
                      Copy
                    </button>
                  </div>

                  <p class="server-hint">
                    Use this URL when connecting your AI client to the MSP server.
                    Generate a token below and provide it as a Bearer token for authentication.
                  </p>
                </div>
              </div>

              <button
                class="create-btn"
                @click="openCreateModal"
              >
                + New Token
              </button>
            </div>

            <div
              v-if="loading"
              class="loading-state"
            >
              Loading...
            </div>

            <div
              v-else
              class="table-wrapper"
            >
              <table class="tokens-table">
                <thead>
                <tr>
                  <th>Name</th>
                  <th>Expires</th>
                  <th>Last used</th>
                  <th></th>
                </tr>
                </thead>

                <tbody>
                <tr
                  v-for="token in tokens"
                  :key="token.id"
                >
                  <td>
                    <div class="token-name">
                      {{ token.name }}
                    </div>
                  </td>

                  <td>
                    <div class="table-date">
                      {{ formatDate(token.expirationDate) }}
                    </div>
                  </td>

                  <td>
                    <div class="table-date">
                      {{
                        token.lastUsedDate
                          ? formatDate(token.lastUsedDate)
                          : 'Never used'
                      }}
                    </div>
                  </td>

                  <td class="actions">
                    <button
                      class="delete-btn"
                      @click="openDeleteModal(token)"
                    >
                      ✕
                    </button>
                  </td>
                </tr>

                <tr v-if="tokens.length === 0">
                  <td
                    colspan="4"
                    class="empty-state"
                  >
                    No tokens found
                  </td>
                </tr>
                </tbody>
              </table>
            </div>
          </div>

        </div>
      </div>
    </div>

    <div v-else-if="loading">
      <div class="loading_message">
        <p>Loading...</p>
      </div>
    </div>

    <div v-else>
      <div class="not_authorised_message">
        <router-link to="/login">
          Please log in to access this page
        </router-link>
      </div>
    </div>

    <!-- CREATE TOKEN MODAL -->
    <div v-if="showCreateModal" class="modal-overlay">
      <div class="modal">
        <button class="close-btn" @click="closeCreateModal">✕</button>

        <h2>Create New Token</h2>

        <div class="form-group">
          <label for="token-name">Token name</label>
          <input
            id="token-name"
            v-model="newToken.name"
            type="text"
            placeholder="Enter token name"
            class="modal-input"
          />
        </div>

        <div class="form-group">
          <label for="token-expiration">Expiration date (optional)</label>
          <input
            id="token-expiration"
            v-model="newToken.expirationDate"
            type="datetime-local"
            class="modal-input"
          />
        </div>

        <div class="modal-actions">
          <button class="cancel-btn" @click="closeCreateModal">
            Cancel
          </button>

          <button
            class="confirm-btn"
            :disabled="creating"
            @click="createToken"
          >
            {{ creating ? 'Creating...' : 'Create Token' }}
          </button>
        </div>
      </div>
    </div>

    <!-- TOKEN RESULT MODAL -->
    <div v-if="showTokenModal" class="modal-overlay">
      <div class="modal">
        <button class="close-btn" @click="closeTokenModal">✕</button>

        <h2>Token Created</h2>

        <p class="warning-text">
          This token will only be shown once.
          Copy and save it now — you won't be able to see it again.
        </p>

        <label for="created-token-output" class="visually-hidden">
          Created token
        </label>

        <textarea
          id="created-token-output"
          readonly
          :value="createdToken"
          class="token-output"
        />

        <div class="modal-actions">
          <button class="copy-btn" @click="copyToken">
            Copy Token
          </button>
        </div>
      </div>
    </div>

    <!-- DELETE MODAL -->
    <div v-if="showDeleteModal" class="modal-overlay">
      <div class="modal delete-modal">
        <button class="close-btn" @click="closeDeleteModal">✕</button>

        <h2>Delete Token</h2>

        <p class="delete-text">
          Are you sure you want to delete
          <strong>{{ deletingToken?.name }}</strong>?
        </p>

        <p class="delete-warning">
          This action cannot be undone.
        </p>

        <div class="modal-actions">
          <button class="cancel-btn" @click="closeDeleteModal">
            Cancel
          </button>

          <button class="danger-btn" @click="deleteToken">
            Delete
          </button>
        </div>
      </div>
    </div>
  </main>
</template>

<script>
import axios from 'axios';
import { mapActions, mapGetters, mapState } from 'vuex';
import ProfileNavPan from '@/components/ProfileNavPan.vue';

export default {
  name: 'MspTokensPage',

  components: {
    ProfileNavPan,
  },

  data() {
    return {
      loading: false,
      creating: false,

      tokens: [],

      showCreateModal: false,
      showTokenModal: false,
      showDeleteModal: false,

      createdToken: '',
      deletingToken: null,

      newToken: {
        name: '',
        expirationDate: '',
      },
    };
  },

  computed: {
    ...mapState(['user']),
    ...mapGetters(['isAuthenticated']),
  },

  async mounted() {
    await this.loadTokens();
  },

  methods: {
    ...mapActions(['fetchUserInfo']),

    async copyServerUrl() {
      try {
        await navigator.clipboard.writeText(
          'https://localhost:8888/mcp',
        );

        window.alert('Server URL copied');
      } catch {
        window.alert('Failed to copy URL');
      }
    },

    async loadTokens() {
      this.loading = true;

      try {
        const response = await axios.get('/api/profile/mcp-token', {
          withCredentials: true,
        });

        this.tokens = response.data;
      } catch (error) {
        console.error(error);
      } finally {
        this.loading = false;
      }
    },

    async createToken() {
      if (!this.newToken.name.trim()) {
        window.alert('Token name is required');
        return;
      }

      this.creating = true;

      try {
        const params = {
          name: this.newToken.name,
        };

        if (this.newToken.expirationDate) {
          params.expirationDate = new Date(
            this.newToken.expirationDate,
          ).toISOString();
        }

        const response = await axios.post(
          '/api/profile/mcp-token',
          null,
          {
            params,
            withCredentials: true,
          },
        );

        this.createdToken = response.data;

        this.closeCreateModal();
        this.showTokenModal = true;

        await this.loadTokens();
      } catch (error) {
        console.error(error);
      } finally {
        this.creating = false;
      }
    },

    openDeleteModal(token) {
      this.deletingToken = token;
      this.showDeleteModal = true;
    },

    closeDeleteModal() {
      this.showDeleteModal = false;
      this.deletingToken = null;
    },

    async deleteToken() {
      if (!this.deletingToken) return;

      try {
        await axios.delete(
          `/api/profile/mcp-token/${this.deletingToken.id}`,
          { withCredentials: true },
        );

        this.tokens = this.tokens.filter(
          (t) => t.id !== this.deletingToken.id,
        );

        this.closeDeleteModal();
      } catch (error) {
        console.error(error);
      }
    },

    async copyToken() {
      try {
        await navigator.clipboard.writeText(this.createdToken);
        window.alert('Token copied');
      } catch {
        window.alert('Failed to copy token');
      }
    },

    openCreateModal() {
      this.showCreateModal = true;
    },

    closeCreateModal() {
      this.showCreateModal = false;

      this.newToken = {
        name: '',
        expirationDate: '',
      };
    },

    closeTokenModal() {
      this.showTokenModal = false;
      this.createdToken = '';
    },

    formatDate(date) {
      return new Date(date).toLocaleString();
    },
  },
  created() {
    if (!this.user) {
      this.fetchUserInfo();
    }
  },
};
</script>

<style scoped>
.main-page {
  padding: 30px;
}

.tokens-page {
  min-height: 100vh;
  padding: 40px 20px;
  display: flex;
  justify-content: center;
}

.tokens-card {
  width: 100%;
  max-width: 1000px;
  background-color: #d4d4d4;
  border: 1px solid #8d9aa5;
  border-radius: 20px;
  padding: 30px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.08);
}

.header {
  display: flex;
  justify-content: space-between;
  align-items: flex-start;
  gap: 20px;
  margin-bottom: 30px;
}

.header h1 {
  margin: 0 0 10px;
  font-size: 2rem;
  color: #1c2d3f;
}

.header p {
  margin: 0;
  max-width: 650px;
  color: #3d4d5f;
  line-height: 1.5;
}

.server-info {
  margin-top: 20px;
  margin-bottom: 30px;
  padding: 16px;
  border: 1px solid #8d9aa5;
  border-radius: 12px;
  background-color: #c8d3dc;
}

.server-label {
  display: block;
  font-weight: 600;
  color: #1c2d3f;
  margin-bottom: 10px;
}

.server-url-container {
  display: flex;
  gap: 10px;
  align-items: center;
}

.server-url {
  flex: 1;
  padding: 10px 12px;
  background: #b8c7d4;
  border-radius: 8px;
  font-family: monospace;
  overflow-x: auto;
}

.copy-url-btn {
  padding: 8px 16px;
  border: 1px solid #8d9aa5;
  border-radius: 8px;
  background-color: #2859a7;
  color: white;
  cursor: pointer;
  transition: 0.2s;
}

.copy-url-btn:hover {
  background-color: #163972;
}

.server-hint {
  margin-top: 10px;
  font-size: 0.9rem;
  color: #3d4d5f;
}

.create-btn,
.confirm-btn,
.cancel-btn,
.copy-btn,
.danger-btn {
  padding: 8px 24px;
  border-radius: 10px;
  border: 1px solid #aaa;
  cursor: pointer;
  font-size: 1rem;
  transition: 0.2s;
}

.create-btn,
.confirm-btn,
.copy-btn {
  background-color: #2859a7;
  color: white;
}

.create-btn:hover,
.confirm-btn:hover,
.copy-btn:hover {
  background-color: #163972;
}

.cancel-btn {
  background-color: #d4d4d4;
}

.cancel-btn:hover {
  background-color: #9e9e9e;
}

.danger-btn {
  background-color: #a72828;
  color: white;
}

.danger-btn:hover {
  background-color: #721616;
}

.table-wrapper {
  overflow-x: auto;
}

.tokens-table {
  width: 100%;
  border-collapse: collapse;
}

.tokens-table thead {
  background-color: #b8c7d4;
}

.tokens-table th {
  padding: 16px;
  text-align: left;
  color: #1f2f40;
  font-size: 0.95rem;
  font-weight: bold;
}

.tokens-table td {
  padding: 18px 16px;
  border-bottom: 1px solid #9daab6;
  color: #1c2d3f;
}

.tokens-table tbody tr:hover {
  background-color: rgba(184, 199, 212, 0.35);
}

.token-name {
  font-weight: 600;
}

.actions {
  width: 70px;
  text-align: right;
}

.delete-btn {
  width: 36px;
  height: 36px;
  border: 1px solid #aaa;
  border-radius: 10px;
  background-color: #a72828;
  color: white;
  cursor: pointer;
  transition: 0.2s;
}

.delete-btn:hover {
  background-color: #721616;
}

.empty-state,
.loading-state {
  text-align: center;
  padding: 40px;
  color: #44576b;
  font-weight: 500;
}

.modal-overlay {
  position: fixed;
  inset: 0;
  background-color: rgba(0, 0, 0, 0.45);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal {
  position: relative;
  width: 100%;
  max-width: 500px;
  background-color: #d4d4d4;
  border: 1px solid #8d9aa5;
  border-radius: 20px;
  padding: 30px;
}

.close-btn {
  position: absolute;
  top: 15px;
  right: 15px;
  width: 34px;
  height: 34px;
  border: 1px solid #8d9aa5;
  border-radius: 10px;
  background-color: #b8c7d4;
  cursor: pointer;
  transition: 0.2s;
}

.close-btn:hover {
  background-color: #9daab6;
}

.modal h2 {
  margin-top: 0;
  color: #1c2d3f;
}

.form-group {
  margin-bottom: 20px;
}

.form-group label {
  display: block;
  margin-bottom: 8px;
  color: #1c2d3f;
  font-weight: 600;
}

.modal-input {
  width: 100%;
  padding: 12px;
  border-radius: 12px;
  border: 1px solid #8d9aa5;
  background-color: #b8c7d4;
  font-family: monospace;
  font-size: 0.95rem;
  box-sizing: border-box;
}

.modal-actions {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  margin-top: 20px;
}

.warning-text {
  color: #7a2d2d;
  font-weight: 600;
  line-height: 1.5;
  margin-bottom: 20px;
}

.delete-text {
  color: #1c2d3f;
  line-height: 1.5;
}

.delete-warning {
  color: #7a2d2d;
  font-weight: 600;
}

.token-output {
  width: 100%;
  height: 120px;
  resize: none;
  border-radius: 12px;
  border: 1px solid #8d9aa5;
  background-color: #b8c7d4;
  padding: 12px;
  font-family: monospace;
  font-size: 0.95rem;
  box-sizing: border-box;
}

.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  white-space: nowrap;
  border: 0;
}

@media (max-width: 768px) {
  .tokens-page {
    padding: 20px 10px;
  }

  .tokens-card,
  .modal {
    padding: 20px;
  }

  .header {
    flex-direction: column;
    align-items: stretch;
  }

  .create-btn {
    width: 100%;
  }

  .modal-actions {
    flex-direction: column;
  }
}
</style>
