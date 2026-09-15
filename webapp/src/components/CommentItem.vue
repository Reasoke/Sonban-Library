<template>
  <div class="comment">
    <div class="comment-header">
      <strong>{{ comment.userCreatorName }}</strong>
      <span class="time">{{ formatDate(comment.createdTime) }}</span>
    </div>

    <div class="comment-body">
      <span v-if="comment.replyToUserName" class="reply-tag">
        @{{ comment.replyToUserName }}
      </span>
      {{ comment.message }}
    </div>

    <button class="reply-btn" @click="$emit('reply', comment)">
      Reply
    </button>
  </div>
</template>

<script>
export default {
  props: {
    comment: Object,
  },
  methods: {
    formatDate(date) {
      return new Date(date).toLocaleString();
    },
  },
};
</script>

<style scoped>
.comment {
  padding: 10px;
  margin: 10px;
  /*border-bottom: 2px solid #ddd;*/
  background-color: rgba(212, 212, 212, 0.5);
}

.comment-header {
  display: flex;
  justify-content: space-between;
  font-size: 14px;
}

.comment-body {
  margin-top: 5px;
}

.reply-tag {
  color: #7f54b7;
  margin-right: 5px;
}

.reply-btn {
  margin-top: 5px;
  background: none;
  border: none;
  color: #007bff;
  cursor: pointer;
}
</style>
