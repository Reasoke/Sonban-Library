<template>
  <div id="app">
    <Header />
    <router-view />
    <Footer />
  </div>
</template>

<script>
import Header from './components/HeaderComponent.vue';
import Footer from './components/FooterComponent.vue';

export default {
  name: 'App',
  components: {
    Header,
    Footer,
  },
};
</script>

<style>
* {
  margin: 0;
  padding: 0;
}

html,
body {
  height: 100%;
  background-color: #eeeadf;
}

#app {
  display: flex;
  flex-direction: column;
  min-height: 100vh;
  width: 100%;
  box-sizing: border-box;
}

router-view {
  flex-grow: 1;
}

footer {
  background-color: #d4d4d4;
  text-align: center;
  padding: 10px;
  margin-top: auto;
}
</style>
