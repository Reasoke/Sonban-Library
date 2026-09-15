<template>
  <div class="comments-section">

    <div class="comment-input">
      <div v-if="replyTo" class="reply-preview">
        Replying to @{{ replyTo.userCreatorName }}
        <button @click="clearReply">✖</button>
      </div>

      <label for="commentInput" class="visually-hidden">
        Write a comment
      </label>

      <textarea
        id="commentInput"
        v-model="newComment"
        placeholder="Write a comment..."
      ></textarea>

      <button class="btn send" @click="submitComment">
        📝 Submit comment
      </button>
    </div>

    <div class="comments-list">
      <CommentItem
        v-for="c in comments"
        :key="c.commentId"
        :comment="c"
        @reply="setReply"
      />
    </div>

  </div>
</template>

<script>
import { mapActions } from 'vuex';
import axios from 'axios';
import CommentItem from './CommentItem.vue';

export default {
  props: {
    productId: Number,
    productType: Number,
  },

  components: { CommentItem },

  data() {
    return {
      newComment: '',
      replyTo: null,
      comments: [],
      isLoading: false,
    };
  },

  computed: {
    // ...mapState(['comments']),
  },

  methods: {
    ...mapActions(['postComment']),

    setReply(comment) {
      this.replyTo = comment;
    },

    clearReply() {
      this.replyTo = null;
    },

    async submitComment() {
      if (!this.newComment.trim()) return;

      try {
        const newComment = await this.postComment({
          productId: this.productId,
          productType: this.productType,
          message: this.newComment,
          replyToUserId: this.replyTo?.userCreatorId || 0,
        });

        this.comments.unshift(newComment);

        this.newComment = '';
        this.replyTo = null;
      } catch (e) {
        console.error(e);
      }
    },

    async fetchComments() {
      this.isLoading = true;

      try {
        const response = await axios.get('/api/comments', {
          params: {
            productId: this.productId,
            productType: this.productType,
          },
        });

        this.comments = response.data;
      } catch (err) {
        console.error(err);
        alert('⚠️ Не вдалося завантажити коментарі');
      } finally {
        this.isLoading = false;
      }
    },
  },
  // mounted() {
  //   this.fetchComments({
  //     productId: this.productId,
  //     productType: this.productType,
  //   });
  // },
  watch: {
    productId: {
      immediate: true,
      handler() {
        this.fetchComments();
      },
    },
  },
};
</script>

<style scoped>
.comments-section {
  margin-top: 40px;
}

.comment-input {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.visually-hidden {
  position: absolute;
  width: 1px;
  height: 1px;
  padding: 0;
  margin: -1px;
  overflow: hidden;
  clip: rect(0, 0, 0, 0);
  border: 0;
}

textarea {
  resize: none;
  min-height: 80px;
  padding: 10px;
  border-radius: 8px;
  border: 1px solid #ccc;
}

.send {
  align-self: flex-end;
  background-color: #4caf50;
  color: white;
  padding: 8px 16px;
  border: none;
  border-radius: 8px;
  cursor: pointer;
  transition: 0.2s;
}

.reply-preview {
  background: #eee;
  padding: 5px 10px;
  border-radius: 8px;
  display: inline-flex;
  align-items: center;
  gap: 10px;
}
</style>
