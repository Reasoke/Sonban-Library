<template>
  <div class="subscription-page">
    <h1>Subscription page</h1>

    <p>There will be a possibility to wiev annd change subscription.</p>

    <button @click="$router.back()">← Back</button>
  </div>
</template>

<script>
export default {
  name: 'SubscriptionPage',
};
</script>

<style scoped>
.subscription-page {
  text-align: center;
  padding: 40px;
  font-size: 18px;
}
</style>
